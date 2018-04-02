using System;
using Moq.Performance;
using Moq.Performance.PerformanceModels;

namespace Moq.Language.Flow
{
	/// <summary>
	/// 
	/// </summary>
	public interface IWith
	{
		/// <summary>
		/// 
		/// </summary>
		event EventHandler StartInvocation;

		/// <summary>
		/// 
		/// </summary>
		event EventHandler EndInvocation;
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TMock"></typeparam>
	/// <typeparam name="TResult"></typeparam>
	public interface IWith<TMock, TResult> : IWith
		where TMock : class
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceContext"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		IReturnsThrows<TMock, TResult> With(IPerformanceContext performanceContext, long time);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="performanceContext"></param>
		/// <param name="moodel"></param>
		/// <returns></returns>
		IReturnsThrows<TMock, TResult> With(IPerformanceContext performanceContext, IPerformanceModel moodel);
	}
}