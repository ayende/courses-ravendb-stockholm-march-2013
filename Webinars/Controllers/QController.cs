using System;
using System.Web.Http;
using Webinars.Indexes;
using Webinars.Models;
using System.Linq;

namespace Webinars.Controllers
{
	public class QController : RavenApiController
	{
		 [HttpGet]
		 public object ByTag(string tag)
		 {
			 return Session.Query<Webinar>()
			               .Where(x => x.Tags.Contains(tag))
			               .ToList();
		 }

		 [HttpGet]
		 public object Stats(string user)
		 {
			 return Session.Query<Users_Stats.ReduceResult, Users_Stats>()
				.FirstOrDefault(x => x.User == user);
		 }

		 [HttpGet]
		 public object Interesting()
		 {
			 return Session.Query<Webinar>("Webinars/ByTag")
			               .Where(x => x.Tags.Contains("philosophy"))
			               .ToList();
		 }
	}
}