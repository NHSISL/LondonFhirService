// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Processings.Metrics.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.Metrics
{
    internal partial class MetricProcessingService
    {
        private static void ValidateOnRetrieveRequestMetricExports(
            Guid? correlationId,
            string? userId,
            MetricStatus? status,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate)
        {
            Validate(
                createException: () => new InvalidArgumentMetricProcessingException(
                    message: "Invalid metric processing arguments. " +
                        "Please correct the errors and try again."),

                (Rule: IsInvalid(correlationId), Parameter: nameof(correlationId)),
                (Rule: IsInvalid(userId), Parameter: nameof(userId)),
                (Rule: IsInvalid(status), Parameter: nameof(status)),
                (Rule: IsBefore(toDate, fromDate), Parameter: nameof(toDate)));
        }

        private static dynamic IsInvalid(Guid? id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is invalid."
        };

        // Absent is no filter; present but blank is a mistake - it would match only rows stamped
        // with no user, which is not what anyone filtering by user is asking for.
        private static dynamic IsInvalid(string? text) => new
        {
            Condition = text is not null && String.IsNullOrWhiteSpace(text),
            Message = "Text is invalid."
        };

        // A status the enum does not name - an ordinal past its end - would match nothing and read as
        // an empty export rather than as the mistake it is.
        private static dynamic IsInvalid(MetricStatus? status) => new
        {
            Condition = status.HasValue && Enum.IsDefined(status.Value) is false,
            Message = "Value is invalid."
        };

        private static dynamic IsBefore(DateTimeOffset? date, DateTimeOffset? otherDate) => new
        {
            Condition = date.HasValue && otherDate.HasValue && date.Value < otherDate.Value,
            Message = "Date must be the same as or after fromDate."
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
