using System.ComponentModel.DataAnnotations.Schema;

namespace ChatWebServer.Models
{
    public class Message
    {
        [Column("messageID")]
        public int MessageID { get; set; }
        [Column("value")]
        public string Value { get; set; }
        [Column("timeStamp")]
        public DateTimeOffset Timestamp { get; set; }
        [Column("FK_userID")]
        public int FK_userID { get; set; }

        public virtual User User { get; set; }
    }
}
