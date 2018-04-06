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
				prev = ev;
				
				for (int j = 0; j < space; j++)
				{
					sb.Append("-");
				}

				sb.Append($"{index++}");
			}

			sb.AppendLine();
			sb.AppendLine();
			
			index = 0;
			foreach (var ev in events)
			{
				sb.AppendLine($"{index++} : {ev}");
			}

			return sb.ToString();
		}

		private readonly IEventRepository eventRepository;
	}
}