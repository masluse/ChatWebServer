using System.ComponentModel.DataAnnotations.Schema;

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

        [Column("role")]
        public string Role { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; }
    }
}
