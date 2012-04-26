﻿using System.ComponentModel.DataAnnotations;
using PhotoHistory.Data;
using System;
using System.Globalization;

namespace PhotoHistory
{
	public class UniqueUsernameAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			UserRepository userRepository = new UserRepository();
			return userRepository.GetByUsername( value as string ) == null;
		}
	}

	public class UniqueEmailAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			UserRepository userRepository = new UserRepository();
			return userRepository.GetByEmail( value as string ) == null;
		}
	}

	public class ValidateEmailAttribute : RegularExpressionAttribute
	{
		public ValidateEmailAttribute() :
			base( @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$" ) { }
	}

	public class DateYearInRangeAttribute : DataTypeAttribute
	{
		private int _min;
		private int _max;

		public DateYearInRangeAttribute(int min, int max) : base( DataType.Date ) 
		{
			_min = min;
			_max = max;
		}

		public override bool IsValid(object value)
		{
			if ( value != null )
			{
				DateTime date = (DateTime)value;
				return (date != null && date.Year >= _min && date.Year <= _max);
			}

			// jesli data nie zostala podana to uznajemy ja za poprawna - jest opcjonalna
			return true;
		}
	}

}

