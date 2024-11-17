﻿using BusinessObject.DTOs;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenGardenAPITest.DAO
{
    public class AccountDAOTest
    {
        private async Task<GreenGardenContext> GetDbContext() // Create a database in memory with mock data.
        {
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique name for each test
                .Options;

            var databaseContext = new GreenGardenContext(options);
            databaseContext.Database.EnsureCreated();

            // Clear the change tracker to ensure no duplicates
            databaseContext.ChangeTracker.Clear();
            // Seed data for Tickets
            if (!await databaseContext.Users.AnyAsync()) // Only add data if the table is empty
            {
                databaseContext.Users.AddRange(
                    new User
                    {
                        UserId = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@example.com",
                        Password = "password123",
                        PhoneNumber = "1234567890",
                        Address = "123 Elm Street",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gender = "Male",
                        ProfilePictureUrl = "http://example.com/john.jpg",
                        IsActive = true,
                        CreatedAt = DateTime.Now.AddDays(-30),
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = 2,
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane.smith@example.com",
                        Password = "securepassword",
                        PhoneNumber = "0987654321",
                        Address = "456 Oak Avenue",
                        DateOfBirth = new DateTime(1995, 5, 15),
                        Gender = "Female",
                        ProfilePictureUrl = "http://example.com/jane.jpg",
                        IsActive = false,
                        CreatedAt = DateTime.Now.AddDays(-15),
                        RoleId = 2
                    }
                    );
                await databaseContext.SaveChangesAsync();
            }

            return databaseContext;
        }

        // Test for method GetAllAccounts
        [Fact]
        public async Task GetAllAccounts_ShouldReturnAllUsers()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            // Act
            var users = AccountDAO.GetAllAccounts();

            // Assert
            Assert.NotNull(users);
            Assert.Equal(2, users.Count); // Expecting 2 users
            Assert.Contains(users, u => u.FirstName == "John" && u.Email == "john.doe@example.com");
            Assert.Contains(users, u => u.FirstName == "Jane" && u.Email == "jane.smith@example.com");
        }

        [Fact]
        public async Task GetAllAccounts_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                 .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique name for each test
                 .Options;

            var emptyContext = new GreenGardenContext(options);
            emptyContext.Database.EnsureCreated();
            AccountDAO.InitializeContext(emptyContext);

            // Act
            var users = AccountDAO.GetAllAccounts();

            // Assert
            Assert.NotNull(users); // Ensure the method doesn't return null
            Assert.Empty(users);   // Check that the list is empty
        }

        [Fact]
        public async Task GetAllAccounts_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<GreenGardenContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new GreenGardenContext(options);
            databaseContext.Database.EnsureCreated();

            // Simulate an exception scenario by setting the context to null or failing in some way
            ActivityDAO.InitializeContext(null);  // Provide null context to force an exception

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(AccountDAO.GetAllAccounts()));

        }

        // Test for method GetAccountById
        [Fact]
        public async Task GetAccountById_ShouldReturnUser_WhenUserIdIsValid()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            // Act
            var result = AccountDAO.GetAccountById(1); // Assuming 1 is a valid user ID from the seed data

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal("John", result.FirstName); // Match the seeded data
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john.doe@example.com", result.Email);
        }

        [Fact]
        public async Task GetAccountById_ShouldReturnNull_WhenUserIdIsInvalid()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            // Act
            var result = AccountDAO.GetAccountById(999); // Assuming 999 is not a valid user ID

            // Assert
            Assert.Null(result);
        }

        // Test for method UpdateProfile
        [Fact]
        public async Task UpdateProfile_ShouldReturnSuccessMessage_WhenUserExists()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 1,
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Email = "updated.email@example.com",
                PhoneNumber = "1112223333",
                Address = "Updated Address",
                DateOfBirth = new DateTime(1992, 2, 20),
                Gender = "Male",
                ProfilePictureUrl = "http://example.com/updated-profile.jpg"
            };

            // Act
            var result = await AccountDAO.UpdateProfile(updateProfileDto);

            // Assert
            Assert.Equal("Cập nhật thông tin thành công.", result);

            var updatedUser = await dbContext.Users.SingleOrDefaultAsync(u => u.UserId == 1);
            Assert.NotNull(updatedUser);
            Assert.Equal("UpdatedFirstName", updatedUser.FirstName);
            Assert.Equal("UpdatedLastName", updatedUser.LastName);
            Assert.Equal("updated.email@example.com", updatedUser.Email);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnErrorMessage_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 999, // Non-existent UserId
                FirstName = "NonExistent",
                LastName = "User",
                Email = "nonexistent.user@example.com",
                PhoneNumber = "0000000000",
                Address = "No Address",
                DateOfBirth = new DateTime(2000, 1, 1),
                Gender = "Unknown",
                ProfilePictureUrl = "http://example.com/nonexistent.jpg"
            };

            // Act
            var result = await AccountDAO.UpdateProfile(updateProfileDto);

            // Assert
            Assert.Equal("Người dùng không tồn tại.", result);
        }

        [Fact]
        public async Task UpdateProfile_ShouldThrowException_WhenDatabaseUpdateFails()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 1,
                FirstName = "FaultyUpdate",
                LastName = "Test",
                Email = "faulty.update@example.com",
                PhoneNumber = "9999999999",
                Address = "Faulty Address",
                DateOfBirth = new DateTime(1995, 5, 5),
                Gender = "Other",
                ProfilePictureUrl = "http://example.com/faulty.jpg"
            };

            var mockContext = new Mock<GreenGardenContext>();
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new DbUpdateException("Simulated database update failure."));

            AccountDAO.InitializeContext(mockContext.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => AccountDAO.UpdateProfile(updateProfileDto));
            Assert.Contains("Đã xảy ra lỗi khi cập nhật thông tin:", exception.Message);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnError_WhenMandatoryFieldsAreNull()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 1, // Valid user
                FirstName = null, // Mandatory field
                LastName = null,  // Mandatory field
                Email = null,     // Mandatory field
                PhoneNumber = "1112223333",
                Address = "Updated Address",
                DateOfBirth = new DateTime(1992, 2, 20),
                Gender = "Male",
                ProfilePictureUrl = "http://example.com/updated-profile.jpg"
            };

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => AccountDAO.UpdateProfile(updateProfileDto));

            // Assert
            Assert.Contains("FirstName, LastName, and Email cannot be null or empty.", exception.Message);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnError_WhenFirstNameIsNull()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 1, // Valid user
                FirstName = null, // Mandatory field is null
                LastName = "UpdatedLastName",
                Email = "updated.email@example.com",
                PhoneNumber = "1112223333",
                Address = "Updated Address",
                DateOfBirth = new DateTime(1992, 2, 20),
                Gender = "Male",
                ProfilePictureUrl = "http://example.com/updated-profile.jpg"
            };

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => AccountDAO.UpdateProfile(updateProfileDto));

            // Assert
            Assert.Contains("FirstName cannot be null or empty.", exception.Message);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnError_WhenLastNameIsNull()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 1, // Valid user
                FirstName = "UpdatedFirstName",
                LastName = null, // Mandatory field is null
                Email = "updated.email@example.com",
                PhoneNumber = "1112223333",
                Address = "Updated Address",
                DateOfBirth = new DateTime(1992, 2, 20),
                Gender = "Male",
                ProfilePictureUrl = "http://example.com/updated-profile.jpg"
            };

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => AccountDAO.UpdateProfile(updateProfileDto));

            // Assert
            Assert.Contains("LastName cannot be null or empty.", exception.Message);
        }

        [Fact]
        public async Task UpdateProfile_ShouldReturnError_WhenEmailIsNull()
        {
            // Arrange
            var dbContext = await GetDbContext();
            AccountDAO.InitializeContext(dbContext);

            var updateProfileDto = new UpdateProfile
            {
                UserId = 1, // Valid user
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Email = null, // Mandatory field is null
                PhoneNumber = "1112223333",
                Address = "Updated Address",
                DateOfBirth = new DateTime(1992, 2, 20),
                Gender = "Male",
                ProfilePictureUrl = "http://example.com/updated-profile.jpg"
            };

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => AccountDAO.UpdateProfile(updateProfileDto));

            // Assert
            Assert.Contains("Email cannot be null or empty.", exception.Message);
        }






    }
}
