using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhotoHistory.Models;
using System.Diagnostics;
using NHibernate;

namespace PhotoHistory.Data
{
	public class UserRepository : DataRepository<UserModel, Int32?>
	{
		public override void Create(UserModel obj)
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

		public override UserModel GetById(int? id)
		{
			using ( var session = GetSession() )
			{
				return session.CreateQuery( "from UserModel where Id = :id" ).SetParameter( "id", id ).UniqueResult<UserModel>();
			}
		}

		public override void Update(UserModel obj)
		{
			using ( var session = GetSession() )
			{
				using ( var transaction = session.BeginTransaction() )
				{
					IQuery query = session.CreateQuery( @"update UserModel set 
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

		public override void Delete(UserModel obj)
		{
			using ( var session = GetSession() )
			{
				using ( var transaction = session.BeginTransaction() )
				{
					session.CreateQuery( "delete from UserModel where Id = :id" ).SetParameter( "id", obj.Id ).ExecuteUpdate();
					transaction.Commit();
				}
			}
		}

		public UserModel GetByUsername(string username)
		{
			using ( var session = GetSession() )
			{
				return session.CreateQuery( "from UserModel where Login = :login" ).SetParameter( "login", username ).UniqueResult<UserModel>();
			}
		}

		public UserModel GetByEmail(string email)
		{
			using ( var session = GetSession() )
			{
				return session.CreateQuery( "from UserModel where Email = :email" ).SetParameter( "email", email ).UniqueResult<UserModel>();
			}
		}
	}
}
