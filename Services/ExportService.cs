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
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Raport przychodów")
                            .FontSize(20)
                            .Bold()
                            .AlignCenter();

                        page.Content()
                            .Column(column =>
                            {
                                column.Item().Text($"Okres: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}");
                                column.Item().PaddingTop(10).Text($"Całkowity przychód: {report.TotalRevenue:F2} PLN").Bold();
                                column.Item().Text($"Liczba lekcji: {report.TotalLessons}");

                                if (report.RevenueByGroup.Any())
                                {
                                    column.Item().PaddingTop(20).Text("Przychód według grup:").Bold();
                                    foreach (var group in report.RevenueByGroup)
                                    {
                                        column.Item().PaddingTop(5).Text($"  {group.GroupName}: {group.Revenue:F2} PLN ({group.LessonCount} lekcji)");
                                    }
                                }
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
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Raport frekwencji")
                            .FontSize(20)
                            .Bold()
                            .AlignCenter();

                        page.Content()
                            .Column(column =>
                            {
                                column.Item().Text($"Okres: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}");
                                column.Item().PaddingTop(10).Text("Podsumowanie:").Bold();
                                column.Item().Text($"  Obecni: {report.TotalPresent}");
                                column.Item().Text($"  Nieobecni (płatne): {report.TotalAbsentPaid}");
                                column.Item().Text($"  Nieobecni (darmowe): {report.TotalAbsentFree}");
                                column.Item().Text($"  Spóźnieni: {report.TotalLate}");

                                if (report.AttendanceByStudent.Any())
                                {
                                    column.Item().PaddingTop(20).Text("Frekwencja według uczniów:").Bold();
                                    foreach (var student in report.AttendanceByStudent)
                                    {
                                        column.Item().PaddingTop(5).Text($"  {student.StudentName}:");
                                        column.Item().Text($"    Obecni: {student.Present}, Nieobecni (płatne): {student.AbsentPaid}, Nieobecni (darmowe): {student.AbsentFree}, Spóźnieni: {student.Late}");
                                    }
                                }
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
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Raport portfeli uczniów")
                            .FontSize(20)
                            .Bold()
                            .AlignCenter();

                        page.Content()
                            .Column(column =>
                            {
                                column.Item().Text($"Data wygenerowania: {report.GeneratedDate:dd.MM.yyyy HH:mm}");
                                column.Item().PaddingTop(10).Text($"Całkowity dodatni bilans: {report.TotalPositiveBalance:F2} PLN").Bold();
                                column.Item().Text($"Całkowity ujemny bilans: {report.TotalNegativeBalance:F2} PLN").Bold();

                                if (report.StudentWallets.Any())
                                {
                                    column.Item().PaddingTop(20).Text("Portfele uczniów:").Bold();
                                    foreach (var wallet in report.StudentWallets)
                                    {
                                        column.Item().PaddingTop(5).Text($"  {wallet.StudentName}:");
                                        column.Item().Text($"    Bilans: {wallet.Balance:F2} PLN");
                                        column.Item().Text($"    Wpłaty: {wallet.TotalPayments:F2} PLN, Opłaty: {wallet.TotalCharges:F2} PLN");
                                    }
                                }
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
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Raport lekcji")
                            .FontSize(20)
                            .Bold()
                            .AlignCenter();

                        page.Content()
                            .Column(column =>
                            {
                                column.Item().Text($"Okres: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}");
                                column.Item().PaddingTop(10).Text("Podsumowanie:").Bold();
                                column.Item().Text($"  Wszystkie lekcje: {report.TotalLessons}");
                                column.Item().Text($"  Zakończone: {report.CompletedLessons}");
                                column.Item().Text($"  Zaplanowane: {report.ScheduledLessons}");
                                column.Item().Text($"  Anulowane: {report.CanceledLessons}");
                                column.Item().Text($"  Święta: {report.HolidayLessons}");

                                if (report.LessonsByGroup.Any())
                                {
                                    column.Item().PaddingTop(20).Text("Lekcje według grup:").Bold();
                                    foreach (var group in report.LessonsByGroup)
                                    {
                                        column.Item().PaddingTop(5).Text($"  {group.GroupName}:");
                                        column.Item().Text($"    Wszystkie: {group.Total}, Zakończone: {group.Completed}, Zaplanowane: {group.Scheduled}, Anulowane: {group.Canceled}, Święta: {group.Holiday}");
                                    }
                                }
                            });
                    });
                });

                document.GeneratePdf(filePath);
            });
        }

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
                writer.WriteLine("Grupa,Przychód,Liczba lekcji");
                foreach (var group in report.RevenueByGroup)
                {
                    writer.WriteLine($"{group.GroupName},{group.Revenue.ToString("F2", CultureInfo.InvariantCulture)},{group.LessonCount}");
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
                writer.WriteLine("Uczeń,Obecni,Nieobecni (płatne),Nieobecni (darmowe),Spóźnieni");
                foreach (var student in report.AttendanceByStudent)
                {
                    writer.WriteLine($"{student.StudentName},{student.Present},{student.AbsentPaid},{student.AbsentFree},{student.Late}");
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
                writer.WriteLine("Uczeń,Bilans,Wpłaty,Opłaty");
                foreach (var wallet in report.StudentWallets)
                {
                    writer.WriteLine($"{wallet.StudentName},{wallet.Balance.ToString("F2", CultureInfo.InvariantCulture)},{wallet.TotalPayments.ToString("F2", CultureInfo.InvariantCulture)},{wallet.TotalCharges.ToString("F2", CultureInfo.InvariantCulture)}");
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
                writer.WriteLine("Grupa,Wszystkie,Zakończone,Zaplanowane,Anulowane,Święta");
                foreach (var group in report.LessonsByGroup)
                {
                    writer.WriteLine($"{group.GroupName},{group.Total},{group.Completed},{group.Scheduled},{group.Canceled},{group.Holiday}");
                }
            });
        }
    }
}
