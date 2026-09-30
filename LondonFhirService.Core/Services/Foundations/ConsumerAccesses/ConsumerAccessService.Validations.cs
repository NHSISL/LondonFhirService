// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    internal partial class ConsumerAccessService
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

        /// <summary>
        /// 401 is the dependency saying it does not know the consumer: an answer about the caller,
        /// not a fault in the dependency, so it is a dependency validation failure rather than the
        /// critical dependency failure EnsureSuccessStatusCode used to make of it. The body goes
        /// into Data - problem details from the access decision, or nothing at all when it is the
        /// dependency's own authentication rejecting this service's token, which is the one case
        /// worth telling apart when reading the log.
        /// </summary>
        private static void ValidateConsumerAccessResponseIsAnswered(ConsumerAccessResponse consumerAccessResponse)
        {
            if (consumerAccessResponse is null)
            {
                throw new NullConsumerAccessServiceException(
                    message: "Consumer access response is null.");
            }

            if (consumerAccessResponse.StatusCode is HttpStatusCode.Unauthorized)
            {
                var unauthorizedConsumerAccessServiceException =
                    new UnauthorizedConsumerAccessServiceException(
                        message: "Consumer access service does not recognise the consumer.");

                unauthorizedConsumerAccessServiceException.AddData(
                    key: nameof(ConsumerAccessResponse.Content),
                    values: consumerAccessResponse.Content);

                throw unauthorizedConsumerAccessServiceException;
            }
        }

        /// <summary>
        /// Mirrors the ValidateStorage* checks every sibling foundation service performs on a
        /// broker response. Localising an unusable access response here makes it a ConsumerAccess
        /// dependency failure rather than a NullReferenceException surfacing three layers up.
        /// </summary>
        private static void ValidateConsumerAccessResponse(ConsumerAccess consumerAccess)
        {
            if (consumerAccess is null)
            {
                throw new NullConsumerAccessServiceException(
                    message: "Consumer access response is null.");
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
