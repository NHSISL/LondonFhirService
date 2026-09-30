// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Text.Json;
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

        private const string AccessMediaType = "application/json";
        private const string ProblemMediaType = "application/problem+json";
        private const string ConsumerUnknownErrorCode = "ConsumerUnknown";

        private const string CheckScopeAdvice =
            "Check ConsumerAccessConfiguration:scope and this service's managed identity / app registration.";

        private const string GrantRoleAdvice =
            "Grant its identity the required app role on the Consumer Access Service app registration.";

        /// <summary>
        /// The dependency uses 401 and 403 for two different things, told apart only by the body:
        ///
        ///   401 problem, errorCode ConsumerUnknown - the consumer is not registered. An answer
        ///       about the caller, so a dependency validation failure the orchestration sends down
        ///       its unauthorized path.
        ///   403 application/json Access body       - a refusal of the consumer, read as the
        ///       ConsumerAccess it carries and audited upstream as a denial.
        ///   any other 401                          - this service's bearer token missing or
        ///       rejected, or something in front of the dependency refusing it.
        ///   any other 403                          - this service's identity missing an app role.
        ///
        /// The last two are configuration faults, so they are critical dependency failures, with a
        /// message that names the cause and what to check. Reading one as an unknown consumer would
        /// answer every consumer 404 at once and look like nothing more than a quiet day.
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
                ConsumerAccessProblemDetails maybeProblemDetails = ReadProblemDetails(consumerAccessResponse);

                if (maybeProblemDetails?.ErrorCode == ConsumerUnknownErrorCode)
                {
                    var unauthorizedConsumerAccessServiceException =
                        new UnauthorizedConsumerAccessServiceException(
                            message: "Consumer access service does not recognise the consumer.");

                    AddResponseData(
                        unauthorizedConsumerAccessServiceException,
                        consumerAccessResponse,
                        maybeProblemDetails);

                    throw unauthorizedConsumerAccessServiceException;
                }

                throw CreateFailedConsumerAccessAuthenticationException(
                    consumerAccessResponse,
                    maybeProblemDetails);
            }

            if (consumerAccessResponse.StatusCode is HttpStatusCode.Forbidden
                && IsAccessDecision(consumerAccessResponse) is false)
            {
                throw CreateFailedConsumerAccessAuthorizationException(
                    consumerAccessResponse,
                    ReadProblemDetails(consumerAccessResponse));
            }
        }

        private static FailedConsumerAccessAuthenticationException CreateFailedConsumerAccessAuthenticationException(
            ConsumerAccessResponse consumerAccessResponse,
            ConsumerAccessProblemDetails maybeProblemDetails)
        {
            string message = maybeProblemDetails is null
                ? "Consumer Access Service answered 401 without a readable problem details body "
                    + $"({DescribeResponse(consumerAccessResponse)}), so the refusal did not come from its "
                    + "access decision - most likely a gateway or its authentication rejected this service's "
                    + $"bearer token. {CheckScopeAdvice}"

                : "Consumer Access Service rejected this service's bearer token "
                    + $"({DescribeProblem(maybeProblemDetails)}). {CheckScopeAdvice}";

            var failedConsumerAccessAuthenticationException =
                new FailedConsumerAccessAuthenticationException(message);

            AddResponseData(
                failedConsumerAccessAuthenticationException,
                consumerAccessResponse,
                maybeProblemDetails);

            return failedConsumerAccessAuthenticationException;
        }

        private static FailedConsumerAccessAuthorizationException CreateFailedConsumerAccessAuthorizationException(
            ConsumerAccessResponse consumerAccessResponse,
            ConsumerAccessProblemDetails maybeProblemDetails)
        {
            string message = maybeProblemDetails is null
                ? "Consumer Access Service answered 403 with neither an access decision nor a readable "
                    + $"problem details body ({DescribeResponse(consumerAccessResponse)}), so it refused this "
                    + $"service rather than the consumer - most likely a gateway or its authorization. "
                    + GrantRoleAdvice

                : "This service authenticated to Consumer Access Service but lacks the required permission "
                    + $"({DescribeProblem(maybeProblemDetails)}). {GrantRoleAdvice}";

            var failedConsumerAccessAuthorizationException =
                new FailedConsumerAccessAuthorizationException(message);

            AddResponseData(
                failedConsumerAccessAuthorizationException,
                consumerAccessResponse,
                maybeProblemDetails);

            return failedConsumerAccessAuthorizationException;
        }

        private static string DescribeProblem(ConsumerAccessProblemDetails problemDetails) =>
            $"{problemDetails.ErrorCode ?? "no error code"}: {problemDetails.Detail ?? "no detail"}";

        private static string DescribeResponse(ConsumerAccessResponse consumerAccessResponse) =>
            $"content type: {NoneIfEmpty(consumerAccessResponse.ContentType)}; "
                + $"WWW-Authenticate: {NoneIfEmpty(consumerAccessResponse.WwwAuthenticate)}";

        private static string NoneIfEmpty(string text) =>
            string.IsNullOrEmpty(text) ? "none" : text;

        /// <summary>
        /// Everything needed to diagnose the refusal from the log alone: which status, which code,
        /// the dependency's own correlation id for its side of the trail, and - for a refusal with
        /// no problem on it - what did arrive.
        /// </summary>
        private static void AddResponseData(
            Xeption exception,
            ConsumerAccessResponse consumerAccessResponse,
            ConsumerAccessProblemDetails maybeProblemDetails)
        {
            exception.AddData(key: "StatusCode", values: ((int)consumerAccessResponse.StatusCode).ToString());
            exception.AddData(key: "ErrorCode", values: maybeProblemDetails?.ErrorCode ?? string.Empty);
            exception.AddData(key: "Detail", values: maybeProblemDetails?.Detail ?? string.Empty);
            exception.AddData(key: "CorrelationId", values: maybeProblemDetails?.CorrelationId ?? string.Empty);
            exception.AddData(key: "ContentType", values: consumerAccessResponse.ContentType ?? string.Empty);

            exception.AddData(
                key: "WwwAuthenticate",
                values: consumerAccessResponse.WwwAuthenticate ?? string.Empty);
        }

        /// <summary>
        /// Null for anything that is not a readable application/problem+json body. Strict on the
        /// media type on purpose: that is the contract, and a body that merely looks like a problem
        /// is exactly what an intermediary's error page might do.
        /// </summary>
        private static ConsumerAccessProblemDetails ReadProblemDetails(ConsumerAccessResponse consumerAccessResponse)
        {
            if (IsMediaType(consumerAccessResponse, ProblemMediaType) is false
                || string.IsNullOrWhiteSpace(consumerAccessResponse.Content))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<ConsumerAccessProblemDetails>(
                    consumerAccessResponse.Content,
                    JsonSerializerOptions.Web);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// An access decision is an application/json object carrying isAccessAllowed - the Access
        /// body a 200 carries. Anything else on a 403 did not decide anything about the consumer.
        /// </summary>
        private static bool IsAccessDecision(ConsumerAccessResponse consumerAccessResponse)
        {
            if (IsMediaType(consumerAccessResponse, AccessMediaType) is false
                || string.IsNullOrWhiteSpace(consumerAccessResponse.Content))
            {
                return false;
            }

            try
            {
                using JsonDocument jsonDocument = JsonDocument.Parse(consumerAccessResponse.Content);

                return jsonDocument.RootElement.ValueKind is JsonValueKind.Object
                    && jsonDocument.RootElement.EnumerateObject().Any(property =>
                        string.Equals(property.Name, "isAccessAllowed", StringComparison.OrdinalIgnoreCase));
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static bool IsMediaType(ConsumerAccessResponse consumerAccessResponse, string mediaType) =>
            string.Equals(consumerAccessResponse.ContentType, mediaType, StringComparison.OrdinalIgnoreCase);

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
