using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Raven.Abstractions.Commands;
using Raven.Abstractions.Data;
using Webinars.Models;
using System.Linq;
using Raven.Client;

namespace Webinars.Controllers
{
	public class WebinarsController : RavenApiController
	{
		[HttpGet]
		public void Subscripe(string id)
		{
			DocumentStore.Value.Changes()
						 .ForDocument(id)
						 .Subscribe(x =>
							 {
								 //var scream = @"http://www.shockwave-sound.com/sound-effects/scream-sounds/2scream.wav";
								 // new SoundPlayer(scream).Play();
							 });
		}

		[HttpGet]
		public async Task<string> Create(string presenter, [FromUri] string[] tags, string desc)
		{
			var webinar = new Webinar
				{
					Description = desc,
					ScheduledAt = DateTime.Today.AddDays(7),
					Presenter = presenter,
				};
			webinar.Tags.AddRange(tags);
			await AsyncSession.StoreAsync(webinar);

			return webinar.Id;
		}

		[HttpGet]
		public void Vote(string qid, string user)
		{
			Session.Advanced.UseOptimisticConcurrency = true;

			var q = Session.Load<Question>(qid);
			q.Voters.Add(user);
		}

		[HttpGet]
		public void Vote2(string qid, string user)
		{
			Session.Advanced.Defer(new ScriptedPatchCommandData
				{
					Patch = new ScriptedPatchRequest
						{
							Script = @"
if(_.contains(this.Voters, user))
	return;
this.Voters.push(user);
this.Votes++;
",
							Values =
								{
									{"user", user}
								}
						}
				});
		}

		[HttpGet]
		public async Task<PushStreamContent> ExportQuestions(string webinarId)
		{
			var q = AsyncSession.Query<Question>().Where(x => x.WebinarId == webinarId);
			var result = await AsyncSession.Advanced.StreamAsync(q);
			return new PushStreamContent(async (stream, content, ctx) =>
				{
					using (var writer = new StreamWriter(stream))
						while (await result.MoveNextAsync())
						{
							var question = result.Current.Document;
							await writer.WriteAsync(result.Current.Key);
							await writer.WriteAsync(": ");
							await writer.WriteLineAsync(question.Text);
						}
				}, "text/plain");
		}


		[HttpGet]
		public async Task<object> Questions(string webinarId)
		{
			using (DocumentStore.Value.AggressivelyCacheFor(TimeSpan.FromHours(5)))
			{
				return await AsyncSession.LoadAsync<Webinar>(webinarId);
			}
		}

		[HttpGet]
		public string Ask(string webinarId, string user, string q, [FromUri]string[] tags)
		{
			for (int i = 0; i < 100; i++)
			{
				var question = new Question
				{
					WebinarId = webinarId,
					Text = q,
					UserId = user
				};

				question.Tags.AddRange(tags);
				Session.Store(question);
			}
			return "done";
		}

		[HttpGet]
		public Webinar Load(int id)
		{
			return Session.Load<Webinar>(id);
		}
	}
}