namespace ChatWebServer.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }

        public int FK_userID { get; set; }
        public User User { get; set; }
    }
}
