using Contracts365ApproveTask.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Contracts365ApproveTask.Services
{
    /// <summary>
    /// Service responsible for reading and deserializing HTTP request data.
    /// This class abstracts the complexity of working with HTTP requests and provides
    /// a testable interface for reading JSON content from requests.
    /// </summary>
    public class HttpRequestReader : IHttpRequestReader
    {
        private readonly ILogger<HttpRequestReader> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestReader"/> class.
        /// </summary>
        /// <param name="logger">Logger for telemetry and diagnostics.</param>
        /// <exception cref="ArgumentNullException">Thrown if logger is null.</exception>
        public HttpRequestReader(ILogger<HttpRequestReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Reads and deserializes JSON data from an HTTP request into the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON content into.</typeparam>
        /// <param name="request">The HTTP request containing JSON data.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the deserialized object of type T, or default(T) if deserialization fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
        /// <remarks>
        /// This method uses the built-in ReadFromJsonAsync extension method but adds error handling
        /// and logging to make debugging easier in case of deserialization errors.
        /// </remarks>
        public async Task<T> ReadFromJsonAsync<T>(HttpRequestData request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                return await request.ReadFromJsonAsync<T>(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing HTTP request body to type {type}: {error}",
                    typeof(T).Name, ex.Message);
                throw; 
            }
        }
    }
}