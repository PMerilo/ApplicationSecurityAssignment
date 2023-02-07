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
    [BindProperties]
    [ValidateReCaptcha]
    public class ChangePasswordModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationUserService applicationUserService;
        private readonly AuditService auditService;
        public ChangePasswordModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationUserService applicationUserService, AuditService auditService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.applicationUserService = applicationUserService;
            this.auditService = auditService;
        }

        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords must match")]
        public string ConfirmPassword { get; set; }
        //public async Task<IActionResult> OnGetAsync()
        //{
        //    var user = await userManager.GetUserAsync(User);
        //    if (user.PasswordHash == null || user.LastPasswordChanged.AddDays(1).CompareTo(DateTimeOffset.UtcNow) > 0)
        //    {
        //        TempData["FlashMessage.Text"] = "You can only change your password after 1 day";
        //        TempData["FlashMessage.Type"] = "danger";
        //        return Redirect("/Home");
        //    }
        //    return Page();
        //}

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["FlashMessage.Text"] = "Passwords do not match";
                TempData["FlashMessage.Type"] = "danger";
                return Page();
            }
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["FlashMessage.Text"] = "Something went wrong";
                TempData["FlashMessage.Type"] = "danger";
                return Redirect("/");
            }

            if (!applicationUserService.ValidatePreviousPassword(user.UserName, Password))
            {
				TempData["FlashMessage.Text"] = "Cannot use your previous 2 passwords";
				TempData["FlashMessage.Type"] = "danger";
				return Page();
			}
            applicationUserService.UpdatePreviousPassword(user.UserName);
            var result = await userManager.ChangePasswordAsync(user, OldPassword, Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
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

            TempData["FlashMessage.Text"] = "Successfully Changed password! Please login with your new password";
            TempData["FlashMessage.Type"] = "success";
			await signInManager.RefreshSignInAsync(user);
			return RedirectToPage("/Account/Login");
            
        }
    }
}
