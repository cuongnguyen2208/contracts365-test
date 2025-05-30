Contracts 365 Task Approval Application

Overview

This is a task approval application developed for the Contracts 365 coding test. It consists of a front-end built with Angular 17 and a back-end using Azure Functions with C# Durable Functions. The application allows users to initiate an approval process, approve, or reject tasks via a web interface, with a simulated email notification system.

Key Features





Front-end: Angular 17 web app with a user-friendly interface, including an email input field and three buttons (Start Approval, Approve, Reject).



Back-end: Azure Functions with Durable Functions, providing three HTTP endpoints (StartApproval, Approve, Reject).



Email Simulation: Simulated email notifications (logging to console) for start, approval, and rejection events, designed for easy integration with SendGrid in the future.



Unit Testing: Comprehensive unit tests for the back-end using xUnit and Moq.



CORS: Configured to allow front-end requests from http://localhost:4200.

Project Structure

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


API Endpoints





POST /api/StartApproval:





Body: { "userEmail": "<email>" }



Response: { "instanceId": "<guid>", "userEmail": "<email>", "status": "Started" }



Simulates sending a "start" email.



POST /api/Approve:





Body: { "instanceId": "<guid>" }



Response: { "message": "Approval event sent." }



Simulates sending an "approval" email.



POST /api/Reject:





Body: { "instanceId": "<guid>" }



Response: { "message": "Rejection event sent." }



Simulates sending a "rejection" email.
