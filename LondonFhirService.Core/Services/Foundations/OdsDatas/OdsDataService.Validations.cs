// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.OdsDatas
{
    public partial class OdsDataService
    {
        private async ValueTask ValidateOdsDataOnAddAsync(OdsData odsData)
        {
            ValidateOdsDataIsNotNull(odsData);

            Validate(
                createException: () => new InvalidOdsDataException(
                    message: "Invalid odsData. Please correct the errors and try again."),

                (Rule: IsInvalid(odsData.Id), Parameter: nameof(OdsData.Id)),
                (Rule: IsInvalid(odsData.OrganisationCode), Parameter: nameof(OdsData.OrganisationCode)));
        }

        private async ValueTask ValidateOdsDataOnModifyAsync(OdsData odsData)
        {
            ValidateOdsDataIsNotNull(odsData);

            Validate(
                createException: () => new InvalidOdsDataException(
                    message: "Invalid odsData. Please correct the errors and try again."),

                (Rule: IsInvalid(odsData.Id), Parameter: nameof(OdsData.Id)),
                (Rule: IsInvalid(odsData.OrganisationCode), Parameter: nameof(OdsData.OrganisationCode)));
        }

        public static void ValidateOdsDataId(Guid odsDataId) =>
            Validate(
                createException: () => new InvalidOdsDataException(
                    message: "Invalid odsData. Please correct the errors and try again."),

                (Rule: IsInvalid(odsDataId), Parameter: nameof(OdsData.Id)));

        private static void ValidateStorageOdsData(OdsData maybeOdsData, Guid odsDataId)
        {
            if (maybeOdsData is null)
            {
                throw new NotFoundOdsDataException(message: $"OdsData not found with Id: {odsDataId}");
            }
        }

        private static void ValidateOdsDataIsNotNull(OdsData odsData)
        {
            if (odsData is null)
            {
                throw new NullOdsDataException(message: "OdsData is null.");
            }
        }

        private static void ValidateAgainstStorageOdsDataOnModify(OdsData inputOdsData, OdsData storageOdsData)
        { }

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

        private static dynamic IsInvalidLength(string text, int maxLength) => new
        {
            Condition = IsExceedingLength(text, maxLength),
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static bool IsExceedingLength(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is invalid"
        };

        private static dynamic IsSameAs(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private static dynamic IsNotSame(
            string first,
            string second,
            string secondName) => new
            {
                Condition = first != second,
                Message = $"Text is not the same as {secondName}"
            };

        private static dynamic IsNotSame(
            DateTimeOffset first,
            DateTimeOffset second,
            string secondName) => new
            {
                Condition = first != second,
                Message = $"Date is not the same as {secondName}"
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