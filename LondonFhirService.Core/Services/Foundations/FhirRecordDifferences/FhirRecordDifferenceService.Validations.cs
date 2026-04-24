// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.FhirRecordDifferences
{
    public partial class FhirRecordDifferenceService
    {
        private async ValueTask ValidateFhirRecordDifferenceOnAdd(FhirRecordDifference fhirRecordDifference)
        {
            ValidateFhirRecordDifferenceIsNotNull(fhirRecordDifference);
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                createException: () => new InvalidFhirRecordDifferenceException(
                    "Invalid fhirRecordDifference. Please correct the errors and try again."),

                (Rule: IsInvalid(fhirRecordDifference.Id), Parameter: nameof(FhirRecordDifference.Id)),
                (Rule: IsInvalid(fhirRecordDifference.PrimaryId), Parameter: nameof(FhirRecordDifference.PrimaryId)),
                (Rule: IsInvalid(fhirRecordDifference.SecondaryId), Parameter: nameof(FhirRecordDifference.SecondaryId)),
                (Rule: IsInvalid(fhirRecordDifference.CorrelationId), Parameter: nameof(FhirRecordDifference.CorrelationId)),
                (Rule: IsInvalid(fhirRecordDifference.DiffJson), Parameter: nameof(FhirRecordDifference.DiffJson)),
                (Rule: IsInvalid(fhirRecordDifference.CreatedDate), Parameter: nameof(FhirRecordDifference.CreatedDate)),
                (Rule: IsInvalid(fhirRecordDifference.CreatedBy), Parameter: nameof(FhirRecordDifference.CreatedBy)),
                (Rule: IsInvalid(fhirRecordDifference.UpdatedDate), Parameter: nameof(FhirRecordDifference.UpdatedDate)),
                (Rule: IsInvalid(fhirRecordDifference.UpdatedBy), Parameter: nameof(FhirRecordDifference.UpdatedBy)),
                (Rule: IsGreaterThan(fhirRecordDifference.CorrelationId, 255), Parameter: nameof(FhirRecordDifference.CorrelationId)),
                (Rule: IsGreaterThan(fhirRecordDifference.CreatedBy, 255), Parameter: nameof(FhirRecordDifference.CreatedBy)),
                (Rule: IsGreaterThan(fhirRecordDifference.UpdatedBy, 255), Parameter: nameof(FhirRecordDifference.UpdatedBy)),

                (Rule: IsNotSame(
                    firstDate: fhirRecordDifference.UpdatedDate,
                    secondDate: fhirRecordDifference.CreatedDate,
                    secondDateName: nameof(FhirRecordDifference.CreatedDate)),
                Parameter: nameof(FhirRecordDifference.UpdatedDate)),

                (Rule: IsNotSame(
                    first: currentUserId,
                    second: fhirRecordDifference.CreatedBy),
                Parameter: nameof(FhirRecordDifference.CreatedBy)),

                (Rule: IsNotSame(
                    first: fhirRecordDifference.UpdatedBy,
                    second: fhirRecordDifference.CreatedBy,
                    secondName: nameof(FhirRecordDifference.CreatedBy)),
                Parameter: nameof(FhirRecordDifference.UpdatedBy)),

                (Rule: await IsNotRecentAsync(fhirRecordDifference.CreatedDate), Parameter: nameof(FhirRecordDifference.CreatedDate)));
        }

        private async ValueTask ValidateFhirRecordDifferenceOnModify(FhirRecordDifference fhirRecordDifference)
        {
            ValidateFhirRecordDifferenceIsNotNull(fhirRecordDifference);
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                createException: () => new InvalidFhirRecordDifferenceException(
                    "Invalid fhirRecordDifference. Please correct the errors and try again."),

                (Rule: IsInvalid(fhirRecordDifference.Id), Parameter: nameof(FhirRecordDifference.Id)),
                (Rule: IsInvalid(fhirRecordDifference.PrimaryId), Parameter: nameof(FhirRecordDifference.PrimaryId)),
                (Rule: IsInvalid(fhirRecordDifference.SecondaryId), Parameter: nameof(FhirRecordDifference.SecondaryId)),
                (Rule: IsInvalid(fhirRecordDifference.CorrelationId), Parameter: nameof(FhirRecordDifference.CorrelationId)),
                (Rule: IsInvalid(fhirRecordDifference.DiffJson), Parameter: nameof(FhirRecordDifference.DiffJson)),
                (Rule: IsInvalid(fhirRecordDifference.CreatedDate), Parameter: nameof(FhirRecordDifference.CreatedDate)),
                (Rule: IsInvalid(fhirRecordDifference.CreatedBy), Parameter: nameof(FhirRecordDifference.CreatedBy)),
                (Rule: IsInvalid(fhirRecordDifference.UpdatedDate), Parameter: nameof(FhirRecordDifference.UpdatedDate)),
                (Rule: IsInvalid(fhirRecordDifference.UpdatedBy), Parameter: nameof(FhirRecordDifference.UpdatedBy)),
                (Rule: IsGreaterThan(fhirRecordDifference.CorrelationId, 255), Parameter: nameof(FhirRecordDifference.CorrelationId)),
                (Rule: IsGreaterThan(fhirRecordDifference.CreatedBy, 255), Parameter: nameof(FhirRecordDifference.CreatedBy)),
                (Rule: IsGreaterThan(fhirRecordDifference.UpdatedBy, 255), Parameter: nameof(FhirRecordDifference.UpdatedBy)),

                 (Rule: IsNotSame(
                    first: currentUserId,
                    second: fhirRecordDifference.UpdatedBy),
                Parameter: nameof(FhirRecordDifference.UpdatedBy)),

                (Rule: IsSame(
                    firstDate: fhirRecordDifference.UpdatedDate,
                    secondDate: fhirRecordDifference.CreatedDate,
                    secondDateName: nameof(FhirRecordDifference.CreatedDate)),
                Parameter: nameof(FhirRecordDifference.UpdatedDate)),

                (Rule: await IsNotRecentAsync(fhirRecordDifference.UpdatedDate), Parameter: nameof(FhirRecordDifference.UpdatedDate)));
        }

        private void ValidateFhirRecordDifferenceId(Guid fhirRecordDifferenceId) =>
            Validate(
                createException: () => new InvalidFhirRecordDifferenceException(
                    "Invalid fhirRecordDifference. Please correct the errors and try again."),

                validations: (Rule: IsInvalid(fhirRecordDifferenceId), Parameter: nameof(FhirRecordDifference.Id)));

        private static void ValidateStorageFhirRecordDifference(FhirRecordDifference maybeFhirRecordDifference, Guid fhirRecordDifferenceId)
        {
            if (maybeFhirRecordDifference is null)
            {
                throw new NotFoundFhirRecordDifferenceException(
                    message: $"Couldn't find fhirRecordDifference with Id: {fhirRecordDifferenceId}.");
            }
        }

        private static void ValidateFhirRecordDifferenceIsNotNull(FhirRecordDifference fhirRecordDifference)
        {
            if (fhirRecordDifference is null)
            {
                throw new NullFhirRecordDifferenceException(message: "FhirRecordDifference is null.");
            }
        }

        private static void ValidateAgainstStorageFhirRecordDifferenceOnModify(
            FhirRecordDifference inputFhirRecordDifference,
            FhirRecordDifference storageFhirRecordDifference)
        {
            Validate(
                createException: () => new InvalidFhirRecordDifferenceException(
                    "Invalid fhirRecordDifference. Please correct the errors and try again."),

                (Rule: IsNotSame(
                    firstDate: inputFhirRecordDifference.CreatedDate,
                    secondDate: storageFhirRecordDifference.CreatedDate,
                    secondDateName: nameof(FhirRecordDifference.CreatedDate)),
                Parameter: nameof(FhirRecordDifference.CreatedDate)),

                (Rule: IsNotSame(
                    first: inputFhirRecordDifference.CreatedBy,
                    second: storageFhirRecordDifference.CreatedBy,
                    secondName: nameof(FhirRecordDifference.CreatedBy)),
                Parameter: nameof(FhirRecordDifference.CreatedBy)),

                (Rule: IsSame(
                    firstDate: inputFhirRecordDifference.UpdatedDate,
                    secondDate: storageFhirRecordDifference.UpdatedDate,
                    secondDateName: nameof(FhirRecordDifference.UpdatedDate)),
                Parameter: nameof(FhirRecordDifference.UpdatedDate)));
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

        private static dynamic IsGreaterThan(string text, int maxLength) => new
        {
            Condition = IsExceedingLength(text, maxLength),
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static bool IsExceedingLength(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static dynamic IsSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private static dynamic IsNotSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate != secondDate,
                Message = $"Date is not the same as {secondDateName}"
            };

        private static dynamic IsNotSame(
            string first,
            string second) => new
            {
                Condition = first != second,
                Message = $"Expected value to be '{first}' but found '{second}'."
            };

        private static dynamic IsNotSame(
            Guid firstId,
            Guid secondId,
            string secondIdName) => new
            {
                Condition = firstId != secondId,
                Message = $"Id is not the same as {secondIdName}"
            };

        private static dynamic IsNotSame(
           string first,
           string second,
           string secondName) => new
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