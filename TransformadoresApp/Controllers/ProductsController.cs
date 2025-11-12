using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TransformadoresApp.Data;
using TransformadoresApp.Models;
using TransformadoresApp.ViewModels;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador,Operario")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int PageSize = 10; 

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Products
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search)) {
                query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));
            }

            int totalProducts = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new ProductListViewModel
            {
                Products = products,
                Search = search,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)PageSize)
            };

            return View(vm);
        }

        // IMPORTAR DESDE EXCEL
        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0) {
                TempData["Error"] = "Debe seleccionar un archivo Excel válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var workbook = new XLWorkbook(memoryStream);
                var worksheet = workbook.Worksheet(1);

                if (worksheet == null) {
                    TempData["Error"] = "El archivo Excel no contiene hojas válidas.";
                    return RedirectToAction(nameof(Index));
                }

                var expectedHeaders = new[] { "Código", "Nombre", "Potencia (kVA)", "Pérdidas Po (W)", "Pérdidas Pcc (W)", "Ucc (%)", "Largo (mm)", "Ancho (mm)", "Alto (mm)", "Diámetro (mm)", "Peso (Kg)" };

                var headers = worksheet.Row(1).Cells().Select(c => c.GetValue<string>().Trim()).ToList();

                if (!expectedHeaders.SequenceEqual(headers.Take(expectedHeaders.Length), StringComparer.OrdinalIgnoreCase)) {
                    TempData["Error"] = "El archivo no tiene el formato correcto para Transformadores.";
                    return RedirectToAction(nameof(Index));
                }

                int imported = 0;

                foreach (var row in worksheet.RowsUsed().Skip(1)) {
                    var code = row.Cell(1).GetValue<string>()?.Trim();
                    var name = row.Cell(2).GetValue<string>()?.Trim();

                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) continue;

                    if (await _db.Products.AnyAsync(p => p.Code == code)) continue;

                    var product = new Product
                    {
                        Code = code,
                        Name = name,
                        PotenciaKVA = int.TryParse(row.Cell(3).GetValue<string>(), out var potencia) ? potencia : null,
                        PerdidasPo = int.TryParse(row.Cell(4).GetValue<string>(), out var po) ? po : null,
                        PerdidasPcc = int.TryParse(row.Cell(5).GetValue<string>(), out var pcc) ? pcc : null,
                        Ucc = double.TryParse(row.Cell(6).GetValue<string>(), out var ucc) ? ucc : null,
                        Largo = int.TryParse(row.Cell(7).GetValue<string>(), out var largo) ? largo : null,
                        Ancho = int.TryParse(row.Cell(8).GetValue<string>(), out var ancho) ? ancho : null,
                        Alto = int.TryParse(row.Cell(9).GetValue<string>(), out var alto) ? alto : null,
                        Diametro = int.TryParse(row.Cell(10).GetValue<string>(), out var diam) ? diam : null,
                        Peso = int.TryParse(row.Cell(11).GetValue<string>(), out var peso) ? peso : null
                    };

                    var validationResults = new List<ValidationResult>();

                    if (!Validator.TryValidateObject(product, new ValidationContext(product), validationResults, true)) continue;

                    _db.Products.Add(product);
                    imported++;
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Se importaron {imported} transformadores correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al importar: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador,Operario")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _db.Products
                .Include(p => p.BomItems!)
                    .ThenInclude(b => b.Material)
                        .ThenInclude(m => m.UnitOfMeasure)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,Alto,Ancho,Diametro,Largo,PerdidasPcc,PerdidasPo,Peso,PotenciaKVA,Ucc")] Product product)
        {
            if (ModelState.IsValid) {
                _db.Add(product);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Code,Name,Alto,Ancho,Diametro,Largo,PerdidasPcc,PerdidasPo,Peso,PotenciaKVA,Ucc")] Product product)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid) {
                try
                {
                    _db.Update(product);
                    await _db.SaveChangesAsync();
                    TempData["Info"] = $"Transformador '{product.Name}' actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Products.Any(e => e.ProductId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(product);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _db.Products.FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);

            if (product != null) {
                bool hasBomItems = await _db.BomItems.AnyAsync(b => b.ProductId == id);

                if (hasBomItems) {
                    TempData["Error"] = "Este transformador no se puede eliminar porque tiene materiales de fabricación asociados. Para poder realizar esta acción, elimine los materiales de fabricación de éste transformador en la vista 'Detalles'.";
                    return View(product);
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
