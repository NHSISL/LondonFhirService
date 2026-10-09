// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Api.Extensions.Exceptions
{
    public static class ServerErrorExtension
    {
        public static Xeption ToInnerMessageOnly(this Exception exception) =>
            new Xeption(message: exception.InnerException?.Message ?? exception.Message);
    }
}
