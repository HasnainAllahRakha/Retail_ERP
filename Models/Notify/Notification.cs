using Erp.Models.ApplicationUsers;

namespace Erp.Models.Notify
{
    //**Notification Relationship Explanation**//
    //*Notification(Many) --> (many)ApplicationUser(Each notification belongs to exactly one user)*//

    public class Notification
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        //Relationships(foreign key)
        public string UserId { get; set; }  // optional (null for broadcast)

        //Relationships(Navigational Properties)
        public ApplicationUser User { get; set; }
    }
}
