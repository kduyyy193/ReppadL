using Microsoft.AspNetCore.Mvc;
using ReppadL.IRepo;
using ReppadL.Model;

namespace ReppadL.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmployeeController: ControllerBase
	{
		private readonly IEmployeeRepo _employeeRepo;

		public EmployeeController(IEmployeeRepo employeeRepo)
		{
			_employeeRepo = employeeRepo;
		}

		[HttpGet("GetAll")]
		public async Task<IActionResult> GetAll()
		{
			var result = await _employeeRepo.GetAll();

            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("GetbyCode/{code}")]
        public async Task<IActionResult> GetbyCode(int code)
        {
            var result = await _employeeRepo.Getbycode(code);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    return BadRequest("Invalid employee data");
                }

                var createdEmployee = await _employeeRepo.Create(employee);

                if (createdEmployee != null)
                {
                    return CreatedAtRoute("GetbyCode", new { code = createdEmployee.Result?.code }, createdEmployee);
                }
                else
                {
                    return StatusCode(500, "Failed to create employee");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] Employee employee, int code)
        {
            var _result = await _employeeRepo.Update(employee, code);
            return Ok(_result);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(int code)
        {
            var _result = await _employeeRepo.Remove(code);
            return Ok(_result);
        }
    }
}

