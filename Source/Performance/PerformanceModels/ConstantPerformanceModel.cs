namespace Moq.Performance.PerformanceModels
{
	/// <summary>
	/// 
	/// </summary>
	public class ConstantPerformanceModel : IPerformanceModel
	{
		private readonly long constantTime;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="constantTime"></param>
		public ConstantPerformanceModel(long constantTime)
		{
			this.constantTime = constantTime;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double DrawTime()
		{
			return constantTime;
		}
	}
}