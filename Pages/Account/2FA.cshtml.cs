using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spoonful.Services;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class _2FAModel : PageModel
    {

        private readonly ISmsSender smsSender;
        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }

        public _2FAModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ISmsSender smsSender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.smsSender = smsSender;
        }
        [BindProperty]
        public string OTP { get; set; } 
        public async Task<IActionResult> OnGet()
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("/Index");
            }
            var providers = await userManager.GetValidTwoFactorProvidersAsync(user);
            var code = await userManager.GenerateTwoFactorTokenAsync(user, providers.FirstOrDefault());
            await smsSender.SendSmsAsync(user.PhoneNumber, $"Security Code: {code}");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            var providers = await userManager.GetValidTwoFactorProvidersAsync(user);
            var result = await signInManager.TwoFactorSignInAsync(providers.FirstOrDefault(), OTP, false, false);
            if (result.Succeeded)
            {
                TempData["FlashMessage.Text"] = "Successfully Logged In";
                TempData["FlashMessage.Type"] = "success";
                return RedirectToPage("/Home");
            }
            TempData["FlashMessage.Text"] = "Invalid OTP";
            TempData["FlashMessage.Type"] = "danger";
            return Page();
        }
    }
}
