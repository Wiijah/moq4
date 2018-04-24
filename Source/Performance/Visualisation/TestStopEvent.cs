using System;

namespace Moq.Performance.Visualisation
{
	/// <summary>
	/// 
	/// </summary>
	public class TestStopEvent : IPerformanceEvent
	{
		/// <inheritdoc />
		public DateTime TimeStamp { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeStamp"></param>
		public TestStopEvent(DateTime timeStamp)
		{
			this.TimeStamp = timeStamp;
		}

		/// <inheritdoc />
		public override String ToString()
		{
			return "Test Stop";
		}
	}
}