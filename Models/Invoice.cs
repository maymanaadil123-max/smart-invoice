using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInvoice.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Currency { get; set; } = "SDG"; // قيمة افتراضية

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);

        [Required]
        public string Status { get; set; } = "Unpaid";

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; } = "";

        // Customer
        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // User
        public string? UserId { get; set; }  // خليها nullable لتجنب خطأ NOT NULL
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [NotMapped]
        public decimal Total => Quantity * Price; // تحسب تلقائي
    }
}