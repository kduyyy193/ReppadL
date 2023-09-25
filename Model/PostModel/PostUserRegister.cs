using System;
namespace ReppadL.Model.PostModel
{
	public class PostUserRegister
    {
        public required string username { get; set; }
        public string email { get; set; } = "";
        public string phone { get; set; } = "";
        public required string password { get; set; }
    }
}

