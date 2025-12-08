using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Web.Services;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models.Auth;
using SchoolManagementSystem.Web.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SchoolManagementSystem.Tests
{
    public class MessageServiceTests
    {
        private DbContextOptions<SchoolDbContext> _options;
        private Mock<ILogger<MessageService>> _mockLogger;

        public MessageServiceTests()
        {
            _options = new DbContextOptionsBuilder<SchoolDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<MessageService>>();
        }

        private SchoolDbContext CreateContext()
        {
            return new SchoolDbContext(_options);
        }

        [Fact]
        public async Task SendMessageAsync_SendsMessage_WhenReceiverExists()
        {
            using (var context = CreateContext())
            {
                context.Users.Add(new User { Id = "user2", Email = "receiver@test.com", FirstName="R", LastName="R" });
                await context.SaveChangesAsync();

                var service = new MessageService(context, _mockLogger.Object);
                await service.SendMessageAsync("user1", "receiver@test.com", "Test Subject", "Test Content");
            }

            using (var context = CreateContext())
            {
                var msg = await context.Messages.FirstOrDefaultAsync();
                Assert.NotNull(msg);
                Assert.Equal("user1", msg.SenderId);
                Assert.Equal("user2", msg.ReceiverId);
                Assert.Equal("Test Subject", msg.Subject);
            }
        }
    }
}
