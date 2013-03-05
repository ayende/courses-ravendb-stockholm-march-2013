using System.Linq;
using Raven.Client.Indexes;
using Webinars.Models;

namespace Webinars.Indexes
{
	public class Users_Stats : AbstractMultiMapIndexCreationTask<Users_Stats.ReduceResult>
	{
		public class ReduceResult
		{
			public string User { get; set; }
			public int Webinars { get; set; }
			public int Questions { get; set; }
		}

		public Users_Stats()
		{
			AddMap<Webinar>(webinars =>
							from webinar in webinars
							select new ReduceResult
								{
									User = webinar.Presenter,
									Webinars = 1,
									Questions = 0
								}
			);

			AddMap<Question>(questions =>
			                 from question in questions 
							 select new ReduceResult
								 {
									 Webinars = 0,
									 Questions = 1,
									 User = question.UserId
								 }
			);

			Reduce = results =>
			         from result in results
			         group result by result.User
			         into g
			         select new ReduceResult
				         {
					         Webinars = g.Sum(x => x.Webinars),
					         Questions = g.Sum(x => x.Questions),
					         User = g.Key
				         };
		}
	}
}