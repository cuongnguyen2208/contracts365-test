using Microsoft.Azure.Functions.Worker.Http;

namespace Contracts365ApproveTask.Interfaces
{
    /// <summary>
    /// Defines operations for reading and deserializing HTTP request data in Azure Functions.
    /// This interface abstracts the process of extracting structured data from HTTP requests,
    /// providing a consistent approach to request parsing across function endpoints.
    /// </summary>
    /// <remarks>
    /// The IHttpRequestReader serves as an abstraction layer between Azure Functions and 
    /// HTTP request processing, simplifying the extraction of data from requests and
    /// facilitating testing by allowing request reading to be mocked.
    /// </remarks>
    public interface IHttpRequestReader
    {
        /// <summary>
        /// Reads and deserializes the JSON body of an HTTP request into a specified type.
        /// </summary>
        /// <typeparam name="T">The target type to deserialize the JSON content into.</typeparam>
        /// <param name="request">The HTTP request containing JSON data to be read.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous read operation, with the result being 
        /// the deserialized object of type <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// This method handles the deserialization of JSON request bodies into strongly-typed
        /// objects, enabling type-safe access to request data. It's particularly useful for
        /// processing POST and PUT requests where the body contains structured data.
        /// </remarks>
        Task<T> ReadFromJsonAsync<T>(HttpRequestData request, CancellationToken cancellationToken = default);
    }
}