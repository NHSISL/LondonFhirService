// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;

namespace LondonFhirService.Core.Services.Foundations.ConsumerAccesses
{
    public partial class ConsumerAccessService
    {
        private async ValueTask ValidateConsumerAccessOnAddAsync(ConsumerAccess consumerAccess)
        {
            ValidateConsumerAccessIsNotNull(consumerAccess);
            User currentUser = await this.securityBroker.GetCurrentUserAsync();

            Validate(
                (Rule: IsInvalid(consumerAccess.Id), Parameter: nameof(ConsumerAccess.Id)),
                (Rule: IsInvalid(consumerAccess.ConsumerId), Parameter: nameof(ConsumerAccess.ConsumerId)),
                (Rule: IsInvalid(consumerAccess.OrgCode), Parameter: nameof(ConsumerAccess.OrgCode)),
                (Rule: IsInvalid(consumerAccess.CreatedBy), Parameter: nameof(ConsumerAccess.CreatedBy)),
                (Rule: IsInvalid(consumerAccess.UpdatedBy), Parameter: nameof(ConsumerAccess.UpdatedBy)),
                (Rule: IsInvalid(consumerAccess.CreatedDate), Parameter: nameof(ConsumerAccess.CreatedDate)),
                (Rule: IsInvalid(consumerAccess.UpdatedDate), Parameter: nameof(ConsumerAccess.UpdatedDate)),
                (Rule: IsInvalidLength(consumerAccess.CreatedBy, 255), Parameter: nameof(ConsumerAccess.CreatedBy)),
                (Rule: IsInvalidLength(consumerAccess.UpdatedBy, 255), Parameter: nameof(ConsumerAccess.UpdatedBy)),
                (Rule: IsInvalidLength(consumerAccess.OrgCode, 15), Parameter: nameof(ConsumerAccess.OrgCode)),

                (Rule: IsNotSame(
                    first: consumerAccess.UpdatedBy,
                    second: consumerAccess.CreatedBy,
                    secondName: nameof(ConsumerAccess.CreatedBy)),
                Parameter: nameof(ConsumerAccess.UpdatedBy)),

                (Rule: IsNotSame(
                    first: currentUser.UserId,
                    second: consumerAccess.CreatedBy),
                Parameter: nameof(ConsumerAccess.CreatedBy)),

                (Rule: IsNotSame(
                    first: consumerAccess.CreatedDate,
                    second: consumerAccess.UpdatedDate,
                    secondName: nameof(ConsumerAccess.CreatedDate)),
                Parameter: nameof(ConsumerAccess.UpdatedDate)),

                (Rule: await IsNotRecentAsync(consumerAccess.CreatedDate),
                Parameter: nameof(ConsumerAccess.CreatedDate)));
        }

        private static void ValidateConsumerAccessOnRetrieveById(Guid consumerAccessId)
        {
            Validate(
                (Rule: IsInvalid(consumerAccessId), Parameter: nameof(ConsumerAccess.Id)));
        }

        private async ValueTask ValidateConsumerAccessOnModifyAsync(ConsumerAccess consumerAccess)
        {
            ValidateConsumerAccessIsNotNull(consumerAccess);
            User currentUser = await this.securityBroker.GetCurrentUserAsync();

            Validate(
                (Rule: IsInvalid(consumerAccess.Id), Parameter: nameof(ConsumerAccess.Id)),
                (Rule: IsInvalid(consumerAccess.ConsumerId), Parameter: nameof(ConsumerAccess.ConsumerId)),
                (Rule: IsInvalid(consumerAccess.OrgCode), Parameter: nameof(ConsumerAccess.OrgCode)),
                (Rule: IsInvalid(consumerAccess.CreatedBy), Parameter: nameof(ConsumerAccess.CreatedBy)),
                (Rule: IsInvalid(consumerAccess.UpdatedBy), Parameter: nameof(ConsumerAccess.UpdatedBy)),
                (Rule: IsInvalid(consumerAccess.CreatedDate), Parameter: nameof(ConsumerAccess.CreatedDate)),
                (Rule: IsInvalid(consumerAccess.UpdatedDate), Parameter: nameof(ConsumerAccess.UpdatedDate)),
                (Rule: IsInvalidLength(consumerAccess.CreatedBy, 255), Parameter: nameof(ConsumerAccess.CreatedBy)),
                (Rule: IsInvalidLength(consumerAccess.UpdatedBy, 255), Parameter: nameof(ConsumerAccess.UpdatedBy)),
                (Rule: IsInvalidLength(consumerAccess.OrgCode, 15), Parameter: nameof(ConsumerAccess.OrgCode)),

                (Rule: IsNotSame(
                    first: currentUser.UserId,
                    second: consumerAccess.UpdatedBy),
                Parameter: nameof(ConsumerAccess.UpdatedBy)),

                (Rule: IsSameAs(
                    createdDate: consumerAccess.CreatedDate,
                    updatedDate: consumerAccess.UpdatedDate,
                    createdDateName: nameof(ConsumerAccess.CreatedDate)),
                Parameter: nameof(ConsumerAccess.UpdatedDate)),

                (Rule: await IsNotRecentAsync(consumerAccess.UpdatedDate),
                Parameter: nameof(ConsumerAccess.UpdatedDate)));
        }

        private static void ValidateConsumerAccessOnRemoveById(Guid consumerAccessId) =>
            Validate((Rule: IsInvalid(consumerAccessId), Parameter: nameof(ConsumerAccess.Id)));

        private static void ValidateOnRetrieveAllOrganisationUserHasAccessTo(Guid consumerId) =>
            Validate((Rule: IsInvalid(consumerId), Parameter: nameof(ConsumerAccess.ConsumerId)));

        private static void ValidateStorageConsumerAccess(ConsumerAccess maybeConsumerAccess, Guid maybeId)
        {
            if (maybeConsumerAccess is null)
            {
                throw new NotFoundConsumerAccessException($"Consumer access not found with Id: {maybeId}");
            }
        }

        private async ValueTask ValidateAgainstStorageConsumerAccessOnModifyAsync(
            ConsumerAccess consumerAccess,
            ConsumerAccess maybeConsumerAccess)
        {
            Validate(
                (Rule: IsNotSame(
                    consumerAccess.CreatedDate,
                    maybeConsumerAccess.CreatedDate,
                    nameof(maybeConsumerAccess.CreatedDate)),
                Parameter: nameof(ConsumerAccess.CreatedDate)),

                (Rule: IsSameAs(
                    consumerAccess.UpdatedDate,
                    maybeConsumerAccess.UpdatedDate,
                    nameof(maybeConsumerAccess.UpdatedDate)),
                Parameter: nameof(ConsumerAccess.UpdatedDate)));
        }

        private async ValueTask ValidateAgainstStorageConsumerAccessOnDeleteAsync(
            ConsumerAccess consumerAccess,
            ConsumerAccess maybeConsumerAccess)
        {
            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            Validate(
                (Rule: IsNotSame(
                    consumerAccess.CreatedDate,
                    maybeConsumerAccess.CreatedDate,
                    nameof(maybeConsumerAccess.CreatedDate)),
                Parameter: nameof(ConsumerAccess.CreatedDate)),

                (Rule: IsNotSame(
                    first: maybeConsumerAccess.UpdatedDate,
                    second: consumerAccess.UpdatedDate),
                Parameter: nameof(ConsumerAccess.UpdatedDate)));
        }

        private static void ValidateConsumerAccessIsNotNull(ConsumerAccess consumerAccess)
        {
            if (consumerAccess is null)
            {
                throw new NullConsumerAccessException("User access is null.");
            }
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is invalid"
        };

        private static dynamic IsInvalid(string name) => new
        {
            Condition = String.IsNullOrWhiteSpace(name),
            Message = "Text is invalid"
        };

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is invalid"
        };

        private static dynamic IsInvalidLength(string text, int maxLength) => new
        {
            Condition = IsExceedingLength(text, maxLength),
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static bool IsExceedingLength(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static dynamic IsNotSame(
            DateTimeOffset first,
            DateTimeOffset second) => new
            {
                Condition = first != second,
                Message = $"Expected value to be '{first}' but found '{second}'."
            };

        private static dynamic IsNotSame(
            DateTimeOffset first,
            DateTimeOffset second,
            string secondName) => new
            {
                Condition = first != second,
                Message = $"Date is not the same as {secondName}"
            };

        private static dynamic IsNotSame(
            string first,
            string second) => new
            {
                Condition = first != second,
                Message = $"Expected value to be '{first}' but found '{second}'."
            };

        private static dynamic IsNotSame(
            string first,
            string second,
            string secondName) => new
            {
                Condition = first != second,
                Message = $"Text is not the same as {secondName}"
            };


        private static dynamic IsSameAs(
            DateTimeOffset createdDate,
            DateTimeOffset updatedDate,
            string createdDateName) => new
            {
                Condition = createdDate == updatedDate,
                Message = $"Date is the same as {createdDateName}"
            };

        private async ValueTask<dynamic> IsNotRecentAsync(DateTimeOffset date)
        {
            var (isNotRecent, startDate, endDate) = await IsDateNotRecentAsync(date);

            return new
            {
                Condition = isNotRecent,
                Message = $"Date is not recent. Expected a value between {startDate} and {endDate} but found {date}"
            };
        }

        private async ValueTask<(bool IsNotRecent, DateTimeOffset StartDate, DateTimeOffset EndDate)>
            IsDateNotRecentAsync(DateTimeOffset date)
        {
            int pastThreshold = 90;
            int futureThreshold = 0;
            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            if (currentDateTime == default)
            {
                return (false, default, default);
            }

            DateTimeOffset startDate = currentDateTime.AddSeconds(-pastThreshold);
            DateTimeOffset endDate = currentDateTime.AddSeconds(futureThreshold);
            bool isNotRecent = date < startDate || date > endDate;

            return (isNotRecent, startDate, endDate);
        }

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidConsumerAccessException =
                new InvalidConsumerAccessException(
                    message: "Invalid consumer access. Please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidConsumerAccessException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidConsumerAccessException.ThrowIfContainsErrors();
        }
    }
}
