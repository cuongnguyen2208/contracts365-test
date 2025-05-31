using Contracts365ApproveTask.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Contracts365ApproveTask.Services
{
    /// <summary>
    /// Service responsible for creating and writing HTTP responses.
    /// This class abstracts the complexity of working with HTTP responses and provides
    /// a testable interface for creating responses and serializing objects to JSON.
    /// </summary>
    public class HttpResponseWriter : IHttpResponseWriter
    {
        /// <summary>
        /// Creates an HTTP response with the specified status code.
        /// </summary>
        /// <param name="request">The HTTP request to create a response for.</param>
        /// <param name="statusCode">The HTTP status code to use for the response (defaults to 200 OK).</param>
        /// <returns>A new HTTP response data object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
        public HttpResponseData CreateResponse(HttpRequestData request, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.CreateResponse(statusCode);
        }

        /// <summary>
        /// Serializes an object to JSON and writes it to the HTTP response.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="response">The HTTP response to write the JSON data to.</param>
        /// <param name="data">The object to serialize to JSON.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the response or data is null.</exception>
        /// <remarks>
        /// This method sets the content type to application/json and handles serializing
        /// the provided object to the response body.
        /// </remarks>
        public async Task WriteAsJsonAsync<T>(HttpResponseData response, T data, CancellationToken cancellationToken = default)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            await response.WriteAsJsonAsync(data, cancellationToken).ConfigureAwait(false);
        }
    }
}