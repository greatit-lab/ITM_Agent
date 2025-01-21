// Services\PdfMergeManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Geom;

namespace ITM_Agent.Services
{
    public class PdfMergeManager
    {
        private readonly LogManager logManager; // 로깅 관리자
        private string _outputFolder;

        private string OutputFolder
        {
            get => _outputFolder;
            set
            {
                if (!Directory.Exists(value))
                {
                    logManager.LogError($"[PdfMergeManager] Output folder does not exist: {value}");
                    throw new DirectoryNotFoundException("Output folder does not exist.");
                }

                _outputFolder = value;
            }
        }

        public PdfMergeManager(string defaultOutputFolder, LogManager logManager)
        {
            this.logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));

            if (!Directory.Exists(defaultOutputFolder))
            {
                logManager.LogError($"[PdfMergeManager] Default output folder does not exist: {defaultOutputFolder}");
                throw new DirectoryNotFoundException("Default output folder does not exist.");
            }

            _outputFolder = defaultOutputFolder;
            logManager.LogEvent($"[PdfMergeManager] Initialized with default output folder: {defaultOutputFolder}");
        }

        public void UpdateOutputFolder(string outputFolder)
        {
            OutputFolder = outputFolder;
            logManager.LogEvent($"[PdfMergeManager] Output folder updated to: {outputFolder}");
        }

        public void MergeImagesToPdf(List<string> imagePaths, string outputPdfPath)
        {
            try
            {
                if (imagePaths == null || imagePaths.Count == 0)
                {
                    logManager.LogEvent("[PdfMergeManager] No images to merge. Aborting MergeImagesToPdf().");
                    return;
                }
        
                // 출력 경로 폴더 확인
                string pdfDirectory = Path.GetDirectoryName(outputPdfPath);
                if (string.IsNullOrEmpty(pdfDirectory))
                {
                    logManager.LogError($"[PdfMergeManager] Invalid outputPdfPath: {outputPdfPath}");
                    return;
                }
        
                if (!Directory.Exists(pdfDirectory))
                {
                    Directory.CreateDirectory(pdfDirectory);
                    logManager.LogEvent($"[PdfMergeManager] Created directory: {pdfDirectory}");
                }
        
                logManager.LogEvent($"[PdfMergeManager] Starting MergeImagesToPdf with custom page sizes. Output: {outputPdfPath}, Images: {imagePaths.Count}");
        
                // PDF 작성 시작
                using (var pdfWriter = new PdfWriter(outputPdfPath))
                using (var pdfDocument = new PdfDocument(pdfWriter))
                using (var document = new Document(pdfDocument))
                {
                    // 문서 여백을 0으로 설정 (이미지를 페이지 끝까지 꽉 채우려면 필요)
                    document.SetMargins(0, 0, 0, 0);
        
                    for (int i = 0; i < imagePaths.Count; i++)
                    {
                        string filePath = imagePaths[i];
                        try
                        {
                            // 이미지 로드
                            var imageData = ImageDataFactory.Create(filePath);
                            var image = new Image(imageData);
                            float imgWidthPx = image.GetImageWidth();
                            float imgHeightPx = image.GetImageHeight();
        
                            // --------------------------
                            // (1) 페이지 크기 설정 (픽셀 그대로 사용)
                            //     만약 DPI 변환을 원하면, 예: pageWidthPt = imgWidthPx * 72f / dpi;
                            // --------------------------
                            float pageWidthPt = imgWidthPx;
                            float pageHeightPt = imgHeightPx;
                            PageSize customSize = new PageSize(pageWidthPt, pageHeightPt);
        
                            // (i > 0)일 때는 새 페이지로 넘어가야 함
                            // (처음 페이지는 문서 생성 시 자동 생성)
                            if (i > 0)
                            {
                                // 페이지 넘어가기 (AreaBreak)
                                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                            }
        
                            // 현재 페이지 사이즈를 변경
                            // (주의: AreaBreak 이후에 SetDefaultPageSize를 해야 해당 페이지가 해당 크기 적용)
                            pdfDocument.SetDefaultPageSize(customSize);
        
                            // --------------------------
                            // (2) 이미지 크기/위치 설정
                            // --------------------------
                            // autoScale(false) 후, 이미지 크기를 "페이지 전체"에 맞춤
                            image.SetAutoScale(false);
                            image.SetFixedPosition(0, 0);
                            image.SetWidth(pageWidthPt);
                            image.SetHeight(pageHeightPt);
        
                            // 이미지 배치
                            document.Add(image);
        
                            logManager.LogDebug($"[PdfMergeManager] Added image as page {i+1}: {filePath} (size: {imgWidthPx} x {imgHeightPx})");
                        }
                        catch (Exception exImg)
                        {
                            logManager.LogError($"[PdfMergeManager] Error adding image '{filePath}' to PDF: {exImg.Message}");
                        }
                    }
        
                    document.Close();
                }
        
                logManager.LogEvent($"[PdfMergeManager] Successfully created PDF: {outputPdfPath}");
            }
            catch (Exception ex)
            {
                logManager.LogError($"[PdfMergeManager] MergeImagesToPdf failed. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
