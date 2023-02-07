using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spoonful.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web;

namespace ApplicationSecurityAssignment.Pages.Account
{
	[ValidateReCaptcha]
	public class RegisterModel : PageModel
    {
		private UserManager<ApplicationUser> userManager { get; }
		private SignInManager<ApplicationUser> signInManager { get; }
		private IWebHostEnvironment _environment;
		private IDataProtectionProvider _dataProtectionProvider;
        private readonly AuditService _auditService;
        private readonly ApplicationUserService _applicationUserService;

        [BindProperty]
		public Register RModel { get; set; }
		public RegisterModel(UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager, IWebHostEnvironment environment, IDataProtectionProvider dataProtectionProvider, AuditService auditService, ApplicationUserService  applicationUserService)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this._environment = environment;
			this._dataProtectionProvider = dataProtectionProvider;
			this._auditService = auditService;
			this._applicationUserService = applicationUserService;

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
					PhoneNumber = "+65" + RModel.MobileNumber,
					DeliveryAddress = HttpUtility.HtmlEncode(RModel.DeliveryAddress),
					CreditCard = protector.Protect(RModel.CreditCard),
					AboutMe = HttpUtility.HtmlEncode(RModel.AboutMe)
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
                    await _applicationUserService.SetUserRoleAsync(user.UserName, Roles.Member);
                    _auditService.Log(new AuditLog
                    {
                        Action = AuditService.Event.Register,
                        Description = "This user was created at /Account/Register",
                        Role = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
                        ApplicationUserId = user.Id,
                        ApplicationUser = user
                    });
                    await signInManager.SignInAsync(user, false);
					return RedirectToPage("/Index");
				}
				foreach (var error in result.Errors)
				{
					if (error.Code == "DuplicateUserName")
					{
						continue;
					}
					ModelState.AddModelError("", error.Description);
				}
			}
			return Page();

		}

		public class Register
        {
			[Required]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Invalid Name")]
            public string FullName { get; set; }

			[Required]
			[DataType(DataType.EmailAddress)]
			public string Email { get; set; }

			[Required]
			public string Gender { get; set; }

			[Required]
			[DataType(DataType.PhoneNumber)]
			[RegularExpression(@"^([0-9]{8,})$", ErrorMessage = "Invalid Mobile Number")]
			public string MobileNumber { get; set; }

			[Required]
			public string DeliveryAddress { get; set; }

			[Required]
			[DataType(DataType.CreditCard)]
			[RegularExpression(@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11})$", ErrorMessage = "Invalid Credit Card Number")]
			public string CreditCard { get; set; }

			[Required]
			[DataType(DataType.Password)]
			[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{12,}$", ErrorMessage = "Invalid Password.")]
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
