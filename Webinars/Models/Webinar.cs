using System;
using System.Collections.Generic;

namespace Webinars.Models
{
	public class Webinar
	{
		public string Id { get; set; } 
		public DateTimeOffset ScheduledAt { get; set; }
		public string Presenter { get; set; }
		public List<string> Tags { get; set; }
		public string Description { get; set; }

		public Webinar()
		{
			Tags = new List<string>();
		}
	}

	public class Question
	{
		public string Id { get; set; } 
		public string WebinarId { get; set; }
		public string Text { get; set; }
		public List<string> Tags { get; set; }
		public string UserId { get; set; }

		public HashSet<string> Voters { get; set; }

		public int Votes { get { return Voters.Count; }}

		public Question()
		{
			Tags = new List<string>();
			Voters = new HashSet<string>();
		}
	}
}