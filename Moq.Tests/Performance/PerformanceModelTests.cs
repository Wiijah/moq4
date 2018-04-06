using System;
using System.Linq;
using Moq.Performance.PerformanceModels;
using Xunit;

namespace Moq.Tests.Performance
{
	public class PerformanceModelTests
	{
		[Fact]
		public void GaussianDistributionSamplingFits()
		{
			var mean = 100;
			var stddev = 20;
			var gaussian = new GaussianDistributionPerformanceModel(mean, stddev);
			var samples = new double[1000];

			for (int i = 0; i < 1000; i++)
			{
				samples[i] = gaussian.DrawTime();
			}

			var estimates = estimateNormalParameters(samples);
			
			Assert.True((Math.Abs(mean - estimates.Item1) / mean) * 100 < 1, $"Expected mean: {mean}, but got: {estimates.Item1}");
			Assert.True((Math.Abs(stddev - estimates.Item2) / stddev) * 100 < 1, $"Expected stdDev: {stddev}, but got: {estimates.Item2}");
		}

		[Fact]
		public void UniformDistributionSamplingFits()
		{
			// Null Hypothesis  : samples follow a uniform distribution
			// Other Hyptohesis : samples do not follow a uniform distribution
 			var min = 500;
			var max = 1000;
			var uniform = new UniformDistributionPerformanceModel(min, max);

			var length = (max - min) * 5;
			var samples = new long[length];

			for (int i = 0; i < length; i++)
			{
				samples[i] = (long) uniform.DrawTime();
			}

			var buckets = new int[max - min + 1];
			for (int i = 0; i < length; i++)
			{
				buckets[samples[i] - min]++;
			}

			double chi_squared = 0;
			for (int i = 0; i < max - min + 1; i++)
			{
				chi_squared += Math.Pow((buckets[i] - 5), 2) / 5;
			}
			
			Assert.True(chi_squared < 553.127); // Assert p-value > 0.05, we do not have enough evidence to reject the 
			                                    // null hypothesis with significance level 0.05
		}

		[Fact]
		public void LogNormalSamplingFits()
		{
			double logScale = 1;
			double shape = 0.1;
			var lognormal = new LogNormalDistributionPerformanceModel(logScale, shape);

			var samples = new double[1000];
			for (int i = 0; i < 1000; i++)
			{
				var res = lognormal.DrawTime();
				while (res < 0) res = lognormal.DrawTime();

				samples[i] = Math.Log(res);
			}

			var estimates = estimateNormalParameters(samples);
			
			Assert.True((Math.Abs(estimates.Item1 - logScale)/logScale) * 100 < 5, $"Expected logScale: {logScale}, but got: {estimates.Item1}");
			Assert.True((Math.Abs(estimates.Item2 - shape)/shape) * 100 < 5, $"Expected shape: {shape}, but got {estimates.Item2}");
		}

		[Fact]
		public void WeibullSamplingFits()
		{
			double scale = 10;
			double shape = 1;
			var weibull = new WeibullDistributionPerformanceModel(shape, scale);
			
			var samples = new long[1000];

			for (int i = 0; i < samples.Length; i++)
			{
				var res = weibull.DrawTime();

				samples[i] = (long) Math.Round(res - 0.5, MidpointRounding.AwayFromZero);
			}

			var buckets = new int[20];
			var bucketsExpected = new double[20];
			foreach (var sample in samples)
			{
				if (sample < 0 || sample >= 20) continue;
				buckets[sample]++;
			}

			for (int i = 0; i < buckets.Length; i++)
			{
				bucketsExpected[i] = (WeibullCdf(i + 1, scale, shape) - WeibullCdf(i, scale, shape)) * 1000;
			}

			var chi_squared = buckets.Select((bucketCount, i) => Math.Pow(bucketCount - bucketsExpected[i], 2) / bucketsExpected[i]).Sum();

			Assert.True(chi_squared < 30.144, $"Assumed chi-squared less than: {30.144}, but was {chi_squared}");
		}

		[Fact]
		public void CauchySamplingFits()
		{
			var location = 10;
			var scale = 0.5;
			var cauchy = new CauchyDistributionPerformanceModel(location, scale);

			var samples = new long[1000];
			
			for (int i = 0; i < samples.Length; i++)
			{
				var res = cauchy.DrawTime();

				samples[i] = (long) Math.Round(res - 0.5, MidpointRounding.AwayFromZero);
			}

			var buckets = new int[20];
			var bucketsExpected = new double[20];
			foreach (var sample in samples)
			{
				if (sample < 0 || sample >= 20) continue;
				buckets[sample]++;
			}

			for (int i = 0; i < buckets.Length; i++)
			{
				bucketsExpected[i] = (CauchyCdf(i + 1, location, scale) - CauchyCdf(i, location, scale)) * 1000;
			}

			var chi_squared = buckets.Select((bucketCount, i) => Math.Pow(bucketCount - bucketsExpected[i], 2) / bucketsExpected[i]).Sum();

			Assert.True(chi_squared < 30.144, $"Assumed chi-squared less than: {30.144}, but was {chi_squared}");
		}
		
		[Fact]
		public void ExponentialSamplingFits()
		{
			double rate = 0.5;
			var exponential = new ExponentialDistributionPerformanceModel(rate);

			var samples = new long[1000];
			
			for (int i = 0; i < samples.Length; i++)
			{
				var res = exponential.DrawTime();

				samples[i] = (long) Math.Round(res - 0.5, MidpointRounding.AwayFromZero);
			}

			var buckets = new int[20];
			var bucketsExpected = new double[20];
			foreach (var sample in samples)
			{
				if (sample < 0 || sample >= 20) continue;
				buckets[sample]++;
			}

			for (int i = 0; i < buckets.Length; i++)
			{
				bucketsExpected[i] = (ExponentialCdf(i + 1, rate) - ExponentialCdf(i, rate)) * 1000;
			}

			var chi_squared = buckets.Select((bucketCount, i) => Math.Pow(bucketCount - bucketsExpected[i], 2) / bucketsExpected[i]).Sum();

			Assert.True(chi_squared < 30.144, $"Assumed chi-squared less than: {30.144}, but was {chi_squared}");
		}

		private double ExponentialCdf(long x, double rate)
		{
			return x < 0 ? 0 : 1 - Math.Exp(-rate * x);
		}
		
		private double CauchyCdf(long x, double location, double scale)
		{
			return 1 / Math.PI * Math.Atan((x - location) / scale) + 1 / 2;
		}

		private double WeibullCdf(long x, double scale, double shape)
		{
			return 1 - Math.Exp(-Math.Pow(x / scale, shape));
		}

		private Tuple<double, double> estimateNormalParameters(double[] samples)
		{
			double estMean = 0;
			for (int i = 0; i < 1000; i++)
			{
				estMean += samples[i];
			}

			estMean = estMean / 1000;

			double estVariance = 0;
			for (int i = 0; i < 1000; i++)
			{
				estVariance += Math.Pow((samples[i] - estMean), 2);
			}
			
			estVariance = estVariance / 1000;
			var estStdDev = Math.Sqrt(estVariance);
			
			return new Tuple<double, double>(estMean, estStdDev);
		}
	}
}