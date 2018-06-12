using System;
using System.Collections.Generic;
using System.Linq;
using Moq.Performance.PerformanceModels;

namespace Moq.Performance.Assertions
{
	/// <summary>
	/// 
	/// </summary>
	public static class PerformanceAssert
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="runtimes"></param>
		/// <param name="model"></param>
		public static void RuntimesMatch(IEnumerable<TimeSpan> runtimes, IPerformanceModel model)
		{
			var enumerableToArray = runtimes as TimeSpan[] ?? runtimes.ToArray();
			if (enumerableToArray.Length < 1000) throw new Exception("Need at least 1000 samples to match to model");
		
			var runtimesArray = enumerableToArray.OrderBy(x => Guid.NewGuid()).Take(1000).ToArray();	
			var min = runtimesArray.Min();
			var max = runtimesArray.Max();
			var samples = new long[runtimesArray.Length];

			for (int i = 0; i < samples.Length; i++)
			{
				var res = model.DrawTime();

				samples[i] = (long) Math.Round(res - 0.5, MidpointRounding.AwayFromZero);
			}

			var bucketsLength = (int) (min.TotalMilliseconds - max.TotalMilliseconds);
			
			var bucketsExpected = new int[bucketsLength];
			foreach (var sample in samples)
			{
				if (sample < min.TotalMilliseconds || sample >= max.TotalMilliseconds) continue;
				bucketsExpected[sample - (int) min.TotalMilliseconds]++;
			}

			var buckets = new int[bucketsLength];
			foreach (var runtime in runtimesArray)
			{
				buckets[(int) runtime.TotalMilliseconds - (int) min.TotalMilliseconds]++;
			}

			var chi_squared = buckets.Select((bucketCount, i) => Math.Pow(bucketCount - bucketsExpected[i], 2) / bucketsExpected[i]).Sum();

			if (chi_squared >= 1074.679) throw new Exception("Model does not match");
		}

		/// <summary>
		/// </summary>
		/// <param name="runtimes"></param>
		/// <param name="percentile"></param>
		/// <param name="cond"></param>
		public static void Percentile(IEnumerable<TimeSpan> runtimes, int percentile, PercentileCondition cond)
		{
			var runtimesArray = runtimes as TimeSpan[] ?? runtimes.ToArray();
			
			int toSkip = Convert.ToInt32(Math.Truncate(runtimesArray.Length * (percentile / 100.0)));
			var valueAtPercentile = runtimesArray.OrderBy(r => r.Ticks).Skip(toSkip).First();
			
			if (!cond.Check(valueAtPercentile)) throw new Exception("Percentile assertion failed");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="limit"></param>
		/// <returns></returns>
		public static PercentileCondition LessThan(TimeSpan limit)
		{
			return new PercentileCondition(actual => actual.Ticks < limit.Ticks);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="limit"></param>
		/// <returns></returns>
		public static PercentileCondition MoreThan(TimeSpan limit)
		{
			return new PercentileCondition(actual => actual.Ticks > limit.Ticks);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lowerLimit"></param>
		/// <param name="higherLimit"></param>
		/// <returns></returns>
		public static PercentileCondition Between(TimeSpan lowerLimit, TimeSpan higherLimit)
		{
			return new PercentileCondition(actual => actual.Ticks > lowerLimit.Ticks && actual.Ticks < higherLimit.Ticks);
		}

		/// <summary>
		/// 
		/// </summary>
		public class PercentileCondition
		{
			/// <summary>
			/// 
			/// </summary>
			/// <param name="condition"></param>
			public PercentileCondition(Func<TimeSpan, bool> condition)
			{
				this.condition = condition;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="perc"></param>
			/// <returns></returns>
			public bool Check(TimeSpan perc)
			{
				return condition(perc);
			}
			
			private readonly Func<TimeSpan, bool> condition;
		}
	}
}