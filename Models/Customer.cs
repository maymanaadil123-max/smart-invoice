using System.ComponentModel.DataAnnotations;

namespace SmartInvoice.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        // إضافي لو حابة تربطي مع Invoice لاحقًا
        // public ICollection<Invoice> Invoices { get; set; }
    }
}