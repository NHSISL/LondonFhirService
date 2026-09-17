// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Manage.Models.Foundations.Patients
{
    /// <summary>
    /// What the provider answered, and the id the Api host filed the call under.
    ///
    /// The two travel together because they arrive together and are separated immediately after:
    /// the controller writes the id to a response header and the payload to the body. Keeping
    /// them paired this far means the controller never has to ask the broker a second question
    /// about a response that no longer exists.
    ///
    /// CorrelationId is empty when the upstream sent no header, which is what an older build of
    /// the Api does, so the page has to cope with not having one.
    /// </summary>
    public class StructuredRecordResponse
    {
        public string PayloadText { get; set; }
        public string CorrelationId { get; set; }
    }
}
