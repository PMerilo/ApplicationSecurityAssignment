using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Spoonful.Services;
using System.Text.Encodings.Web;
using System.Text;

namespace ApplicationSecurityAssignment.Pages
{
	public class HomeModel : PageModel
	{
        private readonly IEmailService emailService;
        private readonly ISmsSender smsSender;
        private UserManager<ApplicationUser> userManager { get; }
		private SignInManager<ApplicationUser> signInManager { get; }

		public HomeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService, ISmsSender smsSender)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
            this.emailService = emailService;
            this.smsSender = smsSender;
		}
        [BindProperty]
		public ApplicationUser RModel { get; set; }
		public async Task<IActionResult> OnGet()
		{
			var user = await userManager.GetUserAsync(User);
			RModel = user;
			//if (!user.EmailConfirmed)
			//{
			//	TempData["FlashMessage.Link"] = "You email has not been verified! Click here to verify it.";
			//	TempData["FlashMessage.Url"] = "/Account/Verify";
			//	TempData["FlashMessage.Type"] = "warning";
			//}
            return Page();
		}

        public async Task<IActionResult> OnPostSendCode(string provider)
        {
            var user = await userManager.GetUserAsync(User);
            if (provider == "Email")
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                "/Account/Verify",
                pageHandler: "Email",
                    values: new { code = code, username = user.UserName },
                    protocol: Request.Scheme);

                var result = emailService.SendEmail(
                    user.Email,
                    "Reset Password",
                    $"Please verify your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                    null,
                    null);
                return RedirectToPage("/Home");
            }
            else
            {
                var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                var result = smsSender.SendSmsAsync(
                    user.PhoneNumber,
                    $"Security Code: {code}");
                return RedirectToPage("/Account/Verify");
            }
        }

		public async Task<IActionResult> OnPostEnable2FA()
        {
			var user = await userManager.GetUserAsync(User);
			await userManager.SetTwoFactorEnabledAsync(user, true);
            return RedirectToPage();
		}

		public async Task<IActionResult> OnPostDisable2FA()
		{
			var user = await userManager.GetUserAsync(User);
			await userManager.SetTwoFactorEnabledAsync(user, false);
			return RedirectToPage();
		}

	}
}
