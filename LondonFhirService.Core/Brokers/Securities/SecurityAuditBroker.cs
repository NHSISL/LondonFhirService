// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using ISL.Security.Client.Clients;
using ISL.Security.Client.Models.Clients;
using Microsoft.AspNetCore.Http;

namespace LondonFhirService.Core.Brokers.Securities
{
    /// <summary>
    /// Provides security-related functionalities such as user authentication, claim verification, and role checks.
    /// Supports both REST API (using <see cref="IHttpContextAccessor"/>) and Azure Functions (using access token).
    /// </summary>
    public class SecurityAuditBroker : ISecurityAuditBroker
    {
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly ISecurityClient securityClient;
        private readonly SecurityConfigurations securityConfigurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuditBroker"/> class 
        /// using <see cref="IHttpContextAccessor"/>.
        /// This constructor is intended for REST API usage.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        public SecurityAuditBroker(
            IHttpContextAccessor httpContextAccessor,
            SecurityConfigurations securityConfigurations)
        {
            claimsPrincipal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
            securityClient = new SecurityClient();
            this.securityConfigurations = securityConfigurations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuditBroker"/> class using an access token.
        /// This constructor is intended for Azure Function / non REST API usage.
        /// </summary>
        /// <param name="accessToken">A JWT access token containing user claims.</param>
        /// <param name="securityConfigurations">Contains information of the audit properties to target.</param>
        public SecurityAuditBroker(string accessToken, SecurityConfigurations securityConfigurations)
        {
            claimsPrincipal = GetClaimsPrincipalFromToken(accessToken);
            securityClient = new SecurityClient();
            this.securityConfigurations = securityConfigurations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuditBroker"/> 
        /// class using a <see cref="ClaimsPrincipal"/>.
        /// This constructor is intended for Azure Functions or non-REST API usage.
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="ClaimsPrincipal"/> containing user claims.</param>
        /// <param name="securityConfigurations">Contains information of the audit properties to target.</param>
        public SecurityAuditBroker(ClaimsPrincipal claimsPrincipal, SecurityConfigurations securityConfigurations)
        {
            this.claimsPrincipal = claimsPrincipal;
            this.securityConfigurations = securityConfigurations;
            securityClient = new SecurityClient();
        }

        /// <summary>
        /// Extracts a <see cref="ClaimsPrincipal"/> from a given JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> containing claims from the token.</returns>
        private static ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");

            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Applies auditing metadata for an add operation to the specified entity.
        /// Sets created and updated audit fields based on the current user.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit.</param>
        /// <returns>The audited entity with add metadata applied.</returns>
        public async ValueTask<T> ApplyAddAuditValuesAsync<T>(T entity) =>
            await securityClient.Audits.ApplyAddAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Applies auditing metadata for a modify operation to the specified entity.
        /// Sets updated audit fields based on the current user.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit.</param>
        /// <returns>The audited entity with modify metadata applied.</returns>
        public async ValueTask<T> ApplyModifyAuditValuesAsync<T>(T entity) =>
            await securityClient.Audits.ApplyModifyAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Applies auditing metadata for a remove (soft delete) operation to the specified entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit for removal.</param>
        /// <returns>The audited entity with remove metadata applied.</returns>
        public async ValueTask<T> ApplyRemoveAuditValuesAsync<T>(T entity) =>
            await securityClient.Audits.ApplyRemoveAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Ensures that add audit values (e.g., created by/date) remain unchanged during modify operations.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity being modified.</param>
        /// <param name="storageEntity">The original stored entity used to preserve original audit values.</param>
        /// <returns>The entity with original add audit values retained.</returns>
        public async ValueTask<T> EnsureAddAuditValuesRemainsUnchangedOnModifyAsync<T>(
            T entity,
            T storageEntity) =>
                await securityClient.Audits
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(entity, storageEntity, securityConfigurations);


        /// <summary>
        /// Retrieves the current user identifier from the given claims principal.
        /// </summary>
        /// <param name="claimsPrincipal">The user context containing claims.</param>
        /// <returns>The user identifier string.</returns>
        /// <remarks>
        /// If no valid user identifier is found, a fallback (such as <c>"Anonymous"</c>) may be returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// string userId = await auditClient.GetCurrentUserIdAsync(User);
        /// // e.g. "Alice" or "Anonymous"
        /// </code>
        /// </example>
        public async ValueTask<string> GetCurrentUserIdAsync() =>
            await securityClient.Audits.GetCurrentUserIdAsync(claimsPrincipal);
    }
}
