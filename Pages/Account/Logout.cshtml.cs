using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuditService auditService;
        public LogoutModel(SignInManager<ApplicationUser> signInManager, AuditService auditService, UserManager<ApplicationUser> userManager)
        {
            this.signInManager = signInManager;
            this.auditService = auditService;
            this.userManager = userManager;
        }
        public void OnGet() { }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var user = await userManager.GetUserAsync(User);
            await signInManager.SignOutAsync();
            auditService.Log(new AuditLog
            {
                Action = AuditService.Event.Logout,
                Description = "This user logged out at /Account/Logout",
                Role = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
                ApplicationUserId = user.Id,
                ApplicationUser = user
            });
            return RedirectToPage("/Account/Login");
        }
        public async Task<IActionResult> OnPostDontLogoutAsync()
        {
            return RedirectToPage("/Index");
        }
    }
}
