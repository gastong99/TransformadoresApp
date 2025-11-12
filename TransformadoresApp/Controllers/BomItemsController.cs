using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models;

namespace TransformadoresApp.Controllers
{
    public class BomItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BomItemsController(ApplicationDbContext db) { _db = db; }

        // GET: /BomItems
        public async Task<IActionResult> Index()
        {
            var list = await _db.BomItems
                .Include(b => b.Product)
                .Include(b => b.Material)
                .ThenInclude(m => m.UnitOfMeasure)
                .OrderBy(b => b.Product.Name)
                .ToListAsync();

            return View(list);
        }

        // GET: BomItems/Create
        public IActionResult Create(int productId)
        {
            // Buscar el producto en la base de datos
            var product = _db.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null) return NotFound();

            var bomItem = new BomItem
            {
                ProductId = productId,
                Product = product
            };

            var materials = _db.Materials.OrderBy(m => m.Name).ToList();
            materials.Insert(0, new Material { MaterialId = 0, Name = "Seleccionar..." });
            ViewData["MaterialId"] = new SelectList(materials, "MaterialId", "Name", 0);

            return View(bomItem);
        }

        // POST: BomItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,MaterialId,QuantityPerUnit")] BomItem bomItem)
        {

            if (bomItem == null) return BadRequest();

            if (bomItem.QuantityPerUnit <= 0) ModelState.AddModelError(nameof(BomItem.QuantityPerUnit), "La cantidad debe ser mayor que 0.");

            // Comprobación previa: evitar duplicados product + material
            bool exists = await _db.BomItems
                .AnyAsync(b => b.ProductId == bomItem.ProductId && b.MaterialId == bomItem.MaterialId);

            if (exists) ModelState.AddModelError("", "Este material ya está asociado a este transformador. Si desea cambiar la cantidad, modifíquelo desde la tabla de materiales de fabricación.");

            if (ModelState.IsValid){
                try
                {
                    _db.Add(bomItem);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Details", "Products", new { id = bomItem.ProductId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR guardando BOM: " + ex);
                    ModelState.AddModelError("", "Ocurrió un error al guardar el material. Intente nuevamente.");
                }
            }

            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == bomItem.ProductId);
            bomItem.Product = product; 

            ViewData["ProductId"] = bomItem.ProductId;

            var materials = _db.Materials.OrderBy(m => m.Name).ToList();
            materials.Insert(0, new Material { MaterialId = 0, Name = "Seleccionar..." });
            ViewData["MaterialId"] = new SelectList(materials, "MaterialId", "Name", bomItem.MaterialId);

            return View(bomItem);
        }


        // GET: /BomItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _db.BomItems
                .Include(b => b.Product)
                .Include(b => b.Material)
                .ThenInclude(m => m.UnitOfMeasure)
                .FirstOrDefaultAsync(b => b.BomItemId == id);

            if (item == null) return NotFound();

            ViewData["ProductId"] = new SelectList(_db.Products.OrderBy(p => p.Name), "ProductId", "Name", item.ProductId);
            ViewData["MaterialId"] = new SelectList(_db.Materials.OrderBy(m => m.Name), "MaterialId", "Name", item.MaterialId);

            return View(item);
        }


        // POST: /BomItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BomItemId,ProductId,MaterialId,QuantityPerUnit")] BomItem bomItem)
        {
            if (id != bomItem.BomItemId) return NotFound();

            if (bomItem.QuantityPerUnit <= 0) ModelState.AddModelError(nameof(BomItem.QuantityPerUnit), "La cantidad debe ser mayor que 0.");

            // evitar duplicado distinto (al editar cambiar material a uno que ya exista)
            bool duplicate = await _db.BomItems.AnyAsync(b =>
                b.ProductId == bomItem.ProductId &&
                b.MaterialId == bomItem.MaterialId &&
                b.BomItemId != bomItem.BomItemId);
            if (duplicate) ModelState.AddModelError("", "Ya existe ese material en la estructura de ese producto.");

            if (ModelState.IsValid) {
                try
                {
                    _db.Update(bomItem);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Details", "Products", new { id = bomItem.ProductId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _db.BomItems.AnyAsync(e => e.BomItemId == id)) return NotFound();
                    else throw;
                }
            }

            ViewData["ProductId"] = new SelectList(_db.Products.OrderBy(p => p.Name), "ProductId", "Name", bomItem.ProductId);
            ViewData["MaterialId"] = new SelectList(_db.Materials.OrderBy(m => m.Name), "MaterialId", "Name", bomItem.MaterialId);

            return View(bomItem);
        }

        // GET: /BomItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _db.BomItems
                .Include(b => b.Product)
                .Include(b => b.Material)
                .ThenInclude(m => m.UnitOfMeasure)
                .FirstOrDefaultAsync(b => b.BomItemId == id);

            if (item == null) return NotFound();

            ViewData["ProductId"] = item.ProductId;

            return View(item);
        }

        // POST: /BomItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.BomItems.FindAsync(id);
            if (item != null) {
                int productId = item.ProductId;
                _db.BomItems.Remove(item);
                await _db.SaveChangesAsync();
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitOfMeasure(int materialId)
        {
            var material = await _db.Materials
                .Include(m => m.UnitOfMeasure)
                .FirstOrDefaultAsync(m => m.MaterialId == materialId);

            if (material == null) return NotFound();

            return Json(new { unit = material.UnitOfMeasure.Name });
        }
    }
}
