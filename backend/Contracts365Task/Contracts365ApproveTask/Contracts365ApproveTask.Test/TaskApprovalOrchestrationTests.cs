using Contracts365ApproveTask.Constants;
using Contracts365ApproveTask.Dtos;
using Contracts365ApproveTask.Exceptions;
using Contracts365ApproveTask.Interfaces;
using Contracts365ApproveTask.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Contracts365ApproveTask.Tests
{
    /// <summary>
    /// Unit tests for the TaskApprovalOrchestration class.
    /// These tests verify the functionality of the approval workflow process.
    /// </summary>
    public class TaskApprovalOrchestrationTests
    {
        private readonly Mock<ILogger<TaskApprovalOrchestration>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpRequestReader> _requestReaderMock;
        private readonly Mock<IHttpResponseWriter> _responseWriterMock;
        private readonly TaskApprovalOrchestration _orchestration;

        /// <summary>
        /// Initializes a new instance of the TaskApprovalOrchestrationTests class.
        /// Sets up all the required mocks for dependency injection.
        /// </summary>
        public TaskApprovalOrchestrationTests()
        {
            _loggerMock = new Mock<ILogger<TaskApprovalOrchestration>>();
            _emailServiceMock = new Mock<IEmailService>();
            _requestReaderMock = new Mock<IHttpRequestReader>();
            _responseWriterMock = new Mock<IHttpResponseWriter>();

            _orchestration = new TaskApprovalOrchestration(
                _loggerMock.Object,
                _emailServiceMock.Object,
                _requestReaderMock.Object,
                _responseWriterMock.Object);
        }

        #region StartApproval Tests

        /// <summary>
        /// Tests that StartApproval returns OK status and properly schedules a new orchestration
        /// when provided with a valid email address.
        /// </summary>
        [Fact]
        public async Task StartApproval_ValidEmail_ReturnsOkWithResponse()
        {
            // Arrange
            var userEmail = "test@example.com";
            var request = new ApprovalRequest { UserEmail = userEmail };
            var instanceId = Guid.NewGuid().ToString();

            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var mockResponse = new Mock<HttpResponseData>(new Mock<FunctionContext>().Object);
            mockResponse.SetupProperty(r => r.StatusCode);
            mockResponse.Object.StatusCode = HttpStatusCode.OK;

            _responseWriterMock
                .Setup(w => w.CreateResponse(mockRequest.Object, HttpStatusCode.OK))
                .Returns(mockResponse.Object);

            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");
            mockDurableClient
                .Setup(c => c.ScheduleNewOrchestrationInstanceAsync(
                    It.IsAny<TaskName>(),
                    It.Is<string>(s => s == userEmail),
                    It.IsAny<StartOrchestrationOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(instanceId);

            // Act
            var response = await _orchestration.StartApproval(mockRequest.Object, mockDurableClient.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify mocks were called correctly
            _requestReaderMock.Verify(r => r.ReadFromJsonAsync<ApprovalRequest>(
                mockRequest.Object, It.IsAny<CancellationToken>()), Times.Once);

            _responseWriterMock.Verify(w => w.CreateResponse(
                mockRequest.Object, HttpStatusCode.OK), Times.Once);

            _responseWriterMock.Verify(w => w.WriteAsJsonAsync(
                response, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

            mockDurableClient.Verify(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                userEmail,
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that StartApproval throws InvalidInputException when the email is empty.
        /// </summary>
        [Fact]
        public async Task StartApproval_EmptyEmail_ThrowsInvalidInputException()
        {
            // Arrange
            var request = new ApprovalRequest { UserEmail = string.Empty };
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.StartApproval(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyEmail, exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                It.IsAny<string>(),
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that StartApproval throws InvalidInputException when the request is null.
        /// </summary>
        [Fact]
        public async Task StartApproval_NullRequest_ThrowsInvalidInputException()
        {
            // Arrange
            ApprovalRequest request = null;
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.StartApproval(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyEmail, exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                It.IsAny<string>(),
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that StartApproval throws EmailValidationException when the email format is invalid.
        /// </summary>
        [Fact]
        public async Task StartApproval_InvalidEmail_ThrowsEmailValidationException()
        {
            // Arrange
            var invalidEmail = "invalid-email";
            var request = new ApprovalRequest { UserEmail = invalidEmail };
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EmailValidationException>(() =>
                _orchestration.StartApproval(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal($"Invalid email format: {invalidEmail}", exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.ScheduleNewOrchestrationInstanceAsync(
                It.IsAny<TaskName>(),
                It.IsAny<string>(),
                It.IsAny<StartOrchestrationOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Approve Tests

        /// <summary>
        /// Tests that Approve returns OK status and properly raises an approval event
        /// when provided with a valid instance ID.
        /// </summary>
        [Fact]
        public async Task Approve_ValidInstanceId_ReturnsOkWithResponse()
        {
            // Arrange
            var instanceId = Guid.NewGuid().ToString();
            var request = new ApprovalRequest { InstanceId = instanceId };
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var mockResponse = new Mock<HttpResponseData>(new Mock<FunctionContext>().Object);
            mockResponse.SetupProperty(r => r.StatusCode);
            mockResponse.Object.StatusCode = HttpStatusCode.OK;

            _responseWriterMock
                .Setup(w => w.CreateResponse(mockRequest.Object, HttpStatusCode.OK))
                .Returns(mockResponse.Object);

            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");
            mockDurableClient
                .Setup(c => c.RaiseEventAsync(
                    instanceId,
                    TaskApprovalConstants.Events.ApprovalEvent,
                    TaskApprovalConstants.Actions.Approve,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _orchestration.Approve(mockRequest.Object, mockDurableClient.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify mocks were called correctly
            _requestReaderMock.Verify(r => r.ReadFromJsonAsync<ApprovalRequest>(
                mockRequest.Object, It.IsAny<CancellationToken>()), Times.Once);

            _responseWriterMock.Verify(w => w.CreateResponse(
                mockRequest.Object, HttpStatusCode.OK), Times.Once);

            _responseWriterMock.Verify(w => w.WriteAsJsonAsync(
                response, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

            mockDurableClient.Verify(c => c.RaiseEventAsync(
                instanceId,
                TaskApprovalConstants.Events.ApprovalEvent,
                TaskApprovalConstants.Actions.Approve,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Approve throws InvalidInputException when the instance ID is empty.
        /// </summary>
        [Fact]
        public async Task Approve_EmptyInstanceId_ThrowsInvalidInputException()
        {
            // Arrange
            var request = new ApprovalRequest { InstanceId = string.Empty };
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.Approve(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyInstanceId, exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.RaiseEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that Approve throws InvalidInputException when the request is null.
        /// </summary>
        [Fact]
        public async Task Approve_NullRequest_ThrowsInvalidInputException()
        {
            // Arrange
            ApprovalRequest request = null;
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.Approve(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyInstanceId, exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.RaiseEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Reject Tests

        /// <summary>
        /// Tests that Reject returns OK status and properly raises a rejection event
        /// when provided with a valid instance ID.
        /// </summary>
        [Fact]
        public async Task Reject_ValidInstanceId_ReturnsOkWithResponse()
        {
            // Arrange
            var instanceId = Guid.NewGuid().ToString();
            var request = new ApprovalRequest { InstanceId = instanceId };
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            var mockResponse = new Mock<HttpResponseData>(new Mock<FunctionContext>().Object);
            mockResponse.SetupProperty(r => r.StatusCode);
            mockResponse.Object.StatusCode = HttpStatusCode.OK;

            _responseWriterMock
                .Setup(w => w.CreateResponse(mockRequest.Object, HttpStatusCode.OK))
                .Returns(mockResponse.Object);

            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");
            mockDurableClient
                .Setup(c => c.RaiseEventAsync(
                    instanceId,
                    TaskApprovalConstants.Events.ApprovalEvent,
                    TaskApprovalConstants.Actions.Reject,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _orchestration.Reject(mockRequest.Object, mockDurableClient.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify mocks were called correctly
            _requestReaderMock.Verify(r => r.ReadFromJsonAsync<ApprovalRequest>(
                mockRequest.Object, It.IsAny<CancellationToken>()), Times.Once);

            _responseWriterMock.Verify(w => w.CreateResponse(
                mockRequest.Object, HttpStatusCode.OK), Times.Once);

            _responseWriterMock.Verify(w => w.WriteAsJsonAsync(
                response, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

            mockDurableClient.Verify(c => c.RaiseEventAsync(
                instanceId,
                TaskApprovalConstants.Events.ApprovalEvent,
                TaskApprovalConstants.Actions.Reject,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Reject throws InvalidInputException when the instance ID is empty.
        /// </summary>
        [Fact]
        public async Task Reject_EmptyInstanceId_ThrowsInvalidInputException()
        {
            // Arrange
            var request = new ApprovalRequest { InstanceId = string.Empty };
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.Reject(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyInstanceId, exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.RaiseEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that Reject throws InvalidInputException when the request is null.
        /// </summary>
        [Fact]
        public async Task Reject_NullRequest_ThrowsInvalidInputException()
        {
            // Arrange
            ApprovalRequest request = null;
            var mockRequest = new Mock<HttpRequestData>(new Mock<FunctionContext>().Object);
            var mockDurableClient = new Mock<DurableTaskClient>("TestHubName");

            _requestReaderMock
                .Setup(r => r.ReadFromJsonAsync<ApprovalRequest>(mockRequest.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(request);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.Reject(mockRequest.Object, mockDurableClient.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyInstanceId, exception.Message);

            // Verify that the client was never called
            mockDurableClient.Verify(c => c.RaiseEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region RunOrchestrator Tests

        /// <summary>
        /// Tests that RunOrchestrator returns "Approve" when the approval event is received.
        /// Verifies the orchestration flow including initial email, waiting for event, and approval email.
        /// </summary>
        [Fact]
        public async Task RunOrchestrator_ApproveEvent_ReturnsApprove()
        {
            // Arrange
            var userEmail = "test@example.com";
            var contextMock = new Mock<TaskOrchestrationContext>();

            // Setup GetInput to return a valid email
            contextMock.Setup(c => c.GetInput<string>()).Returns(userEmail);

            // Setup initial email sending
            contextMock.Setup(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskApprovalStarted),
                It.IsAny<TaskOptions>()))
                .Returns(Task.CompletedTask);

            // Setup WaitForExternalEvent to return "Approve"
            contextMock.Setup(c => c.WaitForExternalEvent<string>(
                TaskApprovalConstants.Events.ApprovalEvent,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(TaskApprovalConstants.Actions.Approve);

            // Setup email sending after approval
            contextMock.Setup(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskApproved),
                It.IsAny<TaskOptions>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orchestration.RunOrchestrator(contextMock.Object);

            // Assert
            Assert.Equal(TaskApprovalConstants.Actions.Approve, result);

            // Verify all expected methods were called
            contextMock.Verify(c => c.GetInput<string>(), Times.Once);
            contextMock.Verify(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskApprovalStarted),
                It.IsAny<TaskOptions>()), Times.Once);
            contextMock.Verify(c => c.WaitForExternalEvent<string>(
                TaskApprovalConstants.Events.ApprovalEvent,
                It.IsAny<CancellationToken>()), Times.Once);
            contextMock.Verify(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskApproved),
                It.IsAny<TaskOptions>()), Times.Once);
        }

        /// <summary>
        /// Tests that RunOrchestrator returns "Reject" when the rejection event is received.
        /// Verifies the orchestration flow including initial email, waiting for event, and rejection email.
        /// </summary>
        [Fact]
        public async Task RunOrchestrator_RejectEvent_ReturnsReject()
        {
            // Arrange
            var userEmail = "test@example.com";
            var contextMock = new Mock<TaskOrchestrationContext>();

            // Setup GetInput to return a valid email
            contextMock.Setup(c => c.GetInput<string>()).Returns(userEmail);

            // Setup initial email sending
            contextMock.Setup(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskApprovalStarted),
                It.IsAny<TaskOptions>()))
                .Returns(Task.CompletedTask);

            // Setup WaitForExternalEvent to return "Reject"
            contextMock.Setup(c => c.WaitForExternalEvent<string>(
                TaskApprovalConstants.Events.ApprovalEvent,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(TaskApprovalConstants.Actions.Reject);

            // Setup email sending after rejection
            contextMock.Setup(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskRejected),
                It.IsAny<TaskOptions>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orchestration.RunOrchestrator(contextMock.Object);

            // Assert
            Assert.Equal(TaskApprovalConstants.Actions.Reject, result);

            // Verify all expected methods were called
            contextMock.Verify(c => c.GetInput<string>(), Times.Once);
            contextMock.Verify(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskApprovalStarted),
                It.IsAny<TaskOptions>()), Times.Once);
            contextMock.Verify(c => c.WaitForExternalEvent<string>(
                TaskApprovalConstants.Events.ApprovalEvent,
                It.IsAny<CancellationToken>()), Times.Once);
            contextMock.Verify(c => c.CallActivityAsync(
                TaskApprovalConstants.FunctionNames.SendEmail,
                It.Is<EmailRequest>(r => r.Email == userEmail &&
                                         r.Subject == TaskApprovalConstants.EmailTemplates.Subjects.TaskRejected),
                It.IsAny<TaskOptions>()), Times.Once);
        }

       

        /// <summary>
        /// Tests that RunOrchestrator throws InvalidInputException when the email is empty.
        /// </summary>
        [Fact]
        public async Task RunOrchestrator_EmptyEmail_ThrowsInvalidInputException()
        {
            // Arrange
            var contextMock = new Mock<TaskOrchestrationContext>();

            // Setup GetInput to return an empty email
            contextMock.Setup(c => c.GetInput<string>()).Returns(string.Empty);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
                _orchestration.RunOrchestrator(contextMock.Object));

            Assert.Equal(TaskApprovalConstants.Errors.EmptyEmail, exception.Message);
        }

        /// <summary>
        /// Tests that RunOrchestrator throws EmailValidationException when the email format is invalid.
        /// </summary>
        [Fact]
        public async Task RunOrchestrator_InvalidEmail_ThrowsEmailValidationException()
        {
            // Arrange
            var invalidEmail = "invalid-email";
            var contextMock = new Mock<TaskOrchestrationContext>();

            // Setup GetInput to return an invalid email
            contextMock.Setup(c => c.GetInput<string>()).Returns(invalidEmail);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<EmailValidationException>(() =>
                _orchestration.RunOrchestrator(contextMock.Object));

            Assert.Equal($"Invalid email format: {invalidEmail}", exception.Message);
        }

        #endregion

        #region SendEmail Tests

        /// <summary>
        /// Tests that SendEmail returns true when the email service successfully sends an email.
        /// </summary>
        [Fact]
        public async Task SendEmail_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new EmailRequest
            {
                Email = "test@example.com",
                Subject = "Test Subject",
                Content = "Test Content"
            };

            _emailServiceMock.Setup(s => s.SendEmailAsync(request))
                .ReturnsAsync(true);

            // Act
            var result = await _orchestration.SendEmail(request);

            // Assert
            Assert.True(result);
            _emailServiceMock.Verify(s => s.SendEmailAsync(request), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmail returns false when the email service fails to send an email.
        /// </summary>
        [Fact]
        public async Task SendEmail_ServiceFailure_ReturnsFalse()
        {
            // Arrange
            var request = new EmailRequest
            {
                Email = "test@example.com",
                Subject = "Test Subject",
                Content = "Test Content"
            };

            _emailServiceMock.Setup(s => s.SendEmailAsync(request))
                .ReturnsAsync(false);

            // Act
            var result = await _orchestration.SendEmail(request);

            // Assert
            Assert.False(result);
            _emailServiceMock.Verify(s => s.SendEmailAsync(request), Times.Once);
        }

        /// <summary>
        /// Tests that SendEmail throws ArgumentNullException when the request is null.
        /// </summary>
        [Fact]
        public async Task SendEmail_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            EmailRequest request = null;
            _emailServiceMock.Setup(s => s.SendEmailAsync(null))
                .ThrowsAsync(new ArgumentNullException(nameof(request)));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _orchestration.SendEmail(request));

            _emailServiceMock.Verify(s => s.SendEmailAsync(null), Times.Once);
        }

        #endregion
    }
}