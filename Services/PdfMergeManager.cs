using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

namespace ITM_Agent.Services
{
    public class PdfMergeManager
    {
        private string _outputFolder;

        public PdfMergeManager(string defaultOutputFolder)
        {
            if (!Directory.Exists(defaultOutputFolder))
                throw new DirectoryNotFoundException("Default output folder does not exist.");

            _outputFolder = defaultOutputFolder;
        }

        public void UpdateOutputFolder(string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
                throw new DirectoryNotFoundException("Output folder does not exist.");

            _outputFolder = outputFolder;
        }

        public async Task MergeImagesToPDF(string targetFolder, int waitTime)
        {
            if (!Directory.Exists(targetFolder))
                throw new DirectoryNotFoundException("Target folder does not exist.");

            var groupedFiles = Directory.GetFiles(targetFolder, "*.jpg")
                .GroupBy(f =>
                {
                    var match = Regex.Match(Path.GetFileName(f), @"_(\d+)\.jpg$");
                    return match.Success ? Path.GetFileNameWithoutExtension(f).Split('_')[0] : null;
                })
                .Where(g => g.Key != null)
                .ToList();

            await Task.Delay(waitTime * 1000);

            foreach (var group in groupedFiles)
            {
                string outputFilePath = Path.Combine(_outputFolder, $"{group.Key}.pdf");

                using (var pdfWriter = new PdfWriter(outputFilePath))
                using (var pdfDocument = new PdfDocument(pdfWriter))
                using (var document = new Document(pdfDocument))
                {
                    foreach (var filePath in group)
                    {
                        var imageData = ImageDataFactory.Create(filePath);
                        var image = new iText.Layout.Element.Image(imageData);
                        image.SetAutoScale(true);
                        document.Add(image);
                    }

                    document.Close();
                }
            }
        }
    }
}
