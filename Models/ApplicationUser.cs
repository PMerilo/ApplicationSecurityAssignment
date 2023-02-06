using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ApplicationSecurityAssignment.Models
{
    public class ApplicationUser : IdentityUser
    {
		[PersonalData]
		[Required]
		public string FullName { get; set; }

		[PersonalData]
		[Required]
		public string Gender { get; set; }

		[PersonalData]

		[Required]
		[DataType(DataType.CreditCard)]
		public string CreditCard { get; set; }

		[PersonalData]
		[Required]
		public string DeliveryAddress { get; set; }

		[PersonalData]
		public string? AboutMe { get; set; }

		[PersonalData]
		public string? PhotoURL { get; set; }

		public DateTimeOffset LastPasswordChanged { get; set; } = DateTimeOffset.UtcNow;

		public ICollection<PreviousPassword> PreviousPassword { get; set;}

	}
}
