using Opc.System.Services.Reporting.Application.Abstractions;
using Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport;
using Opc.System.Services.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Opc.System.Services.Reporting.Infrastructure.ReportGeneration;

/// <summary>
/// A concrete generator that uses the QuestPDF library to create a report in PDF format from a given data model.
/// To handle the specific logic and layout for converting structured report data into a PDF document.
/// </summary>
public class PdfGenerator : IReportFormatGenerator
{
    /// <inheritdoc />
    public ReportFormat Format => ReportFormat.Pdf;

    /// <summary>
    /// Generates a PDF report from the provided data model.
    /// </summary>
    /// <param name="data">The data model containing all information for the report.</param>
    /// <returns>A memory stream containing the generated PDF document.</returns>
    public Stream Generate(ReportDataModel data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text(data.ReportTitle)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Spacing(20);

                        col.Item().Text($"Report Period: {data.TimeRange.StartTime:g} to {data.TimeRange.EndTime:g}");
                        
                        if (data.DataPoints.Any())
                        {
                            col.Item().Element(c => ComposeTable(c, data.DataPoints));
                        }

                        if (data.Anomalies.Any())
                        {
                            col.Item().Element(c => ComposeAnomalies(c, data.Anomalies));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
    
    private void ComposeTable(IContainer container, IEnumerable<DataPoint> dataPoints)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(150);
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Tag");
                header.Cell().Element(CellStyle).Text("Timestamp");
                header.Cell().Element(CellStyle).Text("Value");

                static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var item in dataPoints)
            {
                table.Cell().Element(CellStyle).Text(item.Tag);
                table.Cell().Element(CellStyle).Text($"{item.Timestamp:g}");
                table.Cell().Element(CellStyle).Text(item.Value.ToString());
                
                static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
        });
    }
    
    private void ComposeAnomalies(IContainer container, IEnumerable<AnomalyInsight> anomalies)
    {
        container.Column(col => 
        {
            col.Item().Text("Anomaly Insights").Bold().FontSize(16);
            col.Item().Table(table => 
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(150);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
                
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Tag");
                    header.Cell().Element(CellStyle).Text("Timestamp");
                    header.Cell().Element(CellStyle).Text("Actual Value");
                    header.Cell().Element(CellStyle).Text("Expected Value");
                    static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                foreach (var item in anomalies)
                {
                    table.Cell().Element(CellStyle).Text(item.Tag);
                    table.Cell().Element(CellStyle).Text($"{item.Timestamp:g}");
                    table.Cell().Element(CellStyle).Text(item.ActualValue.ToString("F2"));
                    table.Cell().Element(CellStyle).Text(item.ExpectedValue.ToString("F2"));
                    static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        });
    }
}