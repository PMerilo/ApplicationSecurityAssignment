using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Spoonful.Services;
using System.Text.Encodings.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using System.Web;

namespace ApplicationSecurityAssignment.Pages
{
	public class HomeModel : PageModel
	{
        private readonly IEmailService emailService;
        private readonly ISmsSender smsSender;
        private UserManager<ApplicationUser> userManager { get; }
		private SignInManager<ApplicationUser> signInManager { get; }
		private IDataProtectionProvider _dataProtectionProvider;

		public HomeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService, ISmsSender smsSender, IDataProtectionProvider dataProtectionProvider)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
            this.emailService = emailService;
            this.smsSender = smsSender;
            _dataProtectionProvider = dataProtectionProvider;
		}
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public string Password { get; set; }
        public int MinPasswordAge { get; set; }

        public string DeliveryAddress { get; set; }

        public string CreditCard { get; set; }

        public string? AboutMe { get; set; }
        public string? PhotoURL { get; set; }
        public bool Enabled2FA { get; set; } 
        public async Task<IActionResult> OnGet()
		{
			var user = await userManager.GetUserAsync(User);
			var protector = _dataProtectionProvider.CreateProtector("MySecretKey");
			FullName = user.FullName;
			Email = user.Email;
			Gender = user.Gender;
			PhoneNumber = user.PhoneNumber;
			DeliveryAddress = HttpUtility.HtmlDecode(user.DeliveryAddress);
			CreditCard = protector.Unprotect(user.CreditCard);
			AboutMe = HttpUtility.HtmlDecode(user.AboutMe);
            PhotoURL = user.PhotoURL;
            Enabled2FA = user.TwoFactorEnabled;
            PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            Password = user.PasswordHash;
            MinPasswordAge = user.LastPasswordChanged.AddDays(1).CompareTo(DateTimeOffset.UtcNow);
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
			TempData["FlashMessage.Text"] = "2FA Enabled";
			TempData["FlashMessage.Type"] = "success";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostDisable2FA()
		{
			var user = await userManager.GetUserAsync(User);
			await userManager.SetTwoFactorEnabledAsync(user, false);
			TempData["FlashMessage.Text"] = "2FA Disabled";
			TempData["FlashMessage.Type"] = "warning";
			return RedirectToPage();
		}

	}
}
