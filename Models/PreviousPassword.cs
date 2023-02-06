using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace ApplicationSecurityAssignment.Models
{
	public class PreviousPassword
	{
		public int Id { get; set; }

		[Required]
		public string Password { get; set; }

		[Required]
		public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

		[Required]
		public string ApplicationUserId { get; set; }

		[Required]
		public ApplicationUser ApplicationUser { get; set; }
	}
}
