using System;

namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class UniformDistributionPerformanceModel : IPerformanceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public long Min { get; }
		/// <summary>
		/// 
		/// </summary>
		public long Max { get; }


		/// <inheritdoc />
		public UniformDistributionPerformanceModel(long min, long max)
		{
			this.Min = min;
			this.Max = max;
			this.random = new Random();
		}

		/// <inheritdoc />
		public double DrawTime()
		{
			int overflow = 0;
			long newMin = Min;
			long newMax = Max;

			while (newMin >= int.MaxValue)
			{
				overflow++;
				newMin -= int.MaxValue;
				newMax -= int.MaxValue;
			}
			
			return random.Next((int) newMin, ((int) newMax) + 1) + int.MaxValue * overflow;
		}

		private Random random;
	}
}