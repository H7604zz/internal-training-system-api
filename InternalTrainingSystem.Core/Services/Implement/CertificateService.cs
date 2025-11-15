using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepo;

        public CertificateService(ICertificateRepository certificateRepo)
        {
            _certificateRepo = certificateRepo;
            QuestPDF.Settings.License = LicenseType.Community;
            
            // Đảm bảo encoding UTF-8 cho tiếng Việt
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task<CertificateResponse?> GetCertificateAsync(int courseId, string userId)
        {
            return await _certificateRepo.GetCertificateAsync(courseId, userId);
        }

        public async Task<List<CertificateResponse>> GetCertificateByUserAsync(string userId)
        {
            return await _certificateRepo.GetCertificateByUserAsync(userId);
        }

        public async Task IssueCertificateAsync(string userId, int courseId)
        {
            await _certificateRepo.IssueCertificateAsync(userId, courseId);
        }

        public async Task<byte[]> GenerateCertificatePdfAsync(int courseId, string userId)
        {
            var certificate = await _certificateRepo.GetCertificateAsync(courseId, userId);
            
            if (certificate == null)
            {
                throw new InvalidOperationException("Chứng chỉ không tồn tại.");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.PageColor(Colors.White);
                    
                    // Không set font family cụ thể - để QuestPDF tự động xử lý Unicode
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().Column(column =>
                    {
                        // Gradient purple border - giảm padding để vừa 1 trang
                        column.Item().Background("#9F7AEA").Padding(20).Column(outerBorderColumn =>
                        {
                            // Inner white content với double border xanh
                            outerBorderColumn.Item().Border(6).BorderColor("#4A90E2").Background(Colors.White)
                                .Padding(20).Column(contentColumn =>
                                {
                                    // Header with Logo
                                    contentColumn.Item().AlignCenter().Column(headerCol =>
                                    {
                                        // Logo placeholder - tròn với checkmark (giảm size)
                                        headerCol.Item().AlignCenter().Width(50).Height(50)
                                            .CornerRadius(25)
                                            .Border(2).BorderColor("#4A90E2")
                                            .Background(Colors.White)
                                            .AlignCenter().AlignMiddle()
                                            .Text("✓").FontSize(24).FontColor("#4A90E2").Bold();

                                        headerCol.Item().PaddingTop(6);

                                        // Organization name
                                        headerCol.Item().AlignCenter().Text("HỆ THỐNG ĐÀO TẠO NỘI BỘ")
                                            .FontSize(10).Bold().FontColor("#2c3e50");
                                    });

                                    contentColumn.Item().PaddingVertical(8);

                                    // Title Section
                                    contentColumn.Item().AlignCenter().Column(titleCol =>
                                    {
                                        titleCol.Item().AlignCenter().Text("CHỨNG CHỈ HOÀN THÀNH")
                                            .FontSize(24).Bold().FontColor("#4A5EAA");

                                        titleCol.Item().PaddingVertical(6);

                                        // Decorative line với icon giữa
                                        titleCol.Item().AlignCenter().Width(400).Row(decorRow =>
                                        {
                                            decorRow.RelativeItem().Height(1.5f).Background("#4A90E2");
                                            decorRow.ConstantItem(25).AlignCenter().Text("✦")
                                                .FontSize(12).FontColor("#4A90E2");
                                            decorRow.RelativeItem().Height(1.5f).Background("#4A90E2");
                                        });
                                    });

                                    contentColumn.Item().PaddingVertical(8);

                                    // Body
                                    contentColumn.Item().Column(bodyCol =>
                                    {
                                        bodyCol.Item().AlignCenter().Text("Trao cho học viên:")
                                            .FontSize(13).Italic().FontColor("#555555");

                                        bodyCol.Item().PaddingVertical(5);

                                        // Employee name với border bottom
                                        bodyCol.Item().AlignCenter().Column(nameCol =>
                                        {
                                            nameCol.Item().AlignCenter().Text(certificate.FullName.ToUpper())
                                                .FontSize(20).Bold().FontColor("#2c3e50");
                                            nameCol.Item().PaddingTop(4).AlignCenter().Width(280).Height(2.5f)
                                                .Background("#4A90E2");
                                        });

                                        bodyCol.Item().PaddingVertical(8);

                                        bodyCol.Item().AlignCenter().Text("Đã hoàn thành khóa học:")
                                            .FontSize(12).FontColor("#555555");

                                        bodyCol.Item().PaddingVertical(5);

                                        bodyCol.Item().AlignCenter().Text(certificate.CourseName)
                                            .FontSize(16).Bold().FontColor("#4A90E2");

                                        bodyCol.Item().PaddingVertical(8);

                                        // Course Info Grid với background xám - giảm size
                                        bodyCol.Item().AlignCenter().Width(450).Background("#f8f9fa")
                                            .Border(1.5f).BorderColor("#e9ecef")
                                            .Padding(12).Row(gridRow =>
                                            {
                                                gridRow.RelativeItem().Column(leftCol =>
                                                {
                                                    leftCol.Item().AlignCenter().Text("Mã khóa học:")
                                                        .FontSize(9).FontColor("#666666");
                                                    leftCol.Item().PaddingTop(4).AlignCenter().Text(certificate.CourseCode)
                                                        .FontSize(12).Bold().FontColor("#2c3e50");
                                                });

                                                gridRow.RelativeItem().Column(rightCol =>
                                                {
                                                    rightCol.Item().AlignCenter().Text("Tên chứng chỉ:")
                                                        .FontSize(9).FontColor("#666666");
                                                    rightCol.Item().PaddingTop(4).AlignCenter().Text(certificate.CertificateName)
                                                        .FontSize(12).Bold().FontColor("#2c3e50");
                                                });
                                            });

                                        bodyCol.Item().PaddingVertical(8);

                                        bodyCol.Item().AlignCenter().Text($"Ngày cấp: {certificate.IssueDate:dd/MM/yyyy}")
                                            .FontSize(12).Bold().FontColor("#555555");
                                    });

                                    contentColumn.Item().PaddingVertical(8);

                                    // Footer - Signatures (giảm size)
                                    contentColumn.Item().Row(footerRow =>
                                    {
                                        // Left side - SEAL placeholder
                                        footerRow.RelativeItem().Column(sealCol =>
                                        {
                                            sealCol.Item().Width(65).Height(65)
                                                .CornerRadius(32.5f)
                                                .Border(2.5f).BorderColor("#4A90E2")
                                                .AlignCenter().AlignMiddle()
                                                .Text("SEAL").FontSize(14).Bold().FontColor("#4A90E2");
                                        });

                                        // Right side - Signature
                                        footerRow.RelativeItem().AlignCenter().Column(signCol =>
                                        {
                                            signCol.Item().PaddingBottom(30).Text(""); // Space for signature
                                            signCol.Item().Width(160).Height(1)
                                                .Background("#333333");
                                            signCol.Item().PaddingTop(6).AlignCenter().Text("Giám đốc đào tạo")
                                                .FontSize(11).Bold().FontColor("#2c3e50");
                                        });
                                    });
                                });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
