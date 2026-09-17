// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.CompareQueue
{
    internal partial class CompareQueueOrchestrationService
    {
        private static void ValidateChangeFhirRecordStatus(Guid fhirRecordId)
        {
            Validate(
                createException: () => new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again."),
                (Rule: IsInvalid(fhirRecordId), Parameter: nameof(fhirRecordId)));
        }

        private static void ValidateFinalizeClaimedFhirRecord(
            CompareQueueItem compareQueueItem,
            StatusType terminalStatus)
        {
            if (compareQueueItem?.SecondaryFhirRecord is null)
            {
                throw new NullCompareQueueItemException(
                    message: "Compare queue item is null, fix errors and try again.");
            }

            Validate(
                createException: () => new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again."),
                (Rule: IsInvalid(compareQueueItem.SecondaryFhirRecord.Id),
                    Parameter: nameof(compareQueueItem.SecondaryFhirRecord.Id)),
                (Rule: IsNotTerminal(terminalStatus), Parameter: nameof(terminalStatus)));
        }

        private static void ValidatePersistFhirRecordDifferences(CompareQueueItem compareQueueItem)
        {
            if (compareQueueItem is null)
            {
                throw new NullCompareQueueItemException(
                    message: "Compare queue item is null, fix errors and try again.");
            }
        }

        private static void ValidateCompareQueueItemOnRetainClaim(CompareQueueItem compareQueueItem)
        {
            if (compareQueueItem?.SecondaryFhirRecord is null)
            {
                throw new NullCompareQueueItemException(
                    message: "Compare queue item is null, fix errors and try again.");
            }

            Validate(
                createException: () => new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again."),
                (Rule: IsInvalid(compareQueueItem.SecondaryFhirRecord.Id),
                    Parameter: nameof(compareQueueItem.SecondaryFhirRecord.Id)));
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is invalid"
        };

        /// <summary>
        /// The settle hardcodes IsProcessed to true, which is only right for a status nothing
        /// comes back from. Passing Processing here would turn the fenced write into a lease
        /// renewal that also marks the row processed, and the row would then be both in flight
        /// and reported as done.
        /// </summary>
        private static dynamic IsNotTerminal(StatusType status) => new
        {
            Condition = status != StatusType.Completed && status != StatusType.Failed,
            Message = "Status is not terminal"
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
