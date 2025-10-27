// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public partial class ProviderService
    {
        private async ValueTask ValidateProviderOnAdd(Provider provider)
        {
            ValidateProviderIsNotNull(provider);
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                createException: () => new InvalidProviderServiceException(
                    message: "Invalid provider. Please correct the errors and try again."),

(Rule: IsInvalid(provider.Id), Parameter: nameof(Provider.Id)),
                (Rule: IsInvalid(provider.Name), Parameter: nameof(Provider.Name)),
                (Rule: IsInvalid(provider.CreatedDate), Parameter: nameof(Provider.CreatedDate)),
                (Rule: IsInvalid(provider.CreatedBy), Parameter: nameof(Provider.CreatedBy)),
                (Rule: IsInvalid(provider.UpdatedDate), Parameter: nameof(Provider.UpdatedDate)),
                (Rule: IsInvalid(provider.UpdatedBy), Parameter: nameof(Provider.UpdatedBy)),
                (Rule: IsGreaterThan(provider.Name, 500), Parameter: nameof(Provider.Name)),
                (Rule: IsGreaterThan(provider.CreatedBy, 255), Parameter: nameof(Provider.CreatedBy)),
                (Rule: IsGreaterThan(provider.UpdatedBy, 255), Parameter: nameof(Provider.UpdatedBy)),

                (Rule: IsNotSame(
                    firstDate: provider.UpdatedDate,
                    secondDate: provider.CreatedDate,
                    secondDateName: nameof(Provider.CreatedDate)),
                Parameter: nameof(Provider.UpdatedDate)),

                (Rule: IsNotSame(
                    first: currentUserId,
                    second: provider.CreatedBy),
                Parameter: nameof(Provider.CreatedBy)),

                (Rule: IsNotSame(
                    first: provider.UpdatedBy,
                    second: provider.CreatedBy,
                    secondName: nameof(Provider.CreatedBy)),
                Parameter: nameof(Provider.UpdatedBy)),

                (Rule: await IsNotRecentAsync(provider.CreatedDate), Parameter: nameof(Provider.CreatedDate)));
        }

        private static void ValidateProviderIsNotNull(Provider provider)
        {
            if (provider is null)
            {
                throw new NullProviderServiceException(message: "Provider is null.");
            }
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
