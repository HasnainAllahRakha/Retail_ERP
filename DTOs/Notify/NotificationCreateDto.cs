namespace Erp.DTOs.Notify
{
    public class NotificationCreateDto
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string UserId { get; set; } = null!; // Recipient user
        public string Type { get; set; } = "System"; // e.g., System, Alert, Reminder
    }
}
