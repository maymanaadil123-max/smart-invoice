using Microsoft.EntityFrameworkCore;
using SmartInvoice.Data;
using SmartInvoice.Services;
using System;

public class InvoiceReminderService
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public InvoiceReminderService(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendRemindersAsync()
    {
        var unpaidInvoices = _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status == "Unpaid" && i.DueDate < DateTime.Now.AddDays(3)) // قبل 3 أيام من الاستحقاق
            .ToList();

        foreach (var invoice in unpaidInvoices)
        {
            string subject = $"Reminder: Invoice #{invoice.Id} is due soon";
            string body = $"Hello {invoice.Customer.Name},<br/><br/>" +
                          $"This is a reminder that your invoice #{invoice.Id} with total {invoice.Total} {invoice.Currency} " +
                          $"is due on {invoice.DueDate:dd/MM/yyyy}.<br/><br/>Thank you.";

            await _emailService.SendEmailAsync(invoice.Customer.Email, subject, body);
        }
    }
}