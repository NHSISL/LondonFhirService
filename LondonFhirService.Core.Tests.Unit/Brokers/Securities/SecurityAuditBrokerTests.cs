// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Security.Claims;
using ISL.Security.Client.Clients;
using ISL.Security.Client.Clients.Audits;
using ISL.Security.Client.Clients.Users;
using ISL.Security.Client.Models.Clients;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Brokers.Securities;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Core.Tests.Unit.Brokers.Securities
{
    public partial class SecurityAuditBrokerTests
    {
        private readonly Mock<ISecurityClient> securityClientMock;
        private readonly Mock<IAuditClient> auditClientMock;
        private readonly Mock<IUserClient> userClientMock;
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly SecurityAuditBroker securityAuditBroker;

        public SecurityAuditBrokerTests()
        {
            this.securityClientMock = new Mock<ISecurityClient>();
            this.auditClientMock = new Mock<IAuditClient>();
            this.userClientMock = new Mock<IUserClient>();

            this.securityClientMock.SetupGet(client =>
                client.Audits)
                    .Returns(this.auditClientMock.Object);

            this.securityClientMock.SetupGet(client =>
                client.Users)
                    .Returns(this.userClientMock.Object);

            // A principal of its own rather than an empty one, so a broker that asked the client
            // about any other principal - a fresh one, say - would fail the verifications.
            this.claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, GetRandomString()) },
                    authenticationType: "Test"));

            this.securityAuditBroker = new SecurityAuditBroker(
                claimsPrincipal: this.claimsPrincipal,
                securityConfigurations: new SecurityConfigurations(),
                securityClient: this.securityClientMock.Object);
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static User CreateUser(string displayName, string givenName, string surname) =>
            new User(
                userId: GetRandomString(),
                givenName: givenName,
                surname: surname,
                displayName: displayName,
                email: GetRandomString(),
                jobTitle: GetRandomString(),
                roles: [],
                claims: []);

        private void VerifyNoOtherSecurityClientCalls()
        {
            this.auditClientMock.VerifyNoOtherCalls();
            this.userClientMock.VerifyNoOtherCalls();
        }
    }
}
