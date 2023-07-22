using System.ComponentModel.DataAnnotations;

namespace ChatWebServer.Models
{
    public class Message
    {
        [Column("messageID")]
        public int MessageID { get; set; }
        [Column("value")]
        public string Value { get; set; }
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
        [Column("FK_userID")]
        public int FK_userID { get; set; }
    }
}
