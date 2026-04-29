// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly AppointmentMatcherService appointmentMatcherService;

        public AppointmentMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.appointmentMatcherService =
                new AppointmentMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();
    }
}
