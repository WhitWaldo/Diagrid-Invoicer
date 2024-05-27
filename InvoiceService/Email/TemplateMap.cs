namespace InvoiceService.Email;

/// <summary>
/// Provides a mapping between SendGrid dynamic template IDs and their purpose.
/// </summary>
public static class TemplateMap
{
    /// <summary>
    /// The SendGrid dynamic template ID for the invoice approval email.
    /// </summary>
    public const string ApprovalTemplateId = "d-b4a348ff64284fe194fb8da25ed9b440";

    /// <summary>
    /// The SendGrid dynamic template ID for the rejected invoice email.
    /// </summary>
    public const string RejectedTemplateId = "d-8231a52fc0c84c089591c529b6a49241";

    /// <summary>
    /// The SendGrid dynamic template ID for the customer email.
    /// </summary>
    public const string CustomerTemplateId = "d-3bd92648f16b4fecb4f9e79c8a1c0699";
}