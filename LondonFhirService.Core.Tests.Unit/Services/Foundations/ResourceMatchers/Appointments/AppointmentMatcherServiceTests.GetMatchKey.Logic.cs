// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnDdsIdentifierAsMatchKeyForAppointmentWithDdsIdentifierAsync()
        {
            // given
            string expectedDdsIdentifierValue = GetRandomDdsIdentifierValue();

            JsonElement appointmentResource =
                CreateAppointmentWithDdsIdentifier(expectedDdsIdentifierValue);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.appointmentMatcherService.GetMatchKeyAsync(
                    appointmentResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedDdsIdentifierValue);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnDdsIdentifierWhenAppointmentHasMultipleIdentifiersAsync()
        {
            // given
            string expectedDdsIdentifierValue = GetRandomDdsIdentifierValue();

            JsonElement appointmentResource =
                CreateAppointmentWithMultipleIdentifiers(expectedDdsIdentifierValue);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.appointmentMatcherService.GetMatchKeyAsync(
                    appointmentResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedDdsIdentifierValue);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullMatchKeyForAppointmentWithoutIdentifiersAsync()
        {
            // given
            JsonElement appointmentResource = CreateAppointmentWithoutIdentifier();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.appointmentMatcherService.GetMatchKeyAsync(
                    appointmentResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
