using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Spoonful.Services;
using System.Text.Encodings.Web;
using System.Text;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class VerifyModel : PageModel
    {
        private readonly IEmailService emailService;
        private readonly ISmsSender smsSender;
        private readonly UserManager<ApplicationUser> userManager;

        public VerifyModel(IEmailService emailService, ISmsSender smsSender, UserManager<ApplicationUser> userManager)
        {
            this.emailService = emailService;
            this.smsSender = smsSender;
            this.userManager = userManager;
        }
        [BindProperty]
        public string OTP { get; set; }
        [BindProperty]
        public string Destination { get; set; }

        public async Task OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            Destination = user.PhoneNumber;
        }
        public async Task<IActionResult> OnPost()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var result = await userManager.ChangePhoneNumberAsync(user, Destination, OTP);
                if (result.Succeeded)
                {

                    TempData["FlashMessage.Text"] = "Successfully Verified Phone Number";
                    TempData["FlashMessage.Type"] = "success";
                    return RedirectToPage("/Home");
                }
            }
            TempData["FlashMessage.Text"] = "Something went wrong";
            TempData["FlashMessage.Type"] = "danger";
            return RedirectToPage();
        }
       
    }
}
