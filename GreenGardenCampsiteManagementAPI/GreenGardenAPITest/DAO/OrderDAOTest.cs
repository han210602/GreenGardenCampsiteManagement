﻿using BusinessObject.DTOs;
using BusinessObject.Models;
using Castle.Core.Resource;
using DataAccess.DAO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenGardenAPITest.DAO
{
    public class OrderDAOTest
    {
        private async Task<GreenGardenContext> GetDbContextWithMockData()
        {
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            if (!await dbContext.Orders.AnyAsync())
            {
                dbContext.Roles.AddRange(
                    new Role { RoleId = 1, RoleName = "Admin" },
                    new Role { RoleId = 2, RoleName = "Customer" },
                    new Role { RoleId = 3, RoleName = "Employee" }
                );

                dbContext.Users.AddRange(
                    new User { UserId = 1, FirstName = "John", LastName = "Doe", RoleId = 2, Email = "john.doe@example.com", Password = "password123" },
                    new User { UserId = 2, FirstName = "Jane", LastName = "Smith", RoleId = 2, Email = "jane.smith@example.com", Password = "password123" }
                );

                dbContext.Activities.AddRange(
                    new Activity { ActivityId = 1, ActivityName = "Outdoor Event" },
                    new Activity { ActivityId = 2, ActivityName = "Indoor Meeting" }
                );

                dbContext.Orders.AddRange(
                    new Order
                    {
                        OrderId = 1,
                        CustomerId = 1,
                        ActivityId = 1,
                        OrderDate = DateTime.UtcNow.AddDays(-5),
                        StatusOrder = true,
                        TotalAmount = 200.00m
                    },
                    new Order
                    {
                        OrderId = 2,
                        CustomerId = 1,
                        ActivityId = 2,
                        OrderDate = DateTime.UtcNow.AddDays(-3),
                        StatusOrder = false,
                        TotalAmount = 100.00m
                    },
                    new Order
                    {
                        OrderId = 3,
                        CustomerId = 1,
                        ActivityId = 2,
                        OrderDate = DateTime.UtcNow.AddDays(-4),
                        StatusOrder = true,
                        TotalAmount = 300.00m
                    }
                );

                await dbContext.SaveChangesAsync();
            }

            return dbContext;
        }


        // Unit test for getAllOrder method
        [Fact]
        public async Task GetAllOrder_ShouldReturnListOfOrderDTO()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext); // Initialize DAO context

            // Act
            var result = OrderDAO.getAllOrder();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Ensure there are 2 orders in the result

            var firstOrder = result.First();
            firstOrder.OrderId.Should().Be(1);
            firstOrder.CustomerId.Should().Be(1);
            firstOrder.EmployeeId.Should().Be(1);
            firstOrder.ActivityName.Should().Be("Outdoor Event");
            firstOrder.TotalAmount.Should().Be(200.00m);
            firstOrder.StatusOrder.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllOrder_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var emptyContext = new GreenGardenContext(options);
            emptyContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(emptyContext);

            // Act
            var result = OrderDAO.getAllOrder();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // Expect an empty list
        }

        [Fact]
        public async Task GetAllOrder_ShouldThrowException_WhenDatabaseThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(null);

            // Act & Assert
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(OrderDAO.getAllOrder()));
            exception.Message.Should().Be("Object reference not set to an instance of an object.");

        }

        // Unit test for getAllOrder method

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnAllOrdersSortedByOrderDateDescending()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(2, result[0].OrderId); // OrderId = 2 (most recent date)
            Assert.Equal(3, result[1].OrderId);   // OrderId = 3
            Assert.Equal(1, result[2].OrderId);   // OrderId = 1
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnOrdersWithStatusOrderTrue()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, statusOrder: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.True(o.StatusOrder));
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnOrdersWithStatusOrderFalse()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, statusOrder: false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.False(result[0].StatusOrder);
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnOrdersWithSpecificActivityId()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, activityId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.Equal(2, o.ActivityId));
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnOrdersWithStatusOrderTrueAndSpecificActivityId()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, statusOrder: true, activityId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].StatusOrder);
            Assert.Equal(2, result[0].ActivityId);
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnEmptyListForNonExistentCustomer()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(9999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnEmptyListForInvalidStatusAndActivityCombination()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, statusOrder: true, activityId: 9999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnOrdersWithActivityId1()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, activityId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Only one order exists with ActivityId = 1
            Assert.All(result, o => Assert.Equal(1, o.ActivityId));
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnEmptyListForActivityId3()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1, activityId: 3);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // No orders exist with ActivityId = 3
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var emptyContext = new GreenGardenContext(options);
            emptyContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(emptyContext);

            // Act
            var result = OrderDAO.GetCustomerOrders(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // Expect an empty list
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldThrowException_WhenDatabaseThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(null);

            // Act & Assert
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(OrderDAO.GetCustomerOrders(1)));
            exception.Message.Should().Be("Object reference not set to an instance of an object.");

        }

        // Unit test for GetAllOrderDepositAndUsing method
        [Fact]
        public async Task GetAllOrderDepositAndUsing_ShouldReturnCorrectOrders()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData(); // Create a mock context with data
            OrderDAO.InitializeContext(dbContext); // Initialize OrderDAO with the context

            // Act
            var result = OrderDAO.getAllOrderDepositAndUsing(); // Call the method

            // Assert
            Assert.NotNull(result); // Ensure result is not null
            Assert.Single(result); // Only one order matches the criteria in mock data

            var order = result.First(); // Get the first (and only) order
            Assert.Equal(2, order.ActivityId); // Ensure ActivityId = 2
            Assert.True(order.StatusOrder); // Ensure StatusOrder = true
            Assert.Equal(3, order.OrderId); // Check the OrderId of the matching order
            Assert.Equal("JohnDoe", order.CustomerName); // Check CustomerName (mock data concatenation)


        }

        [Fact]
        public async Task GetAllOrderDepositAndUsing_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var emptyContext = new GreenGardenContext(options);
            emptyContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(emptyContext);

            // Act
            var result = OrderDAO.getAllOrderDepositAndUsing();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // Expect an empty list
        }

        [Fact]
        public async Task GetAllOrderDepositAndUsing_ShouldThrowException_WhenDatabaseThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(null);

            // Act & Assert
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(OrderDAO.getAllOrderDepositAndUsing()));
            exception.Message.Should().Be("Object reference not set to an instance of an object.");

        }

        // Unit test for EnterDeposit method
        private async Task<GreenGardenContext> GetDbContextWithMockData2()
        {
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique in-memory DB for testing
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            if (!await dbContext.Orders.AnyAsync())
            {
                dbContext.Roles.AddRange(
                    new Role { RoleId = 1, RoleName = "Admin" },
                    new Role { RoleId = 2, RoleName = "Customer" },
                    new Role { RoleId = 3, RoleName = "Employee" }
                );

                dbContext.Users.AddRange(
                    new User
                    {
                        UserId = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        PhoneNumber = "123456789",
                        RoleId = 2,
                        Email = "john.doe@example.com",
                        Password = "password123"
                    },
                    new User
                    {
                        UserId = 2,
                        FirstName = "Jane",
                        LastName = "Smith",
                        PhoneNumber = "987654321",
                        RoleId = 3,
                        Email = "jane.smith@example.com",
                        Password = "password123"
                    }
                );

                dbContext.Activities.AddRange(
                    new Activity { ActivityId = 1, ActivityName = "Outdoor Event" },
                    new Activity { ActivityId = 2, ActivityName = "Indoor Event" }
                );

                dbContext.Orders.AddRange(
                    new Order
                    {
                        OrderId = 1,
                        CustomerId = 1,
                        EmployeeId = 2,
                        OrderDate = DateTime.UtcNow.AddDays(-5),
                        OrderUsageDate = DateTime.UtcNow.AddDays(2),
                        Deposit = 0,
                        TotalAmount = 200.00m,
                        AmountPayable = 0,
                        StatusOrder = false,
                        ActivityId = 1
                    },
                    new Order
                    {
                        OrderId = 2,
                        CustomerId = 1,
                        EmployeeId = 2,
                        OrderDate = DateTime.UtcNow.AddDays(-2),
                        OrderUsageDate = DateTime.UtcNow.AddDays(1),
                        Deposit = 70.00m,
                        TotalAmount = 300.00m,
                        AmountPayable = 230.00m,
                        StatusOrder = true,
                        ActivityId = 2
                    }
                );

                await dbContext.SaveChangesAsync();
            }

            return dbContext;
        }

        [Fact]
        public async Task EnterDeposit_ShouldUpdateOrder_WhenOrderExists()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData2();
            OrderDAO.InitializeContext(dbContext); // Initialize the DAO with the context

            int orderId = 1; // An order that exists in the mock data
            decimal depositAmount = 100.00m; // New deposit amount

            // Act
            var result = OrderDAO.EnterDeposit(orderId, depositAmount); // Call the method

            // Assert
            Assert.True(result); // Ensure the method returns true

            var updatedOrder = await dbContext.Orders.FindAsync(orderId); // Fetch the updated order

            Assert.NotNull(updatedOrder); // Ensure the order exists
            Assert.Equal(depositAmount, updatedOrder.Deposit); // Check if deposit is updated
            Assert.True(updatedOrder.StatusOrder); // Ensure StatusOrder is set to true
            Assert.Equal(updatedOrder.TotalAmount - depositAmount, updatedOrder.AmountPayable); // Check AmountPayable
        }

        [Fact]
        public async Task EnterDeposit_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData2();
            OrderDAO.InitializeContext(dbContext);

            int nonExistentOrderId = 9999; // Order that doesn't exist in the mock data
            decimal depositAmount = 100.00m;

            // Act
            var result = OrderDAO.EnterDeposit(nonExistentOrderId, depositAmount); // Call the method

            // Assert
            Assert.False(result); // Ensure the method returns false
        }


        // Unit test for DeleteOrder method
        private async Task<GreenGardenContext> GetDbContextWithMockDataForDelete()
        {
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            if (!await dbContext.Orders.AnyAsync())
            {
                dbContext.Orders.AddRange(
                    new Order { OrderId = 1, CustomerId = 1, TotalAmount = 100.00m },
                    new Order { OrderId = 2, CustomerId = 2, TotalAmount = 200.00m }
                );

                dbContext.OrderTicketDetails.AddRange(
                    new OrderTicketDetail { TicketId = 1, OrderId = 1 },
                    new OrderTicketDetail { TicketId = 2, OrderId = 1 }
                );

                dbContext.OrderFoodDetails.AddRange(
                    new OrderFoodDetail { ItemId = 1, OrderId = 1 },
                    new OrderFoodDetail { ItemId = 2, OrderId = 1 }
                );

                dbContext.OrderCampingGearDetails.AddRange(
                    new OrderCampingGearDetail { GearId = 1, OrderId = 1 }
                );

                dbContext.OrderFoodComboDetails.AddRange(
                    new OrderFoodComboDetail { ComboId = 1, OrderId = 1 }
                );

                await dbContext.SaveChangesAsync();
            }

            return dbContext;
        }

        [Fact]
        public async Task DeleteOrder_ShouldDeleteOrderAndRelatedDetails_WhenOrderExists()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockDataForDelete();
            OrderDAO.InitializeContext(dbContext);

            int orderIdToDelete = 1; // Existing order ID with related details.

            // Pre-Assert: Ensure the order and related details exist before deletion
            Assert.NotNull(dbContext.Orders.FirstOrDefault(o => o.OrderId == orderIdToDelete));
            Assert.NotEmpty(dbContext.OrderTicketDetails.Where(o => o.OrderId == orderIdToDelete));
            Assert.NotEmpty(dbContext.OrderFoodDetails.Where(o => o.OrderId == orderIdToDelete));
            Assert.NotEmpty(dbContext.OrderCampingGearDetails.Where(o => o.OrderId == orderIdToDelete));
            Assert.NotEmpty(dbContext.OrderFoodComboDetails.Where(o => o.OrderId == orderIdToDelete));

            // Act
            var result = OrderDAO.DeleteOrder(orderIdToDelete);

            // Assert: Check deletion result
            Assert.True(result);

            // Post-Assert: Verify order and related details are deleted
            Assert.Null(dbContext.Orders.FirstOrDefault(o => o.OrderId == orderIdToDelete));
            Assert.Empty(dbContext.OrderTicketDetails.Where(o => o.OrderId == orderIdToDelete));
            Assert.Empty(dbContext.OrderFoodDetails.Where(o => o.OrderId == orderIdToDelete));
            Assert.Empty(dbContext.OrderCampingGearDetails.Where(o => o.OrderId == orderIdToDelete));
            Assert.Empty(dbContext.OrderFoodComboDetails.Where(o => o.OrderId == orderIdToDelete));
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockDataForDelete();
            OrderDAO.InitializeContext(dbContext);

            int nonExistentOrderId = 9999; // Non-existent order ID.

            // Act
            var result = OrderDAO.DeleteOrder(nonExistentOrderId);

            // Assert
            Assert.False(result); // Ensure the deletion returns false for non-existent order.
        }

        // Unit test for CreateUniqueOrder method
        private async Task<GreenGardenContext> GetDbContextWithMockDataForCreateUniqueOrderTests()
        {
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            // Seed initial data
            if (!await dbContext.Tickets.AnyAsync())
            {
                dbContext.Roles.AddRange(
                    new Role { RoleId = 1, RoleName = "Admin" },
                    new Role { RoleId = 2, RoleName = "Customer" },
                    new Role { RoleId = 3, RoleName = "Employee" }
                );

                // Add Customers
                dbContext.Users.AddRange(
                    new User
                    {
                        UserId = 1,
                        FirstName = "Customer",
                        LastName = "One",
                        PhoneNumber = "123456789",
                        RoleId = 3
                    },
                    new User
                    {
                        UserId = 2,
                        FirstName = "Customer",
                        LastName = "Two",
                        PhoneNumber = "987654321",
                        RoleId = 3
                    }
                );

                // Add Tickets
                dbContext.Tickets.AddRange(
                    new Ticket { TicketId = 1, TicketName = "Standard Ticket", Price = 20.00m },
                    new Ticket { TicketId = 2, TicketName = "VIP Ticket", Price = 50.00m }
                );

                // Add Camping Gears
                dbContext.CampingGears.AddRange(
                    new CampingGear { GearId = 1, GearName = "Tent", RentalPrice = 30.00m },
                    new CampingGear { GearId = 2, GearName = "Sleeping Bag", RentalPrice = 15.00m }
                );

                // Add Food Items
                dbContext.FoodAndDrinks.AddRange(
                    new FoodAndDrink { ItemId = 1, ItemName = "Vegan Meal", Price = 10.00m },
                    new FoodAndDrink { ItemId = 2, ItemName = "Chicken Meal", Price = 15.00m }
                );

                // Add Food Combos
                dbContext.FoodCombos.AddRange(
                    new FoodCombo { ComboId = 1, ComboName = "Family Combo", Price = 25.00m },
                    new FoodCombo { ComboId = 2, ComboName = "Party Combo", Price = 40.00m }
                );

                await dbContext.SaveChangesAsync();
            }

            return dbContext;
        }

        [Fact]
        public async Task CreateUniqueOrder_ShouldCreateOrderWithAllDetails_WhenAllFieldsAreProvided()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData(); // Ensure this method sets up necessary test data
            OrderDAO.InitializeContext(dbContext);

            var orderRequest = new CreateUniqueOrderRequest
            {
                Order = new OrderAddDTO
                {
                    EmployeeId = 1,
                    CustomerName = "John Doe",
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    PhoneCustomer = "123456789"
                },
                OrderTicket = new List<OrderTicketAddlDTO>
        {
            new OrderTicketAddlDTO { TicketId = 1, Quantity = 2 },
            new OrderTicketAddlDTO { TicketId = 2, Quantity = 1 }
        },
                OrderCampingGear = new List<OrderCampingGearAddDTO>
        {
            new OrderCampingGearAddDTO { GearId = 1, Quantity = 3 }
        },
                OrderFood = new List<OrderFoodAddDTO>
        {
            new OrderFoodAddDTO { ItemId = 1, Quantity = 2, Description = "Vegan Meal" }
        },
                OrderFoodCombo = new List<OrderFoodComboAddDTO>
        {
            new OrderFoodComboAddDTO { ComboId = 1, Quantity = 1 }
        }
            };

            // Act
            var result = OrderDAO.CreateUniqueOrder(orderRequest);

            // Assert

            Assert.True(result); // Ensure the method returns true

            // Verify the order and associated details are added correctly
            var createdOrder = dbContext.Orders.FirstOrDefault(o => o.CustomerName == "John Doe");
            Assert.NotNull(createdOrder);
            Assert.Equal(50.00m, createdOrder.Deposit);
            Assert.Equal(150.00m, createdOrder.AmountPayable);

            var orderTickets = dbContext.OrderTicketDetails.Where(t => t.OrderId == createdOrder.OrderId).ToList();
            Assert.Equal(2, orderTickets.Count);
            Assert.Contains(orderTickets, t => t.TicketId == 1 && t.Quantity == 2);
            Assert.Contains(orderTickets, t => t.TicketId == 2 && t.Quantity == 1);

            var orderGears = dbContext.OrderCampingGearDetails.Where(g => g.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderGears);
            Assert.Contains(orderGears, g => g.GearId == 1 && g.Quantity == 3);

            var orderFoods = dbContext.OrderFoodDetails.Where(f => f.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderFoods);
            Assert.Contains(orderFoods, f => f.ItemId == 1 && f.Quantity == 2 && f.Description == "Vegan Meal");

            var orderCombos = dbContext.OrderFoodComboDetails.Where(c => c.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderCombos);
            Assert.Contains(orderCombos, c => c.ComboId == 1 && c.Quantity == 1);
        }

        [Fact]
        public async Task CreateUniqueOrder_ShouldReturnFalse_WhenOrderTicketIsNull()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            var orderRequest = new CreateUniqueOrderRequest
            {
                Order = new OrderAddDTO
                {
                    EmployeeId = 1,
                    CustomerName = "John Doe",
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    PhoneCustomer = "123456789"
                },
                OrderTicket = null, // Ticket is null
                OrderCampingGear = null,
                OrderFood = null,
                OrderFoodCombo = null
            };

            // Act
            var result = OrderDAO.CreateUniqueOrder(orderRequest);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateUniqueOrder_ShouldCreateOrder_WhenOnlyOrderTicketIsProvided()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            var orderRequest = new CreateUniqueOrderRequest
            {
                Order = new OrderAddDTO
                {
                    EmployeeId = 1,
                    CustomerName = "John Doe",
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    PhoneCustomer = "123456789"
                },
                OrderTicket = new List<OrderTicketAddlDTO>
        {
            new OrderTicketAddlDTO { TicketId = 1, Quantity = 2 }
        },
                OrderCampingGear = null,
                OrderFood = null,
                OrderFoodCombo = null
            };

            // Act
            var result = OrderDAO.CreateUniqueOrder(orderRequest);

            // Assert
            Assert.True(result);

            var createdOrder = dbContext.Orders.FirstOrDefault(o => o.CustomerName == "John Doe");
            Assert.NotNull(createdOrder);

            var orderTickets = dbContext.OrderTicketDetails.Where(t => t.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderTickets);
            Assert.Contains(orderTickets, t => t.TicketId == 1 && t.Quantity == 2);
        }

        [Fact]
        public async Task CreateUniqueOrder_ShouldCreateOrder_WhenOnlyOrderCampingGearIsNull()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            var orderRequest = new CreateUniqueOrderRequest
            {
                Order = new OrderAddDTO
                {
                    EmployeeId = 1,
                    CustomerName = "John Doe",
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    PhoneCustomer = "123456789"
                },
                OrderTicket = new List<OrderTicketAddlDTO>
        {
            new OrderTicketAddlDTO { TicketId = 1, Quantity = 2 }
        },
                OrderCampingGear = null, // Camping gear is null
                OrderFood = new List<OrderFoodAddDTO>
        {
            new OrderFoodAddDTO { ItemId = 1, Quantity = 2, Description = "Vegan Meal" }
        },
                OrderFoodCombo = new List<OrderFoodComboAddDTO>
        {
            new OrderFoodComboAddDTO { ComboId = 1, Quantity = 1 }
        }
            };

            // Act
            var result = OrderDAO.CreateUniqueOrder(orderRequest);

            // Assert
            Assert.True(result);

            var createdOrder = dbContext.Orders.FirstOrDefault(o => o.CustomerName == "John Doe");
            Assert.NotNull(createdOrder);

            var orderTickets = dbContext.OrderTicketDetails.Where(t => t.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderTickets);
            Assert.Contains(orderTickets, t => t.TicketId == 1 && t.Quantity == 2);

            var orderFoods = dbContext.OrderFoodDetails.Where(f => f.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderFoods);
            Assert.Contains(orderFoods, f => f.ItemId == 1 && f.Quantity == 2 && f.Description == "Vegan Meal");

            var orderCombos = dbContext.OrderFoodComboDetails.Where(c => c.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderCombos);
            Assert.Contains(orderCombos, c => c.ComboId == 1 && c.Quantity == 1);
        }

        [Fact]
        public async Task CreateUniqueOrder_ShouldCreateOrder_WhenOnlyOrderFoodIsNull()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            var orderRequest = new CreateUniqueOrderRequest
            {
                Order = new OrderAddDTO
                {
                    EmployeeId = 1,
                    CustomerName = "John Doe",
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    PhoneCustomer = "123456789"
                },
                OrderTicket = new List<OrderTicketAddlDTO>
        {
            new OrderTicketAddlDTO { TicketId = 1, Quantity = 2 }
        },
                OrderCampingGear = new List<OrderCampingGearAddDTO>
        {
            new OrderCampingGearAddDTO { GearId = 1, Quantity = 3 }
        },
                OrderFood = null, // Food is null
                OrderFoodCombo = new List<OrderFoodComboAddDTO>
        {
            new OrderFoodComboAddDTO { ComboId = 1, Quantity = 1 }
        }
            };

            // Act
            var result = OrderDAO.CreateUniqueOrder(orderRequest);

            // Assert
            Assert.True(result);

            var createdOrder = dbContext.Orders.FirstOrDefault(o => o.CustomerName == "John Doe");
            Assert.NotNull(createdOrder);

            var orderTickets = dbContext.OrderTicketDetails.Where(t => t.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderTickets);
            Assert.Contains(orderTickets, t => t.TicketId == 1 && t.Quantity == 2);

            var orderGears = dbContext.OrderCampingGearDetails.Where(g => g.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderGears);
            Assert.Contains(orderGears, g => g.GearId == 1 && g.Quantity == 3);

            var orderCombos = dbContext.OrderFoodComboDetails.Where(c => c.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderCombos);
            Assert.Contains(orderCombos, c => c.ComboId == 1 && c.Quantity == 1);
        }

        [Fact]
        public async Task CreateUniqueOrder_ShouldCreateOrder_WhenOnlyOrderFoodComboIsNull()
        {
            // Arrange
            var dbContext = await GetDbContextWithMockData();
            OrderDAO.InitializeContext(dbContext);

            var orderRequest = new CreateUniqueOrderRequest
            {
                Order = new OrderAddDTO
                {
                    EmployeeId = 1,
                    CustomerName = "John Doe",
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    PhoneCustomer = "123456789"
                },
                OrderTicket = new List<OrderTicketAddlDTO>
        {
            new OrderTicketAddlDTO { TicketId = 1, Quantity = 2 }
        },
                OrderCampingGear = new List<OrderCampingGearAddDTO>
        {
            new OrderCampingGearAddDTO { GearId = 1, Quantity = 3 }
        },
                OrderFood = new List<OrderFoodAddDTO>
        {
            new OrderFoodAddDTO { ItemId = 1, Quantity = 2, Description = "Vegan Meal" }
        },
                OrderFoodCombo = null // Food combo is null
            };

            // Act
            var result = OrderDAO.CreateUniqueOrder(orderRequest);

            // Assert
            Assert.True(result);

            var createdOrder = dbContext.Orders.FirstOrDefault(o => o.CustomerName == "John Doe");
            Assert.NotNull(createdOrder);

            var orderTickets = dbContext.OrderTicketDetails.Where(t => t.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderTickets);
            Assert.Contains(orderTickets, t => t.TicketId == 1 && t.Quantity == 2);

            var orderGears = dbContext.OrderCampingGearDetails.Where(g => g.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderGears);
            Assert.Contains(orderGears, g => g.GearId == 1 && g.Quantity == 3);

            var orderFoods = dbContext.OrderFoodDetails.Where(f => f.OrderId == createdOrder.OrderId).ToList();
            Assert.Single(orderFoods);
            Assert.Contains(orderFoods, f => f.ItemId == 1 && f.Quantity == 2 && f.Description == "Vegan Meal");
        }

        // Unit test for GetOrderDetail method
        [Fact]
        public async Task GetOrderDetail_ShouldReturnOrderDetails_WhenOrderIdExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var dbContext = new GreenGardenContext(options))
            {
                // Seed mock data
                dbContext.Roles.Add(new Role { RoleId = 3, RoleName = "Employee" });
                dbContext.Users.Add(new User
                {
                    UserId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "customer1@example.com",  // Add Email
                    Password = "hashed_password1",  // Add Password
                    RoleId = 3
                });

                dbContext.Tickets.AddRange(
                    new Ticket { TicketId = 1, TicketName = "VIP Ticket", Price = 50.00m }
                );

                dbContext.CampingGears.Add(new CampingGear { GearId = 1, GearName = "Tent", RentalPrice = 30.00m });

                dbContext.FoodAndDrinks.Add(new FoodAndDrink { ItemId = 1, ItemName = "Vegan Meal", Price = 15.00m });

                dbContext.FoodCombos.Add(new FoodCombo { ComboId = 1, ComboName = "Family Combo", Price = 25.00m });

                var order = new Order
                {
                    OrderId = 100,
                    EmployeeId = 1,
                    CustomerName = "Jane Doe",
                    OrderDate = DateTime.UtcNow,
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    AmountPayable = 150.00m,
                    StatusOrder = true,
                    ActivityId = 1,
                    PhoneCustomer = "123456789"
                };
                dbContext.Orders.Add(order);

                dbContext.OrderTicketDetails.Add(new OrderTicketDetail
                {
                    OrderId = 100,
                    TicketId = 1,
                    Quantity = 2
                });

                dbContext.OrderCampingGearDetails.Add(new OrderCampingGearDetail
                {
                    OrderId = 100,
                    GearId = 1,
                    Quantity = 3
                });

                dbContext.OrderFoodDetails.Add(new OrderFoodDetail
                {
                    OrderId = 100,
                    ItemId = 1,
                    Quantity = 2
                });

                dbContext.OrderFoodComboDetails.Add(new OrderFoodComboDetail
                {
                    OrderId = 100,
                    ComboId = 1,
                    Quantity = 1
                });

                await dbContext.SaveChangesAsync();

                // Check if the data is saved correctly
                var orderInDb = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == 100);
                Assert.NotNull(orderInDb); // Verify that the order exists in the DB

                var ticketDetailsInDb = await dbContext.OrderTicketDetails.FirstOrDefaultAsync(t => t.OrderId == 100);
                Assert.NotNull(ticketDetailsInDb); // Verify ticket details exist

                var campingGearDetailsInDb = await dbContext.OrderCampingGearDetails.FirstOrDefaultAsync(g => g.OrderId == 100);
                Assert.NotNull(campingGearDetailsInDb); // Verify camping gear details exist

                var foodDetailsInDb = await dbContext.OrderFoodDetails.FirstOrDefaultAsync(f => f.OrderId == 100);
                Assert.NotNull(foodDetailsInDb); // Verify food details exist

                var foodComboDetailsInDb = await dbContext.OrderFoodComboDetails.FirstOrDefaultAsync(c => c.OrderId == 100);
                Assert.NotNull(foodComboDetailsInDb); // Verify food combo details exist



                OrderDAO.InitializeContext(dbContext);

                // Act
                var result = OrderDAO.GetOrderDetail(100);

                // Assert
                Assert.NotNull(result);  // Ensure the result is not null
                Assert.Equal(100, result.OrderId);  // Assert Order ID
                Assert.Equal("John Doe", result.EmployeeName);  // Correct employee name
                Assert.Equal("Jane Doe", result.CustomerName);  // Correct customer name
                Assert.Equal(50.00m, result.Deposit);  // Assert Deposit
                Assert.Equal(150.00m, result.AmountPayable);  // Assert Amount Payable
                Assert.Equal(1, result.OrderTicketDetails.Count);  // Check the number of tickets
                Assert.Equal(1, result.OrderCampingGearDetails.Count);  // Check the number of camping gear
                Assert.Equal(1, result.OrderFoodDetails.Count);  // Check the number of food items
                Assert.Equal(1, result.OrderFoodComboDetails.Count);  // Check the number of food combos

                // Verify ticket details
                var ticket = result.OrderTicketDetails.First();
                Assert.Equal(1, ticket.TicketId);
                Assert.Equal("VIP Ticket", ticket.Name);
                Assert.Equal(2, ticket.Quantity);
                Assert.Equal(50.00m, ticket.Price);

                // Verify camping gear details
                var campingGear = result.OrderCampingGearDetails.First();
                Assert.Equal(1, campingGear.GearId);
                Assert.Equal("Tent", campingGear.Name);
                Assert.Equal(3, campingGear.Quantity);
                Assert.Equal(30.00m, campingGear.Price);

                // Verify food details
                var food = result.OrderFoodDetails.First();
                Assert.Equal(1, food.ItemId);
                Assert.Equal("Vegan Meal", food.Name);
                Assert.Equal(2, food.Quantity);
                Assert.Equal(15.00m, food.Price);

                // Verify food combo details
                var combo = result.OrderFoodComboDetails.First();
                Assert.Equal(1, combo.ComboId);
                Assert.Equal("Family Combo", combo.Name);
                Assert.Equal(1, combo.Quantity);
                Assert.Equal(25.00m, combo.Price);
            }
        }

        [Fact]
        public void GetOrderDetail_ShouldReturnNull_WhenOrderIdDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var dbContext = new GreenGardenContext(options))
            {
                dbContext.Database.EnsureCreated();
            }

            OrderDAO.InitializeContext(new GreenGardenContext(options));

            // Act
            var result = OrderDAO.GetOrderDetail(999); // Non-existent ID

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrderDetail_ShouldThrowException_WhenDatabaseThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new GreenGardenContext(options);
            dbContext.Database.EnsureCreated();

            OrderDAO.InitializeContext(null);

            // Act & Assert
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(OrderDAO.GetOrderDetail(1)));
            exception.Message.Should().Be("Object reference not set to an instance of an object.");

        }

        // Unit test for GetCustomerOrderDetail method
        [Fact]
        public async Task GetCustomerOrderDetail_ShouldReturnCorrectOrderDetails_WhenOrderIdExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var dbContext = new GreenGardenContext(options))
            {
                // Seed mock data
                dbContext.Roles.Add(new Role { RoleId = 3, RoleName = "Employee" });
                dbContext.Users.Add(new User
                {
                    UserId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "customer1@example.com",  // Add Email
                    Password = "hashed_password1",  // Add Password
                    RoleId = 3
                });

                dbContext.Tickets.AddRange(
                    new Ticket { TicketId = 1, TicketName = "VIP Ticket", Price = 50.00m }
                );

                dbContext.CampingGears.Add(new CampingGear { GearId = 1, GearName = "Tent", RentalPrice = 30.00m });

                dbContext.FoodAndDrinks.Add(new FoodAndDrink { ItemId = 1, ItemName = "Vegan Meal", Price = 15.00m });

                dbContext.FoodCombos.Add(new FoodCombo { ComboId = 1, ComboName = "Family Combo", Price = 25.00m });

                var order = new Order
                {
                    OrderId = 100,
                    EmployeeId = 1,
                    CustomerName = "Jane Doe",
                    OrderDate = DateTime.UtcNow,
                    OrderUsageDate = DateTime.UtcNow.AddDays(5),
                    Deposit = 50.00m,
                    TotalAmount = 200.00m,
                    AmountPayable = 150.00m,
                    StatusOrder = true,
                    ActivityId = 1,
                    PhoneCustomer = "123456789"
                };
                dbContext.Orders.Add(order);

                dbContext.OrderTicketDetails.Add(new OrderTicketDetail
                {
                    OrderId = 100,
                    TicketId = 1,
                    Quantity = 2
                });

                dbContext.OrderCampingGearDetails.Add(new OrderCampingGearDetail
                {
                    OrderId = 100,
                    GearId = 1,
                    Quantity = 3
                });

                dbContext.OrderFoodDetails.Add(new OrderFoodDetail
                {
                    OrderId = 100,
                    ItemId = 1,
                    Quantity = 2
                });

                dbContext.OrderFoodComboDetails.Add(new OrderFoodComboDetail
                {
                    OrderId = 100,
                    ComboId = 1,
                    Quantity = 1
                });

                dbContext.OrderComboDetails.Add(new OrderComboDetail
                {
                    OrderId = 100,
                    ComboId = 1,
                    Quantity = 1
                });

                await dbContext.SaveChangesAsync();
            }

            // Act
            CustomerOrderDetailDTO result;
            using (var dbContext = new GreenGardenContext(options))
            {
                OrderDAO.InitializeContext(dbContext);  // Assuming InitializeContext is necessary

                result = OrderDAO.GetCustomerOrderDetail(100); // Get order details by ID
            }

            // Assert
            Assert.NotNull(result);  // Ensure the result is not null
            Assert.Equal(100, result.OrderId);  // Assert Order ID
            Assert.Equal("Jane Doe", result.CustomerName);  // Assert Customer Name
            Assert.Equal("123456789", result.PhoneCustomer);  // Assert Phone Number
            Assert.Equal(50.00m, result.Deposit);  // Assert Deposit
            Assert.Equal(200.00m, result.TotalAmount);  // Assert Total Amount
            Assert.Equal(150.00m, result.AmountPayable);  // Assert Amount Payable
            Assert.True(result.StatusOrder.HasValue);  // Assert StatusOrder is not null
            Assert.Equal(1, result.ActivityId);  // Assert ActivityId
            Assert.Equal("Activity Name", result.ActivityName);  // Assert ActivityName (assuming it's set in the mock)

            // Verify related data
            Assert.Single(result.OrderTicketDetails);  // Ensure there is one ticket detail
            var ticketDetail = result.OrderTicketDetails.First();
            Assert.Equal(1, ticketDetail.TicketId);
            Assert.Equal("VIP Ticket", ticketDetail.Name);
            Assert.Equal(2, ticketDetail.Quantity);
            Assert.Equal(50.00m, ticketDetail.Price);

            Assert.Single(result.OrderCampingGearDetails);  // Ensure there is one camping gear detail
            var gearDetail = result.OrderCampingGearDetails.First();
            Assert.Equal(1, gearDetail.GearId);
            Assert.Equal("Tent", gearDetail.Name);
            Assert.Equal(3, gearDetail.Quantity);
            Assert.Equal(30.00m, gearDetail.Price);

            Assert.Single(result.OrderFoodDetails);  // Ensure there is one food detail
            var foodDetail = result.OrderFoodDetails.First();
            Assert.Equal(1, foodDetail.ItemId);
            Assert.Equal("Vegan Meal", foodDetail.Name);
            Assert.Equal(2, foodDetail.Quantity);
            Assert.Equal(15.00m, foodDetail.Price);

            Assert.Single(result.OrderFoodComboDetails);  // Ensure there is one food combo detail
            var foodComboDetail = result.OrderFoodComboDetails.First();
            Assert.Equal(1, foodComboDetail.ComboId);
            Assert.Equal("Family Combo", foodComboDetail.Name);
            Assert.Equal(1, foodComboDetail.Quantity);
            Assert.Equal(25.00m, foodComboDetail.Price);

            Assert.Single(result.OrderComboDetails);  // Ensure there is one combo detail
            var comboDetail = result.OrderComboDetails.First();
            Assert.Equal(1, comboDetail.ComboId);
            Assert.Equal("Family Combo", comboDetail.Name);
            Assert.Equal(1, comboDetail.Quantity);
            Assert.Equal(25.00m, comboDetail.Price);
        }

    }

}
