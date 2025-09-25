using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Data;
using SmartInvoice.Models;
using SmartInvoice.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInvoice.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public InvoiceController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // عرض كل الفواتير
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices.Include(i => i.Customer).ToListAsync();
            return View(invoices);
        }

        // تفاصيل فاتورة
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // إنشاء فاتورة جديدة
        public IActionResult Create()
        {
            ViewBag.Customers = GetCustomersSelectList() ?? new List<SelectListItem>();
            ViewBag.Currencies = GetCurrencies() ?? new List<string>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                invoice.Amount = invoice.Quantity * invoice.Price;
                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // إعادة تحميل القوائم لو حدث خطأ في الفورم
            ViewBag.Customers = GetCustomersSelectList() ?? new List<SelectListItem>();
            ViewBag.Currencies = GetCurrencies() ?? new List<string>();
            return View(invoice);
        }

        // تعديل فاتورة
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            ViewBag.Customers = GetCustomersSelectList() ?? new List<SelectListItem>();
            ViewBag.Currencies = GetCurrencies() ?? new List<string>();
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    invoice.Amount = invoice.Quantity * invoice.Price;
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Invoices.Any(e => e.Id == invoice.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Customers = GetCustomersSelectList() ?? new List<SelectListItem>();
            ViewBag.Currencies = GetCurrencies() ?? new List<string>();
            return View(invoice);
        }

        // حذف فاتورة
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                                        .Include(i => i.Customer)
                                        .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // إرسال الفاتورة بالبريد
        public async Task<IActionResult> SendInvoice(int id)
        {
            var invoice = await _context.Invoices.Include(i => i.Customer).FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null) return NotFound();

            string subject = $"Invoice #{invoice.Id}";
            string body = $"Hello {invoice.Customer.Name},<br/><br/>Here is your invoice with total: {invoice.Total} {invoice.Currency}.<br/><br/>Thank you.";

            await _emailService.SendEmailAsync(invoice.Customer.Email, subject, body);

            TempData["Message"] = "Invoice sent successfully!";
            return RedirectToAction(nameof(Index));
        }

        // تعليم الفاتورة كمدفوعة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                invoice.Status = "Paid";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ✅ Helpers
        private List<SelectListItem> GetCustomersSelectList()
        {
            // رجعي قائمة فارغة لو ما في عملاء
            return _context.Customers
                           .Select(c => new SelectListItem
                           {
                               Value = c.Id.ToString(),
                               Text = c.Name
                           }).ToList() ?? new List<SelectListItem>();
        }

        private List<string> GetCurrencies()
        {
            return new List<string> { "SDG", "USD", "EUR", "GBP", "SAR" }; // أضفت الريال السعودي
        }
    }
}