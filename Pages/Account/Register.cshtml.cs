using ApplicationSecurityAssignment.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ApplicationSecurityAssignment.Pages.Account
{
    public class RegisterModel : PageModel
    {
		private UserManager<ApplicationUser> userManager { get; }
		private SignInManager<ApplicationUser> signInManager { get; }
		private IWebHostEnvironment _environment;
		private IDataProtectionProvider _dataProtectionProvider;

		[BindProperty]
		public Register RModel { get; set; }
		public RegisterModel(UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager, IWebHostEnvironment environment, IDataProtectionProvider dataProtectionProvider)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this._environment = environment;
			this._dataProtectionProvider = dataProtectionProvider;

		}
		public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
			if (ModelState.IsValid)
			{
				var protector = _dataProtectionProvider.CreateProtector("MySecretKey");
				var user = new ApplicationUser()
				{
					FullName = RModel.FullName,
					Email = RModel.Email,
					UserName = RModel.Email,
					Gender = RModel.Gender,
					PhoneNumber = RModel.MobileNumber,
					DeliveryAddress = RModel.DeliveryAddress,
					CreditCard = protector.Protect(RModel.CreditCard),
					AboutMe = RModel.AboutMe,
					PhotoURL = "",
				};
				if (RModel.Photo != null)
				{
					if (RModel.Photo.Length > 2 * 1024 * 1024)
					{
						ModelState.AddModelError("Upload",
						"File size cannot exceed 2MB.");
						return Page();
					}
					var uploadsFolder = "uploads";
					var imageFile = Guid.NewGuid() + Path.GetExtension(
					RModel.Photo.FileName);
					var imagePath = Path.Combine(_environment.ContentRootPath,
					"wwwroot", uploadsFolder, imageFile);
					using var fileStream = new FileStream(imagePath,
					FileMode.Create);
					await RModel.Photo.CopyToAsync(fileStream);
					user.PhotoURL = string.Format("/{0}/{1}", uploadsFolder,
					imageFile);
				}
				var result = await userManager.CreateAsync(user, RModel.Password);
				if (result.Succeeded)
				{
					await signInManager.SignInAsync(user, false);
					return RedirectToPage("Index");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return Page();

		}

		public class Register
        {
			[Required]
			public string FullName { get; set; }

			[Required]
			[DataType(DataType.EmailAddress)]
			public string Email { get; set; }

			[Required]
			public string Gender { get; set; }

			[Required]
			[DataType(DataType.PhoneNumber)]
			public string MobileNumber { get; set; }

			[Required]
			public string DeliveryAddress { get; set; }

			[Required]
			[DataType(DataType.CreditCard)]
			public string CreditCard { get; set; }

			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			[Required]
			[DataType(DataType.Password)]
			[Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
			public string ConfirmPassword { get; set; }

			public string? AboutMe { get; set; }

			public IFormFile? Photo { get; set; }
		}

	}
}
