using System;

namespace Moq.Tests.Performance.PerformanceDemo.Code
{
	public class CentralComponent
	{
		public CentralComponent(ILoginComponent loginComponent, IDataQueryComponent dataQueryComponent)
		{
			this.loginComponent = loginComponent;
			this.dataQueryComponent = dataQueryComponent;
		}
		
		public string AuthenticateAndGetData(string username, string password)
		{
			if (!this.loginComponent.Authenticate(username, password)) return String.Empty;
			
			return this.dataQueryComponent.GetData();
		}

		private ILoginComponent loginComponent;
		private IDataQueryComponent dataQueryComponent;
	}
}