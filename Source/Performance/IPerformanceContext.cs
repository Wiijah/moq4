using System;
using Moq.Language.Flow;

namespace Moq.Performance
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPerformanceContext
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceTest"></param>
		void Run(Action performanceTest);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceTest"></param>
		/// <param name="number"></param>
		void Run(Action performanceTest, int number);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="timeTaken"></param>
		void AddTo(IWith setup, long timeTaken);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="model"></param>
		void AddTo(IWith setup, IPerformanceModel model);
		
		/// <summary>
		/// 
		/// </summary>
		long TimeTaken { get; }
	}
}