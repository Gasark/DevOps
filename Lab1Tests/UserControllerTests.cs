using Lab1.Controllers;
using Lab1.Data;
using Lab1.DTOs;
using Lab1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1Tests
{
    [TestClass]
    public class UsersControllerTests
    {
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") 
                .Options;

            return new ApplicationDbContext(options);
        }

        [TestInitialize]
        public async Task SeedDatabase()
        {
            using var context = CreateInMemoryDbContext();
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Id = 1, Name = "John Doe", Email = "user1@example.com" },
                    new User { Id = 2, Name = "Mark Murphy", Email = "Murphy@example.com" },
                    new User { Id = 3, Name = "Edward Roberts", Email = "Roberts@example.com" },
                    new User { Id = 4, Name = "Mike Lawrences", Email = "Lawrences@example.com" },
                    new User { Id = 5, Name = "Francis Wright", Email = "Wright1@example.com" },
                    new User { Id = 7, Name = "David", Email = "Dav@gmail.com" }
                );
                await context.SaveChangesAsync();
            }
        }

        [TestMethod]
        public async Task GetAllUsers_ReturnsListOfUsers()
        {
            using var context = CreateInMemoryDbContext();
            await SeedDatabase();

            var controller = new UsersController(context);

            var result = await controller.GetAllUsers();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var users = okResult.Value as List<UserDto>;
            Assert.IsNotNull(users);
            Assert.AreEqual(6, users.Count); // Має повернутися 6 користувачів із бази
        }

        [TestMethod]
        public async Task GetUserById_ReturnsCorrectUser()
        {
            using var context = CreateInMemoryDbContext();
            await SeedDatabase();

            var controller = new UsersController(context);

            var result = await controller.GetUserById(2); 

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var user = okResult.Value as UserDto;
            Assert.IsNotNull(user);
            Assert.AreEqual(2, user.Id);
            Assert.AreEqual("Mark Murphy", user.Name);
        }

        [TestMethod]
        public async Task CreateUser_AddsUserToDatabase()
        {
            using var context = CreateInMemoryDbContext();
            await SeedDatabase();

            var controller = new UsersController(context);

            var newUser = new UserDto { Name = "John Smith", Email = "john.smith@example.com" };

            var result = await controller.CreateUser(newUser);

            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);

            var user = await context.Users.FirstOrDefaultAsync(u => u.Name == "John Smith");
            Assert.IsNotNull(user);
            Assert.AreEqual("john.smith@example.com", user.Email);
        }

        [TestMethod]
        public async Task UpdateUser_UpdatesUserDetails()
        {
            using var context = CreateInMemoryDbContext();
            await SeedDatabase();

            var controller = new UsersController(context);

            var updatedUser = new UserDto { Id = 3, Name = "Edward Updated", Email = "updated@example.com" };

            var result = await controller.UpdateUser(3, updatedUser);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var user = await context.Users.FindAsync(3);
            Assert.IsNotNull(user);
            Assert.AreEqual("Edward Updated", user.Name);
            Assert.AreEqual("updated@example.com", user.Email);
        }

        [TestMethod]
        public async Task DeleteUser_RemovesUserFromDatabase()
        {
            using var context = CreateInMemoryDbContext();
            await SeedDatabase();

            var controller = new UsersController(context);

            var result = await controller.DeleteUser(4); 

            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var user = await context.Users.FindAsync(4);
            Assert.IsNull(user);
        }
    }
}
