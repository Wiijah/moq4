using System.Collections.Generic;

namespace Moq.Performance.Visualisation
{
	/// <summary>
	/// 
	/// </summary>
	public interface IEventRepository
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ev"></param>
		void AddEvent(IPerformanceEvent ev);
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerable<IPerformanceEvent> GetEvents();
	}
}