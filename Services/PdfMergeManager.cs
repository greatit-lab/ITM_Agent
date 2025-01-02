using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.Threading.Tasks;

namespace ITM_Agent.Services
{
    public class PdfMergeManager
    {
        //private string _outputFolder;
        
        private string OutputFolder
        {
          get => _outputFolder;
          set
          {
            if (!Directory.Exists(value))
              throw new DirectoryNotFoundException("Output folder does not exist.");
              
              _outputFolder = value;
          }
        }
        
        private string _outputFolder;
        
        public void UpdateOutputFolder(string outputFolder)
        {
            OutputFolder = outputFolder;  // 속성을 통해 값 설정
        }
        
        public PdfMergeManager(string defaultOutputFolder)
        {
            if (!Directory.Exists(defaultOutputFolder))
                throw new DirectoryNotFoundException("Default output folder does not exist.");

            _outputFolder = defaultOutputFolder;
        }
        
        public void MergeImagesToPDF(string targetFolder, string outputFolder)
        {
            if (!Directory.Exists(targetFolder))
                throw new DirectoryNotFoundException($"Target folder does not exist: {targetFolder}");
                
            if (!Directory.Exists(outputFolder))
                throw new DirectoryNotFoundException($"Output folder does not exist: {outputFolder}");
            
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
                
                using (var pdfWriter = new PdfWriter(outputFilePath))
                using (var pdfDocument = new PdfDocument(pdfWriter))
                using (var document = new Document(pdfDocument))
                {
                    foreach (var filePath in group.OrdereBy(f => f))
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
