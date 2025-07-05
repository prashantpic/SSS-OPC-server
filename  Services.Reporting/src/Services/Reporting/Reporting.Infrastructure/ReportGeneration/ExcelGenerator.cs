using ClosedXML.Excel;
using Opc.System.Services.Reporting.Application.Abstractions;
using Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport;
using Opc.System.Services.Reporting.Application.Models;

namespace Opc.System.Services.Reporting.Infrastructure.ReportGeneration;

/// <summary>
/// A concrete generator that uses the ClosedXML library to create a report in Excel format from a given data model.
/// To handle the specific logic for converting structured report data into an Excel (.xlsx) file.
/// </summary>
public class ExcelGenerator : IReportFormatGenerator
{
    /// <inheritdoc />
    public ReportFormat Format => ReportFormat.Excel;

    /// <summary>
    /// Generates an Excel report from the provided data model.
    /// </summary>
    /// <param name="data">The data model containing all information for the report.</param>
    /// <returns>A memory stream containing the generated Excel workbook.</returns>
    public Stream Generate(ReportDataModel data)
    {
        using var workbook = new XLWorkbook();
        
        if (data.DataPoints.Any())
        {
            var historicalDataSheet = workbook.Worksheets.Add("Historical Data");
            historicalDataSheet.Cell(1, 1).Value = "Tag";
            historicalDataSheet.Cell(1, 2).Value = "Timestamp";
            historicalDataSheet.Cell(1, 3).Value = "Value";

            var header = historicalDataSheet.Row(1);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            var historicalData = data.DataPoints.Select(dp => new
            {
                dp.Tag,
                Timestamp = dp.Timestamp.ToString("g"),
                Value = dp.Value.ToString()
            });

            historicalDataSheet.Cell(2, 1).InsertData(historicalData);
            historicalDataSheet.Columns().AdjustToContents();
        }

        if (data.Anomalies.Any())
        {
            var anomaliesSheet = workbook.Worksheets.Add("Anomaly Insights");
            anomaliesSheet.Cell(1, 1).Value = "Tag";
            anomaliesSheet.Cell(1, 2).Value = "Timestamp";
            anomaliesSheet.Cell(1, 3).Value = "Actual Value";
            anomaliesSheet.Cell(1, 4).Value = "Expected Value";
            anomaliesSheet.Cell(1, 5).Value = "Severity";

            var header = anomaliesSheet.Row(1);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;

            anomaliesSheet.Cell(2, 1).InsertData(data.Anomalies);
            anomaliesSheet.Columns().AdjustToContents();
        }

        var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}