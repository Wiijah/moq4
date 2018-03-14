﻿using System;
using System.Diagnostics;
using System.Threading;
using Moq.Performance;
using Xunit;

namespace Moq.Tests
{
	public class PerformanceTest
	{
		[Fact]
		public void PerformanceMatchesReality()
		{
			IPerformanceModel testModel = new ConstantPerformanceModel(500);
			IPerformanceModel realModel = new ConstantPerformanceModel(100);
			
			IPerformanceContext performanceContext = new PerformanceContext();
			
			ClassUnderTest actualClassUnderTest = new ClassUnderTest();
			ClassUnderTest testClassUnderTest = new ClassUnderTest();
			var mockDependency = new Mock<ITestDependency>();
			mockDependency.Setup(x => x.DependencyFunction(testModel)).With(performanceContext, testModel);

			long realTime = actualClassUnderTest.FunctionUnderTest(testModel, new TestDependency());
			performanceContext.Run(() => testClassUnderTest.FunctionUnderTest(testModel, mockDependency.Object));
			
			Assert.True(Math.Abs(realTime - performanceContext.TimeTaken)/realTime * 100 < 1);
		}
	}

	internal class ClassUnderTest
	{
		public long FunctionUnderTest(IPerformanceModel model, ITestDependency dependency)
		{
			Stopwatch timer = Stopwatch.StartNew();
			
			// Work in function
			Thread.Sleep(10);
			
			// Work in dependency
			dependency.DependencyFunction(model);
			
			return timer.ElapsedMilliseconds;
		}
	}

	public interface ITestDependency
	{
		int DependencyFunction(IPerformanceModel model);
	}

	internal class TestDependency : ITestDependency
	{
		public int DependencyFunction(IPerformanceModel model)
		{
			Thread.Sleep((int) model.DrawTime());
			return 0;
		}
	}
}