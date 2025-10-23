// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Consumers
{
    public partial class ConsumerService
    {
        private async ValueTask ValidateConsumerOnAdd(Consumer consumer)
        {
            ValidateConsumerIsNotNull(consumer);
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate<InvalidConsumerServiceException>(
                message: "Invalid consumer. Please correct the errors and try again.",
                (Rule: IsInvalid(consumer.Id), Parameter: nameof(Consumer.Id)),
                (Rule: IsInvalid(consumer.UserId), Parameter: nameof(Consumer.UserId)),
                (Rule: IsInvalid(consumer.Name), Parameter: nameof(Consumer.Name)),
                (Rule: IsInvalid(consumer.CreatedDate), Parameter: nameof(Consumer.CreatedDate)),
                (Rule: IsInvalid(consumer.CreatedBy), Parameter: nameof(Consumer.CreatedBy)),
                (Rule: IsInvalid(consumer.UpdatedDate), Parameter: nameof(Consumer.UpdatedDate)),
                (Rule: IsInvalid(consumer.UpdatedBy), Parameter: nameof(Consumer.UpdatedBy)),
                (Rule: IsGreaterThan(consumer.UserId, 255), Parameter: nameof(Consumer.UserId)),
                (Rule: IsGreaterThan(consumer.Name, 255), Parameter: nameof(Consumer.Name)),
                (Rule: IsGreaterThan(consumer.CreatedBy, 255), Parameter: nameof(Consumer.CreatedBy)),
                (Rule: IsGreaterThan(consumer.UpdatedBy, 255), Parameter: nameof(Consumer.UpdatedBy)),

                (Rule: IsNotSame(
                    firstDate: consumer.UpdatedDate,
                    secondDate: consumer.CreatedDate,
                    secondDateName: nameof(Consumer.CreatedDate)),
                Parameter: nameof(Consumer.UpdatedDate)),

                (Rule: IsNotSame(
                    first: currentUserId,
                    second: consumer.CreatedBy),
                Parameter: nameof(Consumer.CreatedBy)),

                (Rule: IsNotSame(
                    first: consumer.UpdatedBy,
                    second: consumer.CreatedBy,
                    secondName: nameof(Consumer.CreatedBy)),
                Parameter: nameof(Consumer.UpdatedBy)),

                (Rule: await IsNotRecentAsync(consumer.CreatedDate), Parameter: nameof(Consumer.CreatedDate)));
        }

        private async ValueTask ValidateConsumerOnModify(Consumer consumer)
        {
            ValidateConsumerIsNotNull(consumer);
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate<InvalidConsumerServiceException>(
                message: "Invalid consumer. Please correct the errors and try again.",
                (Rule: IsInvalid(consumer.Id), Parameter: nameof(Consumer.Id)),
                (Rule: IsInvalid(consumer.UserId), Parameter: nameof(Consumer.UserId)),
                (Rule: IsInvalid(consumer.Name), Parameter: nameof(Consumer.Name)),
                (Rule: IsInvalid(consumer.CreatedDate), Parameter: nameof(Consumer.CreatedDate)),
                (Rule: IsInvalid(consumer.CreatedBy), Parameter: nameof(Consumer.CreatedBy)),
                (Rule: IsInvalid(consumer.UpdatedDate), Parameter: nameof(Consumer.UpdatedDate)),
                (Rule: IsInvalid(consumer.UpdatedBy), Parameter: nameof(Consumer.UpdatedBy)),
                (Rule: IsGreaterThan(consumer.UserId, 255), Parameter: nameof(Consumer.UserId)),
                (Rule: IsGreaterThan(consumer.Name, 255), Parameter: nameof(Consumer.Name)),
                (Rule: IsGreaterThan(consumer.CreatedBy, 255), Parameter: nameof(Consumer.CreatedBy)),
                (Rule: IsGreaterThan(consumer.UpdatedBy, 255), Parameter: nameof(Consumer.UpdatedBy)),

                (Rule: IsNotSame(
                    first: currentUserId,
                    second: consumer.UpdatedBy),
                Parameter: nameof(Consumer.UpdatedBy)),

                (Rule: IsSame(
                    firstDate: consumer.UpdatedDate,
                    secondDate: consumer.CreatedDate,
                    secondDateName: nameof(Consumer.CreatedDate)),
                Parameter: nameof(Consumer.UpdatedDate)),

                (Rule: await IsNotRecentAsync(consumer.UpdatedDate), Parameter: nameof(Consumer.UpdatedDate)));
        }

        private static void ValidateConsumerId(Guid consumerId) =>
            Validate<InvalidConsumerServiceException>(
                message: "Invalid consumer. Please correct the errors and try again.",
                validations: (Rule: IsInvalid(consumerId), Parameter: nameof(Consumer.Id)));

        private static void ValidateStorageConsumer(Consumer maybeConsumer, Guid consumerId)
        {
            if (maybeConsumer is null)
            {
                throw new NotFoundConsumerServiceException(message: $"Couldn't find consumer with consumerId: {consumerId}.");
            }
        }

        private static void ValidateConsumerIsNotNull(Consumer consumer)
        {
            if (consumer is null)
            {
                throw new NullConsumerServiceException(message: "Consumer is null.");
            }
        }

        private static void ValidateAgainstStorageConsumerOnModify(
            Consumer inputConsumer,
            Consumer storageConsumer)
        {
            Validate<InvalidConsumerServiceException>(
                message: "Invalid consumer. Please correct the errors and try again.",
                (Rule: IsNotSame(
                        firstDate: inputConsumer.CreatedDate,
                        secondDate: storageConsumer.CreatedDate,
                        secondDateName: nameof(Consumer.CreatedDate)),
                    Parameter: nameof(Consumer.CreatedDate)),

                (Rule: IsNotSame(
                        first: inputConsumer.CreatedBy,
                        second: storageConsumer.CreatedBy,
                        secondName: nameof(Consumer.CreatedBy)),
                    Parameter: nameof(Consumer.CreatedBy)),

                (Rule: IsSame(
                        firstDate: inputConsumer.UpdatedDate,
                        secondDate: storageConsumer.UpdatedDate,
                        secondDateName: nameof(Consumer.UpdatedDate)),
                    Parameter: nameof(Consumer.UpdatedDate)));
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalid(string text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static dynamic IsGreaterThan(string text, int maxLength) => new
        {
            Condition = IsExceedingLength(text, maxLength),
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static bool IsExceedingLength(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static dynamic IsSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private static dynamic IsNotSame(string first, string second) => new
        {
            Condition = first != second,
            Message = $"Expected value to be '{first}' but found '{second}'."
        };

        private static dynamic IsNotSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate != secondDate,
                Message = $"Date is not the same as {secondDateName}"
            };

        private static dynamic IsNotSame(string first, string second, string secondName) => new
        {
            Condition = first != second,
            Message = $"Text is not the same as {secondName}"
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

        private static void Validate<T>(string message, params (dynamic Rule, string Parameter)[] validations)
            where T : Xeption
        {
            var invalidDataException = (T)Activator.CreateInstance(typeof(T), message);

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
