using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

using Shop.DAL.Repository.IRepository;
using Shop.Models;

namespace Shop.Areas.API
{
    [ApiController]
    [Route("api/[controller]/")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet("GetAll")]
        // api/Product/GetAll
        public async Task<ActionResult> Get()
        {
            try
            {
                var products = await _productRepository.GetAll(
                    includeProperties: "Category,ProductUsage"
                );
                return Ok(JsonSerializer.Serialize(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetByName")]
        // api/Product/GetByName?name=Python
        public async Task<ActionResult> GetByName(string name)
        {
            try
            {
                var products = await _productRepository.GetAll(
                    p => p.Name.Contains(name),
                    includeProperties: "Category,ProductUsage"
                );
                return Ok(JsonSerializer.Serialize(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetById")]
        // api/Product/GetById?id=2003
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var products = await _productRepository.GetAll(
                    p => p.Id.Equals(id),
                    includeProperties: "Category,ProductUsage"
                );
                return Ok(JsonSerializer.Serialize(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("Add")]
        // api/Product/Add
        public async Task<ActionResult> Add([FromBody] Product product)
        {
            try
            {
                await _productRepository.Add(product);
                await _productRepository.Save();
                return Ok("Product added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("Update/{id}")]
        // api/Product/Update/1
        public async Task<ActionResult> Update(int id, [FromBody] Product product)
        {
            try
            {
                var productUpdate = await _productRepository.Find(id);
                if (productUpdate is null)
                {
                    return NotFound("Product not found");
                }

                productUpdate.Name = product.Name;
                productUpdate.ShortDesc = product.ShortDesc;
                productUpdate.Description = product.Description;
                productUpdate.Price = product.Price;
                productUpdate.Image = product.Image;
                productUpdate.CategoryId = product.CategoryId;
                productUpdate.ProductUsageId = product.ProductUsageId;

                _productRepository.Update(productUpdate);
                await _productRepository.Save();
                return Ok("Product updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{id}")]
        // api/Product/Delete/1
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var productDelete = await _productRepository.Find(id);

                if (productDelete is null)
                {
                    return NotFound("Product not found");
                }

                _productRepository.Remove(productDelete);
                await _productRepository.Save();
                return Ok("Product deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
