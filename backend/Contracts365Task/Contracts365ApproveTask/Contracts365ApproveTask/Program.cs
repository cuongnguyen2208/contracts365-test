using Contracts365ApproveTask.Interfaces;
using Contracts365ApproveTask.Middlewares;
using Contracts365ApproveTask.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.DurableTask.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<ExceptionHandlingMiddleware>();
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddDurableTaskWorker(builder =>
    {
        builder.UseGrpc();
    })
    .AddSingleton<IEmailService, EmailService>()
    .AddSingleton<IHttpRequestReader, HttpRequestReader>()
    .AddSingleton<IHttpResponseWriter, HttpResponseWriter>();

builder.Build().Run();
