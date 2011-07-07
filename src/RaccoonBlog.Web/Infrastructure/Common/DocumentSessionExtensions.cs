using System;
using System.Linq;
using System.Web;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;
using Raven.Client;
using Raven.Client.Linq;

namespace RaccoonBlog.Web.Infrastructure.Common
{
	public static class DocumentSessionExtensions
	{
		public static Lazy<PostReference> GetNextPrevPost(this IDocumentSession session, Post compareTo, bool isNext)
		{
			var queryable = session.Query<Post>();
			if (isNext)
			{
				queryable = queryable
					.Where(post => post.PublishAt > compareTo.PublishAt)
					.OrderBy(post => post.PublishAt);
			}
			else
			{
				queryable = queryable
					.Where(post => post.PublishAt < compareTo.PublishAt)
					.OrderByDescending(post => post.PublishAt);
			}

			var postReference = queryable
			  .Where(x => x.IsDeleted == false)
			  .Select(p => new PostReference { Id = p.Id, Title = p.Title })
			  .Lazily();

			return new Lazy<PostReference>(() => postReference.Value.FirstOrDefault());
		}

		public static User GetCurrentUser(this IDocumentSession session)
		{
			if (HttpContext.Current.Request.IsAuthenticated == false)
				return null;

			var email = HttpContext.Current.User.Identity.Name;
			var user = session.GetUserByEmail(email);
			return user;
		}

		public static User GetUserByEmail(this IDocumentSession session, string email)
		{
			return session.Query<User>()
				.Where(u => u.Email == email)
				.FirstOrDefault();
		}


		public static Commenter GetCommenter(this IDocumentSession session, string commenterKey)
		{
			Guid guid;
			if (Guid.TryParse(commenterKey, out guid) == false)
				return null;
			return GetCommenter(session, guid);
		}

		public static Commenter GetCommenter(this IDocumentSession session, Guid commenterKey)
		{
			return session.Query<Commenter>()
						.Where(x => x.Key == commenterKey)
						.FirstOrDefault();
		}
	}
}