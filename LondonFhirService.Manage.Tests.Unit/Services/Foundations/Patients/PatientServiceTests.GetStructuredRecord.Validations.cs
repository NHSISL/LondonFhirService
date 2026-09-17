// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Brokers.Https;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Manage.Services.Foundations.Patients;
using Moq;
using Xeptions;

namespace LondonFhirService.Manage.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfRequestIsNullAsync()
        {
            // given
            StructuredRecordRequest nullStructuredRecordRequest = null;

            var nullPatientServiceException =
                new NullPatientServiceException(
                    message: "Patient request is null.");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: nullPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecordAsync(
                    nullStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfNhsNumberIsInvalidAsync(
            string invalidNhsNumber)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            inputStructuredRecordRequest.NhsNumber = invalidNhsNumber;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(StructuredRecordRequest.NhsNumber),
                value: "Text is invalid");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: invalidPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(InvalidDatesOfBirth))]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfDateOfBirthIsMalformedAsync(
            string malformedDateOfBirth)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            inputStructuredRecordRequest.DateOfBirth = malformedDateOfBirth;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(StructuredRecordRequest.DateOfBirth),

                value: "Text must be a valid date string in format 'yyyy-MM-dd' " +
                    "e.g. '2002-10-01'");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: invalidPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A blank credential on the request is not an error on its own - it means fall back -
        /// so this is the case where neither source supplies one and the call would otherwise go
        /// out with an empty client_id.
        /// </summary>
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfCredentialsResolveToNothingAsync()
        {
            // given
            var blankPatientConfiguration = new PatientConfiguration
            {
                AuthUrl = $"https://{GetRandomString()}.example.nhs.uk/token",
                ClientId = string.Empty,
                ClientSecret = null,
                Scope = "   ",
                GrantType = string.Empty,

                GetStructuredRecordUrl =
                    $"https://{GetRandomString()}.example.nhs.uk/$getstructuredrecord"
            };

            var blankCredentialsHttpBrokerMock = new Mock<IHttpBroker>();

            var blankCredentialsPatientService = new PatientService(
                httpBroker: blankCredentialsHttpBrokerMock.Object,
                patientConfiguration: blankPatientConfiguration);

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            inputStructuredRecordRequest.ClientId = string.Empty;
            inputStructuredRecordRequest.ClientSecret = null;
            inputStructuredRecordRequest.Scope = "   ";
            inputStructuredRecordRequest.GrantType = string.Empty;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            foreach (string parameter in new List<string>
            {
                nameof(StructuredRecordRequest.ClientId),
                nameof(StructuredRecordRequest.ClientSecret),
                nameof(StructuredRecordRequest.Scope),
                nameof(StructuredRecordRequest.GrantType)
            })
            {
                invalidPatientServiceException.UpsertDataList(
                    key: parameter,
                    value: "Text is invalid");
            }

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: invalidPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                blankCredentialsPatientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            blankCredentialsHttpBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// An unconfigured host must say which setting is missing rather than failing inside
        /// HttpClient on a null url, which is why the two endpoints are validated per request
        /// alongside the caller's own fields.
        /// </summary>
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfUrlsAreNotConfiguredAsync()
        {
            // given
            var unconfiguredPatientConfiguration = new PatientConfiguration
            {
                AuthUrl = null,
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = GetRandomString(),
                GetStructuredRecordUrl = "   "
            };

            var unconfiguredHttpBrokerMock = new Mock<IHttpBroker>();

            var unconfiguredPatientService = new PatientService(
                httpBroker: unconfiguredHttpBrokerMock.Object,
                patientConfiguration: unconfiguredPatientConfiguration);

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.AuthUrl),
                value: "Text is invalid");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.GetStructuredRecordUrl),
                value: "Text is invalid");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: invalidPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                unconfiguredPatientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            unconfiguredHttpBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfConfigurationIsNullAsync()
        {
            // given
            var unconfiguredHttpBrokerMock = new Mock<IHttpBroker>();

            var unconfiguredPatientService = new PatientService(
                httpBroker: unconfiguredHttpBrokerMock.Object,
                patientConfiguration: null);

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var nullPatientServiceException =
                new NullPatientServiceException(
                    message: "Patient configuration is null.");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: nullPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                unconfiguredPatientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            unconfiguredHttpBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(UnusableTokenResponses))]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordIfTokenResponseIsUnusableAsync(
            string unusableTokenResponse)
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(unusableTokenResponse);

            // when
            ValueTask<string> getStructuredRecordTask =
                this.patientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceDependencyException actualPatientServiceDependencyException =
                await Assert.ThrowsAsync<PatientServiceDependencyException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceDependencyException.InnerException
                .Should().BeOfType<InvalidAccessTokenPatientServiceException>();

            // The structured record call is never made. Without a token it could only ever be
            // answered with a 401, and the page would report that instead of the real fault.
            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
