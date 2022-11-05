using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Model;
using WarehouseApp.Service;

namespace WarerhouseApp.Controllers
{
    [Route("api/warhouses2")]
    [ApiController]
    public class WarehouseController2 : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public WarehouseController2(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost("async")]
        public async Task<IActionResult> RegisterProduct(ProductRegistration pr)
        {
            try
            {
                return Ok(await _databaseService.RegisterProductByStoredProcedureAsync(pr));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
