using System;
using System.Diagnostics;
using Moq.Language.Flow;

namespace Moq.Performance
{
	/// <summary>
	/// 
	/// </summary>
	public class PerformanceContext : IPerformanceContext
	{
		private Stopwatch timer;
		private long virtualTime = 0;
		
		/// <summary>
		/// 
		/// </summary>
		public long TimeTaken => timer.ElapsedMilliseconds + virtualTime;

		/// <summary>
		/// 
		/// </summary>
		public PerformanceContext()
		{
			timer = new Stopwatch();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceTest"></param>
		public void Run(Action performanceTest)
		{
			timer.Start();
			performanceTest.Invoke();
			timer.Stop();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="timeTaken"></param>
		public void AddTo(IWith setup, long timeTaken)
		{
			setup.Invoked += (_, __) => { virtualTime += timeTaken; };
		}

	}
}