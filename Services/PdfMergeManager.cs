// Services\PdfMergeManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using ITM_Agent.Services;

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

        public void MergeImagesToPDF(string targetFolder, string outputFolder)
        {
            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    logManager.LogError($"[PdfMergeManager] Target folder does not exist: {targetFolder}");
                    throw new DirectoryNotFoundException($"Target folder does not exist: {targetFolder}");
                }

                if (!Directory.Exists(outputFolder))
                {
                    logManager.LogError($"[PdfMergeManager] Output folder does not exist: {outputFolder}");
                    throw new DirectoryNotFoundException($"Output folder does not exist: {outputFolder}");
                }

                logManager.LogEvent($"[PdfMergeManager] Starting PDF merge. Target folder: {targetFolder}, Output folder: {outputFolder}");

                var groupedFiles = Directory.GetFiles(targetFolder, "*.jpg")
                    .GroupBy(file =>
                    {
                        var filename = Path.GetFileNameWithoutExtension(file);
                        var underscoreIndex = filename.LastIndexOf('_');
                        return underscoreIndex > 0 ? filename.Substring(0, underscoreIndex) : null;
                    })
                    .Where(g => g.Key != null)
                    .ToList();

                foreach (var group in groupedFiles)
                {
                    string outputFilePath = Path.Combine(outputFolder, $"{group.Key}.pdf");

                    try
                    {
                        using (var pdfWriter = new PdfWriter(outputFilePath))
                        using (var pdfDocument = new PdfDocument(pdfWriter))
                        using (var document = new Document(pdfDocument))
                        {
                            foreach (var filePath in group.OrderBy(f => f))
                            {
                                var imageData = ImageDataFactory.Create(filePath);
                                var image = new iText.Layout.Element.Image(imageData);
                                image.SetAutoScale(true);
                                document.Add(image);
                            }

                            document.Close();
                        }

                        logManager.LogEvent($"[PdfMergeManager] Successfully created PDF: {outputFilePath}");
                    }
                    catch (Exception ex)
                    {
                        logManager.LogError($"[PdfMergeManager] Error creating PDF: {outputFilePath}. Exception: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logManager.LogError($"[PdfMergeManager] MergeImagesToPDF failed. Exception: {ex.Message}");
                throw; // 예외를 다시 던져 상위 호출자가 처리할 수 있도록 함
            }
        }
    }
}
