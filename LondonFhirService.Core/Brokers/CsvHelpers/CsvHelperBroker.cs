// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NHSISL.CsvHelperClient.Clients;

namespace LondonFhirService.Core.Brokers.CsvHelpers
{
    /// <summary>
    /// Registered as a singleton. The client holds no per-call state - every call is handed the
    /// rows and the stream to write them to - so there is nothing to gain from building a new one
    /// for each request.
    /// </summary>
    public class CsvHelperBroker : ICsvHelperBroker
    {
        private readonly ICsvClient csvClient;

        public CsvHelperBroker() =>
            this.csvClient = new CsvClient();

        public async ValueTask MapObjectToCsvAsync<T>(
            IAsyncEnumerable<T> @object,
            Stream outputStream,
            bool addHeaderRecord,
            Dictionary<string, int> fieldMappings = null,
            bool? shouldAddTrailingComma = false,
            CancellationToken cancellationToken = default) =>
            await this.csvClient.MapObjectToCsvAsync(
                @object,
                outputStream,
                addHeaderRecord,
                fieldMappings,
                shouldAddTrailingComma,
                cancellationToken);
    }
}
