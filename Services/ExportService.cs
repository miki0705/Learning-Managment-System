using Learning_Management_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public class ExportService : IExportService
    {
        public ExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region Helper Styles & Logic
        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container.Background(Colors.Grey.Lighten4)
                            .DefaultTextStyle(x => x.Bold().FontSize(10))
                            .PaddingVertical(8)
                            .PaddingHorizontal(5)
                            .BorderBottom(2)
                            .BorderColor(Colors.Grey.Darken2);
        }

        private static IContainer RowStyle(IContainer container, bool isAlternate = false)
        {
            var style = container.BorderBottom(0.5f)
                            .BorderColor(Colors.Grey.Lighten2)
                            .PaddingVertical(6)
                            .PaddingHorizontal(5);
            
            if (isAlternate)
            {
                style = style.Background(Colors.Grey.Lighten5);
            }
            
            return style;
        }

        private static IContainer SummaryBoxStyle(IContainer container)
        {
            return container.Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .Background(Colors.Grey.Lighten5)
                            .Padding(12);
        }

        // Helper method to get color for attendance status
        private QuestPDF.Infrastructure.Color GetAttendanceStatusColor(AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => Colors.Green.Medium,
                AttendanceStatus.AbsentPaid => Colors.Orange.Medium,
                AttendanceStatus.AbsentFree => Colors.Red.Medium,
                AttendanceStatus.Late => Colors.Blue.Medium,
                _ => Colors.Grey.Medium
            };
        }

        // Helper method to get color for lesson status
        private QuestPDF.Infrastructure.Color GetLessonStatusColor(LessonStatus status)
        {
            return status switch
            {
                LessonStatus.Completed => Colors.Green.Medium,
                LessonStatus.Scheduled => Colors.Blue.Medium,
                LessonStatus.Canceled => Colors.Red.Medium,
                LessonStatus.Holiday => Colors.Orange.Medium,
                _ => Colors.Grey.Medium
            };
        }

        // Helper method to translate status to Polish
        private string GetStatusText(AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "Obecny",
                AttendanceStatus.AbsentPaid => "Nieobecny (płatny)",
                AttendanceStatus.AbsentFree => "Nieobecny (bezpłatny)",
                AttendanceStatus.Late => "Spóźniony",
                _ => status.ToString()
            };
        }

        private string GetStatusText(LessonStatus status)
        {
            return status switch
            {
                LessonStatus.Completed => "Zakończona",
                LessonStatus.Scheduled => "Zaplanowana",
                LessonStatus.Canceled => "Anulowana",
                LessonStatus.Holiday => "Święto",
                _ => status.ToString()
            };
        }
        #endregion

        public async Task ExportToPdfAsync(RevenueReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RAPORT PRZYCHODÓW").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().PaddingTop(5).Text($"Okres: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
                            });
                            row.AutoItem().AlignRight().Column(col =>
                            {
                                col.Item().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                                col.Item().Text("ScholarFlow").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            // Summary boxes
                            column.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("CAŁKOWITY PRZYCHÓD").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalRevenue:F2} PLN").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                                row.ConstantItem(15);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("LICZBA LEKCJI").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalLessons}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });
                            });

                            // Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(75);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Data lekcji");
                                    header.Cell().Element(HeaderCellStyle).Text("Grupa");
                                    header.Cell().Element(HeaderCellStyle).Text("Uczeń");
                                    header.Cell().Element(HeaderCellStyle).Text("Status");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Kwota");
                                });

                                int rowIndex = 0;
                                foreach (var transaction in report.Transactions)
                                {
                                    bool isAlternate = rowIndex % 2 == 1;
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text($"{transaction.LessonDate:dd.MM.yyyy}");
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(transaction.GroupName);
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(transaction.StudentName);
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(GetStatusText(transaction.Status))
                                         .FontColor(GetAttendanceStatusColor(transaction.Status));
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).AlignRight()
                                         .Text($"{transaction.PriceCharged:F2} PLN").FontColor(Colors.Green.Darken1).Bold();
                                    rowIndex++;
                                }
                            });
                        });
                        
                        page.Footer().Row(row =>
                        {
                            row.RelativeItem().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.AutoItem().AlignRight().Text(x =>
                            {
                                x.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Medium));
                                x.Span("Strona ");
                                x.CurrentPageNumber();
                                x.Span(" z ");
                                x.TotalPages();
                            });
                        });
                    });
                });
                document.GeneratePdf(filePath);
            });
        }

        public async Task ExportToPdfAsync(AttendanceReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RAPORT FREKWENCJI").FontSize(22).Bold().FontColor(Colors.Teal.Darken2);
                                col.Item().PaddingTop(5).Text($"Okres: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
                            });
                            row.AutoItem().AlignRight().Column(col =>
                            {
                                col.Item().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                                col.Item().Text("ScholarFlow").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            // Summary boxes
                            column.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("OBECNI").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalPresent}").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("NIEOBECNI (PŁATNE)").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalAbsentPaid}").FontSize(18).Bold().FontColor(Colors.Orange.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("NIEOBECNI (BEZPŁATNE)").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalAbsentFree}").FontSize(18).Bold().FontColor(Colors.Red.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("SPÓŹNIENI").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalLate}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });
                            });

                            // Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(75);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(120);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Data lekcji");
                                    header.Cell().Element(HeaderCellStyle).Text("Grupa");
                                    header.Cell().Element(HeaderCellStyle).Text("Uczeń");
                                    header.Cell().Element(HeaderCellStyle).Text("Status");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Opłata");
                                });

                                int rowIndex = 0;
                                foreach (var entry in report.Entries)
                                {
                                    bool isAlternate = rowIndex % 2 == 1;
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text($"{entry.LessonDate:dd.MM.yyyy}");
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(entry.GroupName);
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(entry.StudentName);
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(GetStatusText(entry.Status))
                                         .FontColor(GetAttendanceStatusColor(entry.Status)).Bold();
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).AlignRight()
                                         .Text($"{entry.PriceCharged:F2} PLN").FontColor(Colors.Grey.Darken1);
                                    rowIndex++;
                                }
                            });
                        });

                        page.Footer().Row(row =>
                        {
                            row.RelativeItem().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.AutoItem().AlignRight().Text(x =>
                            {
                                x.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Medium));
                                x.Span("Strona ");
                                x.CurrentPageNumber();
                                x.Span(" z ");
                                x.TotalPages();
                            });
                        });
                    });
                });
                document.GeneratePdf(filePath);
            });
        }

        public async Task ExportToPdfAsync(WalletReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RAPORT FINANSOWY UCZNIÓW").FontSize(22).Bold().FontColor(Colors.DeepPurple.Darken2);
                                col.Item().PaddingTop(5).Text($"Wygenerowano: {report.GeneratedDate:dd.MM.yyyy HH:mm}").FontSize(11).FontColor(Colors.Grey.Darken1);
                            });
                            row.AutoItem().AlignRight().Column(col =>
                            {
                                col.Item().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                                col.Item().Text("ScholarFlow").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            // Summary boxes
                            column.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("DODATNI BILANS").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalPositiveBalance:F2} PLN").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                                row.ConstantItem(15);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("UJEMNY BILANS").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalNegativeBalance:F2} PLN").FontSize(18).Bold().FontColor(Colors.Red.Darken2);
                                });
                            });

                            // Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(75);
                                    columns.ConstantColumn(85);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(95);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Data");
                                    header.Cell().Element(HeaderCellStyle).Text("Typ");
                                    header.Cell().Element(HeaderCellStyle).Text("Uczeń");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Kwota");
                                    header.Cell().Element(HeaderCellStyle).PaddingLeft(5).Text("Opis");
                                });

                                int rowIndex = 0;
                                foreach (var transaction in report.Transactions)
                                {
                                    bool isAlternate = rowIndex % 2 == 1;
                                    var amountColor = transaction.Amount >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                                    var typeColor = transaction.TransactionType == "Payment" ? Colors.Green.Medium : Colors.Red.Medium;

                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text($"{transaction.TransactionDate:dd.MM.yyyy}");
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(transaction.TransactionType == "Payment" ? "Wpłata" : "Opłata")
                                         .FontColor(typeColor).Bold();
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(transaction.StudentName);
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).AlignRight()
                                         .Text($"{transaction.Amount:F2} PLN").FontColor(amountColor).Bold();
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).PaddingLeft(5).Text(transaction.Description).FontSize(8);
                                    rowIndex++;
                                }
                            });
                        });

                        page.Footer().Row(row =>
                        {
                            row.RelativeItem().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.AutoItem().AlignRight().Text(x =>
                            {
                                x.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Medium));
                                x.Span("Strona ");
                                x.CurrentPageNumber();
                                x.Span(" z ");
                                x.TotalPages();
                            });
                        });
                    });
                });
                document.GeneratePdf(filePath);
            });
        }

        public async Task ExportToPdfAsync(LessonReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Calibri));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SZCZEGÓŁOWY WYKAZ LEKCJI").FontSize(22).Bold().FontColor(Colors.Grey.Darken3);
                                col.Item().PaddingTop(5).Text($"Okres: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}").FontSize(11).FontColor(Colors.Grey.Darken1);
                            });
                            row.AutoItem().AlignRight().Column(col =>
                            {
                                col.Item().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                                col.Item().Text("ScholarFlow").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            // Summary boxes
                            column.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("WSZYSTKIE").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.TotalLessons}").FontSize(18).Bold().FontColor(Colors.Grey.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("ZAKOŃCZONE").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.CompletedLessons}").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("ZAPLANOWANE").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.ScheduledLessons}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("ANULOWANE").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.CanceledLessons}").FontSize(18).Bold().FontColor(Colors.Red.Darken2);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Element(SummaryBoxStyle).Column(c =>
                                {
                                    c.Item().Text("ŚWIĘTA").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(3).Text($"{report.HolidayLessons}").FontSize(18).Bold().FontColor(Colors.Orange.Darken2);
                                });
                            });

                            // Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(75);
                                    columns.ConstantColumn(85);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(60);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Data");
                                    header.Cell().Element(HeaderCellStyle).Text("Godzina");
                                    header.Cell().Element(HeaderCellStyle).Text("Grupa");
                                    header.Cell().Element(HeaderCellStyle).Text("Status");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Frekw.");
                                });

                                int rowIndex = 0;
                                foreach (var lesson in report.Lessons)
                                {
                                    bool isAlternate = rowIndex % 2 == 1;
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text($"{lesson.LessonDate:dd.MM.yyyy}");
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text($"{lesson.StartTime:HH:mm}-{lesson.EndTime:HH:mm}");
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(lesson.GroupName);
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).Text(GetStatusText(lesson.Status))
                                         .FontColor(GetLessonStatusColor(lesson.Status)).Bold();
                                    table.Cell().Element(c => RowStyle(c, isAlternate)).AlignRight().Text($"{lesson.AttendanceCount}");
                                    
                                    if (!string.IsNullOrEmpty(lesson.Note))
                                    {
                                        table.Cell().ColumnSpan(5).PaddingLeft(10).PaddingTop(2).PaddingBottom(5)
                                             .Text($"Notatka: {lesson.Note}").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                                    }
                                    rowIndex++;
                                }
                            });
                        });

                        page.Footer().Row(row =>
                        {
                            row.RelativeItem().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.AutoItem().AlignRight().Text(x =>
                            {
                                x.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Medium));
                                x.Span("Strona ");
                                x.CurrentPageNumber();
                                x.Span(" z ");
                                x.TotalPages();
                            });
                        });
                    });
                });
                document.GeneratePdf(filePath);
            });
        }

        #region CSV Exports
        public async Task ExportToCsvAsync(RevenueReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Raport przychodów");
                writer.WriteLine($"Okres,{report.StartDate:dd.MM.yyyy},{report.EndDate:dd.MM.yyyy}");
                writer.WriteLine($"Całkowity przychód,{report.TotalRevenue.ToString("F2", CultureInfo.InvariantCulture)} PLN");
                writer.WriteLine($"Liczba lekcji,{report.TotalLessons}");
                writer.WriteLine();
                writer.WriteLine("ID,Data lekcji,Grupa,Uczeń,Status,Kwota,ID Lekcji");
                foreach (var transaction in report.Transactions)
                {
                    writer.WriteLine($"{transaction.AttendanceId},{transaction.LessonDate:dd.MM.yyyy},{transaction.GroupName},{transaction.StudentName},{transaction.Status},{transaction.PriceCharged.ToString("F2", CultureInfo.InvariantCulture)},{transaction.LessonId}");
                }
            });
        }

        public async Task ExportToCsvAsync(AttendanceReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Raport frekwencji");
                writer.WriteLine($"Okres,{report.StartDate:dd.MM.yyyy},{report.EndDate:dd.MM.yyyy}");
                writer.WriteLine($"Obecni,{report.TotalPresent}");
                writer.WriteLine($"Nieobecni (płatne),{report.TotalAbsentPaid}");
                writer.WriteLine($"Nieobecni (darmowe),{report.TotalAbsentFree}");
                writer.WriteLine($"Spóźnieni,{report.TotalLate}");
                writer.WriteLine();
                writer.WriteLine("ID,Data lekcji,Grupa,Uczeń,Status,Opłata,ID Lekcji");
                foreach (var entry in report.Entries)
                {
                    writer.WriteLine($"{entry.AttendanceId},{entry.LessonDate:dd.MM.yyyy},{entry.GroupName},{entry.StudentName},{entry.Status},{entry.PriceCharged.ToString("F2", CultureInfo.InvariantCulture)},{entry.LessonId}");
                }
            });
        }

        public async Task ExportToCsvAsync(WalletReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Raport portfeli uczniów");
                writer.WriteLine($"Data wygenerowania,{report.GeneratedDate:dd.MM.yyyy HH:mm}");
                writer.WriteLine($"Całkowity dodatni bilans,{report.TotalPositiveBalance.ToString("F2", CultureInfo.InvariantCulture)} PLN");
                writer.WriteLine($"Całkowity ujemny bilans,{report.TotalNegativeBalance.ToString("F2", CultureInfo.InvariantCulture)} PLN");
                writer.WriteLine();
                writer.WriteLine("ID,Data,Typ,Uczeń,Kwota,Opis,ID Lekcji,ID Płatności");
                foreach (var transaction in report.Transactions)
                {
                    writer.WriteLine($"{transaction.TransactionId},{transaction.TransactionDate:dd.MM.yyyy},{transaction.TransactionType},{transaction.StudentName},{transaction.Amount.ToString("F2", CultureInfo.InvariantCulture)},{transaction.Description},{transaction.LessonId?.ToString() ?? ""},{transaction.PaymentId?.ToString() ?? ""}");
                }
            });
        }

        public async Task ExportToCsvAsync(LessonReportDto report, string filePath)
        {
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Raport lekcji");
                writer.WriteLine($"Okres,{report.StartDate:dd.MM.yyyy},{report.EndDate:dd.MM.yyyy}");
                writer.WriteLine($"Wszystkie lekcje,{report.TotalLessons}");
                writer.WriteLine($"Zakończone,{report.CompletedLessons}");
                writer.WriteLine($"Zaplanowane,{report.ScheduledLessons}");
                writer.WriteLine($"Anulowane,{report.CanceledLessons}");
                writer.WriteLine($"Święta,{report.HolidayLessons}");
                writer.WriteLine();
                writer.WriteLine("ID,Data,Godzina rozpoczęcia,Godzina zakończenia,Grupa,Status,Frekwencja,Notatka");
                foreach (var lesson in report.Lessons)
                {
                    writer.WriteLine($"{lesson.LessonId},{lesson.LessonDate:dd.MM.yyyy},{lesson.StartTime:HH:mm},{lesson.EndTime:HH:mm},{lesson.GroupName},{lesson.Status},{lesson.AttendanceCount},\"{lesson.Note ?? ""}\"");
                }
            });
        }
        #endregion
    }
}