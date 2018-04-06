using System;
using Moq.Language.Flow;

namespace Moq.Performance.Visualisation
{
	/// <summary>
	/// 
	/// </summary>
	public class SetupCalledEvent : IPerformanceEvent
	{
		/// <inheritdoc />
		public DateTime TimeStamp { get; }
		/// <summary>
		/// 
		/// </summary>
		public IWith Setup { get; }
		/// <summary>
		/// 
		/// </summary>
		public TimeSpan Duration { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeStamp"></param>
		/// <param name="setup"></param>
		/// <param name="duration"></param>
		public SetupCalledEvent(DateTime timeStamp, IWith setup, TimeSpan duration)
		{
			this.TimeStamp = timeStamp;
			this.Setup = setup;
			this.Duration = duration;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Setup.ToString();
		}
	}
}