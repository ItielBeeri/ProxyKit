using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProxyKit
{
    public class ForwardContext
    {
        private readonly HttpClient _httpClient;

        internal ForwardContext(
            HttpContext httpContext,
            HttpRequestMessage upstreamRequest,
            HttpClient httpClient)
        {
            _httpClient = httpClient;
            HttpContext = httpContext;
            UpstreamRequest = upstreamRequest;
        }

        /// <summary>
        /// The incoming HttpContext
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// The upstream request message.
        /// </summary>
        public HttpRequestMessage UpstreamRequest { get; }

        /// <summary>
        /// Sends the upstream request to the upstream host.
        /// </summary>
        /// <returns>An <see cref="HttpResponseMessage"/> the represents the proxy response.</returns>
        public async Task<HttpResponseMessage> Send(CancellationToken cancellationToken = default)
        {
            using var linkedCts = 
                CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted, cancellationToken);

            try
            {
                return await _httpClient
                    .SendAsync(
                        UpstreamRequest,
                        HttpCompletionOption.ResponseHeadersRead,
                        linkedCts.Token)
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is IOException)
            {
                return new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
            }
            catch (OperationCanceledException)
            {
                // Happens when Timeout is low and upstream host is not reachable.
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
            catch (HttpRequestException ex)
                when (ex.InnerException is IOException || ex.InnerException is SocketException)
            {
                // Happens when server is not reachable
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
        }
    }
}