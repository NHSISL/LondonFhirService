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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
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
                    "e.g. '1994-05-21'");

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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
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
                patientConfiguration: blankPatientConfiguration,
                loggingBroker: this.loggingBrokerMock.Object);

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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            blankCredentialsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
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
                patientConfiguration: unconfiguredPatientConfiguration,
                loggingBroker: this.loggingBrokerMock.Object);

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.AuthUrl),
                value: "Text must be a valid absolute http or https url");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.GetStructuredRecordUrl),
                value: "Text must be a valid absolute http or https url");

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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            unconfiguredHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfConfigurationIsNullAsync()
        {
            // given
            var unconfiguredHttpBrokerMock = new Mock<IHttpBroker>();

            var unconfiguredPatientService = new PatientService(
                httpBroker: unconfiguredHttpBrokerMock.Object,
                patientConfiguration: null,
                loggingBroker: this.loggingBrokerMock.Object);

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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            unconfiguredHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        /// <summary>
        /// The shape an unconfigured host actually ships in: not blank, so the old blank-only
        /// check passed it straight through to HttpClient, which answered a relative uri with an
        /// InvalidOperationException and turned a configuration problem into a 500 about nothing.
        /// </summary>
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfUrlsAreNotAbsoluteAsync()
        {
            // given
            var placeholderPatientConfiguration = new PatientConfiguration
            {
                AuthUrl = "override_this_in_your_appsettings.Development.json_file",
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = GetRandomString(),
                GetStructuredRecordUrl = "api/patient/$getstructuredrecord"
            };

            var placeholderHttpBrokerMock = new Mock<IHttpBroker>();

            var placeholderPatientService = new PatientService(
                httpBroker: placeholderHttpBrokerMock.Object,
                patientConfiguration: placeholderPatientConfiguration,
                loggingBroker: this.loggingBrokerMock.Object);

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.AuthUrl),
                value: "Text must be a valid absolute http or https url");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.GetStructuredRecordUrl),
                value: "Text must be a valid absolute http or https url");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: invalidPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                placeholderPatientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            placeholderHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// An absolute uri is not automatically one HttpClient will dial. Before the scheme check
        /// these all passed validation and failed later inside HttpClient with a
        /// NotSupportedException, which reached the operator as a 500 naming nothing - on a rule
        /// whose whole job is to name the setting that is wrong.
        /// </summary>
        [Theory]
        [MemberData(nameof(UndialableAbsoluteUrls))]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfUrlSchemeIsNotHttpAsync(
            string undialableUrl)
        {
            // given
            var misconfiguredPatientConfiguration = new PatientConfiguration
            {
                AuthUrl = undialableUrl,
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = GetRandomString(),
                GetStructuredRecordUrl = $"https://{GetRandomString()}.example.nhs.uk/record"
            };

            var misconfiguredHttpBrokerMock = new Mock<IHttpBroker>();

            var misconfiguredPatientService = new PatientService(
                httpBroker: misconfiguredHttpBrokerMock.Object,
                patientConfiguration: misconfiguredPatientConfiguration,
                loggingBroker: this.loggingBrokerMock.Object);

            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;

            var invalidPatientServiceException =
                new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again.");

            invalidPatientServiceException.UpsertDataList(
                key: nameof(PatientConfiguration.AuthUrl),
                value: "Text must be a valid absolute http or https url");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient validation error occurred, please fix errors and try again.",
                    innerException: invalidPatientServiceException);

            // when
            ValueTask<string> getStructuredRecordTask =
                misconfiguredPatientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualPatientServiceValidationException.Should()
                .BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceValidationException))),
                        Times.Once);

            misconfiguredHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The other half of the same rule: a well formed https url has to keep working, or the
        /// scheme check would have simply broken the feature.
        /// </summary>
        [Fact]
        public async Task ShouldAcceptAnHttpsUrlOnGetStructuredRecordAsync()
        {
            // given
            StructuredRecordRequest randomStructuredRecordRequest =
                CreateRandomStructuredRecordRequest();

            StructuredRecordRequest inputStructuredRecordRequest = randomStructuredRecordRequest;
            string randomAccessToken = GetRandomString();
            string randomStructuredRecord = GetRandomString();

            this.httpBrokerMock.Setup(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(CreateTokenResponse(randomAccessToken));

            this.httpBrokerMock.Setup(broker =>
                broker.PostJsonContentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(randomStructuredRecord);

            // when
            string actualStructuredRecord =
                await this.patientService.GetStructuredRecordAsync(
                    inputStructuredRecordRequest,
                    TestContext.Current.CancellationToken);

            // then
            actualStructuredRecord.Should().Be(randomStructuredRecord);

            this.httpBrokerMock.Verify(broker =>
                broker.PostFormUrlEncodedContentAsync(
                    this.patientConfiguration.AuthUrl,
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.Verify(broker =>
                broker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.httpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
