// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using ISL.Security.Client.Clients;
using ISL.Security.Client.Models.Clients;
using ISL.Security.Client.Models.Foundations.Users;
using Microsoft.AspNetCore.Http;
using NHSOneLondon.AuditAndMetrics.Abstractions.Brokers;

namespace LondonFhirService.Core.Brokers.Securities
{
    /// <summary>
    /// Provides security-related functionalities such as user authentication, claim verification, and role checks.
    /// Supports both REST API (using <see cref="IHttpContextAccessor"/>) and Azure Functions (using access token).
    ///
    /// Also answers the audit library's identity port, <see cref="IAuditUserBroker"/>, straight from
    /// the security client - a broker may not call another broker, so the port is not satisfied by
    /// a separate broker sitting on top of this one or <see cref="SecurityBroker"/>.
    ///
    /// The ClaimsPrincipal is captured in the constructor rather than read per call, so this must
    /// be resolved per request. Registered as a singleton it would be built at startup with no
    /// HttpContext, and every audit row and metric span in the system would be stamped anonymous
    /// with nothing failing to signal it.
    /// </summary>
    public class SecurityAuditBroker : ISecurityAuditBroker, IAuditUserBroker
    {
        /// <summary>
        /// Shared for the same reason as <see cref="SecurityBroker"/>: constructing a
        /// SecurityClient builds and abandons a DI container, and this broker is resolved several
        /// times per patient request. The client holds no per-caller state - every method takes
        /// the ClaimsPrincipal as a parameter - so sharing one is safe, and the per-request
        /// ClaimsPrincipal capture below is unchanged.
        /// </summary>
        private static readonly ISecurityClient SharedSecurityClient = new SecurityClient();

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
            securityClient = SharedSecurityClient;
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
            securityClient = SharedSecurityClient;
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
            securityClient = SharedSecurityClient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuditBroker"/> class with the
        /// security client supplied rather than shared, so tests can stand in for it.
        /// </summary>
        internal SecurityAuditBroker(
            ClaimsPrincipal claimsPrincipal,
            SecurityConfigurations securityConfigurations,
            ISecurityClient securityClient)
        {
            this.claimsPrincipal = claimsPrincipal;
            this.securityConfigurations = securityConfigurations;
            this.securityClient = securityClient;
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
                    .EnsureOtherAuditValuesRemainsUnchangedOnModifyAsync(entity, storageEntity, securityConfigurations);


        /// <summary>
        /// Retrieves the user identifier from the given claims principal.
        /// </summary>
        /// <param name="claimsPrincipal">The user context containing claims.</param>
        /// <returns>The user identifier string.</returns>
        /// <remarks>
        /// If no valid user identifier is found, a fallback (such as <c>"Anonymous"</c>) may be returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// string userId = await auditClient.GetUserIdAsync(User);
        /// // e.g. "Alice" or "Anonymous"
        /// </code>
        /// </example>
        public async ValueTask<string> GetUserIdAsync() =>
            await securityClient.Audits.GetUserIdAsync(claimsPrincipal);

        /// <summary>
        /// The audit library's identity port. An empty string rather than a null when no id is
        /// resolved, so callers stamping this onto a row do not have to guard it.
        /// </summary>
        public async ValueTask<string> GetCurrentUserIdAsync() =>
            await securityClient.Audits.GetUserIdAsync(claimsPrincipal) ?? string.Empty;

        /// <summary>
        /// The audit library's identity port. Falls back to the given and family names because
        /// DisplayName is not guaranteed to be populated - a directory entry can carry the parts
        /// without the whole. An empty string rather than a null when nothing is resolvable, so
        /// callers stamping this onto a row do not have to guard it.
        /// </summary>
        public async ValueTask<string> GetCurrentUserDisplayNameAsync()
        {
            User currentUser = await securityClient.Users.GetUserAsync(claimsPrincipal);

            if (currentUser is null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(currentUser.DisplayName) is false)
            {
                return currentUser.DisplayName;
            }

            return $"{currentUser.GivenName} {currentUser.Surname}".Trim();
        }
    }
}
