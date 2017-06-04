// -----------------------------------------------------------------------
// <copyright file="AuthenticationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Graph;
    using IdentityModel.Clients.ActiveDirectory;
    using Services;

    /// <summary>
    /// Authentication provider for the Microsoft Graph service client.
    /// </summary>
    /// <seealso cref="IAuthenticationProvider" />
    public class AuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Name of the authentication header to be utilized. 
        /// </summary>
        private const string AuthHeaderName = "Authorization";

        /// <summary>
        /// The type of token being utilized for the authentication request.
        /// </summary>
        private const string TokenType = "Bearer";

        /// <summary>
        /// Provides access to core application providers..
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// The customer identifier utilized to scope the Microsoft Graph requests.
        /// </summary>
        private readonly string customerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application providers.</param>
        /// <param name="customerId">Identifier for customer whose resources are being accessed.</param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public AuthenticationProvider(IAppService service, string customerId)
        {
            service.AssertNotNull(nameof(service));
            customerId.AssertNotEmpty(nameof(customerId));

            this.customerId = customerId;
            this.service = service;
        }

        /// <summary>
        /// Performs the necessary authentication and injects the required header.
        /// </summary>
        /// <param name="request">The request being made to the Microsoft Graph API.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AuthenticationResult token = await service.Token.GetAppOnlyTokenAsync(
                $"{service.Configuration.ActiveDirectoryEndpoint}/{customerId}",
                service.Configuration.GraphEndpoint);

            request.Headers.Add(AuthHeaderName, $"{TokenType} {token.AccessToken}");
        }
    }
}