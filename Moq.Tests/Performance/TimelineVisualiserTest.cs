using System;
using System.Collections.Generic;
using Moq.Language.Flow;
using Moq.Performance.Visualisation;
using Moq.Performance.Visualisation.Visualisers;
using Xunit;

namespace Moq.Tests.Performance
{
	public class TimelineVisualiserTest
	{
		[Fact]
		public void TimelineVisualiserReturnsCorrectInformation()
		{
			var setup1 = new Mock<IWith>();
			var setup2 = new Mock<IWith>();
			
			var duration1 = new TimeSpan(0, 0, 0, 0, 1);
			var duration2 = new TimeSpan(0, 0, 0, 0, 2);
			var duration3 = new TimeSpan(0, 0, 0, 0, 5);

			var time = DateTime.Now;
			
			var eventRepositoryMock = new Mock<IEventRepository>();
			eventRepositoryMock.Setup(r => r.GetEvents())
				.Returns(new List<IPerformanceEvent>
				{
					new SetupCalledEvent(time, setup1.Object, duration1),
					new SetupCalledEvent(time + new TimeSpan(0, 0, 0, 0, 40), setup2.Object, duration3),
					new SetupCalledEvent(time + new TimeSpan(0, 0, 0, 0, 100), setup1.Object, duration2)
				});
			
			var visualiser = new TimelineVisualiser(eventRepositoryMock.Object);
			

			var result = visualiser.Visualise();

			
			var expectedResult = String.Format("0----------------------------------------1------------------------------------------------------------2\r\n\r\n" +
			                                   "0 : {0}\r\n" +
			                                   "1 : {1}\r\n" +
				                               "2 : {0}\r\n",
				setup1.Object, setup2.Object);
			Assert.Equal(expectedResult, result);
		}
	}
}