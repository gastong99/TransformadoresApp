using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models;

public class BomService
{
    private readonly ApplicationDbContext _db;
    public BomService(ApplicationDbContext db) { _db = db; }

    public async Task<List<(Material material, decimal? totalQty)>> CalculateForProductAsync(int productId, decimal? qty)
    {
        var lines = await _db.BomItems
            .Include(b => b.Material)
            .ThenInclude(m => m.UnitOfMeasure)
            .Where(b => b.ProductId == productId)
            .ToListAsync();

        var result = new List<(Material material, decimal? totalQty)>();

        foreach (var ln in lines) {
            var total = ln.QuantityPerUnit * qty;
            result.Add((ln.Material!, total));
        }

        return result;
    }
}

