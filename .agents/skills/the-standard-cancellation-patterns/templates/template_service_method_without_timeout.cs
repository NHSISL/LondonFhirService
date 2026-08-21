// ---
// skill: the-standard-cancellation-patterns
// type: template
// source-section: "7.4 Whenever a CancellationToken is used, BOTH catch blocks MUST exist"
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

            // No timeout source created — token passed directly
            return await this.storageBroker.Select{Entity}ByIdAsync(
                {entity}Id,
                cancellationToken);
        });

        private async ValueTask<{Entity}> TryCatch(
            Returning{Entity}Function returning{Entity}Function)
        {
            try
            {
                return await returning{Entity}Function();
            }
            // Even without timeout, this catch block MUST exist
            // to prevent OperationCanceledException from being caught by
            // the catch-all exception handler below
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw CreateAndLogServiceException(exception);
            }
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
