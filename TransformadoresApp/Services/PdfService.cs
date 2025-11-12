using TransformadoresApp.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Layout.Borders;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace TransformadoresApp.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _env;

        public PdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<byte[]> GenerateProductionOrderPdfAsync(ProductionOrder order, IEnumerable<BomResultViewModel> bom)
        {
            using var ms = new MemoryStream();

            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(60, 40, 40, 40);

            // Logo
            var logoPath = Path.Combine(_env.WebRootPath, "images", "RostLogo2.jpg");
            if (File.Exists(logoPath)) {
                var logo = new Image(ImageDataFactory.Create(logoPath))
                    .ScaleToFit(110, 110)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT)
                    .SetMarginBottom(10);
                document.Add(logo);
            }

            // Encabezado
            var title = new Paragraph("ORDEN DE PRODUCCIÓN")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetMarginBottom(20);
            document.Add(title);

            // Datos de la orden
            var orderInfo = new Table(2)
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            orderInfo.AddCell(CellInfo("N° de Orden:", order.ProductionOrderId.ToString()));
            orderInfo.AddCell(CellInfo("Fecha:", order.OrderDate.ToLocalTime().ToString("dd/MM/yyyy [HH:mm 'h']")));
            orderInfo.AddCell(CellInfo("Transformador:", order.Product?.Name ?? "-"));
            orderInfo.AddCell(CellInfo("Cantidad a fabricar:", $"{order.Quantity?.ToString("0.##")}"));
            orderInfo.AddCell(CellInfo("Estado:", order.Status.ToString().Replace("EnProceso", "En Proceso")));
            document.Add(orderInfo);

            // Título de materiales
            document.Add(new Paragraph("MATERIALES REQUERIDOS")
                .SetFontSize(14)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetMarginBottom(8));

            // Tabla de materiales
            var table = new Table(new float[] { 4, 2, 2 }).UseAllAvailableWidth();
            table.AddHeaderCell(HeaderCell("Material").SetTextAlignment(TextAlignment.LEFT));
            table.AddHeaderCell(HeaderCell("Cantidad Total"));
            table.AddHeaderCell(HeaderCell("Unidad").SetTextAlignment(TextAlignment.LEFT));

            if (bom != null) {
                foreach (var item in bom) {
                    table.AddCell(NormalCell(item.MaterialName));
                    table.AddCell(NormalCell($"{item.TotalQty:0.##}").SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(NormalCell(item.Unit));
                }
            }
            else {
                table.AddCell(new Cell(1, 3)
                    .Add(new Paragraph("No se pudo calcular materiales."))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetPadding(5));
            }

            document.Add(table);

            // Pie
            document.Add(new Paragraph($"\nGenerado el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm} h")
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontColor(ColorConstants.GRAY));

            document.Close();
            return await Task.FromResult(ms.ToArray());
        }

        // Helpers de formato
        private static Cell HeaderCell(string text) =>
            new Cell()
                .Add(new Paragraph(text)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(14))
                .SetBackgroundColor(new DeviceRgb(200, 200, 200))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));

        private static Cell NormalCell(string text) =>
            new Cell()
                .Add(new Paragraph(text).SetFontSize(12))
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.3f));

        private static Cell CellInfo(string label, string value)
        {
            var cell = new Cell().Add(
                new Paragraph()
                    .Add(new Text(label + " ").SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
                    .Add(new Text(value))
                    .SetFontSize(12)
            );
            cell.SetBorder(Border.NO_BORDER).SetPadding(4);
            return cell;
        }
    }
}