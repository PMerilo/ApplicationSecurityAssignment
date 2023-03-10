using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationSecurityAssignment.Pages.Account
{
	[ValidateReCaptcha]
	public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

		public string ReturnUrl { get; set; }


		private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;
        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuditService auditService)
        {
            this._signInManager = signInManager;
            this._auditService = auditService;
            _userManager = userManager;
        }
        public void OnGet()
        {
            ReturnUrl = Url.Content("~/Home");

		}

		public async Task<IActionResult> OnPostAsync()
        {
			if (ModelState.IsValid)
            {
                var identityResult = await _signInManager.PasswordSignInAsync(LModel.Email, LModel.Password,
                LModel.RememberMe, true);
                var user = await _userManager.FindByEmailAsync(LModel.Email);
                if (user.PasswordHash == null)
                {
					TempData["FlashMessage.Text"] = "This user does not use a regular password login";
					TempData["FlashMessage.Type"] = "warning";
                    return Page();
				}
                if (identityResult.Succeeded)
                {
                    if (user.LastPasswordChanged.AddMonths(1).CompareTo(DateTimeOffset.UtcNow) < 0)
                    {
                        await _signInManager.SignOutAsync();
                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        TempData["FlashMessage.Text"] = "Your password is too old. Please change your password to continue to login";
                        TempData["FlashMessage.Type"] = "warning";
                        return RedirectToPage("/Account/ResetPassword", new {code = code, username = user.UserName});
                    }
                    _auditService.Log(new AuditLog
                    {
                        Action =  AuditService.Event.Login,
                        Description = "This user has logged in at /Account/Login",
                        Role = null,
                        ApplicationUserId = user.Id,
                        ApplicationUser = user,
                    });
                    await _userManager.UpdateSecurityStampAsync(user);
                    await _signInManager.RefreshSignInAsync(user);

					return RedirectToPage("/Home");
                }
                if (identityResult.RequiresTwoFactor)
                {
					return RedirectToPage("/Account/2FA");
                }
				if (identityResult.IsLockedOut)
				{
					ModelState.AddModelError("", "You have too many failed attempts please try again later");
                    _auditService.Log(new AuditLog
                    {
                        Action = AuditService.Event.Lockout,
                        Description = "This user attempted login on a locked out account /Account/Login",
                        Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                        ApplicationUserId = user.Id,
                        ApplicationUser = user,
                    });
                    return Page();
				}
                if (user != null)
                {
                    _auditService.Log(new AuditLog
                    {
                        Action = AuditService.Event.Login,
                        Description = "This user has failed login attempt at /Account/Login",
                        Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                        ApplicationUserId = user.Id,
                        ApplicationUser = user,
                    });
                }
				ModelState.AddModelError("", "Username or Password incorrect");
            }
            return Page();
        }

        public class Login
        {
            [Required]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }

    }
}
