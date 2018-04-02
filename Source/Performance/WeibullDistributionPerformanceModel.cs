using MathNet.Numerics.Distributions;

namespace Moq.Performance
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
			
			this.weibull = new Weibull(Shape, Scale);
		}

		/// <inheritdoc />
		public double DrawTime()
		{
			return this.weibull.Sample();
		}

		private readonly Weibull weibull;
	}
}