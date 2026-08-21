// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using LondonFhirService.Core.Abstractions.Models.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Models.Audits.Exceptions;
using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Audits
{
    internal partial class AuditService
    {
        private static void ValidateAuditOnAdd(IAudit audit)
        {
            ValidateAuditIsNotNull(audit);

            Validate(
                createException: () => new InvalidAuditException(
                    "Invalid audit. Please correct the errors and try again."),

                (Rule: IsInvalid(audit.Id), Parameter: nameof(IAudit.Id)),
                (Rule: IsInvalid(audit.AuditType), Parameter: nameof(IAudit.AuditType)),
                (Rule: IsInvalid(audit.CreatedDate), Parameter: nameof(IAudit.CreatedDate)),
                (Rule: IsInvalid(audit.UpdatedDate), Parameter: nameof(IAudit.UpdatedDate)),
                (Rule: IsGreaterThan(audit.CorrelationId, 255), Parameter: nameof(IAudit.CorrelationId)),
                (Rule: IsGreaterThan(audit.AuditType, 255), Parameter: nameof(IAudit.AuditType)),
                (Rule: IsGreaterThan(audit.LogLevel, 255), Parameter: nameof(IAudit.LogLevel)),
                (Rule: IsGreaterThan(audit.FileName, 1000), Parameter: nameof(IAudit.FileName)),
                (Rule: IsGreaterThan(audit.CreatedBy, 255), Parameter: nameof(IAudit.CreatedBy)),
                (Rule: IsGreaterThan(audit.UpdatedBy, 255), Parameter: nameof(IAudit.UpdatedBy)));
        }

        private static void ValidateAuditOnModify(IAudit audit)
        {
            ValidateAuditIsNotNull(audit);

            Validate(
                createException: () => new InvalidAuditException(
                    "Invalid audit. Please correct the errors and try again."),

                (Rule: IsInvalid(audit.Id), Parameter: nameof(IAudit.Id)),
                (Rule: IsInvalid(audit.AuditType), Parameter: nameof(IAudit.AuditType)),
                (Rule: IsInvalid(audit.UpdatedDate), Parameter: nameof(IAudit.UpdatedDate)),
                (Rule: IsGreaterThan(audit.CorrelationId, 255), Parameter: nameof(IAudit.CorrelationId)),
                (Rule: IsGreaterThan(audit.AuditType, 255), Parameter: nameof(IAudit.AuditType)),
                (Rule: IsGreaterThan(audit.LogLevel, 255), Parameter: nameof(IAudit.LogLevel)),
                (Rule: IsGreaterThan(audit.FileName, 1000), Parameter: nameof(IAudit.FileName)),
                (Rule: IsGreaterThan(audit.UpdatedBy, 255), Parameter: nameof(IAudit.UpdatedBy)));
        }

        private static void ValidateAuditId(Guid auditId) =>
            Validate(
                createException: () => new InvalidAuditException(
                    "Invalid audit. Please correct the errors and try again."),

                validations: (Rule: IsInvalid(auditId), Parameter: nameof(IAudit.Id)));

        private static void ValidateBatchSize(int batchSize) =>
            Validate(
                createException: () => new InvalidAuditException(
                    "Invalid audit. Please correct the errors and try again."),

                validations: (Rule: IsNotPositive(batchSize), Parameter: "BatchSize"));

        private static void ValidateStorageAudit(IAudit maybeAudit, Guid auditId)
        {
            if (maybeAudit is null)
            {
                throw new NotFoundAuditException(
                    message: $"Couldn't find audit with auditId: {auditId}.");
            }
        }

        private static void ValidateAuditIsNotNull(IAudit audit)
        {
            if (audit is null)
            {
                throw new NullAuditException(message: "Audit is null.");
            }
        }

        private static void ValidateAuditsIsNotNull(List<IAudit> audits)
        {
            if (audits is null)
            {
                throw new NullAuditException(message: "Audits is null.");
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
            Condition = (text ?? string.Empty).Length > maxLength,
            Message = $"Text exceeds max length of {maxLength} characters"
        };

        private static dynamic IsNotPositive(int number) => new
        {
            Condition = number <= 0,
            Message = "Value is expected to be greater than zero"
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
