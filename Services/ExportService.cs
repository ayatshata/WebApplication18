using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models.DTOs;
using MughtaribatHouse.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MughtaribatHouse.Services
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportPaymentsToExcelAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _context.Payments
                .Include(p => p.Resident)
                .Include(p => p.ProcessedByUser)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("المدفوعات");

            // Add headers (right-to-left)
            worksheet.Cell(1, 1).Value = "المعرف";
            worksheet.Cell(1, 2).Value = "اسم المقيم";
            worksheet.Cell(1, 3).Value = "المبلغ";
            worksheet.Cell(1, 4).Value = "تاريخ الدفع";
            worksheet.Cell(1, 5).Value = "لشهر";
            worksheet.Cell(1, 6).Value = "طريقة الدفع";
            worksheet.Cell(1, 7).Value = "ملاحظات";
            worksheet.Cell(1, 8).Value = "معالج بواسطة";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add data
            for (int i = 0; i < payments.Count; i++)
            {
                var payment = payments[i];
                worksheet.Cell(i + 2, 1).Value = payment.Id;
                worksheet.Cell(i + 2, 2).Value = payment.Resident.FullName;
                worksheet.Cell(i + 2, 3).Value = payment.Amount;
                worksheet.Cell(i + 2, 4).Value = payment.PaymentDate.ToString("yyyy/MM/dd");
                worksheet.Cell(i + 2, 5).Value = payment.ForMonth.ToString("yyyy/MM");
                worksheet.Cell(i + 2, 6).Value = payment.PaymentMethod;
                worksheet.Cell(i + 2, 7).Value = payment.Notes;
                worksheet.Cell(i + 2, 8).Value = payment.ProcessedByUser?.FullName;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportResidentsToExcelAsync()
        {
            var residents = await _context.Residents
                .Include(r => r.ManagedByUser)
                .OrderBy(r => r.FullName)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("المقيمين");

            // Add headers
            worksheet.Cell(1, 1).Value = "المعرف";
            worksheet.Cell(1, 2).Value = "الاسم الكامل";
            worksheet.Cell(1, 3).Value = "رقم الهوية";
            worksheet.Cell(1, 4).Value = "رقم الجوال";
            worksheet.Cell(1, 5).Value = "البريد الإلكتروني";
            worksheet.Cell(1, 6).Value = "رقم الغرفة";
            worksheet.Cell(1, 7).Value = "تاريخ التسكين";
            worksheet.Cell(1, 8).Value = "الإيجار الشهري";
            worksheet.Cell(1, 9).Value = "الحالة";
            worksheet.Cell(1, 10).Value = "مدير بواسطة";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add data
            for (int i = 0; i < residents.Count; i++)
            {
                var resident = residents[i];
                worksheet.Cell(i + 2, 1).Value = resident.Id;
                worksheet.Cell(i + 2, 2).Value = resident.FullName;
                worksheet.Cell(i + 2, 3).Value = resident.IdentityNumber;
                worksheet.Cell(i + 2, 4).Value = resident.PhoneNumber;
                worksheet.Cell(i + 2, 5).Value = resident.Email;
                worksheet.Cell(i + 2, 6).Value = resident.RoomNumber;
                worksheet.Cell(i + 2, 7).Value = resident.CheckInDate.ToString("yyyy/MM/dd");
                worksheet.Cell(i + 2, 8).Value = resident.MonthlyRent;
                worksheet.Cell(i + 2, 9).Value = resident.IsActive ? "نشط" : "مغادر";
                worksheet.Cell(i + 2, 10).Value = resident.ManagedByUser?.FullName;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportFinancialReportToPdfAsync(FinancialSummaryDto financialData, DateTime startDate, DateTime endDate)
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
                        .AlignCenter()
                        .Text("تقرير مالي - بيت المقتربات")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken3);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Period information
                            column.Item().Border(1).Padding(10).Background(Colors.Grey.Lighten3).Column(innerColumn =>
                            {
                                innerColumn.Item().Text($"الفترة: من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}");
                                innerColumn.Item().Text($"تاريخ التصدير: {DateTime.Now:yyyy/MM/dd}");
                            });

                            // Financial summary
                            column.Item().Border(1).Padding(10).Column(innerColumn =>
                            {
                                innerColumn.Item().Text("الملخص المالي").SemiBold().FontSize(14);
                                innerColumn.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Text("إجمالي الإيرادات:");
                                    row.ConstantItem(100).AlignRight().Text($"{financialData.TotalRevenue:C}");
                                });
                                innerColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("إجمالي المصروفات:");
                                    row.ConstantItem(100).AlignRight().Text($"{financialData.TotalExpenses:C}");
                                });
                                innerColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("صافي الربح:");
                                    row.ConstantItem(100).AlignRight().Text($"{financialData.NetProfit:C}");
                                });
                                innerColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("هامش الربح:");
                                    row.ConstantItem(100).AlignRight().Text($"{financialData.ProfitMargin:F2}%");
                                });
                            });

                            // Revenue by category
                            if (financialData.RevenueByCategory.Any())
                            {
                                column.Item().Border(1).Padding(10).Column(innerColumn =>
                                {
                                    innerColumn.Item().Text("الإيرادات حسب الفئة").SemiBold().FontSize(14);
                                    innerColumn.Item().PaddingTop(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(100);
                                            columns.ConstantColumn(80);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Text("الفئة");
                                            header.Cell().AlignRight().Text("المبلغ");
                                            header.Cell().AlignRight().Text("النسبة");
                                        });

                                        foreach (var item in financialData.RevenueByCategory)
                                        {
                                            table.Cell().Text(item.Category);
                                            table.Cell().AlignRight().Text($"{item.Amount:C}");
                                            table.Cell().AlignRight().Text($"{item.Percentage:F1}%");
                                        }
                                    });
                                });
                            }

                            // Expenses by category
                            if (financialData.ExpensesByCategory.Any())
                            {
                                column.Item().Border(1).Padding(10).Column(innerColumn =>
                                {
                                    innerColumn.Item().Text("المصروفات حسب الفئة").SemiBold().FontSize(14);
                                    innerColumn.Item().PaddingTop(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(100);
                                            columns.ConstantColumn(80);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Text("الفئة");
                                            header.Cell().AlignRight().Text("المبلغ");
                                            header.Cell().AlignRight().Text("النسبة");
                                        });

                                        foreach (var item in financialData.ExpensesByCategory)
                                        {
                                            table.Cell().Text(item.Category);
                                            table.Cell().AlignRight().Text($"{item.Amount:C}");
                                            table.Cell().AlignRight().Text($"{item.Percentage:F1}%");
                                        }
                                    });
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("الصفحة ");
                            text.CurrentPageNumber();
                            text.Span(" من ");
                            text.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportOccupancyReportToPdfAsync(OccupancyReportResult occupancyData)
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
                        .AlignCenter()
                        .Text("تقرير الإشغال - بيت المقتربات")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken3);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Occupancy summary
                            column.Item().Border(1).Padding(10).Column(innerColumn =>
                            {
                                innerColumn.Item().Text("ملخص الإشغال").SemiBold().FontSize(14);
                                innerColumn.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Text("إجمالي الغرف:");
                                    row.ConstantItem(100).AlignRight().Text(occupancyData.TotalRooms.ToString());
                                });
                                innerColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("الغرف المشغولة:");
                                    row.ConstantItem(100).AlignRight().Text(occupancyData.OccupiedRooms.ToString());
                                });
                                innerColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("الغرف الشاغرة:");
                                    row.ConstantItem(100).AlignRight().Text(occupancyData.VacantRooms.ToString());
                                });
                                innerColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("معدل الإشغال:");
                                    row.ConstantItem(100).AlignRight().Text($"{occupancyData.OccupancyRate:F1}%");
                                });
                            });

                            // Room status details
                            if (occupancyData.RoomStatuses.Any())
                            {
                                column.Item().Border(1).Padding(10).Column(innerColumn =>
                                {
                                    innerColumn.Item().Text("تفاصيل حالة الغرف").SemiBold().FontSize(14);
                                    innerColumn.Item().PaddingTop(10).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(80);
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(100);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Text("رقم الغرفة");
                                            header.Cell().Text("الحالة");
                                            header.Cell().Text("اسم المقيم");
                                            header.Cell().Text("تاريخ التسكين");
                                        });

                                        foreach (var room in occupancyData.RoomStatuses)
                                        {
                                            table.Cell().Text(room.RoomNumber);
                                            table.Cell().Text(room.Status);
                                            table.Cell().Text(room.ResidentName);
                                            table.Cell().Text(room.CheckInDate?.ToString("yyyy/MM/dd") ?? "-");
                                        }
                                    });
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("الصفحة ");
                            text.CurrentPageNumber();
                            text.Span(" من ");
                            text.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}