// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.AllergyIntolerances.AllergyIntolerances.Exceptions;

public class NullAllergyIntoleranceException : Xeption
{
    public NullAllergyIntoleranceException(string parameterName)
        : base(message: $"Resource matcher parameter is null: {parameterName}.")
    { }
}
