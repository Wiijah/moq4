using System.Collections.Generic;

namespace Moq.Performance.Visualisation
{
	/// <inheritdoc />
	public class EventRepository : IEventRepository
	{
		/// <inheritdoc />
		public void AddEvent(IPerformanceEvent ev)
		{
			events.Add(ev);
		}

		/// <inheritdoc />
		public IEnumerable<IPerformanceEvent> GetEvents()
		{
			return events;
		}

		private readonly IList<IPerformanceEvent> events = new List<IPerformanceEvent>();
	}
}