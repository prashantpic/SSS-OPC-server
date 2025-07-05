using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reporting.Application.Contracts.Generation;
using Reporting.Application.Models;
using Reporting.Domain.Enums;

namespace Reporting.Infrastructure.Services.Generation;

/// <summary>
/// Implements the logic for generating a PDF report using the QuestPDF library.
/// </summary>
public class PdfReportGenerator : IReportGenerator
{
    public ReportFormat Format => ReportFormat.PDF;

    public Task<byte[]> GenerateAsync(ReportDataModel data, CancellationToken cancellationToken = default)
    {
        var document = new ReportDocument(data);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    private class ReportDocument : IDocument
    {
        private readonly ReportDataModel _data;

        public ReportDocument(ReportDataModel data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(_data.ReportTitle).Bold().FontSize(20);
                    column.Item().Text($"Generated: {_data.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                foreach (var section in _data.DataSections)
                {
                    // Naive implementation with reflection
                    var sectionType = section.GetType();
                    var title = sectionType.GetProperty("Title")?.GetValue(section)?.ToString() ?? "Data Section";
                    var sectionData = sectionType.GetProperty("Data")?.GetValue(section) as System.Collections.IEnumerable;

                    if (sectionData == null) continue;
                    
                    column.Item().Text(title).Bold().FontSize(16).PaddingBottom(5);
                    
                    var firstItem = sectionData.Cast<object>().FirstOrDefault();
                    if(firstItem == null)
                    {
                        column.Item().Text("No data available.").PaddingBottom(20);
                        continue;
                    }
                    
                    var properties = firstItem.GetType().GetProperties();
                    
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in properties)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var prop in properties)
                            {
                                header.Cell().Element(CellStyle).Text(prop.Name);
                            }

                            IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        });

                        foreach (var item in sectionData)
                        {
                            foreach (var prop in properties)
                            {
                                table.Cell().Element(CellStyle).Text(prop.GetValue(item)?.ToString() ?? string.Empty);
                            }
                        }
                        IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                    });
                    
                    column.Item().PaddingBottom(20);
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
            });
        }
    }
}