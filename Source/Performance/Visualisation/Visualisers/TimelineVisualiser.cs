using System;
using System.Linq;
using System.Text;

namespace Moq.Performance.Visualisation.Visualisers
{
	/// <summary>
	/// 
	/// </summary>
	public class TimelineVisualiser : IPerformanceVisualiser
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventRepository"></param>
		public TimelineVisualiser(IEventRepository eventRepository)
		{
			this.eventRepository = eventRepository;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Visualise()
		{
			var events = 
				from ev in eventRepository.GetEvents()
				orderby ev.TimeStamp
				select ev;

			if (events.Count() < 2) return "Something went wrong with the visualisation";
			
			var min = events.First().TimeStamp;
			var max = events.Last().TimeStamp;
			var span = max - min;

			StringBuilder sb = new StringBuilder("0");
			
			var index = 1;
			var prev = events.First();
			
			var scale = 100;
			while (events.Count() > scale) scale *= 10;
			
			for (int i = 1; i < events.Count(); i++)
			{
				var ev = events.ElementAt(i);
				var space = scale * (ev.TimeStamp - prev.TimeStamp).TotalMilliseconds / span.TotalMilliseconds;

				int j;
				double subLength = 0;

				if (prev is SetupCalledEvent setupEvent)
				{
					subLength = scale * setupEvent.Duration.TotalMilliseconds / span.TotalMilliseconds;
				}
				
				for (j = 0; j < subLength; j++)
				{
					var ch = (prev is SetupCalledEvent) ? $"{index-1}"[0] : '-';
					sb.Append(ch);
				}

				for (; j < space; j++)
				{
					sb.Append("-");
				}
				
				prev = ev;
				sb.Append($"{index++}");
			}

			sb.AppendLine();
			sb.AppendLine();
			
			index = 0;
			foreach (var ev in events)
			{
				sb.AppendLine($"{index++} : {ev} at {ev.TimeStamp.TimeOfDay}");
			}

			return sb.ToString();
		}

		private readonly IEventRepository eventRepository;
	}
}