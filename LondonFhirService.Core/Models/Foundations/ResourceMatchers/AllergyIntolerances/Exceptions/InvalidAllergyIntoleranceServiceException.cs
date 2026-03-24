// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.AllergyIntolerances.Exceptions;

public class InvalidAllergyIntoleranceServiceException : Xeption
{
    public InvalidAllergyIntoleranceServiceException()
        : base(message: "Invalid Allergy Intolerance service arguments. Please correct the errors and try again.")
    { }
}