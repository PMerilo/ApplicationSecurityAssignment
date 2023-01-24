using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public Register RModel { get; set; }
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            return Page();
        }

        public class Register
        {
            public string FullName { get; set; }
        }
    }
}
