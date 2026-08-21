// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;
using Xeptions;

using LondonFhirService.Core.Abstractions.Brokers;

namespace LondonFhirService.Clients.AuditAndMetrics.Services.Foundations.Metrics
{
    internal partial class MetricService
    {
        private static void ValidateMetricOnAdd(IMetric metric)
        {
            ValidateMetricIsNotNull(metric);

            Validate(
                createException: () => new InvalidMetricException(
                    "Invalid metric. Please correct the errors and try again."),

                (Rule: IsInvalid(metric.Id), Parameter: nameof(IMetric.Id)),
                (Rule: IsInvalid(metric.CorrelationId), Parameter: nameof(IMetric.CorrelationId)),
                (Rule: IsInvalid(metric.Method), Parameter: nameof(IMetric.Method)),
                (Rule: IsInvalid(metric.Name), Parameter: nameof(IMetric.Name)),
                (Rule: IsInvalid(metric.Started), Parameter: nameof(IMetric.Started)),
                (Rule: IsInvalid(metric.Completed), Parameter: nameof(IMetric.Completed)),
                (Rule: IsInvalid(metric.CreatedDate), Parameter: nameof(IMetric.CreatedDate)),
                (Rule: IsInvalid(metric.Type), Parameter: nameof(IMetric.Type)),
                (Rule: IsInvalid(metric.Status), Parameter: nameof(IMetric.Status)),
                (Rule: IsGreaterThan(metric.Method, 255), Parameter: nameof(IMetric.Method)),
                (Rule: IsGreaterThan(metric.Name, 255), Parameter: nameof(IMetric.Name)),
                (Rule: IsGreaterThan(metric.Target, 255), Parameter: nameof(IMetric.Target)),
                (Rule: IsGreaterThan(metric.ErrorCode, 100), Parameter: nameof(IMetric.ErrorCode)),
                (Rule: IsGreaterThan(metric.Consumer, 255), Parameter: nameof(IMetric.Consumer)),
                (Rule: IsGreaterThan(metric.Description, 1000), Parameter: nameof(IMetric.Description)),
                (Rule: IsNegative(metric.DurationMs), Parameter: nameof(IMetric.DurationMs)),
                (Rule: IsNegative(metric.PayloadBytes), Parameter: nameof(IMetric.PayloadBytes)),

                (Rule: IsBefore(
                    firstDate: metric.Completed,
                    secondDate: metric.Started,
                    secondDateName: nameof(IMetric.Started)),
                Parameter: nameof(IMetric.Completed)),

                (Rule: IsSame(
                    firstId: metric.ParentId,
                    secondId: metric.Id,
                    secondIdName: nameof(IMetric.Id)),
                Parameter: nameof(IMetric.ParentId)));
        }

        private static void ValidateMetricId(Guid metricId) =>
            Validate(
                createException: () => new InvalidMetricException(
                    "Invalid metric. Please correct the errors and try again."),

                validations: (Rule: IsInvalid(metricId), Parameter: nameof(IMetric.Id)));

        private static void ValidateRetentionPeriod(int retentionPeriodInDays) =>
            Validate(
                createException: () => new InvalidMetricException(
                    "Invalid metric. Please correct the errors and try again."),

                validations: (
                    Rule: IsNotPositive(retentionPeriodInDays),
                    Parameter: nameof(AuditAndMetricsConfigurations.RetentionPeriodInDays)));

        private static void ValidatePurgeBatchSize(int purgeBatchSize) =>
            Validate(
                createException: () => new InvalidMetricException(
                    "Invalid metric. Please correct the errors and try again."),

                validations: (
                    Rule: IsNotPositive(purgeBatchSize),
                    Parameter: nameof(AuditAndMetricsConfigurations.PurgeBatchSize)));

        private static void ValidateStorageMetric(IMetric maybeMetric, Guid metricId)
        {
            if (maybeMetric is null)
            {
                throw new NotFoundMetricException(
                    message: $"Couldn't find metric with metricId: {metricId}.");
            }
        }

        private static void ValidateMetricIsNotNull(IMetric metric)
        {
            if (metric is null)
            {
                throw new NullMetricException(message: "Metric is null.");
            }
        }

        private static void ValidateMetricsIsNotNull(List<IMetric> metrics)
        {
            if (metrics is null)
            {
                throw new NullMetricException(message: "Metrics is null.");
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

        private static dynamic IsInvalid(MetricType metricType) => new
        {
            Condition = Enum.IsDefined(metricType) is false,
            Message = "Type is invalid"
        };

        private static dynamic IsInvalid(MetricStatus metricStatus) => new
        {
            Condition = Enum.IsDefined(metricStatus) is false,
            Message = "Status is invalid"
        };

        private static dynamic IsGreaterThan(string text, int maxLength) => new
        {
            Condition = IsExceedingLength(text, maxLength),
            Message = $"Text exceeds max length of {maxLength} characters"
        };

        private static bool IsExceedingLength(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static dynamic IsNegative(double number) => new
        {
            Condition = number < 0,
            Message = "Value is not expected to be negative"
        };

        private static dynamic IsNegative(long? number) => new
        {
            Condition = number.HasValue && number.Value < 0,
            Message = "Value is not expected to be negative"
        };

        private static dynamic IsNotPositive(int number) => new
        {
            Condition = number <= 0,
            Message = "Value is expected to be greater than zero"
        };

        private static dynamic IsBefore(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate < secondDate,
                Message = $"Date is earlier than {secondDateName}"
            };

        private static dynamic IsSame(
            Guid? firstId,
            Guid secondId,
            string secondIdName) => new
            {
                Condition = firstId.HasValue && firstId.Value == secondId,
                Message = $"Id is the same as {secondIdName}"
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
