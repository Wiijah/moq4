using System;

namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class LogNormalDistributionPerformanceModel : IPerformanceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public double LogScale { get; }
		/// <summary>
		/// 
		/// </summary>
		public double Shape { get; }
		
		/// <inheritdoc />
		public LogNormalDistributionPerformanceModel(double logScale, double shape)
		{
			this.LogScale = logScale;
			this.Shape = shape;
			
			this.gaussian = new GaussianDistributionPerformanceModel(0, 1);
		}

		/// <inheritdoc />
		public double DrawTime()
		{
			return Math.Exp(LogScale + Shape * gaussian.DrawTime());
		}

		private readonly GaussianDistributionPerformanceModel gaussian;
	}
}