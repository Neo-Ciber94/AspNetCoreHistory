using AutoMapper;
using AspNetCoreHistory.Models;
using AspNetCoreHistory.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using AspNetCoreHistory.Utilities;

namespace AspNetCoreHistory.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext db;
    private readonly IMapper mapper;

    public ProductController(ApplicationDbContext db, IMapper mapper)
    {
        this.db = db;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts(CancellationToken cancellationToken = default)
    {
        var products = await db.Products.ToListAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetAllProductsHistory(CancellationToken cancellationToken = default)
    {
        var productHistory = await db.ProductHistories.ToListAsync(cancellationToken);
        return Ok(productHistory);
    }


    [HttpGet("history/{id}")]
    public async Task<IActionResult> GetProductHistory(int id, CancellationToken cancellationToken = default)
    {
        var productHistory = await db.ProductHistories.Where(x => x.Id == id).ToListAsync(cancellationToken);
        return Ok(productHistory);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreate input, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map<Product>(input);
        var entry = await db.Products.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(entry.Entity);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProduct(ProductUpdate input, CancellationToken cancellationToken = default)
    {
        var productToUpdate = await db.Products.FirstOrDefaultAsync(x => x.Id == input.Id, cancellationToken);

        if (productToUpdate == null)
        {
            return NotFound();
        }

        productToUpdate = mapper.Map(input, productToUpdate);
        var product = db.Products.Update(productToUpdate).Entity;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(product);
    }

    [HttpPut("history/restore")]
    public async Task<IActionResult> RestoreHistoryVersion(ProductRestoreVersion restore, CancellationToken cancellationToken = default)
    {
        var versionedEntity = await db.ProductHistories
            .Where(x => x.Id == restore.Id)
            .OrderBy(x => x.VersionValidFrom)
            .Select((Value, i) => new { Value, Version = i + 1 })
            .FirstOrDefaultAsync(x => x.Version == restore.Version, cancellationToken);

        if (versionedEntity == null)
        {
            return NotFound("Unable to found the product version");
        }

        var productToRestore = await db.Products.FirstOrDefaultAsync(x => x.Id == restore.Id, cancellationToken);
        var version = versionedEntity.Value;

        if (productToRestore == null)
        {
            var recover = mapper.Map<Product>(version);
            var entry = await db.Products.AddAsync(recover, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Ok(entry.Entity);
        }
        else
        {
            productToRestore.Name = version.Name;
            productToRestore.Price = version.Price;
            var product = db.Update(productToRestore).Entity;
            await db.SaveChangesAsync(cancellationToken);
            return Ok(product);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken = default)
    {
        var productToDelete = await db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (productToDelete == null)
        {
            return NotFound();
        }

        var entry = db.Products.Remove(productToDelete);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(entry.Entity);
    }
}
