using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhotoHistory.Model;
using System.Diagnostics;
using NHibernate;

namespace PhotoHistory.Data
{
	public class UserRepository : DataRepository<User, Int32?>
	{
		public override void Create(User obj)
		{
			using ( var session = GetSession() )
			{
				using ( var transaction = session.BeginTransaction() )
				{
					session.Save( obj );
					transaction.Commit();
				}
			}
		}

		public override User GetById(int? id)
		{
			using ( var session = GetSession() )
			{
				return session.CreateQuery( "from User where Id = :id" ).SetParameter( "id", id ).UniqueResult<User>();
			}
		}

		public override void Update(User obj)
		{
			using ( var session = GetSession() )
			{
				using ( var transaction = session.BeginTransaction() )
				{
					IQuery query = session.CreateQuery( @"update User set 
													Login = :login, 
													Password = :pass,
													Email = :email,
													Active = :active,
													DateOfBirth = :birth,
													About = :about,
													NotifyComment = :comment,
													NotifyPhoto = :photo,
													NotifySubscription = :subscr
													where Id = :id" ).
													SetParameter( "id", obj.Id ).
													SetParameter( "login", obj.Login ).
													SetParameter( "pass", obj.Password ).
													SetParameter( "email", obj.Email ).
													SetParameter( "active", obj.Active ).
													SetParameter( "birth", obj.DateOfBirth ).
													SetParameter( "about", obj.About ).
													SetParameter( "comment", obj.NotifyComment ).
													SetParameter( "photo", obj.NotifyPhoto ).
													SetParameter( "subscr", obj.NotifySubscription );
					query.ExecuteUpdate();
					transaction.Commit();
				}
			}
		}

		public override void Delete(User obj)
		{
			using ( var session = GetSession() )
			{
				using ( var transaction = session.BeginTransaction() )
				{
					session.CreateQuery( "delete from User where Id = :id" ).SetParameter( "id", obj.Id ).ExecuteUpdate();
					transaction.Commit();
				}
			}
		}

		public User GetByUsername(string username)
		{
			using ( var session = GetSession() )
			{
				return session.CreateQuery( "from User where Login = :login" ).SetParameter( "login", username ).UniqueResult<User>();
			}
		}
	}
}
