using System;

namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class GaussianDistributionPerformanceModel : IPerformanceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public long Mean { get; }
		/// <summary>
		/// 
		/// </summary>
		public long StandartDeviation { get; }

		/// <inheritdoc />
		public GaussianDistributionPerformanceModel(long mean, long standartDeviation)
		{
			this.Mean = mean;
			this.StandartDeviation = standartDeviation;
			this.random = new Random();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double DrawTime()
		{
			double x1 = 1 - random.NextDouble();
			double x2 = 1 - random.NextDouble();

			double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
			double dev = y1 * StandartDeviation;
			return dev + Mean;
		}

		private readonly Random random;
	}
}