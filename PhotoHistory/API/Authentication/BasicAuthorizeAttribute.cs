using System.Web.Mvc;
using System.Web;
using System;
using System.Security.Principal;
using System.Text;
using PhotoHistory.API.Common;

namespace PhotoHistory.API.Authentication
{
	public class BasicAuthorizeAttribute : AuthorizeAttribute
	{
		bool _RequireSsl = false;
		public bool RequireSsl
		{
			get { return _RequireSsl; }
			set { _RequireSsl = value; }
		}

		private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
		{
			validationStatus = OnCacheAuthorization( new HttpContextWrapper( context ) );
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if ( filterContext == null ) throw new ArgumentNullException( "filterContext" );

			if ( !Authenticate( filterContext.HttpContext ) )
			{
				filterContext.Result = new HttpBasicUnauthorizedResult();
			}
			else
			{
				// AuthorizeCore is in the base class and does the work of checking if we have
				// specified users or roles when we use our attribute
				if ( AuthorizeCore( filterContext.HttpContext ) )
				{
					HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
					cachePolicy.SetProxyMaxAge( new TimeSpan( 0 ) );
					cachePolicy.AddValidationCallback( CacheValidateHandler, null /* data */);
				}
				else
				{
					// auth failed, display login
					filterContext.Result = new HttpBasicUnauthorizedResult();
				}
			}
		}

		// from here on are private methods to do the grunt work of parsing/verifying the credentials

		private bool Authenticate(HttpContextBase context)
		{
			if ( _RequireSsl && !context.Request.IsSecureConnection && !context.Request.IsLocal )
				return false;

			string authHeader = context.Request.Headers["Authorization"];

			IPrincipal principal;
			if ( TryGetPrincipal( authHeader, out principal ) )
			{
				HttpContext.Current.User = principal;
				return true;
			}

			return false;
		}

		private bool TryGetPrincipal(string authHeader, out IPrincipal principal)
		{
			var creds = ApiHelpers.ParseBasicAuthHeader( authHeader );
			if ( creds != null )
			{
				if ( TryGetPrincipal( creds[0], creds[1], out principal ) )
					return true;
			}

			principal = null;
			return false;
		}

		private bool TryGetPrincipal(string userName, string password, out IPrincipal principal)
		{
			if ( UserAuthentication.TryCredentials(userName, password) == string.Empty )
			{
				// once the user is verified, assign it to an IPrincipal with the identity name and applicable roles
				principal = new GenericPrincipal( new GenericIdentity( userName ), null );
				return true;
			}
			else
			{
				principal = null;
				return false;
			}
		}
	}
}