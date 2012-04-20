using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace PhotoHistory.Data
{
	public abstract class DataRepository<DataType, KeyType> where DataType : class
	{
		protected static ISession GetSession()
		{
			return SessionProvider.SessionFactory.OpenSession();
		}

		public abstract void Create(DataType obj);
		public abstract DataType GetById(KeyType id);
		public abstract void Update(DataType obj);
		public abstract void Delete(DataType obj);
	}
}
