using System;
using System.Linq;
using Moq.Performance.Assertions;
using Xunit;

namespace Moq.Tests.Performance.Assertions
{
	public class AssertionTests
	{
		[Fact]
		public void LessThanAssertionCorrect()
		{
			PerformanceAssert.Percentile(series.Select(s => new TimeSpan(0, 0, 0, 0, s)), 75, PerformanceAssert.LessThan(new TimeSpan(0, 0, 0, 0, 77)));
			PerformanceAssert.Percentile(series.Select(s => new TimeSpan(0, 0, 0, 0, s)), 50, PerformanceAssert.LessThan(new TimeSpan(0, 0, 0, 0, 52)));
			PerformanceAssert.Percentile(series.Select(s => new TimeSpan(0, 0, 0, 0, s)), 25, PerformanceAssert.LessThan(new TimeSpan(0, 0, 0, 0, 27)));
		}
		
		[Fact]
		public void MoreThanAssertionCorrect()
		{
			PerformanceAssert.Percentile(series.Select(s => new TimeSpan(0, 0, 0, 0, s)), 75, PerformanceAssert.MoreThan(new TimeSpan(0, 0, 0, 0, 75)));
			PerformanceAssert.Percentile(series.Select(s => new TimeSpan(0, 0, 0, 0, s)), 75, PerformanceAssert.MoreThan(new TimeSpan(0, 0, 0, 0, 50)));
			PerformanceAssert.Percentile(series.Select(s => new TimeSpan(0, 0, 0, 0, s)), 75, PerformanceAssert.MoreThan(new TimeSpan(0, 0, 0, 0, 25)));
		}

		private static int[] series = new[]
		{
			48,
			98,
			77,
			28,
			71,
			89,
			88,
			55,
			39,
			18,
			31,
			85,
			67,
			27,
			41,
			58,
			47,
			96,
			80,
			11,
			99,
			62,
			68,
			9,
			42,
			43,
			90,
			7,
			44,
			82,
			38,
			33,
			26,
			69,
			92,
			46,
			5,
			94,
			53,
			86,
			15,
			79,
			20,
			95,
			2,
			21,
			66,
			12,
			51,
			4,
			13,
			37,
			56,
			65,
			91,
			25,
			52,
			35,
			1,
			59,
			64,
			30,
			72,
			23,
			76,
			29,
			84,
			3,
			74,
			93,
			87,
			19,
			50,
			17,
			70,
			10,
			32,
			24,
			100,
			45,
			14,
			54,
			34,
			36,
			97,
			16,
			57,
			63,
			49,
			60,
			22,
			78,
			6,
			75,
			83,
			73,
			61,
			8,
			81,
			40
		};
	}
}