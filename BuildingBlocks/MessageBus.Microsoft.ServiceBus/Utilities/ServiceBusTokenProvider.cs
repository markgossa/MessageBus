using Azure.Core;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBus.Microsoft.ServiceBus.Utilities
{
    internal class ServiceBusTokenProvider : TokenCredential
    {
        private readonly string _tenantId;

        public ServiceBusTokenProvider(string tenantId)
        {
            _tenantId = tenantId;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            const string serviceBusTokenUri = "https://servicebus.azure.net/";

            var accessToken = new AzureServiceTokenProvider().GetAccessTokenAsync(serviceBusTokenUri, _tenantId,
                cancellationToken).Result;

            return new AccessToken(accessToken, DateTimeOffset.Now.AddDays(10));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new ValueTask<AccessToken>(Task.FromResult(GetToken(requestContext, cancellationToken)));
    }
}
