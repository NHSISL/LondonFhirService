// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// ISecurityAuditBroker.cs — interface
using System.Threading.Tasks;

namespace {Namespace}.Brokers.Securities
{
    /// <summary>
    /// Provides methods to apply and verify audit metadata on entities,
    /// such as created/updated timestamps and user identifiers.
    /// </summary>
    public interface ISecurityAuditBroker
    {
        /// <summary>
        /// Applies audit values related to entity creation, such as CreatedBy and CreatedDate,
        /// using the provided claims principal and security configuration.
        /// </summary>
        /// <typeparam name="T">The type of the entity being audited.</typeparam>
        /// <param name="entity">The entity to which audit values should be applied.</param>
        /// <returns>A task containing the audited entity.</returns>
        ValueTask<T> ApplyAddAuditValuesAsync<T>(T entity);

        /// <summary>
        /// Applies audit values related to entity modification, such as UpdatedBy and UpdatedDate,
        /// using the provided claims principal and security configuration.
        /// </summary>
        /// <typeparam name="T">The type of the entity being audited.</typeparam>
        /// <param name="entity">The entity to which audit values should be applied.</param>
        /// <returns>A task containing the audited entity.</returns>
        ValueTask<T> ApplyModifyAuditValuesAsync<T>(T entity);

        /// <summary>
        /// Applies audit values related to logical deletion, such as UpdatedBy and UpdatedDate,
        /// using the provided claims principal and security configuration.
        /// </summary>
        /// <typeparam name="T">The type of the entity being audited.</typeparam>
        /// <param name="entity">The entity to which deletion audit values should be applied.</param>
        /// <param name="deletionReason">
        /// The reason the entity is being removed, stamped alongside the other deletion audit
        /// values. When null, any reason already on the entity is left untouched.
        /// </param>
        /// <returns>A task containing the entity with deletion audit values.</returns>
        ValueTask<T> ApplyRemoveAuditValuesAsync<T>(T entity, string? deletionReason = null);

        /// <summary>
        /// Ensures that audit values related to entity creation remain unchanged during modification,
        /// copying them from the stored version of the entity to the current one.
        /// </summary>
        /// <typeparam name="T">The type of the entity being verified.</typeparam>
        /// <param name="entity">The modified entity.</param>
        /// <param name="storageEntity">The original stored entity with correct creation audit values.</param>
        /// <returns>A task containing the entity with preserved creation audit values.</returns>
        ValueTask<T> EnsureAddAuditValuesRemainsUnchangedOnModifyAsync<T>(
            T entity,
            T storageEntity);
    }
}

// SecurityAuditBroker.cs — implementation
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using {Namespace}.Security.Client.Clients;
using {Namespace}.Security.Client.Models.Clients;
using Microsoft.AspNetCore.Http;

namespace {Namespace}.Brokers.Securities
{
    /// <summary>
    /// Provides security-related functionalities such as user authentication, claim verification, and role checks.
    /// Supports both REST API (using <see cref="IHttpContextAccessor"/>) and Azure Functions (using access token).
    /// </summary>
    internal class SecurityAuditBroker : ISecurityAuditBroker
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
            this.securityClient = new SecurityClient();
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
            this.claimsPrincipal = GetClaimsPrincipalFromToken(accessToken);
            this.securityClient = new SecurityClient();
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
            this.securityClient = new SecurityClient();
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
        public ValueTask<T> ApplyAddAuditValuesAsync<T>(T entity) =>
            this.securityClient.Audits.ApplyAddAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Applies auditing metadata for a modify operation to the specified entity.
        /// Sets updated audit fields based on the current user.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit.</param>
        /// <returns>The audited entity with modify metadata applied.</returns>
        public ValueTask<T> ApplyModifyAuditValuesAsync<T>(T entity) =>
                this.securityClient.Audits.ApplyModifyAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Applies auditing metadata for a remove (soft delete) operation to the specified entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit for removal.</param>
        /// <param name="deletionReason">The reason for the removal, or null to leave it unchanged.</param>
        /// <returns>The audited entity with remove metadata applied.</returns>
        public ValueTask<T> ApplyRemoveAuditValuesAsync<T>(T entity, string? deletionReason = null) =>
                this.securityClient.Audits.ApplyRemoveAuditValuesAsync(
                    entity,
                    claimsPrincipal,
                    securityConfigurations,
                    deletionReason);

        /// <summary>
        /// Ensures that add audit values (e.g., created by/date) remain unchanged during modify operations.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity being modified.</param>
        /// <param name="storageEntity">The original stored entity used to preserve original audit values.</param>
        /// <returns>The entity with original add audit values retained.</returns>
        public ValueTask<T> EnsureAddAuditValuesRemainsUnchangedOnModifyAsync<T>(
            T entity,
            T storageEntity) =>
                this.securityClient.Audits
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(entity, storageEntity, securityConfigurations);
    }
}
