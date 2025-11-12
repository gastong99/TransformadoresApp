using TransformadoresApp.Models;

namespace TransformadoresApp.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Unidades de medida
            if (!context.UnitOfMeasures.Any()) {
                context.UnitOfMeasures.AddRange(
                    new UnitOfMeasure { Name = "kg" },
                    new UnitOfMeasure { Name = "m" },
                    new UnitOfMeasure { Name = "und" }
                );
                context.SaveChanges();
            }

            // Materiales
            if (!context.Materials.Any()) {
                var kg = context.UnitOfMeasures.First(u => u.Name == "kg");
                var m = context.UnitOfMeasures.First(u => u.Name == "m");
                var und = context.UnitOfMeasures.First(u => u.Name == "und");

                context.Materials.AddRange(
                    new Material { Code = "MAT-CU", Name = "Cobre", UnitOfMeasureId = kg.UnitOfMeasureId },
                    new Material { Code = "MAT-AC", Name = "Núcleo de acero", UnitOfMeasureId = kg.UnitOfMeasureId },
                    new Material { Code = "MAT-OL", Name = "Aceite dieléctrico", UnitOfMeasureId = m.UnitOfMeasureId },
                    new Material { Code = "MAT-TOR", Name = "Tornillos", UnitOfMeasureId = und.UnitOfMeasureId }
                );
                context.SaveChanges();
            }

            // Transformadores
            if (!context.Products.Any()) {
                context.Products.AddRange(
                    new Product
                    {
                        Code = "TRF-100",
                        Name = "Transformador Trifásico 100 kVA",
                        Alto = 1200,
                        Ancho = 800,
                        Largo = 1000,
                        Diametro = 0,
                        PerdidasPcc = 850,
                        PerdidasPo = 250,
                        Peso = 450,
                        PotenciaKVA = 100,
                        Ucc = 4.5
                    },
                    new Product
                    {
                        Code = "TRF-200",
                        Name = "Transformador Trifásico 200 kVA",
                        Alto = 1400,
                        Ancho = 950,
                        Largo = 1100,
                        Diametro = 0,
                        PerdidasPcc = 1350,
                        PerdidasPo = 400,
                        Peso = 780,
                        PotenciaKVA = 200,
                        Ucc = 5.2
                    },
                    new Product
                    {
                        Code = "TRF-315",
                        Name = "Transformador Trifásico 315 kVA",
                        Alto = 1550,
                        Ancho = 1000,
                        Largo = 1200,
                        Diametro = 0,
                        PerdidasPcc = 1850,
                        PerdidasPo = 500,
                        Peso = 950,
                        PotenciaKVA = 315,
                        Ucc = 5.8
                    }
                );
                context.SaveChanges();
            }

            // Lista de materiales (BOM)
            if (!context.BomItems.Any()) {
                var cobre = context.Materials.First(m => m.Code == "MAT-CU");
                var acero = context.Materials.First(m => m.Code == "MAT-AC");
                var aceite = context.Materials.First(m => m.Code == "MAT-OL");
                var tornillos = context.Materials.First(m => m.Code == "MAT-TOR");

                var t100 = context.Products.First(p => p.Code == "TRF-100");
                var t200 = context.Products.First(p => p.Code == "TRF-200");
                var t315 = context.Products.First(p => p.Code == "TRF-315");

                context.BomItems.AddRange(
                    new BomItem { ProductId = t100.ProductId, MaterialId = cobre.MaterialId, QuantityPerUnit = 15 },
                    new BomItem { ProductId = t100.ProductId, MaterialId = acero.MaterialId, QuantityPerUnit = 25 },
                    new BomItem { ProductId = t100.ProductId, MaterialId = aceite.MaterialId, QuantityPerUnit = 3 },

                    new BomItem { ProductId = t200.ProductId, MaterialId = cobre.MaterialId, QuantityPerUnit = 28 },
                    new BomItem { ProductId = t200.ProductId, MaterialId = acero.MaterialId, QuantityPerUnit = 45 },
                    new BomItem { ProductId = t200.ProductId, MaterialId = aceite.MaterialId, QuantityPerUnit = 6 },
                    new BomItem { ProductId = t200.ProductId, MaterialId = tornillos.MaterialId, QuantityPerUnit = 30 },

                    new BomItem { ProductId = t315.ProductId, MaterialId = cobre.MaterialId, QuantityPerUnit = 42 },
                    new BomItem { ProductId = t315.ProductId, MaterialId = acero.MaterialId, QuantityPerUnit = 70 },
                    new BomItem { ProductId = t315.ProductId, MaterialId = aceite.MaterialId, QuantityPerUnit = 9 }
                );
                context.SaveChanges();
            }
        }
    }
}
