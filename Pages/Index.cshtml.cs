using ApplicationSecurityAssignment.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApplicationSecurityAssignment.Pages
{
    public class IndexModel : PageModel
    {

		public async Task OnGet()
		{
			TempData["Test"] = "Test";
		}
	}
}