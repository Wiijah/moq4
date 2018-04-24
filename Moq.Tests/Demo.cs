using System;
using System.Threading;
using Moq.Performance;
using Moq.Performance.PerformanceModels;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Tests
{
	public class Demo
	{
		private static string TALISKER = "Talisker";

		private ITestOutputHelper output;

		public Demo(ITestOutputHelper output)
		{
			this.output = output;
		}
		
		[Fact]
		public void FillingRemovesInventoryIfInStock()
		{
			//setup - data
			var order = new Order(TALISKER, 50);
			var mock = new Mock<IWarehouse>();

			//setup - expectations
			mock.Setup(x => x.HasInventory(TALISKER, 50)).Returns(true);

			//exercise
			order.Fill(mock.Object);

			//verify state
			Assert.True(order.IsFilled);
			//verify interaction
			mock.VerifyAll();
		}

		[Fact]
		public void FillingDoesNotRemoveIfNotEnoughInStock()
		{
			//setup - data
			var order = new Order(TALISKER, 50);
			var mock = new Mock<IWarehouse>();
			var performanceContext = new PerformanceContext();
			
			//setup - expectations
			mock.Setup(x => x.HasInventory(It.IsAny<string>(), It.IsInRange(0, 100, Range.Inclusive))).With(new ConstantPerformanceModel(500)).Returns(false);
			mock.Setup(x => x.Remove(It.IsAny<string>(), It.IsAny<int>())).Throws(new InvalidOperationException());

			//exercise
			performanceContext.Run(() => order.Fill(mock.Object), 100);

			//verify
			Assert.False(order.IsFilled);
			Assert.True(performanceContext.TimeTaken.TotalMilliseconds < 610, $"Expected performance less than 610ms, but got average performance: {performanceContext.TimeTaken}");
		}

		[Fact]
		public void TestPresenterSelection()
		{
			var mockView = new Mock<IOrdersView>();
			var presenter = new OrdersPresenter(mockView.Object);

			// Check that the presenter has no selection by default
			Assert.Null(presenter.SelectedOrder);

			// Finally raise the event with a specific arguments data
			mockView.Raise(mv => mv.OrderSelected += null, new OrderEventArgs { Order = new Order("moq", 500) });

			// Now the presenter reacted to the event, and we have a selected order
			Assert.NotNull(presenter.SelectedOrder);
			Assert.Equal("moq", presenter.SelectedOrder.ProductName);
		}

		[Fact]
		public void TestFillPerformance()
		{
			var performanceContext = new PerformanceContext();
			var order1 = new Order("TALISKER", 20);
			var order2 = new Order("TALISKER", 20);

			var mock = new Mock<IWarehouse>(performanceContext, MockBehavior.Default);
			mock.Setup(w => w.HasInventory("TALISKER", 20)).With(new GaussianDistributionPerformanceModel(1000, 1)).Returns(true);
			mock.Setup(w => w.Remove("TALISKER", 20)).With(new UniformDistributionPerformanceModel(500, 700));

			performanceContext.Run(() =>
			{
				order1.Fill(mock.Object);
				order2.Fill(mock.Object);

			});
			
			output.WriteLine(performanceContext.TimeRankingVisualisation());
			output.WriteLine("\n\n");
			output.WriteLine(performanceContext.TimelineVisualisation());

			Assert.True(performanceContext.TimeTaken < new TimeSpan(0, 0, 0, 5));
		}

		public class OrderEventArgs : EventArgs
		{
			public Order Order { get; set; }
		}

		public interface IOrdersView
		{
			event EventHandler<OrderEventArgs> OrderSelected;
		}

		public class OrdersPresenter
		{
			public OrdersPresenter(IOrdersView view)
			{
				view.OrderSelected += (sender, args) => DoOrderSelection(args.Order);
			}

			public Order SelectedOrder { get; private set; }

			private void DoOrderSelection(Order selectedOrder)
			{
				// Do something when the view selects an order.
				SelectedOrder = selectedOrder;
			}
		}

		public interface IWarehouse
		{
			bool HasInventory(string productName, int quantity);
			void Remove(string productName, int quantity);
		}

		public class Order
		{
			public string ProductName { get; private set; }
			public int Quantity { get; private set; }
			public bool IsFilled { get; private set; }

			public Order(string productName, int quantity)
			{
				this.ProductName = productName;
				this.Quantity = quantity;
			}

			public void Fill(IWarehouse warehouse)
			{
				Thread.Sleep(100);
				if (warehouse.HasInventory(ProductName, Quantity))
				{
					warehouse.Remove(ProductName, Quantity);
					IsFilled = true;
				}
			}

		}
	}
}
