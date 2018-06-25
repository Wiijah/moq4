using System;
using Moq.Performance;
using Moq.Tests.Performance.PerformanceDemo.Code;
using Moq.Tests.Performance.PerformanceDemo.Models;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Tests.Performance.PerformanceDemo.Tests
{
	public class LoginComponentTest
	{
		private ITestOutputHelper output;

		public LoginComponentTest(ITestOutputHelper output)
		{
			this.output = output;
		}
		
		[Fact]
		public void TestAuthenticatePerformance()
		{
			var username = "User";
			var pass = "Pass";
			var performanceContext = new PerformanceContext();
			
			var loginServiceMock = new Mock<ILoginService>(performanceContext, MockBehavior.Default);
			loginServiceMock.Setup(s => s.Authenticate(username, pass)).With(new LoginServiceModel()).Returns(true);
			
			var loginComponent = new LoginComponent(loginServiceMock.Object);
			
			performanceContext.Run(() => loginComponent.Authenticate(username, pass));
			
			output.WriteLine(performanceContext.TimelineVisualisation());
			
			Assert.True(performanceContext.TimeTaken < new TimeSpan(0, 0, 1));
		}
	}
}