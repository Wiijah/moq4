using System;
using Moq.Performance;

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
		event EventHandler Invoked;
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
		/// <returns></returns>
		IReturnsThrows<TMock, TResult> With(IPerformanceContext performanceContext);
	}
}