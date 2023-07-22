namespace ChatWebServer.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
