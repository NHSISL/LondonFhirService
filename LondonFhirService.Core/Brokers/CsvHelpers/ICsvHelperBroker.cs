// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Brokers.CsvHelpers
{
    public interface ICsvHelperBroker
    {
        ValueTask MapObjectToCsvAsync<T>(
            IAsyncEnumerable<T> @object,
            Stream outputStream,
            bool addHeaderRecord,
            Dictionary<string, int> fieldMappings = null,
            bool? shouldAddTrailingComma = false,
            CancellationToken cancellationToken = default);
    }
}
