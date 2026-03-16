// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.FhirRecords
{
    public partial class FhirRecordService
    {
        private async ValueTask ValidateFhirRecordOnAdd(FhirRecord fhirRecord)
        {
            ValidateFhirRecordIsNotNull(fhirRecord);
            User currentUser = await this.securityBroker.GetCurrentUserAsync();

            Validate(
                createException: () => new InvalidFhirRecordException(
                    "Invalid fhirRecord. Please correct the errors and try again."),

                (Rule: IsInvalid(fhirRecord.Id), Parameter: nameof(FhirRecord.Id)),
                (Rule: IsInvalid(fhirRecord.CorrelationId), Parameter: nameof(FhirRecord.CorrelationId)),
                (Rule: IsInvalid(fhirRecord.JsonPayload), Parameter: nameof(FhirRecord.JsonPayload)),
                (Rule: IsInvalid(fhirRecord.SourceName), Parameter: nameof(FhirRecord.SourceName)),
                (Rule: IsInvalid(fhirRecord.CreatedDate), Parameter: nameof(FhirRecord.CreatedDate)),
                (Rule: IsInvalid(fhirRecord.CreatedBy), Parameter: nameof(FhirRecord.CreatedBy)),
                (Rule: IsInvalid(fhirRecord.UpdatedDate), Parameter: nameof(FhirRecord.UpdatedDate)),
                (Rule: IsInvalid(fhirRecord.UpdatedBy), Parameter: nameof(FhirRecord.UpdatedBy)),
                (Rule: IsGreaterThan(fhirRecord.CorrelationId, 255), Parameter: nameof(FhirRecord.CorrelationId)),
                (Rule: IsGreaterThan(fhirRecord.SourceName, 450), Parameter: nameof(FhirRecord.SourceName)),
                (Rule: IsGreaterThan(fhirRecord.CreatedBy, 255), Parameter: nameof(FhirRecord.CreatedBy)),
                (Rule: IsGreaterThan(fhirRecord.UpdatedBy, 255), Parameter: nameof(FhirRecord.UpdatedBy)),

                (Rule: IsNotSame(
                    firstDate: fhirRecord.UpdatedDate,
                    secondDate: fhirRecord.CreatedDate,
                    secondDateName: nameof(FhirRecord.CreatedDate)),
                Parameter: nameof(FhirRecord.UpdatedDate)),

                (Rule: IsNotSame(
                    first: currentUser.UserId,
                    second: fhirRecord.CreatedBy),
                Parameter: nameof(FhirRecord.CreatedBy)),

                (Rule: IsNotSame(
                    first: fhirRecord.UpdatedBy,
                    second: fhirRecord.CreatedBy,
                    secondName: nameof(FhirRecord.CreatedBy)),
                Parameter: nameof(FhirRecord.UpdatedBy)),

                (Rule: await IsNotRecentAsync(fhirRecord.CreatedDate), Parameter: nameof(FhirRecord.CreatedDate)));
        }

        private async ValueTask ValidateFhirRecordOnModify(FhirRecord fhirRecord)
        {
            ValidateFhirRecordIsNotNull(fhirRecord);
            User currentUser = await this.securityBroker.GetCurrentUserAsync();

            Validate(
                createException: () => new InvalidFhirRecordException(
                    "Invalid fhirRecord. Please correct the errors and try again."),

                (Rule: IsInvalid(fhirRecord.Id), Parameter: nameof(FhirRecord.Id)),
                (Rule: IsInvalid(fhirRecord.CorrelationId), Parameter: nameof(FhirRecord.CorrelationId)),
                (Rule: IsInvalid(fhirRecord.JsonPayload), Parameter: nameof(FhirRecord.JsonPayload)),
                (Rule: IsInvalid(fhirRecord.SourceName), Parameter: nameof(FhirRecord.SourceName)),
                (Rule: IsInvalid(fhirRecord.CreatedDate), Parameter: nameof(FhirRecord.CreatedDate)),
                (Rule: IsInvalid(fhirRecord.CreatedBy), Parameter: nameof(FhirRecord.CreatedBy)),
                (Rule: IsInvalid(fhirRecord.UpdatedDate), Parameter: nameof(FhirRecord.UpdatedDate)),
                (Rule: IsInvalid(fhirRecord.UpdatedBy), Parameter: nameof(FhirRecord.UpdatedBy)),
                (Rule: IsGreaterThan(fhirRecord.CorrelationId, 255), Parameter: nameof(FhirRecord.CorrelationId)),
                (Rule: IsGreaterThan(fhirRecord.SourceName, 450), Parameter: nameof(FhirRecord.SourceName)),
                (Rule: IsGreaterThan(fhirRecord.CreatedBy, 255), Parameter: nameof(FhirRecord.CreatedBy)),
                (Rule: IsGreaterThan(fhirRecord.UpdatedBy, 255), Parameter: nameof(FhirRecord.UpdatedBy)),

                 (Rule: IsNotSame(
                    first: currentUser.UserId,
                    second: fhirRecord.UpdatedBy),
                Parameter: nameof(FhirRecord.UpdatedBy)),

                (Rule: IsSame(
                    firstDate: fhirRecord.UpdatedDate,
                    secondDate: fhirRecord.CreatedDate,
                    secondDateName: nameof(FhirRecord.CreatedDate)),
                Parameter: nameof(FhirRecord.UpdatedDate)),

                (Rule: await IsNotRecentAsync(fhirRecord.UpdatedDate), Parameter: nameof(FhirRecord.UpdatedDate)));
        }

        public void ValidateFhirRecordId(Guid fhirRecordId) =>
            Validate(
                createException: () => new InvalidFhirRecordException(
                    "Invalid fhirRecord. Please correct the errors and try again."),

                validations: (Rule: IsInvalid(fhirRecordId), Parameter: nameof(FhirRecord.Id)));

        private static void ValidateStorageFhirRecord(FhirRecord maybeFhirRecord, Guid fhirRecordId)
        {
            if (maybeFhirRecord is null)
            {
                throw new NotFoundFhirRecordException(
                    message: $"Couldn't find fhirRecord with fhirRecordId: {fhirRecordId}.");
            }
        }

        private static void ValidateFhirRecordIsNotNull(FhirRecord fhirRecord)
        {
            if (fhirRecord is null)
            {
                throw new NullFhirRecordException(message: "FhirRecord is null.");
            }
        }

        private static void ValidateAgainstStorageFhirRecordOnModify(
            FhirRecord inputFhirRecord,
            FhirRecord storageFhirRecord)
        {
            Validate(
                createException: () => new InvalidFhirRecordException(
                    "Invalid fhirRecord. Please correct the errors and try again."),

                (Rule: IsNotSame(
                    firstDate: inputFhirRecord.CreatedDate,
                    secondDate: storageFhirRecord.CreatedDate,
                    secondDateName: nameof(FhirRecord.CreatedDate)),
                Parameter: nameof(FhirRecord.CreatedDate)),

                (Rule: IsNotSame(
                    first: inputFhirRecord.CreatedBy,
                    second: storageFhirRecord.CreatedBy,
                    secondName: nameof(FhirRecord.CreatedBy)),
                Parameter: nameof(FhirRecord.CreatedBy)),

                (Rule: IsSame(
                    firstDate: inputFhirRecord.UpdatedDate,
                    secondDate: storageFhirRecord.UpdatedDate,
                    secondDateName: nameof(FhirRecord.UpdatedDate)),
                Parameter: nameof(FhirRecord.UpdatedDate)));
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