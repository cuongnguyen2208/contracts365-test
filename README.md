# Contracts 365 Task Approval Application

## Overview
This is a task approval application developed for the Contracts 365 coding test. It consists of a front-end built with Angular 17 and a back-end using Azure Functions with C# Durable Functions. The application allows users to initiate an approval process, approve, or reject tasks via a web interface, with a simulated email notification system.

### Key Features
- **Front-end**: Angular 17 web app with a user-friendly interface, including an email input field and three buttons (`Start Approval`, `Approve`, `Reject`).
- **Back-end**: Azure Functions with Durable Functions, providing three HTTP endpoints (`StartApproval`, `Approve`, `Reject`).
- **Email Simulation**: Simulated email notifications (logging to console) for start, approval, and rejection events, designed for easy integration with SendGrid in the future.
- **Unit Testing**: Comprehensive unit tests for the back-end using xUnit and Moq.
- **CORS**: Configured to allow front-end requests from `http://localhost:4200`.

## Project Structure
```
contracts365-test/
├── frontend/                    # Angular 17 front-end
│   ├── src/
│   │   ├── app/
│   │   │   ├── app.component.ts  # Main component with UI logic
│   │   │   ├── app.component.html # UI template with email input and buttons
│   │   │   ├── app.component.scss # UI styling
│   │   │   ├── models/
│   │   │   │   └── app.model.ts  # Model for StartApprovalResponse
│   │   │   ├── services/
│   │   │   │   └── approval.service.ts # Service for API calls
│   │   │   ├── shared/
│   │   │   │   └── interceptors/
│   │   │   │       └── error.interceptor.ts # HTTP error interceptor
│   │   ├── app.config.ts         # Application configuration
│   ├── package.json              # Front-end dependencies
├── backend/                     # Azure Functions back-end
│   ├── Contracts365ApproveTask/
│   │   ├── Constants/
│   │   │   └── TaskApprovalConstants.cs # Constants for errors, events, etc.
│   │   ├── Dtos/
│   │   │   ├── ApprovalRequest.cs # DTO for API requests
│   │   │   ├── EmailRequest.cs    # DTO for email simulation
│   │   ├── Exceptions/
│   │   │   ├── Contracts365ApproveTaskException.cs # Base exception
│   │   │   ├── EmailSendingException.cs # Email error
│   │   │   ├── EmailValidationException.cs # Email validation error
│   │   │   ├── InvalidApprovalEventException.cs # Invalid event error
│   │   │   ├── InvalidInputException.cs # Input validation error
│   │   │   ├── NotFoundException.cs # Not found error
│   │   ├── Helpers/
│   │   │   └── EmailHelper.cs     # Email validation utility
│   │   ├── Middlewares/
│   │   │   └── ExceptionHandlingMiddleware.cs # Global exception handling
│   │   ├── Services/
│   │   │   ├── EmailService.cs    # Email simulation service
│   │   │   ├── IEmailService.cs   # Email service interface
│   │   ├── TaskApprovalOrchestration.cs # Durable Functions logic
│   │   ├── Program.cs             # Functions host configuration
│   │   ├── local.settings.json    # Local configuration (CORS, storage)
│   │   ├── host.json             # Azure Functions host configuration
│   ├── Contracts365ApproveTask.Tests/
│   │   ├── TaskApprovalOrchestrationTests.cs # Unit tests
│   ├── Contracts365ApproveTask.sln # Solution file
├── README.md                    # This file
```

## Requirements
- **Node.js**: v18 or higher (for front-end).
- **.NET SDK**: 8.0 (for back-end).
- **Azure Functions Core Tools**: v4 (for running Azure Functions locally).
- **Azurite**: For local Azure Storage emulation.
- **Visual Studio 2022** or **Visual Studio Code**: For development and testing.
- **Windows**: Tested on Windows 10/11.

## Setup and Running Locally

### Prerequisites
1. Install Node.js: [Download](https://nodejs.org)
2. Install .NET SDK 8.0: [Download](https://dotnet.microsoft.com/download)
3. Install Azure Functions Core Tools:
   ```cmd
   npm install -g azure-functions-core-tools@4 --unsafe-perm true
   ```
4. Install Azurite:
   ```cmd
   npm install -g azurite
   ```

### Front-end Setup
1. Navigate to the front-end directory:
   ```cmd
   cd frontend
   ```
2. Install dependencies:
   ```cmd
   npm install
   ```
3. Run the front-end:
   ```cmd
   ng serve
   ```
   - Access the application at `http://localhost:4200`.

### Back-end Setup
1. Navigate to the back-end directory:
   ```cmd
   cd backend\Contracts365ApproveTask
   ```
2. Start Azurite (in a separate Command Prompt):
   ```cmd
   azurite --silent
   ```
3. Run the back-end:
   ```cmd
   func start --port 7078
   ```
   - The API will be available at `http://localhost:7078/api`.

### Running Unit Tests
1. Navigate to the test directory:
   ```cmd
   cd backend\Contracts365ApproveTask.Tests
   ```
2. Run tests:
   ```cmd
   dotnet test
   ```
   - Expected output: Tests for `EmailService` and email validation pass.

## API Endpoints
- **POST `/api/StartApproval`**:
  - Body: `{ "userEmail": "<email>" }`
  - Response: `{ "instanceId": "<guid>", "userEmail": "<email>", "status": "Started" }`
  - Simulates sending a "Task Approval Started" email.
- **POST `/api/Approve`**:
  - Body: `{ "instanceId": "<guid>" }`
  - Response: `{ "message": "Approval event sent." }`
  - Simulates sending a "Task Approved" email.
- **POST `/api/Reject`**:
  - Body: `{ "instanceId": "<guid>" }`
  - Response: `{ "message": "Rejection event sent." }`
  - Simulates sending a "Task Rejected" email.

## Implementation Details
### Front-end
- **Technology**: Angular 17 with standalone components, Angular Material, and `ngx-toastr` for notifications.
- **UI/UX**:
  - Input field for email with validation (required, email format).
  - Three buttons (`Start Approval`, `Approve`, `Reject`) with Material design, disabled when invalid (e.g., no email or `instanceId`).
  - Toast notifications for success (e.g., "Approval process started") and errors (e.g., "Email is required").
  - Centered, responsive layout with clear typography.
- **API Integration**: `ApprovalService` handles HTTP requests with error interception via `error.interceptor.ts`.

### Back-end
- **Technology**: Azure Functions v4 with C# Durable Functions (Isolated mode).
- **Orchestration**:
  - `RunOrchestrator`: Waits indefinitely for an `ApprovalEvent`, triggers email simulation via `SendEmail` activity.
  - Uses `EmailRequest` DTO for email parameters.
- **HTTP Triggers**:
  - `StartApproval`, `Approve`, `Reject` validate inputs and return JSON responses.
  - Custom exceptions (`InvalidInputException`, `EmailValidationException`) ensure robust error handling.
- **Email Simulation**:
  - `EmailService` logs email details to console, returning `true`.
  - Designed for easy integration with SendGrid by updating `SendEmailAsync`.
- **Data Modeling**:
  - `ApprovalRequest`: DTO for API payloads (`UserEmail`, `InstanceId`).
  - `EmailRequest`: DTO for email simulation (`Email`, `Subject`, `Content`).
- **Error Handling**:
  - `ExceptionHandlingMiddleware` catches exceptions, returning appropriate HTTP status codes and messages.
  - Custom exceptions (`Contracts365ApproveTaskException` and derivatives) for specific error cases.
- **Constants**: `TaskApprovalConstants` centralizes error messages, events, and email templates for maintainability.

## Notes
- **Email Simulation**: Email sending is simulated via console logging. To use SendGrid, update `EmailService.SendEmailAsync` with SendGrid API calls.
- **CORS**: Configured in `local.settings.json` to allow `http://localhost:4200` for local testing.
- **Durable Functions**: Demonstrates orchestration, external events, and activity functions, ready for interview discussions on scalability, state management, and error handling.
- **Unit Tests**: Cover `EmailService` and validation logic. Additional tests for orchestration can be added if required.
- **Deployment**: Backend can be deployed to Azure Functions, frontend to Azure Static Web Apps.