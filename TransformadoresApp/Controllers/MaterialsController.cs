using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TransformadoresApp.Data;
using TransformadoresApp.Models;

namespace TransformadoresApp.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public MaterialsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Materials
        [Authorize(Roles = "Administrador,Operario")]
        public async Task<IActionResult> Index()
        {
            var materials = await _db.Materials
                .Include(m => m.UnitOfMeasure)
                .ToListAsync();

            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(materials);
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

                // Validar encabezados esperados
                var expectedHeaders = new[] { "Código", "Nombre", "Unidad de Medida" };

                var headers = worksheet.Row(1).Cells().Select(c => c.GetValue<string>().Trim()).ToList();

                if (!expectedHeaders.SequenceEqual(headers.Take(expectedHeaders.Length), StringComparer.OrdinalIgnoreCase)) {
                    TempData["Error"] = "El archivo no tiene el formato correcto para Materiales.";
                    return RedirectToAction(nameof(Index));
                }

                int imported = 0;

                foreach (var row in worksheet.RowsUsed().Skip(1)) {
                    var code = row.Cell(1).GetValue<string>().Trim();
                    var name = row.Cell(2).GetValue<string>().Trim();
                    var unitName = row.Cell(3).GetValue<string>().Trim();

                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(unitName)) continue;

                    if (await _db.Materials.AnyAsync(m => m.Code == code)) continue;

                    var unit = await _db.UnitOfMeasures.FirstOrDefaultAsync(u => u.Name.ToLower() == unitName.ToLower());
                    if (unit == null) {
                        unit = new UnitOfMeasure { Name = unitName };
                        _db.UnitOfMeasures.Add(unit);
                        await _db.SaveChangesAsync();
                    }

                    var material = new Material
                    {
                        Code = code,
                        Name = name,
                        UnitOfMeasureId = unit.UnitOfMeasureId
                    };

                    var validationResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(material, new ValidationContext(material), validationResults, true)) continue;

                    _db.Materials.Add(material);
                    imported++;
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Se importaron {imported} materiales correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al importar: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: /Materials/Details/5
        [Authorize(Roles = "Administrador,Operario")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var material = await _db.Materials
                .Include(m => m.UnitOfMeasure)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            return View(material);
        }

        // GET: /Materials/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            var units = _db.UnitOfMeasures.OrderBy(u => u.Name).ToList();
            units.Insert(0, new UnitOfMeasure { UnitOfMeasureId = 0, Name = "Seleccionar..." });
            ViewData["UnitOfMeasureId"] = new SelectList(units, "UnitOfMeasureId", "Name", 0);

            return View();
        }

        // POST: /Materials/Create
        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,UnitOfMeasureId")] Material material)
        {
            if (ModelState.IsValid) {
                _db.Add(material);
                await _db.SaveChangesAsync();

                TempData["Success"] = $"Material '{material.Name}' creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["UnitOfMeasureId"] = new SelectList(_db.UnitOfMeasures, "UnitOfMeasureId", "Name", material.UnitOfMeasureId);

            return View(material);
        }

        // GET: /Materials/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var material = await _db.Materials.FindAsync(id);
            if (material == null) return NotFound();

            ViewData["UnitOfMeasureId"] = new SelectList(_db.UnitOfMeasures, "UnitOfMeasureId", "Name", material.UnitOfMeasureId);

            return View(material);
        }

        // POST: /Materials/Edit/5
        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaterialId,Code,Name,UnitOfMeasureId")] Material material)
        {
            if (id != material.MaterialId) return NotFound();

            if (ModelState.IsValid) {
                try
                {
                    _db.Update(material);
                    await _db.SaveChangesAsync();
                    TempData["Info"] = $"Material '{material.Name}' actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.MaterialId)) return NotFound();
                    throw;
                }
            }

            ViewData["UnitOfMeasureId"] = new SelectList(_db.UnitOfMeasures, "UnitOfMeasureId", "Name", material.UnitOfMeasureId);

            return View(material);
        }

        // GET: /Materials/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var material = await _db.Materials
                .Include(m => m.UnitOfMeasure)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            var usedIn = await _db.BomItems
                .Include(b => b.Product)
                .Where(b => b.MaterialId == id)
                .Select(b => b.Product.Name)
                .Distinct()
                .ToListAsync();

            ViewBag.UsedIn = usedIn;

            return View(material);
        }

        // POST: /Materials/Delete/5
        [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usedIn = await _db.BomItems
                .Include(b => b.Product)
                .Where(b => b.MaterialId == id)
                .Select(b => b.Product.Name)
                .Distinct()
                .ToListAsync();

            if (usedIn.Any()) {
                TempData["Error"] = $"No se puede eliminar: el material está siendo utilizado en los siguientes transformadores: {string.Join(", ", usedIn)}.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var material = await _db.Materials.FindAsync(id);

            if (material != null) {
                _db.Materials.Remove(material);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Material '{material.Name}' eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id) => _db.Materials.Any(e => e.MaterialId == id);
    }
}
