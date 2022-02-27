using System.Net;

namespace Refactored.Email.Configuration
{
    public class SmtpSection
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string From { get; set; }
        //public string Username { get; set; }
        //public string Password { get; set; }

        public NetworkCredential Credentials { get; set; }
    }
}
