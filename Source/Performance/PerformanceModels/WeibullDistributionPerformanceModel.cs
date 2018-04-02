using System;

namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class WeibullDistributionPerformanceModel : IPerformanceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public double Shape { get; }
		/// <summary>
		/// 
		/// </summary>
		public double Scale { get; }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="scale"></param>
		public WeibullDistributionPerformanceModel(double shape, double scale)
		{
			this.Shape = shape;
			this.Scale = scale;

			this.random = new Random();
		}

		/// <inheritdoc />
		public double DrawTime()
		{
			return Scale * Math.Pow(-Math.Log(random.NextDouble()), 1.0 / Shape);
		}
		
		private readonly Random random;
	}
}