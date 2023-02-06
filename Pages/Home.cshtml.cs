using ApplicationSecurityAssignment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApplicationSecurityAssignment.Pages
{
	public class HomeModel : PageModel
	{
		private UserManager<ApplicationUser> userManager { get; }
		private SignInManager<ApplicationUser> signInManager { get; }

		public HomeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
		}
		public async Task OnGet()
		{
			var user = await userManager.GetUserAsync(User);
			if (!user.EmailConfirmed)
			{
				TempData["FlashMessage.Link"] = "You email has not been verified! Click here to verify it.";
				TempData["FlashMessage.Url"] = "/Account/Verify";
				TempData["FlashMessage.Type"] = "warning";
			}

		}
	}
}
