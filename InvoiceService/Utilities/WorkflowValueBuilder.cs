using System.Security.Cryptography;
using System.Text;

namespace InvoiceService.Utilities;

/// <summary>
/// Provides a standard place for approval identifiers to be created.
/// </summary>
public static class WorkflowValueBuilder
{
    /// <summary>
    /// Creates an activity ID that's derived from, but unique from the invoice number to prevent others from approving invoices since the invoice numbers
    /// are guessable.
    /// </summary>
    /// <param name="invoiceNumber">The invoice number.</param>
    /// <returns>A base64 string representing the activity ID.</returns>
    public static async Task<string> CreateApprovalIdAsync(string invoiceNumber)
    {
        //Encode to UTF8 bytes
        var hashValue = $"{invoiceNumber}_invoicer";
        var bytes = Encoding.UTF8.GetBytes(hashValue);

        //Hash with MD5
        using var ms = new MemoryStream(bytes);
        var hashBytes = await MD5.HashDataAsync(ms);

        //Convert to base64 and return
        var base64String = Convert.ToBase64String(hashBytes);
        return base64String;
    }
}