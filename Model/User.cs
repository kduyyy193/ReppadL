using System;
namespace ReppadL.Model
{
    public class User
    {
        public int id { get; set; }
        public required string username { get; set; }
        public string? hashed_password { get; set; }
        public string? salt { get; set; }
        public string email { get; set; } = "";
        public string phone { get; set; } = "";
        public DateTime created_at { get; set; }

        public required string password { get; set; }
    }
}
