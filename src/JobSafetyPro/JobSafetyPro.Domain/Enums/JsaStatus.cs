namespace JobSafetyPro.Domain.Enums;

/// <summary>Assessment lifecycle status.</summary>
public enum JsaStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    /// <summary>Legacy / in-review assessments persisted before final approval.</summary>
    InReview = 5,
}
