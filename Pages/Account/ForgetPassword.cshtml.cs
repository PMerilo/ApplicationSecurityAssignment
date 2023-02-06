using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Spoonful.Services;
using Microsoft.AspNetCore.Authorization;
using ApplicationSecurityAssignment.Models;
using System.ComponentModel.DataAnnotations;
using AspNetCore.ReCaptcha;
using ApplicationSecurityAssignment.Services;

namespace Spoonful.Pages.Account
{
    [AllowAnonymous]
    [ValidateReCaptcha]
    public class ForgetPasswordModel : PageModel
    {
        public readonly IEmailService _emailSender;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly AuditService _auditService;

        public ForgetPasswordModel(IEmailService emailSender, UserManager<ApplicationUser> userManager, AuditService auditService)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _auditService = auditService;
        }

        [BindProperty]
        [Required]
        public string Email { get; set; }
        public void OnGet()
        {
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userManager.FindByEmailAsync(Email);

            TempData["FlashMessage.Text"] = $"A reset password email will be sent to {Email} if it is valid";
            TempData["FlashMessage.Type"] = "info";

            if (user == null)
            {
                return Page();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
            "/Account/ResetPassword",
                pageHandler: null,
                values: new { code = code, username = user.UserName },
                protocol: Request.Scheme);

            var result = _emailSender.SendEmail(
                Email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                null,
                null);
            
            if (!result)
            {
                TempData["FlashMessage.Text"] = $"Failed to send email.";
                TempData["FlashMessage.Type"] = "danger";
            } else
            {
                _auditService.Log(new AuditLog
                {
                    Action = AuditService.Event.ResetPassword,
                    Description = "This user sent a reset password request at /Account/ForgetPassword",
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user
                });
            }
            return Page();
        }
    }
}
