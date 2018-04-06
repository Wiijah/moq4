using System.Collections.Generic;
using Moq.Performance.Visualisation;
using Xunit;

namespace Moq.Tests.Performance
{
	public class EventRepositoryTest
	{
		[Fact]
		public void EventReposityStoresEvents()
		{
			var repository = new EventRepository();
			var event1 = new Mock<IPerformanceEvent>().Object;
			var event2 = new Mock<IPerformanceEvent>().Object;
			
			repository.AddEvent(event1);
			repository.AddEvent(event2);

			Assert.Equal(repository.GetEvents(), new List<IPerformanceEvent> {event1, event2});
		}
	}
}