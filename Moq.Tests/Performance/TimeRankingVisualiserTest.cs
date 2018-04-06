using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq.Language.Flow;
using Moq.Performance.Visualisation;
using Moq.Performance.Visualisation.Visualisers;
using Xunit;

namespace Moq.Tests.Performance
{
	public class TimeRankingVisualiserTest
	{
		[Fact]
		[SuppressMessage("ReSharper", "UseStringInterpolation")]
		public void TimeRankingVisualiserReturnsCorrectInformation()
		{
			var setup1 = new Mock<IWith>();
			var setup2 = new Mock<IWith>();
			
			var duration1 = new TimeSpan(0, 0, 0, 0, 1);
			var duration2 = new TimeSpan(0, 0, 0, 0, 2);
			var duration3 = new TimeSpan(0, 0, 0, 0, 5);
			
			var eventRepositoryMock = new Mock<IEventRepository>();
			eventRepositoryMock.Setup(r => r.GetEvents())
				               .Returns(new List<IPerformanceEvent>
				{
					new SetupCalledEvent(new DateTime(), setup1.Object, duration1),
					new SetupCalledEvent(new DateTime(), setup2.Object, duration3),
					new SetupCalledEvent(new DateTime(), setup1.Object, duration2)
				});
			
			var visualiser = new TimeRankingVisualiser(eventRepositoryMock.Object);
			

			var result = visualiser.Visualise();

			
			var expectedResult = String.Format("Setup rankings from most time taken to least time taken\n" +
			                                   "{0} took {1} ms in total\r\n" +
			                                   "{2} took {3} ms in total\r\n",
											   setup2.Object, duration3.Milliseconds, setup1.Object, (duration1 + duration2).Milliseconds);
			Assert.Equal(result, expectedResult);
		}
	}
}