using System;

namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class CauchyDistributionPerformanceModel : IPerformanceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public double Location { get; }
		/// <summary>
		/// 
		/// </summary>
		public double Scale { get; }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="location"></param>
		/// <param name="scale"></param>
		public CauchyDistributionPerformanceModel(double location, double scale)
		{
			this.Location = location;
			this.Scale = scale;
			
			this.random = new Random();
		}

		/// <inheritdoc />
		public double DrawTime()
		{
			return Location + Scale*Math.Tan(Math.PI*(random.NextDouble() - 0.5));
		}

		private readonly Random random;
	}
}