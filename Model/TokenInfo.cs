using System;
namespace ReppadL.Model
{
    public class TokenInfo
    {
        public int userId { get; set; }
        public required string accessToken { get; set; }
        public required string refreshToken { get; set; }
        public DateTime accessTokenExpiration { get; set; }
        public DateTime refreshTokenExpiration { get; set; }
    }
}

