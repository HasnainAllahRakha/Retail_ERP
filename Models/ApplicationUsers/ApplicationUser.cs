using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Erp.Models.Sales;
using Erp.Models.Notify;


namespace Erp.Models.ApplicationUsers
{
    //**ApplicationUser Relationship Explanation**//
    //*ApplicationUser(1) --> (many)SalesOrder(One user can have many sales order)*//
    //*ApplicationUser(1) --> (many)Notification(One user can have many notifications)*//

    public class ApplicationUser : IdentityUser
    {
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // To disable user account and not delete them

        public string? PasswordResetToken { get; set; }  // For password reset link tracking
        public DateTime? PasswordResetExpiry { get; set; }
        public bool IsPasswordResetUsed { get; set; } = false;

        //Relationships(Navigational Properties)
        public ICollection<SalesOrder> SalesOrders { get; set; }
        public ICollection<Notification> Notifications { get; set; }
     
    }
}