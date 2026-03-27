// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Foundations.JsonElements
{
    public partial class JsonElementService
    {
        private delegate ValueTask<JsonElement> ReturningJsonElementFunction();

        private async ValueTask<JsonElement> TryCatch(ReturningJsonElementFunction returningJsonElementFunction)
        {
            return await returningJsonElementFunction();
        }
    }
}
