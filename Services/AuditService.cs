
using ApplicationSecurityAssignment.Models;
using Microsoft.AspNetCore.Identity;

namespace ApplicationSecurityAssignment.Services
{

    public class AuditService
	{
        public static class Event
        {
            public const string ResetPassword = "ResetPassword";
            public const string ChangePassword = "ChangePassword";
            public const string Login = "Login";
            public const string Logout = "Logout";
            public const string Lockout = "Lockout";
            public const string Register = "Register";

        }

        private readonly AuthDbContext _db;

		public AuditService(AuthDbContext db)
		{
			_db = db;
		}

		public List<AuditLog> GetAll()
		{
			return _db.AuditLogs.ToList();
		}

		public List<AuditLog> GetByUser(string UserName)
		{
			return _db.AuditLogs.Where(a => a.ApplicationUserId == UserName).ToList();
		}

		public AuditLog? Get(int id)
		{
			return _db.AuditLogs.SingleOrDefault(a => a.Id == id);
		}

		public void Log(AuditLog auditLog)
		{
			_db.AuditLogs.Add(auditLog);
			_db.SaveChanges();
		}
	}
}
