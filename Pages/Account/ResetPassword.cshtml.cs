using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ApplicationSecurityAssignment.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Spoonful.Services;
using AspNetCore.ReCaptcha;
using ApplicationSecurityAssignment.Services;

namespace Spoonful.Pages.Account
{
    [AllowAnonymous]
    [BindProperties]
    [ValidateReCaptcha]
    public class ResetPasswordModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationUserService applicationUserService;
        private readonly AuditService auditService;
        public ResetPasswordModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationUserService applicationUserService, AuditService auditService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.applicationUserService = applicationUserService;
            this.auditService = auditService;
        }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords must match")]
        public string ConfirmPassword { get; set; }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync(string code, string username)
        {
            if (!ModelState.IsValid)
            {
                TempData["FlashMessage.Text"] = "Passwords do not match";
                TempData["FlashMessage.Type"] = "danger";
                return Page();
            }
            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                TempData["FlashMessage.Text"] = "Invalid Tokens";
                TempData["FlashMessage.Type"] = "danger";
                return Redirect("/");
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            if (!applicationUserService.ValidatePreviousPassword(user.UserName, Password))
            {
				TempData["FlashMessage.Text"] = "Cannot use your previous 2 passwords";
				TempData["FlashMessage.Type"] = "danger";
				return Page();
			}
            applicationUserService.UpdatePreviousPassword(user.UserName);
            var result = await userManager.ResetPasswordAsync(user, token, Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                TempData["FlashMessage.Text"] = "Invalid Tokens";
                TempData["FlashMessage.Type"] = "danger";
                return Page();
            }

            auditService.Log(new AuditLog
            {
                Action = AuditService.Event.ChangePassword,
                Description = "This user reset their password at /Account/ResetPassword",
                Role = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
                ApplicationUserId = user.Id,
                ApplicationUser = user
            });

            TempData["FlashMessage.Text"] = "Successfully reset password!";
            TempData["FlashMessage.Type"] = "success";
            return RedirectToPage("/Account/Login");
            
        }
    }
}
