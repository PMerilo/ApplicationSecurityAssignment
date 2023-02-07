using ApplicationSecurityAssignment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using System.Web;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
		private readonly SignInManager<ApplicationUser> signInManager;
		private IDataProtectionProvider _dataProtectionProvider;
		private readonly UserManager<ApplicationUser> userManager;
		[BindProperty]
        public Register RModel { get; set; }
        public string ReturnUrl { get; set; }

        public ExternalLoginModel (SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IDataProtectionProvider dataProtectionProvider)
		{
			this.signInManager = signInManager;
            this.userManager = userManager;
			_dataProtectionProvider = dataProtectionProvider;
		}
		public IActionResult OnPost(string provider, string returnUrl = null)
		{
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
			var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string? remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/Home");
			if (remoteError != null)
			{
				//ErrorMessage = $"Error from external provider: {remoteError}";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}
			var info = await signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				//ErrorMessage = "Error loading external login information.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}

			// Sign in the user with this external login provider if the user already has a login.
			var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (result.Succeeded)
			{
				//_logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
				return LocalRedirect(returnUrl);
			}
			if (result.IsLockedOut)
			{
				return RedirectToPage("./Lockout");
			}
			else
			{
				// If the user does not have an account, then ask the user to create an account.
				ReturnUrl = returnUrl;
				if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
				{
                    RModel = new Register
					{
						Email = info.Principal.FindFirstValue(ClaimTypes.Email),
						FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
					};
				}
				return Page();
			}
		}

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                //ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
				var protector = _dataProtectionProvider.CreateProtector("MySecretKey");
				var user = new ApplicationUser
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
					FullName = RModel.FullName,
					Gender = RModel.Gender,
					PhoneNumber = "+65" + RModel.MobileNumber,
					DeliveryAddress = HttpUtility.HtmlEncode(RModel.DeliveryAddress),
					CreditCard = protector.Protect(RModel.CreditCard),
					AboutMe = HttpUtility.HtmlEncode(RModel.AboutMe)
                };

                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
						await userManager.UpdateSecurityStampAsync(user);
						return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            //ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

		public class Register
		{
			[Required]
			[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Invalid Name")]

			public string FullName { get; set; }

			[Required]
			[DataType(DataType.EmailAddress)]
			public string Email { get; set; }

			[Required]
			public string Gender { get; set; }

			[Required]
			[DataType(DataType.PhoneNumber)]
			[RegularExpression(@"^([0-9]{8,})$", ErrorMessage = "Invalid Mobile Number")]
			public string MobileNumber { get; set; }

			[Required]
			public string DeliveryAddress { get; set; }

			[Required]
			[DataType(DataType.CreditCard)]
			[RegularExpression(@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11})$", ErrorMessage = "Invalid Credit Card Number")]
			public string CreditCard { get; set; }

			public string? AboutMe { get; set; }

			public IFormFile? Photo { get; set; }
		}

	}
}
