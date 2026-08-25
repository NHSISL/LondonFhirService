// ---
// skill: the-standard-foundations
// type: template
// source-section: "2.1 Foundation Services"
// ---

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 1: INTERFACE
// ═══════════════════════════════════════════════════════════════════════════════

// I{Entity}Service.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {Namespace}.Models.Foundations.{Entity}s;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public interface I{Entity}Service
    {
        ValueTask<{Entity}> Add{Entity}Async({Entity} {entity}, CancellationToken cancellationToken = default);
        ValueTask<IQueryable<{Entity}>> RetrieveAll{Entity}sAsync(CancellationToken cancellationToken = default);
        ValueTask<{Entity}> Retrieve{Entity}ByIdAsync(Guid {entity}Id, CancellationToken cancellationToken = default);
        ValueTask<{Entity}> Modify{Entity}Async({Entity} {entity}, CancellationToken cancellationToken = default);

        ValueTask<{Entity}> Remove{Entity}ByIdAsync(
            Guid {entity}Id, 
            string? deletionReason = null, 
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> HardRemove{Entity}ByIdAsync(Guid {entity}Id, CancellationToken cancellationToken = default);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 2: SERVICE IMPLEMENTATION
// ═══════════════════════════════════════════════════════════════════════════════

// {Entity}Service.cs — main partial
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {Namespace}.Brokers.DateTimes;
using {Namespace}.Brokers.Events;
using {Namespace}.Brokers.Loggings;
using {Namespace}.Brokers.Securities;
using {Namespace}.Brokers.Storages.Sql;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public partial class {Entity}Service : I{Entity}Service
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IEventBroker eventBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public {Entity}Service(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            IEventBroker eventBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.eventBroker = eventBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<{Entity}> Add{Entity}Async({Entity} {entity}, CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                {entity} = await this.securityAuditBroker.ApplyAddAuditValuesAsync({entity});
                await Validate{Entity}OnAdd({entity});
                {Entity} added{Entity} = await this.storageBroker.Insert{Entity}Async({entity}, cancellationToken);
                var added{Entity}Envelope = new EventEnvelope<{Entity}> { Content = added{Entity} };
                await this.eventBroker.Publish{Entity}Async(added{Entity}Envelope, "{Entity}Added");

                return added{Entity};
            });

        public ValueTask<IQueryable<{Entity}>> RetrieveAll{Entity}sAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.storageBroker.SelectAll{Entity}sAsync();
            });

        public ValueTask<{Entity}> Retrieve{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Validate{Entity}Id({entity}Id);

                {Entity} maybe{Entity} = await this.storageBroker
                    .Select{Entity}ByIdAsync({entity}Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}Id);

                return maybe{Entity};
            });

        public ValueTask<{Entity}> Modify{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                {entity} = await this.securityAuditBroker.ApplyModifyAuditValuesAsync({entity});
                await Validate{Entity}OnModify({entity});

                {Entity} maybe{Entity} =
                    await this.storageBroker.Select{Entity}ByIdAsync({entity}.Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}.Id);

                {entity} = await this.securityAuditBroker
                    .EnsureOtherAuditValuesRemainsUnchangedOnModifyAsync({entity}, maybe{Entity});

                ValidateAgainstStorage{Entity}OnModify(
                    input{Entity}: {entity},
                    storage{Entity}: maybe{Entity});

                {Entity} updated{Entity} = await this.storageBroker.Update{Entity}Async({entity}, cancellationToken);
                var updated{Entity}Envelope = new EventEnvelope<{Entity}> { Content = updated{Entity} };
                await this.eventBroker.Publish{Entity}Async(updated{Entity}Envelope, "{Entity}Modified");

                return updated{Entity};
            });

        public ValueTask<{Entity}> Remove{Entity}ByIdAsync(
            Guid {entity}Id,
            string? deletionReason = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Validate{Entity}Id({entity}Id);

                {Entity} maybe{Entity} = await this.storageBroker
                    .Select{Entity}ByIdAsync({entity}Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}Id);

                if (maybe{Entity}.IsDeleted)
                    return maybe{Entity};

                // pass the reason as an argument — do NOT pre-set it on the entity and rely on
                // the audit call to leave it alone; the reason is the audit broker's to stamp
                {Entity} audited{Entity} =
                    await this.securityAuditBroker.ApplyRemoveAuditValuesAsync(
                        entity: maybe{Entity},
                        deletionReason: deletionReason);

                {Entity} removed{Entity} = 
                    await this.storageBroker.Update{Entity}Async(audited{Entity}, cancellationToken);

                var removed{Entity}Envelope = new EventEnvelope<{Entity}> { Content = removed{Entity} };
                await this.eventBroker.Publish{Entity}Async(removed{Entity}Envelope, "{Entity}Removed");

                return removed{Entity};
            });

        public ValueTask<{Entity}> HardRemove{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Validate{Entity}Id({entity}Id);

                {Entity} maybe{Entity} = await this.storageBroker
                    .Select{Entity}ByIdAsync({entity}Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}Id);

                {Entity} deleted{Entity} = 
                    await this.storageBroker.Delete{Entity}Async(maybe{Entity}, cancellationToken);

                var deleted{Entity}Envelope = new EventEnvelope<{Entity}> { Content = deleted{Entity} };
                await this.eventBroker.Publish{Entity}Async(deleted{Entity}Envelope, "{Entity}Removed");

                return deleted{Entity};
            });
    }
}

// {Entity}Service.Validations.cs — validation partial
using System;
using System.Threading.Tasks;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public partial class {Entity}Service
    {
        private async ValueTask Validate{Entity}OnAdd({Entity} {entity})
        {
            Validate{Entity}IsNotNull({entity});
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                message: "Invalid {entity}. Please correct the errors and try again.",
                (Rule: IsInvalid({entity}.Id), Parameter: nameof({Entity}.Id)),
                (Rule: IsInvalid({entity}.Name), Parameter: nameof({Entity}.Name)),
                (Rule: IsInvalid({entity}.CreatedDate), Parameter: nameof({Entity}.CreatedDate)),
                (Rule: IsInvalid({entity}.CreatedBy), Parameter: nameof({Entity}.CreatedBy)),
                (Rule: IsInvalid({entity}.UpdatedDate), Parameter: nameof({Entity}.UpdatedDate)),
                (Rule: IsInvalid({entity}.UpdatedBy), Parameter: nameof({Entity}.UpdatedBy)),
                (Rule: IsGreaterThan({entity}.Name, 255), Parameter: nameof({Entity}.Name)),
                (Rule: IsGreaterThan({entity}.CreatedBy, 255), Parameter: nameof({Entity}.CreatedBy)),
                (Rule: IsGreaterThan({entity}.UpdatedBy, 255), Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: IsNotSame(
                    firstDate: {entity}.UpdatedDate,
                    secondDate: {entity}.CreatedDate,
                    secondDateName: nameof({Entity}.CreatedDate)),
                Parameter: nameof({Entity}.UpdatedDate)),

                (Rule: IsNotSame(
                    first: currentUserId,
                    second: {entity}.CreatedBy),
                Parameter: nameof({Entity}.CreatedBy)),

                (Rule: IsNotSame(
                    first: {entity}.UpdatedBy,
                    second: {entity}.CreatedBy,
                    secondName: nameof({Entity}.CreatedBy)),
                Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: await IsNotRecentAsync({entity}.CreatedDate), Parameter: nameof({Entity}.CreatedDate)));
        }

        private async ValueTask Validate{Entity}OnModify({Entity} {entity})
        {
            Validate{Entity}IsNotNull({entity});
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                message: "Invalid {entity}. Please correct the errors and try again.",
                (Rule: IsInvalid({entity}.Id), Parameter: nameof({Entity}.Id)),
                (Rule: IsInvalid({entity}.Name), Parameter: nameof({Entity}.Name)),
                (Rule: IsInvalid({entity}.CreatedDate), Parameter: nameof({Entity}.CreatedDate)),
                (Rule: IsInvalid({entity}.CreatedBy), Parameter: nameof({Entity}.CreatedBy)),
                (Rule: IsInvalid({entity}.UpdatedDate), Parameter: nameof({Entity}.UpdatedDate)),
                (Rule: IsInvalid({entity}.UpdatedBy), Parameter: nameof({Entity}.UpdatedBy)),
                (Rule: IsGreaterThan({entity}.Name, 255), Parameter: nameof({Entity}.Name)),
                (Rule: IsGreaterThan({entity}.CreatedBy, 255), Parameter: nameof({Entity}.CreatedBy)),
                (Rule: IsGreaterThan({entity}.UpdatedBy, 255), Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: IsNotSame(
                    first: currentUserId,
                    second: {entity}.UpdatedBy),
                Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: IsSame(
                    firstDate: {entity}.UpdatedDate,
                    secondDate: {entity}.CreatedDate,
                    secondDateName: nameof({Entity}.CreatedDate)),
                Parameter: nameof({Entity}.UpdatedDate)),

                (Rule: await IsNotRecentAsync({entity}.UpdatedDate), Parameter: nameof({Entity}.UpdatedDate)));
        }

        private static void Validate{Entity}Id(Guid {entity}Id)
        {
            Validate(
                message: "Invalid {entity}. Please correct the errors and try again.",
                validations: (Rule: IsInvalid({entity}Id), Parameter: nameof({Entity}.Id)));
        }

        private static void ValidateStorage{Entity}({Entity} maybe{Entity}, Guid {entity}Id)
        {
            if (maybe{Entity} is null)
            {
                throw new NotFound{Entity}ServiceException(
                    message: $"Couldn't find {entity} with {entity}Id: {{entity}Id}.");
            }
        }

        private static void Validate{Entity}IsNotNull({Entity} {entity})
        {
            if ({entity} is null)
            {
                throw new Null{Entity}ServiceException(message: "{Entity} is null.");
            }
        }

        private static void ValidateAgainstStorage{Entity}OnModify(
            {Entity} input{Entity},
            {Entity} storage{Entity})
        {
            Validate(
                message: "Invalid {entity}. Please correct the errors and try again.",

                (Rule: IsNotSame(
                        firstDate: input{Entity}.CreatedDate,
                        secondDate: storage{Entity}.CreatedDate,
                        secondDateName: nameof({Entity}.CreatedDate)),
                    Parameter: nameof({Entity}.CreatedDate)),

                (Rule: IsNotSame(
                        first: input{Entity}.CreatedBy,
                        second: storage{Entity}.CreatedBy,
                        secondName: nameof({Entity}.CreatedBy)),
                    Parameter: nameof({Entity}.CreatedBy)),

                (Rule: IsSame(
                        firstDate: input{Entity}.UpdatedDate,
                        secondDate: storage{Entity}.UpdatedDate,
                        secondDateName: nameof({Entity}.UpdatedDate)),
                    Parameter: nameof({Entity}.UpdatedDate)));
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

        private static void Validate(
            string message,
            params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDataException = new Invalid{Entity}ServiceException(message);

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

// {Entity}Service.Exceptions.cs — exception handling partial
using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public partial class {Entity}Service
    {
        private delegate ValueTask<{Entity}> Returning{Entity}Function();
        private delegate ValueTask<IQueryable<{Entity}>> Returning{Entity}sFunction();

        private async ValueTask<{Entity}> TryCatch(Returning{Entity}Function returning{Entity}Function)
        {
            try
            {
                return await returning{Entity}Function();
            }
            catch (OperationCanceledException operationCanceledException)
               when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var timeoutException = new TimeoutException("The dependency operation timed out.");

                var timeout{Entity}Exception =
                    new Timeout{Entity}Exception(
                        message: "Failed {entity} timeout error occurred, contact support.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyException(timeout{Entity}Exception);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Null{Entity}ServiceException null{Entity}ServiceException)
            {
                throw await CreateAndLogValidationException(null{Entity}ServiceException);
            }
            catch (Invalid{Entity}ServiceException invalid{Entity}ServiceException)
            {
                throw await CreateAndLogValidationException(invalid{Entity}ServiceException);
            }
            catch (SqlException sqlException)
            {
                var failedStorage{Entity}ServiceException =
                    new FailedStorage{Entity}ServiceException(
                        message: "Failed {entity} storage error occurred, contact support.",
                        innerException: sqlException,
                        data: sqlException.Data);

                throw await CreateAndLogCriticalDependencyException(failedStorage{Entity}ServiceException);
            }
            catch (NotFound{Entity}ServiceException notFound{Entity}ServiceException)
            {
                throw await CreateAndLogValidationException(notFound{Entity}ServiceException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExists{Entity}ServiceException =
                    new AlreadyExists{Entity}ServiceException(
                        message: "{Entity} with the same Id already exists.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw await CreateAndLogDependencyValidationException(alreadyExists{Entity}ServiceException);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalid{Entity}ReferenceException =
                    new Invalid{Entity}ReferenceException(
                        message: "Invalid {entity} reference error occurred.",
                        innerException: foreignKeyConstraintConflictException,
                        data: foreignKeyConstraintConflictException.Data);

                throw await CreateAndLogDependencyValidationException(invalid{Entity}ReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var locked{Entity}ServiceException =
                    new Locked{Entity}ServiceException(
                        message: "Locked {entity} record exception, please try again later",
                        innerException: dbUpdateConcurrencyException,
                        data: dbUpdateConcurrencyException.Data);

                throw await CreateAndLogDependencyValidationException(locked{Entity}ServiceException);
            }
            catch (DbUpdateException databaseUpdateException)
            {
                var failedStorage{Entity}ServiceException =
                    new FailedStorage{Entity}ServiceException(
                        message: "Failed {entity} storage error occurred, contact support.",
                        innerException: databaseUpdateException,
                        data: databaseUpdateException.Data);

                throw await CreateAndLogDependencyException(failedStorage{Entity}ServiceException);
            }
            catch (Exception exception)
            {
                var failed{Entity}ServiceException =
                    new Failed{Entity}ServiceException(
                        message: "Failed {entity} service occurred, please contact support",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceException(failed{Entity}ServiceException);
            }
        }

        private async ValueTask<IQueryable<{Entity}>> TryCatch(
            Returning{Entity}sFunction returning{Entity}sFunction)
        {
            try
            {
                return await returning{Entity}sFunction();
            }
            catch (OperationCanceledException operationCanceledException)
               when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var timeoutException = new TimeoutException("The dependency operation timed out.");

                var timeout{Entity}Exception =
                    new Timeout{Entity}Exception(
                        message: "Failed {entity} timeout error occurred, contact support.",
                        innerException: timeoutException,
                        data: timeoutException.Data);

                throw await CreateAndLogDependencyException(timeout{Entity}Exception);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SqlException sqlException)
            {
                var failedStorage{Entity}ServiceException =
                    new FailedStorage{Entity}ServiceException(
                        message: "Failed {entity} storage error occurred, contact support.",
                        innerException: sqlException,
                        data: sqlException.Data);

                throw await CreateAndLogCriticalDependencyException(failedStorage{Entity}ServiceException);
            }
            catch (Exception exception)
            {
                var failed{Entity}ServiceException =
                    new Failed{Entity}ServiceException(
                        message: "Failed {entity} service occurred, please contact support",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceException(failed{Entity}ServiceException);
            }
        }

        private async ValueTask<{Entity}ServiceValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var {entity}ServiceValidationException =
                new {Entity}ServiceValidationException(
                    message: "{Entity} validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}ServiceValidationException);

            return {entity}ServiceValidationException;
        }

        private async ValueTask<{Entity}ServiceDependencyException> CreateAndLogCriticalDependencyException(
            Xeption exception)
        {
            var {entity}ServiceDependencyException =
                new {Entity}ServiceDependencyException(
                    message: "{Entity} dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync({entity}ServiceDependencyException);

            return {entity}ServiceDependencyException;
        }

        private async ValueTask<{Entity}ServiceDependencyValidationException> CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var {entity}ServiceDependencyValidationException =
                new {Entity}ServiceDependencyValidationException(
                    message: "{Entity} dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}ServiceDependencyValidationException);

            return {entity}ServiceDependencyValidationException;
        }

        private async ValueTask<{Entity}ServiceDependencyException> CreateAndLogDependencyException(
            Xeption exception)
        {
            var {entity}ServiceDependencyException =
                new {Entity}ServiceDependencyException(
                    message: "{Entity} dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}ServiceDependencyException);

            return {entity}ServiceDependencyException;
        }

        private async ValueTask<{Entity}ServiceException> CreateAndLogServiceException(
            Xeption exception)
        {
            var {entity}ServiceException =
                new {Entity}ServiceException(
                    message: "{Entity} service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}ServiceException);

            return {entity}ServiceException;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 3: EXCEPTION MODELS
// ═══════════════════════════════════════════════════════════════════════════════

// Null{Entity}ServiceException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Null{Entity}ServiceException : Xeption
    {
        public Null{Entity}ServiceException(string message)
            : base(message)
        { }
    }
}

// Invalid{Entity}ServiceException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Invalid{Entity}ServiceException : Xeption
    {
        public Invalid{Entity}ServiceException(string message)
            : base(message)
        { }
    }
}

// NotFound{Entity}ServiceException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class NotFound{Entity}ServiceException : Xeption
    {
        public NotFound{Entity}ServiceException(string message)
            : base(message)
        { }
    }
}

// AlreadyExists{Entity}ServiceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class AlreadyExists{Entity}ServiceException : Xeption
    {
        public AlreadyExists{Entity}ServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Invalid{Entity}ReferenceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Invalid{Entity}ReferenceException : Xeption
    {
        public Invalid{Entity}ReferenceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Locked{Entity}ServiceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Locked{Entity}ServiceException : Xeption
    {
        public Locked{Entity}ServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// FailedStorage{Entity}ServiceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class FailedStorage{Entity}ServiceException : Xeption
    {
        public FailedStorage{Entity}ServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Failed{Entity}ServiceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Failed{Entity}ServiceException : Xeption
    {
        public Failed{Entity}ServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Timeout{Entity}Exception.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Timeout{Entity}Exception : Xeption
    {
        public Timeout{Entity}Exception(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// {Entity}ServiceValidationException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}ServiceValidationException : Xeption
    {
        public {Entity}ServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}

// {Entity}ServiceDependencyValidationException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}ServiceDependencyValidationException : Xeption
    {
        public {Entity}ServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}

// {Entity}ServiceDependencyException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}ServiceDependencyException : Xeption
    {
        public {Entity}ServiceDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}

// {Entity}ServiceException.cs
using System;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}ServiceException : Xeption
    {
        public {Entity}ServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 4: UNIT TESTS
// ═══════════════════════════════════════════════════════════════════════════════

// {Entity}ServiceTests.cs — test fixture base
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using {Namespace}.Brokers.DateTimes;
using {Namespace}.Brokers.Events;
using {Namespace}.Brokers.Loggings;
using {Namespace}.Brokers.Securities;
using {Namespace}.Brokers.Storages.Sql;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using {Namespace}.Services.Foundations.{Entity}s;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IEventBroker> eventBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly I{Entity}Service {entity}Service;

        public {Entity}ServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.eventBrokerMock = new Mock<IEventBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.{entity}Service = new {Entity}Service(
                storageBroker: this.storageBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                eventBroker: this.eventBrokerMock.Object,
                securityAuditBroker: this.securityAuditBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static IQueryable<{Entity}> CreateRandom{Entity}s()
        {
            return Create{Entity}Filler(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                .AsQueryable();
        }

        private static {Entity} CreateRandomModify{Entity}(DateTimeOffset dateTimeOffset, string userId = "")
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            {Entity} random{Entity} = CreateRandom{Entity}(dateTimeOffset, userId);
            random{Entity}.CreatedDate = random{Entity}.CreatedDate.AddDays(randomDaysInPast);

            return random{Entity};
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        public static TheoryData<int> MinutesBeforeOrAfter()
        {
            int randomNumber = GetRandomNumber();
            int randomNegativeNumber = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomNumber,
                randomNegativeNumber
            };
        }

        private static SqlException GetSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static {Entity} CreateRandom{Entity}() =>
            Create{Entity}Filler(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static {Entity} CreateRandom{Entity}(DateTimeOffset dateTimeOffset, string userId = "") =>
            Create{Entity}Filler(dateTimeOffset, userId).Create();

        private static Filler<{Entity}> Create{Entity}Filler(DateTimeOffset dateTimeOffset, string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId;
            var filler = new Filler<{Entity}>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty({entity} => {entity}.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty({entity} => {entity}.CreatedBy).Use(userId)
                .OnProperty({entity} => {entity}.UpdatedBy).Use(userId);

            return filler;
        }
    }
}

// {Entity}ServiceTests.Add.Logic.cs
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldAdd{Entity}Async()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomUserId = GetRandomString();
            {Entity} random{Entity} = CreateRandom{Entity}(randomDateTimeOffset);
            {Entity} input{Entity} = random{Entity};
            {Entity} auditApplied{Entity} = input{Entity}.DeepClone();
            auditApplied{Entity}.CreatedBy = randomUserId;
            auditApplied{Entity}.CreatedDate = randomDateTimeOffset;
            auditApplied{Entity}.UpdatedBy = randomUserId;
            auditApplied{Entity}.UpdatedDate = randomDateTimeOffset;
            {Entity} storage{Entity} = auditApplied{Entity}.DeepClone();
            {Entity} expected{Entity} = storage{Entity}.DeepClone();

            var expected{Entity}Envelope =
                new EventEnvelope<{Entity}> { Content = storage{Entity} };

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(input{Entity}))
                    .ReturnsAsync(auditApplied{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.Insert{Entity}Async(auditApplied{Entity}, TestContext.Current.CancellationToken))
                    .ReturnsAsync(storage{Entity});

            this.eventBrokerMock.Setup(broker =>
                broker.Publish{Entity}Async(
                    It.Is<EventEnvelope<{Entity}>>(e => e.Content == storage{Entity}),
                    "{Entity}Added"))
                        .Returns(ValueTask.CompletedTask);

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.Add{Entity}Async(input{Entity}, TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.ApplyAddAuditValuesAsync(input{Entity}),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                    broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once());

            this.storageBrokerMock.Verify(broker =>
                    broker.Insert{Entity}Async(auditApplied{Entity}, TestContext.Current.CancellationToken),
                Times.Once);

            this.eventBrokerMock.Verify(broker =>
                broker.Publish{Entity}Async(
                    It.Is<EventEnvelope<{Entity}>>(e => e.Content == storage{Entity}),
                    "{Entity}Added"),
                        Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// NOTE: Additional test files would follow the same pattern:
// - {Entity}ServiceTests.Add.Validations.cs
// - {Entity}ServiceTests.Add.Exceptions.cs
// - {Entity}ServiceTests.RetrieveAll.Logic.cs
// - {Entity}ServiceTests.RetrieveAll.Exceptions.cs
// - {Entity}ServiceTests.RetrieveById.Logic.cs
// - {Entity}ServiceTests.RetrieveById.Validations.cs
// - {Entity}ServiceTests.RetrieveById.Exceptions.cs
// - {Entity}ServiceTests.Modify.Logic.cs
// - {Entity}ServiceTests.Modify.Validations.cs
// - {Entity}ServiceTests.Modify.Exceptions.cs
// - {Entity}ServiceTests.RemoveById.Logic.cs
// - {Entity}ServiceTests.RemoveById.Validations.cs
// - {Entity}ServiceTests.RemoveById.Exceptions.cs
// - {Entity}ServiceTests.HardRemoveById.Logic.cs
// - {Entity}ServiceTests.HardRemoveById.Validations.cs
// - {Entity}ServiceTests.HardRemoveById.Exceptions.cs
//
// All follow the AAA (Arrange-Act-Assert) pattern with:
// - Mock setup for dependencies
// - Service method invocation
// - Assertions on results and mock verifications
