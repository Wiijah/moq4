using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moq.Performance.Visualisation.Visualisers
{
	/// <summary>
	/// 
	/// </summary>
	public class TimeRankingVisualiser : IPerformanceVisualiser
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="repository"></param>
		public TimeRankingVisualiser(IEventRepository repository)
		{
			this.repository = repository;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Visualise()
		{
			var orderedSetups = (from ev in this.repository.GetEvents()
				where ev is SetupCalledEvent
				group ((SetupCalledEvent) ev).Duration.TotalMilliseconds by ((SetupCalledEvent) ev).Setup
				into g
				select new {Setup = g.Key, Duration = g.Sum()})
				.OrderByDescending(setup => setup.Duration);
			
			StringBuilder sb = new StringBuilder("Setup rankings from most time taken to least time taken\n");
			foreach (var setup in orderedSetups)
			{
				sb.AppendLine($"{setup.Setup} took {setup.Duration} ms in total");
			}
			
			return sb.ToString();
		}
		
		private readonly IEventRepository repository;
	}
}