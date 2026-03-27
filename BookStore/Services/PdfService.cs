using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

namespace BookStore.Services
{
    
    public class PdfService
    {
        public byte[] GenerateReceipt(string userName, List<string> books, decimal total)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);

                var boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                doc.Add(new Paragraph("E-Shelf Receipt").SetFont(boldFont));
                doc.Add(new Paragraph($"User: {userName}"));
                doc.Add(new Paragraph($"Date: {DateTime.Now}"));
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph("Books Purchased:"));

                foreach (var book in books)
                {
                    doc.Add(new Paragraph("- " + book));
                }

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"Total: {total:C}"));

                doc.Close();

                return stream.ToArray();
            }
        }
    }
}
