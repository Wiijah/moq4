using System;
using System.Threading;

namespace Moq.Tests.Performance.PerformanceDemo.Code
{
	public interface IDataQueryComponent
	{
		string GetData();
	}
	
	public class DataQueryComponent : IDataQueryComponent
	{
		public DataQueryComponent(IDataQueryService dataQueryService)
		{
			this.dataQueryService = dataQueryService;
		}

		public string GetData()
		{
			if (!this.CheckAuthenticated()) return String.Empty;

			return this.dataQueryService.GetData();
		}

		private bool CheckAuthenticated()
		{
			Thread.Sleep(100); // simulate work checking authentication
			return true;
		}

		private IDataQueryService dataQueryService;
	}

	public interface IDataQueryService
	{
		string GetData();
	}
}