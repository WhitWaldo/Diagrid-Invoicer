using System.Reflection;
using System.Text.RegularExpressions;
using BuilderService.Attributes;
using BuilderService.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;

namespace BuilderService.Operations;

/// <summary>
/// Used to generate PDF files for the given invoice data.
/// </summary>
public sealed class InvoiceGenerator(ILoggerFactory? loggerFactory)
{
    /// <summary>
    /// Used for logging and telemetry.
    /// </summary>
    private readonly ILogger _logger = loggerFactory?.CreateLogger<InvoiceGenerator>() ??
                                       NullLoggerFactory.Instance.CreateLogger<InvoiceGenerator>();

    /// <summary>
    /// The name of the base invoice template file.
    /// </summary>
    private const string InvoiceTemplateFileName = "BaseInvoice.docx";

    /// <summary>
    /// Given a set of invoice fields to use as the source data, this uses a base Word document as a template, replaces
    /// the various fields in it and produces an array of bytes that represent a fully-formed PDF document.
    /// </summary>
    /// <param name="fields">The invoice data to populate the document with.</param>
    /// <returns>An array of bytes representing a fully-formed PDF document.</returns>
    public async Task<byte[]> GenerateInvoiceAsync(InvoiceFields fields)
    {
        await using var fileStream = new FileStream(InvoiceTemplateFileName, FileMode.Open, FileAccess.ReadWrite,
            FileShare.ReadWrite);
        using var document = new WordDocument(fileStream, FormatType.Docx);
        
        //Replace each of the static fields in the document
        var extractedFields = ExtractFields(fields);
        _logger.LogInformation("Replacing the {numberStaticFields} static fields in the document", extractedFields.Count);
        foreach (var (key, fieldValue) in extractedFields)
        {
            var fieldKey = new Regex($"{{{{{key}}}}}");
            _logger.LogTrace("Replacing {fieldKey} with {fieldValue}", fieldKey, fieldValue);
            document.Replace(fieldKey, fieldValue);
        }

        //Insert each of the line items into the table - start by finding the table
        //Find the table by its "title"
        //Add the line items starting at row index 1
        _logger.LogTrace("Locating table named 'InvoiceTable' in document to ");
        if (document.FindItemByProperty(EntityType.Table, "Title", "InvoiceTable") is WTable table)
        {
            _logger.LogTrace("Located table");

            //Get the second row
            var secondRow = table.Rows[1];

            //Create a number of rows equal to the number of line items minus one (since it comes with one row already)
            AddRowsToTable(table, secondRow, fields.LineItems.Count - 1);
            _logger.LogTrace("Added {rowCount} rows to the table using the existing second row as a template", fields.LineItems.Count - 1);

            //Insert data to cells for each row
            var lineItems = fields.LineItems.ToList();
            for (var a = 0; a < lineItems.Count; a++)
            {
                var rowIndex = a + 1; //Start from index 1
                var row = table.Rows[rowIndex];
                _logger.LogTrace("Adding line item data {lineItemData} to table at {rowIndex}", lineItems[a], rowIndex);
                InsertDataToCells(row, lineItems[a]);
            }
        }
        
        //Convert into a PDF document
        using var renderer = new DocIORenderer();
        var pdfDocument = renderer.ConvertToPDF(document);
        _logger.LogInformation("Successfully converted document into PDF");

        //Saves the PDF document to a memory stream
        using var memoryStream = new MemoryStream();
        pdfDocument.Save(memoryStream);
        memoryStream.Position = 0;

        //Capture the bytes from the memory stream in an array
        var pdfBytes = memoryStream.ToArray();

        document.Close();
        pdfDocument.Close();

        _logger.LogInformation("Successfully completed invoice generation yielding array of {byteArrayLength} bytes", pdfBytes.LongLength);
        return pdfBytes;
    }

    /// <summary>
    /// Adds line item rows to table, understanding that it already comes with one row.
    /// </summary>
    /// <param name="sourceRow">The source row to make a copy of.</param>
    /// <param name="rowCount">The number of additional rows to add.</param>
    /// <param name="table">The table the row is being added to.</param>
    private static void AddRowsToTable(IWTable table, WTableRow sourceRow, int rowCount)
    {
        for (var a = 0; a < rowCount; a++)
        {
            //Add another row by cloning the source row
            var newRow = sourceRow.Clone();

            //Iterate through the cells of the cloned row
            for (var b = 0; b < newRow.Cells.Count; b++)
            {
                var cell = newRow.Cells[b];

                //Set the text to an empty value
                cell.Paragraphs[0].Text = string.Empty;
            }

            //Insert the cloned row to index 2 (so it follows the source row)
            table.Rows.Insert(2, newRow);
        }
    }

    /// <summary>
    /// Applies the line item data to a row in a document table.
    /// </summary>
    /// <param name="row">The row to apply the data to.</param>
    /// <param name="lineItem">The source line item data.</param>
    private static void InsertDataToCells(WTableRow row, InvoiceLineItem lineItem)
    {
        var data = new List<string>
        {
            lineItem.Quantity,
            lineItem.Description,
            lineItem.UnitPrice,
            lineItem.Total
        };

        for (var a = 0; a < row.Cells.Count; a++)
        {
            row.Cells[a].Paragraphs[0].Text = data[a];
        }
    }
    
    /// <summary>
    /// Creates a map wherein the value of the Field attribute is mapped to the value of the property.
    /// </summary>
    /// <param name="fields">The invoice data to pull the values from.</param>
    /// <returns></returns>
    private static Dictionary<string, string> ExtractFields(InvoiceFields fields)
    {
        var result = new Dictionary<string, string>();
        var properties = typeof(InvoiceFields).GetProperties();

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<FieldAttribute>();
            if (attribute != null)
            {
                var value = property.GetValue(fields)?.ToString() ?? string.Empty;
                result[attribute.Name] = value;
            }
        }

        return result;
    }
}