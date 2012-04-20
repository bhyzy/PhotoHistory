using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoHistory.Model
{
	public class User
	{
		public virtual int? Id { get; set; }
		public virtual string Login { get; set; }
		public virtual string Password { get; set; }
		public virtual string Email { get; set; }
		public virtual bool Active { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
		public virtual string About { get; set; }
		public virtual bool NotifyComment { get; set; }
		public virtual bool NotifyPhoto { get; set; }
		public virtual bool NotifySubscription { get; set; }
	}
}
