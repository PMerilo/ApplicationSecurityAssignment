using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spoonful.Services;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class VerifyModel : PageModel
    {
        private readonly IEmailService emailService;
        public string Provider { get; set; }

        [BindProperty]
        public string OTP { get; set; }
        [BindProperty]
        public string Destination { get; set; }
        public void OnGetEmail()
        {
            Provider = "Email";
        }
        public void OnPostEmail()
        {
            Provider = "Email";
            

        }
        public void OnGetPhone()
        {
            Provider = "Phone";

        }
        public void OnPostPhone()
        {
            Provider = "Phone";

        }
        public void SendCode()
        {

        }
    }
}
