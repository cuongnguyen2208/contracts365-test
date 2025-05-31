using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Contracts365ApproveTask.Interfaces
{
    /// <summary>
    /// Defines operations for creating and writing HTTP responses in Azure Functions.
    /// This interface abstracts the HTTP response creation and serialization process,
    /// providing a consistent way to generate responses across function endpoints.
    /// </summary>
    /// <remarks>
    /// The IHttpResponseWriter serves as an abstraction layer between Azure Functions and 
    /// HTTP response handling, making it easier to create standardized responses and
    /// facilitating testing by allowing response creation to be mocked.
    /// </remarks>
    public interface IHttpResponseWriter
    {
        /// <summary>
        /// Creates a new HTTP response with the specified status code.
        /// </summary>
        /// <param name="request">The original HTTP request data used to create the response.</param>
        /// <param name="statusCode">The HTTP status code to include in the response (defaults to 200 OK).</param>
        /// <returns>A new HTTP response object ready for content to be written to.</returns>
        /// <remarks>
        /// This method initializes a new response object without content, allowing the content
        /// to be added separately using other methods like <see cref="WriteAsJsonAsync{T}"/>.
        /// </remarks>
        HttpResponseData CreateResponse(HttpRequestData request, HttpStatusCode statusCode = HttpStatusCode.OK);

        /// <summary>
        /// Writes the specified data object to the HTTP response as JSON.
        /// </summary>
        /// <typeparam name="T">The type of data to serialize to JSON.</typeparam>
        /// <param name="response">The HTTP response to which the JSON will be written.</param>
        /// <param name="data">The data object to serialize to JSON and write to the response.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        /// <remarks>
        /// This method handles the serialization of objects to JSON and sets appropriate
        /// content type headers in the response. The response status code should be set
        /// before calling this method, typically by using <see cref="CreateResponse"/>.
        /// </remarks>
        Task WriteAsJsonAsync<T>(HttpResponseData response, T data, CancellationToken cancellationToken = default);
    }
}