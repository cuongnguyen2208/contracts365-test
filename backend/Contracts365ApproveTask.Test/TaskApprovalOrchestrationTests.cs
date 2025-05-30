using Contracts365ApproveTask.Dtos;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Contracts365ApproveTask.Tests;

public class TaskApprovalOrchestrationTests
{
    [Fact]
    public async Task RunOrchestrator_ApproveEvent_ReturnsApproved()
    {
        // Arrange
        var context = new Mock<IDurableOrchestrationContext>();
        context.Setup(c => c.GetInput<string>()).Returns("cuong@abc.com");
        context.Setup(c => c.WaitForExternalEvent<string>("ApprovalEvent")).ReturnsAsync("Approve");
        context.Setup(c => c.CallActivityAsync<string>("SendEmail", It.IsAny<EmailRequest>())).ReturnsAsync(string.Empty);

        // Act
        var result = await TaskApprovalOrchestration.RunOrchestrator(context.Object);

        // Assert
        Assert.Equal("Approved", result);
        context.Verify(c => c.CallActivityAsync("SendEmail",
            It.Is<EmailRequest>(e => e.Email == "cuong@abc.com" && e.Subject.Contains("completed"))),
            Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_RejectEvent_ReturnsRejected()
    {
        // Arrange
        var context = new Mock<IDurableOrchestrationContext>();
        context.Setup(c => c.GetInput<string>()).Returns("cuong@abc.com");
        context.Setup(c => c.WaitForExternalEvent<string>("ApprovalEvent")).ReturnsAsync("Reject");
        context.Setup(c => c.CallActivityAsync<string>("SendEmail", It.IsAny<EmailRequest>())).ReturnsAsync(string.Empty);

        // Act
        var result = await TaskApprovalOrchestration.RunOrchestrator(context.Object);

        // Assert
        Assert.Equal("Rejected", result);
        context.Verify(c => c.CallActivityAsync("SendEmail",
            It.Is<EmailRequest>(e => e.Email == "cuong@abc.com" && e.Subject.Contains("rejected"))),
            Times.Once);
    }

    [Fact]
    public async Task RunOrchestrator_NullEmail_ThrowsArgumentException()
    {
        // Arrange
        var context = new Mock<IDurableOrchestrationContext>();
        context.Setup(c => c.GetInput<string?>()).Returns((string?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => TaskApprovalOrchestration.RunOrchestrator(context.Object));
    }

    [Fact]
    public async Task RunOrchestrator_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var context = new Mock<IDurableOrchestrationContext>();
        context.Setup(c => c.GetInput<string>()).Returns("cuong@abc.com");
        context.Setup(c => c.WaitForExternalEvent<string>("ApprovalEvent")).ReturnsAsync("Invalid");
        context.Setup(c => c.CallActivityAsync("SendEmail", It.IsAny<EmailRequest>())).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => TaskApprovalOrchestration.RunOrchestrator(context.Object));
    }

    [Fact]
    public async Task SendEmail_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var request = new EmailRequest
        {
            Email = "cuong@abc.com",
            Subject = "Test Subject",
            Content = "Test Content"
        };

        // Act
        var result = await TaskApprovalOrchestration.SendEmail(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendEmail_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => TaskApprovalOrchestration.SendEmail(null));
    }

    [Fact]
    public async Task StartApproval_ValidRequest_ReturnsOkResponse()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ApprovalRequest { UserEmail = "cuong@abc.com" }),
                Encoding.UTF8,
                "application/json")
        };

        var client = new Mock<IDurableOrchestrationClient>();
        client.Setup(c => c.StartNewAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        // Act
        var response = await TaskApprovalOrchestration.StartApproval(request, client.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        client.Verify(c => c.StartNewAsync("TaskApprovalOrchestration",
            It.IsAny<string>(), "cuong@abc.com"), Times.Once);
    }

    [Fact]
    public async Task StartApproval_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ApprovalRequest { UserEmail = "" }),
                Encoding.UTF8,
                "application/json")
        };

        var client = new Mock<IDurableOrchestrationClient>();

        // Act
        var response = await TaskApprovalOrchestration.StartApproval(request, client.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ValidRequest_ReturnsOkResponse()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ApprovalRequest { InstanceId = "test-instance" }),
                Encoding.UTF8,
                "application/json")
        };

        var client = new Mock<IDurableOrchestrationClient>();
        client.Setup(c => c.RaiseEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await TaskApprovalOrchestration.Approve(request, client.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        client.Verify(c => c.RaiseEventAsync("test-instance", "ApprovalEvent", "Approve"), Times.Once);
    }

    [Fact]
    public async Task Approve_EmptyInstanceId_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ApprovalRequest { InstanceId = "" }),
                Encoding.UTF8,
                "application/json")
        };

        var client = new Mock<IDurableOrchestrationClient>();

        // Act
        var response = await TaskApprovalOrchestration.Approve(request, client.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reject_ValidRequest_ReturnsOkResponse()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ApprovalRequest { InstanceId = "test-instance" }),
                Encoding.UTF8,
                "application/json")
        };

        var client = new Mock<IDurableOrchestrationClient>();
        client.Setup(c => c.RaiseEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await TaskApprovalOrchestration.Reject(request, client.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        client.Verify(c => c.RaiseEventAsync("test-instance", "ApprovalEvent", "Reject"), Times.Once);
    }

    [Fact]
    public async Task Reject_EmptyInstanceId_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ApprovalRequest { InstanceId = "" }),
                Encoding.UTF8,
                "application/json")
        };

        var client = new Mock<IDurableOrchestrationClient>();

        // Act
        var response = await TaskApprovalOrchestration.Reject(request, client.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RunOrchestrator_InvalidEmailFormat_ThrowsArgumentException()
    {
        // Arrange
        var context = new Mock<IDurableOrchestrationContext>();
        context.Setup(c => c.GetInput<string>()).Returns("invalid-email");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => TaskApprovalOrchestration.RunOrchestrator(context.Object));
        Assert.Contains("Invalid email format", exception.Message);
    }
}
