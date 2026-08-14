// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService
    {
        private static void ValidateOnCheckConsumerAccess(ValidateAccessRequest request)
        {
            ValidateRequestIsNotNull(request);

            Validate(
                createException: () => new InvalidConsumerAccessServiceException(
                    message: "Invalid consumer access. Please correct the errors and try again."),

                (Rule: IsInvalid(request.ConsumerUserId),
                Parameter: nameof(ValidateAccessRequest.ConsumerUserId)),

                (Rule: IsInvalid(request.NhsNumber),
                Parameter: nameof(ValidateAccessRequest.NhsNumber)),

                (Rule: IsInvalid(request.CorrelationId),
                Parameter: nameof(ValidateAccessRequest.CorrelationId)));
        }

        private static void ValidateRequestIsNotNull(ValidateAccessRequest request)
        {
            if (request is null)
            {
                throw new NullConsumerAccessServiceException(
                    message: "Consumer access is null.");
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is invalid"
        };

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
