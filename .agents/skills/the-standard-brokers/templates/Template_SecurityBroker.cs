// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// ISecurityBroker.cs — interface
using System.Threading.Tasks;
using {Namespace}.Models.Securities;

namespace {Namespace}.Brokers.Securities
{
    public interface ISecurityBroker
    {
        ValueTask<User> GetCurrentUserAsync();
        ValueTask<bool> IsCurrentUserAuthenticatedAsync();
        ValueTask<bool> IsInRoleAsync(string roleName);
        ValueTask<bool> UserHasClaimAsync(string claimType, string claimValue);
        ValueTask<bool> UserHasClaimAsync(string claimType);
    }
}

// SecurityBroker.cs — implementation
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using {Namespace}.Models.Securities;
using {Namespace}.Security.Client.Clients;
using Microsoft.AspNetCore.Http;

namespace {Namespace}.Brokers.Securities
{
    /// <summary>
    /// Provides security-related functionalities such as user authentication, claim verification, and role checks.
    /// Supports both REST API (using <see cref="IHttpContextAccessor"/>) and Azure Functions (using access token).
    /// </summary>
    public class SecurityBroker : ISecurityBroker
    {
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly ISecurityClient securityClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBroker"/> class 
        /// using <see cref="IHttpContextAccessor"/>.
        /// This constructor is intended for REST API usage.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        public SecurityBroker(IHttpContextAccessor httpContextAccessor)
        {
            claimsPrincipal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
            this.securityClient = new SecurityClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBroker"/> class using an access token.
        /// This constructor is intended for Azure Function / non REST API usage.
        /// </summary>
        /// <param name="accessToken">A JWT access token containing user claims.</param>
        public SecurityBroker(string accessToken)
        {
            claimsPrincipal = GetClaimsPrincipalFromToken(accessToken);
            this.securityClient = new SecurityClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBroker"/> class using a <see cref="ClaimsPrincipal"/>.
        /// This constructor is intended for Azure Functions or non-REST API usage.
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="ClaimsPrincipal"/> containing user claims.</param>
        public SecurityBroker(ClaimsPrincipal claimsPrincipal)
        {
            this.claimsPrincipal = claimsPrincipal;
            this.securityClient = new SecurityClient();
        }

        /// <summary>
        /// Retrieves details of the current authenticated user based on claims.
        /// </summary>
        /// <returns>An <see cref="User"/> object containing user details.</returns>
        public async ValueTask<User> GetCurrentUserAsync()
        {
            var user = await this.securityClient.Users.GetUserAsync(claimsPrincipal);

            return new User(
                userId: user.UserId,
                givenName: user.GivenName,
                surname: user.Surname,
                displayName: user.DisplayName,
                email: user.Email,
                jobTitle: user.JobTitle,
                roles: user.Roles,
                claims: user.Claims);
        }

        /// <summary>
        /// Determines whether the current user is authenticated.
        /// </summary>
        /// <returns>True if the user is authenticated; otherwise, false.</returns>
        public async ValueTask<bool> IsCurrentUserAuthenticatedAsync() =>
            await this.securityClient.Users.IsUserAuthenticatedAsync(claimsPrincipal);

        /// <summary>
        /// Checks if the current user is in a specified role.
        /// </summary>
        /// <param name="roleName">The role name to check.</param>
        /// <returns>True if the user is in the specified role; otherwise, false.</returns>
        public async ValueTask<bool> IsInRoleAsync(string roleName) =>
            await this.securityClient.Users.IsUserInRoleAsync(claimsPrincipal, roleName);

        /// <summary>
        /// Checks whether the current user has a specific claim with a given value.
        /// </summary>
        /// <param name="claimType">The type of the claim.</param>
        /// <param name="claimValue">The value of the claim.</param>
        /// <returns>True if the user has the claim with the specified value; otherwise, false.</returns>
        public async ValueTask<bool> UserHasClaimAsync(string claimType, string claimValue) =>
            await this.securityClient.Users.UserHasClaimAsync(claimsPrincipal, claimType, claimValue);

        /// <summary>
        /// Checks whether the current user has a specific claim type.
        /// </summary>
        /// <param name="claimType">The type of the claim.</param>
        /// <returns>True if the user has the claim; otherwise, false.</returns>
        public async ValueTask<bool> UserHasClaimAsync(string claimType) =>
            await this.securityClient.Users.UserHasClaimAsync(claimsPrincipal, claimType);

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
    }
}
