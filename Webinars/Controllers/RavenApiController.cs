using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Webinars.Indexes;
using Webinars.Models;

namespace Webinars.Controllers
{
	public abstract class RavenApiController : ApiController
	{
		public static Lazy<IDocumentStore> DocumentStore = new Lazy<IDocumentStore>(() =>
			{
				var docStore = new DocumentStore
					{
						Url = "http://localhost:8080",
						DefaultDatabase = "Webinars",
					};

				var hiloGenerator = new HiLoKeyGenerator("questions", 32);
				docStore.Conventions.RegisterIdConvention<Question>((dbName, cmd, q) =>
					 q.WebinarId + "/questions/" + hiloGenerator.NextId(cmd));

				docStore.Initialize();

				IndexCreation.CreateIndexes(typeof(Users_Stats).Assembly, docStore);

				return docStore;
			});

		private IDocumentSession session;
		public IAsyncDocumentSession asyncSession;

		public override async Task<HttpResponseMessage> ExecuteAsync(
			HttpControllerContext controllerContext,
			CancellationToken cancellationToken)
		{
			var result = await base.ExecuteAsync(controllerContext, cancellationToken);
			if (session != null)
				session.SaveChanges();
			if (asyncSession != null)
				await asyncSession.SaveChangesAsync();
			return result;
		}

		public IDocumentSession Session
		{
			get
			{
				if (asyncSession != null)
					throw new NotSupportedException("Can't use both sync & async sessions in the same action");
				return session ?? (session = DocumentStore.Value.OpenSession());
			}
			set { session = value; }
		}

		public IAsyncDocumentSession AsyncSession
		{
			get
			{
				if (session != null)
					throw new NotSupportedException("Can't use both sync & async sessions in the same action");
				return asyncSession ?? (asyncSession = DocumentStore.Value.OpenAsyncSession());
			}
			set { asyncSession = value; }
		}
	}
}