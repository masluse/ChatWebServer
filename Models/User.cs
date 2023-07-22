using System.ComponentModel.DataAnnotations;

namespace ChatWebServer.Models
{
    public class User
    {
        [Column("userID")]
        public int UserID { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("password")]
        public string Password { get; set; }
    }
}
