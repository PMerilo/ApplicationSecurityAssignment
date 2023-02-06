using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Spoonful.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
		private readonly ApplicationUserService applicationUserService;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly AuditService auditService;

		public ChangePasswordModel (ApplicationUserService applicationUserService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AuditService auditService)
		{
			this.applicationUserService = applicationUserService;
			this.userManager = userManager;
			this.auditService = auditService;
			this.signInManager = signInManager;
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

			applicationUserService.UpdatePreviousPassword(user.UserName);
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
