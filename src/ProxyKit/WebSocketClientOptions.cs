using System.Net;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace ProxyKit
{
    public class WebSocketClientOptions
    {
        internal WebSocketClientOptions(
            ClientWebSocketOptions options,
            HttpContext httpContext)
        {
            Options = options;
            HttpContext = httpContext;
        }

        /// <summary>
        /// Get or sets the cookies associated with the upstream websocket request.
        /// </summary>
        public CookieContainer Cookies
        {
            get => Options.Cookies;
            set => Options.Cookies = value;
        }

        /// <summary>
        ///     The incoming HttpContext.
        /// </summary>
        public HttpContext HttpContext { get; }

        public ClientWebSocketOptions Options { get; }

        /// <summary>
        ///     Set a header on the upstream websocket request.
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        public void SetRequestHeader(string headerName, string headerValue)
        {
            Options.SetRequestHeader(headerName, headerValue);
        }
    }
}