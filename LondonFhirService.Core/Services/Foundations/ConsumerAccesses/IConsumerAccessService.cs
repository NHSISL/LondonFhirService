// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public interface IConsumerAccessService
    {
        ValueTask<ConsumerAccess> CheckConsumerAccessAsync(
            string NhsNumber,
            Guid CorrelationId,
            CancellationToken cancellationToken = default);
    }
}
