﻿namespace Moq.Performance
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
		public long DrawTime()
		{
			return constantTime;
		}
	}
}