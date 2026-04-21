// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationService
    {
        private static void ValidateChangeFhirRecordStatus(Guid fhirRecordId)
        {
            Validate(
                createException: () => new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again."),
                (Rule: IsInvalid(fhirRecordId), Parameter: nameof(fhirRecordId)));
        }

        private static void ValidatePersistFhirRecordDifferences(CompareQueueItem compareQueueItem)
        {
            if (compareQueueItem is null)
            {
                throw new NullCompareQueueItemException(
                    message: "Compare queue item is null, fix errors and try again.");
            }
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is invalid"
        };

        private static void Validate<T>(
            Func<T> createException,
            params (dynamic Rule, string Parameter)[] validations)
            where T : Xeption
        {
            T invalidDataException = createException();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDataException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDataException.ThrowIfContainsErrors();
        }
    }
}
