using FraudDetectionWeb.Models;

namespace FraudDetectionWeb.DTOs
{
	public class LoginRequest
	{
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
