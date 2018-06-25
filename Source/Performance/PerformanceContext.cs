﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
			this.taskId = Task.CurrentId;
			this.eventRepository.AddEvent(new TestStartEvent(DateTime.Now));
			timer.Start();
			performanceTest.Invoke();
			timer?.Stop();
			this.eventRepository.AddEvent(new TestStopEvent(DateTime.Now + virtualTime));

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
				eventRepository.AddEvent(
					new SetupCalledEvent(
						DateTime.Now + virtualTime,
						setup,
						timeTaken));
					
				virtualTime += timeTaken;
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
								
				eventRepository.AddEvent(
					new SetupCalledEvent(
						DateTime.Now + virtualTime, 
						setup, 
						new TimeSpan(0, 0, 0, 0, (int) time)));
				
				virtualTime += new TimeSpan(0, 0, 0, 0, (int) time);

			};
		}

		/// <inheritdoc />
		public void AddTo(IWith setup, TimeSpan timeTaken, Func<IWith, bool> isRelevantWhenOnOtherThread)
		{
			setup.StartInvocation += (_, __) => { timer.Stop(); };
			setup.EndInvocation += (_, __) =>
			{
				if (Task.CurrentId == taskId || isRelevantWhenOnOtherThread(setup) && Task.CurrentId != taskId)
				{
					virtualTime += timeTaken;
				}
			};
		}

		/// <inheritdoc />
		public void AddTo(IWith setup, IPerformanceModel model, Func<IWith, bool> isRelevantWhenOnOtherThread)
		{
			setup.StartInvocation += (_, __) => { timer.Stop(); };
			setup.EndInvocation += (_, __) =>
			{
				if (Task.CurrentId == taskId || isRelevantWhenOnOtherThread(setup) && Task.CurrentId != taskId)
				{
					virtualTime += new TimeSpan(0, 0, 0, 0, (int) model.DrawTime());
				}
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
		private int? taskId;
	}
}