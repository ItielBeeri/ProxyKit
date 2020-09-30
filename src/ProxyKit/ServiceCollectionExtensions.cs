using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ProxyKit
{
    public static class ServiceCollectionExtensions
    {
        internal const string DefaultProxyKitHttpClientName = "ProxyKitClient";

        public static IServiceCollection AddProxy(
            this IServiceCollection services,
            Action<IHttpClientBuilder> configureHttpClientBuilder = null,
            Action<ProxyOptions> configureOptions = null) => AddProxy(
                services,
                new Dictionary<string, Action<IHttpClientBuilder>>
                {
                    [DefaultProxyKitHttpClientName] = null
                },
                configureOptions);

        public static IServiceCollection AddProxy(
            this IServiceCollection services,
            IDictionary<string, Action<IHttpClientBuilder>> configureHttpClientBuilder,
            Action<ProxyOptions> configureOptions = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureHttpClientBuilder is null)
            {
                configureHttpClientBuilder = new Dictionary<string, Action<IHttpClientBuilder>>();
            }
            if (!configureHttpClientBuilder.ContainsKey(DefaultProxyKitHttpClientName))
            {
                configureHttpClientBuilder.Add(DefaultProxyKitHttpClientName, null);
            }

            foreach (var clientName in configureHttpClientBuilder.Keys)
            {
                var httpClientBuilder = services
                    .AddHttpClient(clientName)
                    .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                    {
                        AllowAutoRedirect = false,
                        UseCookies = false
                    });

                configureHttpClientBuilder[clientName]?.Invoke(httpClientBuilder);
            }

            configureOptions = configureOptions ?? (_ => { });
            services
                .Configure(configureOptions)
                .AddOptions<ProxyOptions>();
            return services;
        }
    }
}
