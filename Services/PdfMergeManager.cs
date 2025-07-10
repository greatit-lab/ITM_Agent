// Services\PdfMergeManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;

namespace ITM_Agent.Services
{
    public class PdfMergeManager
    {
        private readonly LogManager logManager; // 로깅 관리자
        private string _outputFolder;
        private readonly bool isDebugMode;
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
        
                // 출력 폴더 및 파일 경로 준비
                string pdfDirectory = System.IO.Path.GetDirectoryName(outputPdfPath);
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
        
                // -> System.IO.Path로 명시
                string fileName = System.IO.Path.GetFileName(outputPdfPath);
                int imageCount = imagePaths.Count;
                
                logManager.LogEvent(
                    $"[PdfMergeManager] Starting MergeImagesToPdf. " +
                    $"Output: {fileName}, Images: {imageCount} -> Successfully created PDF"
                );
        
                // PDF 생성
                using (var writer = new PdfWriter(outputPdfPath))
                using (var pdfDoc = new PdfDocument(writer))
                using (var document = new Document(pdfDoc))
                {
                    document.SetMargins(0, 0, 0, 0);
                    for (int i = 0; i < imagePaths.Count; i++)
                    {
                        string imgPath = imagePaths[i];
                        try
                        {
                            // 이미지 로드
                            var imgData = ImageDataFactory.Create(imgPath);
                            var img = new Image(imgData);
                            float w = img.GetImageWidth(), h = img.GetImageHeight();
                            var pageSize = new PageSize(w, h);
                            
                            if (i > 0)
                                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                            pdfDoc.SetDefaultPageSize(pageSize);
                            img.SetAutoScale(false);
                            img.SetFixedPosition(0, 0);
                            img.SetWidth(w);
                            img.SetHeight(h);
                            document.Add(img);
                            if (isDebugMode)
                            {
                                logManager.LogDebug($"[PdfMergeManager] Added page {i + 1}: {imgPath} ({w} X {h})");
                            }
                        }
                        catch (Exception exImg)
                        {
                            logManager.LogError($"[PdfMergeManager] Error adding image '{imgPath}': {exImg.Message}");
                        }
                    }
                    document.Close();
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"[PdfMergeManager] MergeImagesToPdf failed. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
