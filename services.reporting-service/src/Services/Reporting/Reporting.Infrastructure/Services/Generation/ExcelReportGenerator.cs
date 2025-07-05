using ClosedXML.Excel;
using Reporting.Application.Contracts.Generation;
using Reporting.Application.Models;
using Reporting.Domain.Enums;

namespace Reporting.Infrastructure.Services.Generation;

/// <summary>
/// Implements the logic for generating an Excel report using the ClosedXML library.
/// </summary>
public class ExcelReportGenerator : IReportGenerator
{
    public ReportFormat Format => ReportFormat.Excel;

    public Task<byte[]> GenerateAsync(ReportDataModel data, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        // Header
        worksheet.Cell("A1").Value = data.ReportTitle;
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;
        worksheet.Cell("A2").Value = $"Generated on: {data.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC";
        worksheet.Range("A1:D1").Merge();
        worksheet.Range("A2:D2").Merge();
        
        int currentRow = 4;

        // Iterate through data sections
        foreach (var section in data.DataSections)
        {
            // This is a naive implementation using reflection.
            // A production system would use strongly-typed section models and switch on their type.
            var sectionType = section.GetType();
            var title = sectionType.GetProperty("Title")?.GetValue(section)?.ToString() ?? "Data Section";
            var sectionData = sectionType.GetProperty("Data")?.GetValue(section) as System.Collections.IEnumerable;

            if (sectionData == null) continue;

            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            worksheet.Range(currentRow, 1, currentRow, 5).Merge();
            currentRow++;

            var firstItem = sectionData.Cast<object>().FirstOrDefault();
            if (firstItem == null)
            {
                worksheet.Cell(currentRow, 1).Value = "No data available for this section.";
                currentRow += 2;
                continue;
            }
            
            // Write headers
            var properties = firstItem.GetType().GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
                worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            currentRow++;
            
            // Write data rows
            foreach (var item in sectionData)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = properties[i].GetValue(item)?.ToString();
                }
                currentRow++;
            }
            currentRow++; // Add a blank row between sections
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        
        return Task.FromResult(stream.ToArray());
    }
}