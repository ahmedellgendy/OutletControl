using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutletControl.Application.Interfaces.Catalog;
using OutletControl.Contracts.Catalog;

namespace OutletControl.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(
        IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Owner,Cashier")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>>
        GetAll()
    {
        var products =
            await _productService.GetAllAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Owner,Cashier")]
    public async Task<ActionResult<ProductDto>>
        GetById(
            int id)
    {
        var product =
            await _productService.GetByIdAsync(
                id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet("category/{categoryId:int}")]
    [Authorize(Roles = "Admin,Owner,Cashier")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>>
        GetByCategory(
            int categoryId)
    {
        var products =
            await _productService
                .GetByCategoryAsync(
                    categoryId);

        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>>
        Create(
            CreateProductRequest request)
    {
        try
        {
            var product =
                await _productService
                    .CreateAsync(
                        request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = product.Id
                },
                product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>>
        Update(
            int id,
            UpdateProductRequest request)
    {
        try
        {
            var product =
                await _productService
                    .UpdateAsync(
                        id,
                        request);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                });
        }
    }
}