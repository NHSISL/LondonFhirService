// ---
// skill: the-standard-cancellation-patterns
// type: template
// source-section: "7.0 Exception Handling Rules - With Timeout"
// ---

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheStandard.Services.Foundations.{Entities}
{
    public partial class {Entity}Service : I{Entity}Service
    {
        private readonly IStorageBroker storageBroker;

        public {Entity}Service(IStorageBroker storageBroker)
        {
            this.storageBroker = storageBroker;
        }

        public ValueTask<{Entity}> Retrieve{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            // Validate cancellation before dependency operation
            cancellationToken.ThrowIfCancellationRequested();

            // Create linked token source for timeout support
            using var timeoutSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(30));

            using var linkedSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutSource.Token);

            return await this.storageBroker.Select{Entity}ByIdAsync(
                {entity}Id,
                linkedSource.Token);
        });

        private async ValueTask<{Entity}> TryCatch(
            Returning{Entity}Function returning{Entity}Function)
        {
            try
            {
                return await returning{Entity}Function();
            }
            // Timeout-guarded catch MUST precede plain OperationCanceledException
            catch (OperationCanceledException)
            when (timeoutSource.IsCancellationRequested)
            {
                var timeoutException =
                    new TimeoutException(
                        "The {entity} retrieval operation timed out.");

                throw CreateAndLogDependencyException(timeoutException);
            }
            // Plain OperationCanceledException MUST be rethrown unchanged
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw CreateAndLogServiceException(exception);
            }
        }

        private {Entity}DependencyException CreateAndLogDependencyException(
            Exception exception)
        {
            var {entity}DependencyException =
                new {Entity}DependencyException(exception);

            // Log exception here
            return {entity}DependencyException;
        }

        private {Entity}ServiceException CreateAndLogServiceException(
            Exception exception)
        {
            var {entity}ServiceException =
                new {Entity}ServiceException(exception);

            // Log exception here
            return {entity}ServiceException;
        }
    }

    public delegate ValueTask<{Entity}> Returning{Entity}Function();
}
