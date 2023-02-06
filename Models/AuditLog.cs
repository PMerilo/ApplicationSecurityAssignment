using System.ComponentModel.DataAnnotations;

namespace ApplicationSecurityAssignment.Models
{

	public class AuditLog
	{
		public int Id { get; set; }
		
		[Required]
		public string Action { get; set; }
		[Required]
		public string Description { get; set; }
		public string? Role { get; set; }
		public string? ApplicationUserId { get; set; }
		public ApplicationUser? ApplicationUser { get; set; }
		public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.Now;



	}
}
