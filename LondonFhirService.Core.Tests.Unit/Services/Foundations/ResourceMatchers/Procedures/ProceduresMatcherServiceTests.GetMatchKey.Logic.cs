// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Procedures
{
    public partial class ProcedureMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenProcedureHasDdsIdentifierAsync()
        {
            // given
            string inputDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string randomId = GetRandomString();

            JsonElement procedureResource = CreateProcedureResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputDdsIdentifierValue;

            // when
            string actualMatchKey =
                await this.procedureMatcherService.GetMatchKeyAsync(procedureResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenProcedureHasNoIdentifierPropertyAsync()
        {
            // given
            string randomId = GetRandomString();

            JsonElement procedureResource =
                CreateProcedureResourceWithoutIdentifierProperty(id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.procedureMatcherService.GetMatchKeyAsync(procedureResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
