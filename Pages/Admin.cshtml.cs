using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApplicationSecurityAssignment.Pages
{
    public class AdminModel : PageModel
    {
        private readonly AuditService auditService;
        private readonly UserManager<ApplicationUser> userManager;

        public AdminModel(AuditService auditService, UserManager<ApplicationUser> userManager)
        {
            this.auditService = auditService;
            this.userManager = userManager;
        }

        public List<AuditLog> AuditList { get; set; }
        public void OnGet()
        {
            AuditList = auditService.GetAll();

		}
    }
}
