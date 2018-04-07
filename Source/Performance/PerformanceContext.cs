using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq.Language.Flow;
using Moq.Performance.PerformanceModels;
using Moq.Performance.Visualisation;
using Moq.Performance.Visualisation.Visualisers;

namespace Moq.Performance
{
	/// <summary>
	/// 
	/// </summary>
	public class PerformanceContext : IPerformanceContext
	{
		/// <summary>
		/// 
		/// </summary>
		public TimeSpan TimeTaken { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public PerformanceContext()
		{
			eventRepository = new EventRepository();
			rankingVisualiser = new TimeRankingVisualiser(eventRepository);
			timelineVisualiser = new TimelineVisualiser(eventRepository);
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
			timer?.Stop();

			TimeTaken = timer.Elapsed + virtualTime;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceTest"></param>
		/// <param name="number"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void Run(Action performanceTest, int number)
		{
			IList<TimeSpan> times = new List<TimeSpan>();
			for (int i = 0; i < number; i++)
			{
				timer.Reset();
				virtualTime = new TimeSpan(0);
				Run(performanceTest);
				times.Add(TimeTaken);
			}

			this.TimeTaken = new TimeSpan(Convert.ToInt64(times.Average(timeSpan => timeSpan.Ticks)));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="timeTaken"></param>
		public void AddTo(IWith setup, TimeSpan timeTaken)
		{
			setup.StartInvocation += (_, __) =>
			{
				timer.Stop();
			};
			setup.EndInvocation += (_, __) =>
			{
				virtualTime += timeTaken;
				
				eventRepository.AddEvent(
					new SetupCalledEvent(
						DateTime.Now + virtualTime, 
						setup, 
						timeTaken));
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="setup"></param>
		/// <param name="model"></param>
		public void AddTo(IWith setup, IPerformanceModel model)
		{
			setup.StartInvocation += (_, __) =>
			{
				timer.Stop();
			};
			setup.EndInvocation += (_, __) =>
			{
				if (timer.IsRunning) timer.Stop();
				
				var time = model.DrawTime();
				virtualTime += new TimeSpan(0, 0, 0, 0, (int) time);
				
				eventRepository.AddEvent(
					new SetupCalledEvent(
						DateTime.Now + virtualTime, 
						setup, 
						new TimeSpan(0, 0, 0, 0, (int) time)));
			};
		}

		/// <inheritdoc />
		public string TimeRankingVisualisation()
		{
			return rankingVisualiser.Visualise();
		}

		/// <inheritdoc />
		public string TimelineVisualisation()
		{
			return timelineVisualiser.Visualise();
		}

		/// <inheritdoc />
		public void StopTimer()
		{
			timer?.Stop();
		}

		/// <inheritdoc />
		public void StartTimer()
		{
			timer?.Start();
		} 

		private Stopwatch timer;
		private TimeSpan virtualTime = new TimeSpan(0);
		private IEventRepository eventRepository;
		private TimeRankingVisualiser rankingVisualiser;
		private TimelineVisualiser timelineVisualiser;
	}
}