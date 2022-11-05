using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Model;
using WarehouseApp.Service;

namespace WarehouseApp.Controllers
{
    

    [Route("api/warehouses")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public WarehouseController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost("async")]
        public async Task<IActionResult> RegisterProduct(ProductRegistration pr)
        {
            try
            {
                return Ok(await _databaseService.RegisterProductAsync(pr));
            } catch(Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
