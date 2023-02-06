using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ApplicationSecurityAssignment.Models;

namespace Spoonful.Services
{
    public static class Roles
    {
        public const string Member = "Member";
        public const string Admin = "Admin";

    }

    public class ApplicationUserService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuthDbContext _db;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        

        public ApplicationUserService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext db, RoleManager<IdentityRole> roleManager, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _passwordHasher = passwordHasher;
        }

        public List<ApplicationUser> GetAll()
        {
            return _db.Users.ToList();
        }

        public async Task SetUserRoleAsync(string UserName, string Role)
        {
            var user = _db.Users.FirstOrDefault(x => x.UserName == UserName);
            if (user == null)
            {
                return;
            }
            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }
            if (!await _roleManager.RoleExistsAsync(Roles.Member))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Member));
            }
            await _userManager.AddToRoleAsync(user, Role);
			_db.SaveChanges();
        }

        public void UpdateLastLogin(string UserName)
        {
            
            _db.SaveChanges();
        }

		public bool ValidatePreviousPassword(string UserName, string NewPassword)
		{
			var user = _db.Users.Include(u => u.PreviousPassword).FirstOrDefault(x => x.UserName == UserName);
			if (user == null)
			{
				return false;
			}
			var passwordQ = user.PreviousPassword.OrderBy(x => x.DateCreated).ToList();
            if (passwordQ.Count == 2)
            {
				passwordQ.RemoveAt(0);
            }
            passwordQ.Add(new PreviousPassword
            {
                Password = user.PasswordHash,
            });
            foreach (var item in passwordQ)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, item.Password, NewPassword);
                if (result == PasswordVerificationResult.Success)
                {
                    return false;
                };
			}
            return true;
		}

		public void UpdatePreviousPassword(string UserName)
		{
			var user = _db.Users.Include(u => u.PreviousPassword).FirstOrDefault(x => x.UserName == UserName);
			if (user == null)
			{
				return;
			}
			var passwordQ = user.PreviousPassword.OrderBy(x => x.DateCreated).ToList();
			if (passwordQ.Count == 2)
			{
				passwordQ.RemoveAt(0);
			}
			passwordQ.Add(new PreviousPassword
			{
				Password = user.PasswordHash,
			});
            user.PreviousPassword = passwordQ.ToList();
            _db.SaveChanges();

		}
	}

    
}
