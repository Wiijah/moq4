using System;
using System.Diagnostics;
using Moq.Language.Flow;
using Moq.Performance.PerformanceModels;

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
		public long TimeTaken
		{
			get;
			private set;
		}

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

			TimeTaken = timer.ElapsedMilliseconds + virtualTime;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceTest"></param>
		/// <param name="number"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void Run(Action performanceTest, int number)
		{
			long average = 0;
			for (int i = 0; i < number; i++)
			{
				timer.Reset();
				virtualTime = 0;
				Run(performanceTest);
				average += TimeTaken;
			}

			this.TimeTaken = average / number;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="timeTaken"></param>
		public void AddTo(IWith setup, long timeTaken)
		{
			setup.StartInvocation += (_, __) => { timer.Stop(); };
			setup.EndInvocation += (_, __) =>
			{
				virtualTime += timeTaken;
				timer.Start();
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="model"></param>
		public void AddTo(IWith setup, IPerformanceModel model)
		{
			setup.StartInvocation += (_, __) => { timer.Stop(); };
			setup.EndInvocation += (_, __) =>
			{
				virtualTime += (long) model.DrawTime();
				timer.Start();
			};
		}

	}
}