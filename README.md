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
│   │   │   ├── services/
│   │   │   │   └── approval.service.ts # Service for API calls
│   ├── package.json             # Front-end dependencies
├── backend/                     # Azure Functions back-end
│   ├── Contract365ApproveTask/
│   │   ├── TaskApprovalOrchestration.cs # Durable Functions logic
│   │   ├── Dtos/
│   │   │   ├── ApprovalRequest.cs # DTO for API requests
│   │   │   ├── EmailRequest.cs    # DTO for email simulation
│   │   ├── local.settings.json    # Local configuration (CORS, storage)
│   │   ├── host.json             # Azure Functions host configuration
│   ├── Contract365ApproveTask.Tests/
│   │   ├── TaskApprovalOrchestrationTests.cs # Unit tests
│   ├── Contract365ApproveTask.sln # Solution file
├── README.md                    # This file
```

## Requirements
- **Node.js**: v18 or higher (for front-end).
- **.NET SDK**: 8.0 (for back-end).
- **Azure Functions Core Tools**: v4 (for running Azure Functions locally).
- **Azurite**: For local Azure Storage emulation.
- **Visual Studio 2022**: For back-end development and testing (optional).
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
4. Access the application at `http://localhost:4200`.

### Back-end Setup
1. Navigate to the back-end directory:
   ```cmd
   cd backend\Contract365ApproveTask
   ```
2. Start Azurite (in a separate Command Prompt):
   ```cmd
   azurite
   ```
3. Run the back-end:
   ```cmd
   func start
   ```
   - The API will be available at `http://localhost:7069/api`.

### Running Unit Tests
1. Navigate to the test directory:
   ```cmd
   cd backend\Contract365ApproveTask.Tests
   ```
2. Run tests:
   ```cmd
   dotnet test
   ```
   - Expected output: 5 tests passed, covering orchestration logic and email simulation.

## API Endpoints
- **POST `/api/StartApproval`**:
  - Body: `{ "userEmail": "<email>" }`
  - Response: `{ "instanceId": "<guid>", "userEmail": "<email>", "status": "Started" }`
  - Simulates sending a "start" email.
- **POST `/api/Approve`**:
  - Body: `{ "instanceId": "<guid>" }`
  - Response: `{ "message": "Approval event sent." }`
  - Simulates sending an "approval" email.
- **POST `/api/Reject`**:
  - Body: `{ "instanceId": "<guid>" }`
  - Response: `{ "message": "Rejection event sent." }`
  - Simulates sending a "rejection" email.

## Implementation Details
### Front-end
- **Technology**: Angular 17 with standalone components.
- **UI/UX**:
  - Input field for email using Angular Material (`mat-form-field`).
  - Three buttons (`Start Approval`, `Approve`, `Reject`) with clear labels, disabled states, and responsive design.
  - Error message for empty email input.
- **API Integration**: `ApprovalService` handles HTTP requests to back-end endpoints.

### Back-end
- **Technology**: Azure Functions v4 with C# Durable Functions (in-process).
- **Orchestration**:
  - `RunOrchestrator`: Manages approval workflow, waits for `ApprovalEvent`, and simulates email notifications.
  - Uses `EmailRequest` DTO for email simulation.
- **HTTP Triggers**:
  - `StartApproval`, `Approve`, `Reject` handle API requests with JSON responses.
  - Input validation ensures `userEmail` and `instanceId` are provided.
- **Email Simulation**:
  - `SendEmail` activity function logs email details to console and returns `true`.
  - Designed for easy replacement with SendGrid (update `SendEmail` function body).
- **Data Modeling**:
  - `ApprovalRequest`: DTO for API request payloads.
  - `EmailRequest`: DTO for email simulation parameters.

## Notes
- **Email Simulation**: Real email sending (via SendGrid) was omitted per agreement with the client, replaced with console logging to simulate notifications.
- **CORS**: Configured in `local.settings.json` to allow `http://localhost:4200` for local testing.
- **Durable Functions**: The application demonstrates orchestration, external events, and activity functions, ready for further discussion in interviews.
