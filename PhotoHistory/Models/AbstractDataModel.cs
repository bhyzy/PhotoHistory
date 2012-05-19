namespace PhotoHistory
{
	public abstract class AbstractDataModel<T>
		where T : AbstractDataModel<T>
	{
		public virtual int? Id { get; set; }

		private int? oldHashCode;

		public override bool Equals(object obj)
		{
			T other = obj as T;
			if ( other == null )
				return false;

			bool otherIsTransient = Equals( other.Id, default( int? ) );
			bool thisIsTransient = Equals( this.Id, default( int? ) );

			if ( otherIsTransient && thisIsTransient )
				return ReferenceEquals( other, this );

			return other.Id.Equals( Id );
		}

		public override int GetHashCode()
		{
			if ( oldHashCode.HasValue )
				return oldHashCode.Value;

			bool thisIsTransient = Equals( this.Id, default( int? ) );
			if ( thisIsTransient )
			{
				oldHashCode = base.GetHashCode();
				return oldHashCode.Value;
			}

			return Id.GetHashCode();
		}

		public static bool operator ==(AbstractDataModel<T> x, AbstractDataModel<T> y)
		{
			return Equals( x, y );
		}
		public static bool operator !=(AbstractDataModel<T> x, AbstractDataModel<T> y)
		{
			return !(x == y);
		}
	}
}