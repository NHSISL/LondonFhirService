// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Brokers.Securities
{
    /// <summary>
    /// The audit library's identity port, answered straight from the security client. The metric
    /// service stamps both values onto every span and the audit service stamps the id onto every
    /// audit row, so a null here would be a blank the caller has to guard, and asking the client
    /// about the wrong principal would be every row attributed to someone else.
    /// </summary>
    public partial class SecurityAuditBrokerTests
    {
        [Fact]
        public async Task ShouldGetCurrentUserIdAsync()
        {
            // given
            string expectedUserId = GetRandomString();

            this.auditClientMock.Setup(client =>
                client.GetUserIdAsync(this.claimsPrincipal))
                    .ReturnsAsync(expectedUserId);

            // when
            string actualUserId = await this.securityAuditBroker.GetCurrentUserIdAsync();

            // then
            actualUserId.Should().Be(expectedUserId);

            this.auditClientMock.Verify(client =>
                client.GetUserIdAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }

        [Fact]
        public async Task ShouldReturnAnEmptyUserIdWhenNoneIsResolvedAsync()
        {
            // given
            this.auditClientMock.Setup(client =>
                client.GetUserIdAsync(this.claimsPrincipal))
                    .ReturnsAsync((string)null);

            // when
            string actualUserId = await this.securityAuditBroker.GetCurrentUserIdAsync();

            // then
            actualUserId.Should().BeEmpty();

            this.auditClientMock.Verify(client =>
                client.GetUserIdAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }

        [Fact]
        public async Task ShouldGetCurrentUserDisplayNameAsync()
        {
            // given
            User user = CreateUser(
                displayName: GetRandomString(),
                givenName: GetRandomString(),
                surname: GetRandomString());

            string expectedDisplayName = user.DisplayName;

            this.userClientMock.Setup(client =>
                client.GetUserAsync(this.claimsPrincipal))
                    .ReturnsAsync(user);

            // when
            string actualDisplayName =
                await this.securityAuditBroker.GetCurrentUserDisplayNameAsync();

            // then
            actualDisplayName.Should().Be(expectedDisplayName);

            this.userClientMock.Verify(client =>
                client.GetUserAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }

        /// <summary>
        /// DisplayName is not guaranteed to be populated - a directory entry can carry the parts
        /// without the whole - so a blank one falls back to the parts rather than to nothing.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldFallBackToGivenNameAndSurnameWhenDisplayNameIsBlankAsync(
            string blankDisplayName)
        {
            // given
            string givenName = GetRandomString();
            string surname = GetRandomString();
            string expectedDisplayName = $"{givenName} {surname}";
            User user = CreateUser(blankDisplayName, givenName, surname);

            this.userClientMock.Setup(client =>
                client.GetUserAsync(this.claimsPrincipal))
                    .ReturnsAsync(user);

            // when
            string actualDisplayName =
                await this.securityAuditBroker.GetCurrentUserDisplayNameAsync();

            // then
            actualDisplayName.Should().Be(expectedDisplayName);

            this.userClientMock.Verify(client =>
                client.GetUserAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }

        [Fact]
        public async Task ShouldNotPadTheFallbackWhenOnlyOneNamePartIsPresentAsync()
        {
            // given
            string givenName = GetRandomString();
            User user = CreateUser(displayName: null, givenName: givenName, surname: null);

            this.userClientMock.Setup(client =>
                client.GetUserAsync(this.claimsPrincipal))
                    .ReturnsAsync(user);

            // when
            string actualDisplayName =
                await this.securityAuditBroker.GetCurrentUserDisplayNameAsync();

            // then
            actualDisplayName.Should().Be(givenName);

            this.userClientMock.Verify(client =>
                client.GetUserAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }

        [Fact]
        public async Task ShouldReturnAnEmptyDisplayNameWhenNoNameIsResolvableAsync()
        {
            // given
            User user = CreateUser(displayName: null, givenName: null, surname: null);

            this.userClientMock.Setup(client =>
                client.GetUserAsync(this.claimsPrincipal))
                    .ReturnsAsync(user);

            // when
            string actualDisplayName =
                await this.securityAuditBroker.GetCurrentUserDisplayNameAsync();

            // then
            actualDisplayName.Should().BeEmpty();

            this.userClientMock.Verify(client =>
                client.GetUserAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }

        [Fact]
        public async Task ShouldReturnAnEmptyDisplayNameWhenNoUserIsResolvedAsync()
        {
            // given
            this.userClientMock.Setup(client =>
                client.GetUserAsync(this.claimsPrincipal))
                    .ReturnsAsync((User)null);

            // when
            string actualDisplayName =
                await this.securityAuditBroker.GetCurrentUserDisplayNameAsync();

            // then
            actualDisplayName.Should().BeEmpty();

            this.userClientMock.Verify(client =>
                client.GetUserAsync(this.claimsPrincipal),
                    Times.Once);

            VerifyNoOtherSecurityClientCalls();
        }
    }
}
