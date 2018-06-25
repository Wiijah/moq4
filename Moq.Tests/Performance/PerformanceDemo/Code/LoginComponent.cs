using System.Threading;

namespace Moq.Tests.Performance.PerformanceDemo.Code
{
	public interface ILoginComponent
	{
		bool Authenticate(string username, string password);
	}
	
	public class LoginComponent : ILoginComponent
	{
		public LoginComponent(ILoginService loginService)
		{
			this.loginService = loginService;
		}

		public bool Authenticate(string username, string password)
		{
			if (!this.CheckCreds(username, password)) return false;
			
			if (this.loginService.Authenticate(username, password) && this.loginService.Authenticate(username, password)) return true;
			
			return false;
		}

		private bool CheckCreds(string username, string password)
		{
			Thread.Sleep(150); // simulate work checking credentials
			return true;
		}

		private ILoginService loginService;
	}

	public interface ILoginService
	{
		bool Authenticate(string username, string password);
	}
}