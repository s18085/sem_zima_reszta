using System;
namespace APBD_zima.Dtos
{
    public class TokenRefreshRequestDto
    {
        public string token { get; set; }
        public string refreshToken { get; set; }
    }
}
