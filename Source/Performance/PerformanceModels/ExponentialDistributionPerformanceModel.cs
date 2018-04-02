using System;

namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class ExponentialDistributionPerformanceModel : IPerformanceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public double Rate { get; }
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rate"></param>
		public ExponentialDistributionPerformanceModel(double rate)
		{
			this.Rate = rate;
			
			this.random = new Random();
		}	
		
		/// <inheritdoc />
		public double DrawTime()
		{
			var r = random.NextDouble();
			while (r == 0.0)
			{
				r = random.NextDouble();
			}

			return -Math.Log(r)/Rate;
		}

		private readonly Random random;
	}
}