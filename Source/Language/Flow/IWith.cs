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
		/// <param name="time"></param>
		/// <returns></returns>
		IReturnsThrows<TMock, TResult> With(TimeSpan time);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		IReturnsThrows<TMock, TResult> With(IPerformanceModel model);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TMock"></typeparam>
	public interface IWith<TMock> : IWith
		where TMock : class
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="time"></param>
		void With(TimeSpan time);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		void With(IPerformanceModel model);
	}
}