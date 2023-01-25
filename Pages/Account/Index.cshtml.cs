using ApplicationSecurityAssignment.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class IndexModel : PageModel
    {
		private IDataProtectionProvider _dataProtectionProvider;
		private UserManager<ApplicationUser> _userManager { get; }

		public IndexModel(UserManager<ApplicationUser> userManager, IDataProtectionProvider dataProtectionProvider)
        {
			_userManager = userManager;
			_dataProtectionProvider = dataProtectionProvider;
		}

		public async Task OnGet()
        {
			var protector = _dataProtectionProvider.CreateProtector("MySecretKey");
			var user = await _userManager.GetUserAsync(User);
			Console.WriteLine(protector.Unprotect(user.CreditCard));
        }
    }
}
