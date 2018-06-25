using Moq.Performance.PerformanceModels;

namespace Moq.Tests.Performance.PerformanceDemo.Models
{
	public class DataQueryServiceModel : IPerformanceModel
	{
		public double DrawTime()
		{
			return 1000;
		}
	}
}