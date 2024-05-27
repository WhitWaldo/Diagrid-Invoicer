using System.Text.Json;
using System.Text.Json.Serialization;
using InvoiceService.Email.Data;
using Shared;

namespace InvoiceService.Email;

/// <summary>
/// Helper utility used to build the email payload.
/// </summary>
/// <param name="configuration"></param>
public sealed class EmailPayloadBuilder(IConfiguration configuration)
{
    private readonly string _fromEmailAddress = configuration.GetValue<string>(Constants.EnvironmentVariableNames.FromEmailAddress);
    private readonly string _fromEmailName = configuration.GetValue<string>(Constants.EnvironmentVariableNames.FromEmailName);

    /// <summary>
    /// Helpers method to build the serialized email payload.
    /// </summary>
    /// <param name="toEmailAddress">The email address(es) to send to.</param>
    /// <param name="bccEmailAddress">If provided, the email address to bcc the email to.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="templateData">The data to populate the dynamic template with.</param>
    /// <param name="templateId">The identifier of the dynamic SendGrid email template.</param>
    /// <param name="attachment">The email attachment to include.</param>
    /// <returns></returns>
    public string BuildSerializedPayload(string toEmailAddress, string subject, ITemplateData templateData, string templateId, EmailAttachment attachment, string? bccEmailAddress = null)
    {
        var fromEmail = new EmailAddress(_fromEmailAddress, _fromEmailName);
        var toEmail = new EmailAddress(toEmailAddress);

        var personalizationData = new TransactionalPersonalizationData(toEmail, templateData);
        if (bccEmailAddress is not null)
        {
            personalizationData.BccEmailAddresses.Add(new EmailAddress(bccEmailAddress));
        }

        var payload = new TransactionalEmailPayload(fromEmail, subject, personalizationData, templateId);
        payload.Attachments.Add(attachment);

        var serializedData = JsonSerializer.Serialize(payload);
        return serializedData;
    }

    /// <summary>
    /// Helper method to create an email attachment.
    /// </summary>
    /// <param name="fileName">The name of the attachment.</param>
    /// <param name="fileBytes">The bytes comprising the file.</param>
    /// <returns>A strongly typed email attachment.</returns>
    public EmailAttachment BuildAttachment(string fileName, byte[] fileBytes) => new(Convert.ToBase64String(fileBytes), "application/pdf", fileName);
}

/// <summary>
/// The payload sent in the SendGrid output binding.
/// </summary>
/// <param name="FromEmailAddress">The email address to indicate where the email came from.</param>
/// <param name="Data">The personalization data comprising the transactional email.</param>
/// <param name="TemplateId">The identifier of the template.</param>
/// <param name="Subject">The subject of the email.</param>
public sealed record TransactionalEmailPayload(
    [property: JsonPropertyName("from")] EmailAddress FromEmailAddress,
    [property: JsonPropertyName("subject")]
    string Subject,
    [property: JsonPropertyName("personalizations")]
    TransactionalPersonalizationData Data,
    [property: JsonPropertyName("template_id")]
    string TemplateId)
{
    /// <summary>
    /// A collection of attachments to be included.
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<EmailAttachment> Attachments { get; init; } = [];
}

/// <summary>
/// Used to attach content to an email.
/// </summary>
/// <param name="Base64EncodedBody">The base64 encoded content of the attachment.</param>
/// <param name="MimeType">The MIME type of the content being attached.</param>
/// <param name="FileName">The attachment's file name.</param>
public sealed record EmailAttachment(
    [property: JsonPropertyName("content")] string Base64EncodedBody,
    [property: JsonPropertyName("type")] string MimeType,
    [property: JsonPropertyName("filename")] string FileName)
{
    /// <summary>
    /// Indicates how the attachment should be attached to the email.
    /// </summary>
    [JsonPropertyName("disposition")]
    public string Disposition => "attachment";
}

/// <summary>
/// Identifies personalization data in the transactional email.
/// </summary>
public sealed record TransactionalPersonalizationData
{
    /// <summary>
    /// Identifies personalization data in the transactional email.
    /// </summary>
    /// <param name="toEmailAddress">The email addresses to send the email to.</param>
    /// <param name="templateData">Details about the dynamic template data to populate.</param>
    public TransactionalPersonalizationData(List<EmailAddress> toEmailAddress,
        ITemplateData templateData)
    {
        ToEmailAddress = toEmailAddress;
        TemplateData = templateData;
    }

    /// <summary>
    /// Identifies personalization data in the transactional email.
    /// </summary>
    /// <param name="toEmailAddress">The email address to send the email to.</param>
    /// <param name="templateData">Details about the dynamic template data to populate.</param>
    public TransactionalPersonalizationData(EmailAddress toEmailAddress, ITemplateData templateData)
    {
        ToEmailAddress = [toEmailAddress];
        TemplateData = templateData;
    }

    /// <summary>The email address(es) to send the email to.</summary>
    [JsonPropertyName("to")]
    public List<EmailAddress> ToEmailAddress { get; init; } = [];

    /// <summary>
    /// An array of recipients that will be bcc'ed on the email.
    /// </summary>
    [JsonPropertyName("bcc")]
    public List<EmailAddress> BccEmailAddresses { get; init; } = [];
    
    /// <summary>Details about the dynamic template data to populate.</summary>
    [JsonPropertyName("dynamic_template_data")]
    public ITemplateData TemplateData { get; init; }
}

/// <summary>
/// Identifies the specified email address.
/// </summary>
public sealed record EmailAddress
{
    /// <summary>
    /// Identifies the specified email address.
    /// </summary>
    /// <param name="email">An email address string value.</param>
    public EmailAddress(string email)
    {
        Email = email;
    }

    /// <summary>
    /// Identifies the specified email address.
    /// </summary>
    /// <param name="email">An email address string value.</param>
    /// <param name="name">The name of the email user.</param>
    public EmailAddress(string email, string name)
    {
        Email = email;
        Name = name;
    }

    /// <summary>
    /// The intended recipient's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; } = null;

    /// <summary>An email address string value.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; }
}