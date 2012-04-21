using System.ComponentModel.DataAnnotations;

namespace PhotoHistory.Model
{
	public class UniqueAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			return true;
		}
	}
}

