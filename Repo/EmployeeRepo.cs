using System.Data;
using Dapper;
using ReppadL.Common;
using ReppadL.Model;
using ReppadL.Model.Data;

namespace ReppadL.Repo
{
	public class EmployeeRepo: IEmployeeRepo
	{
        private readonly DBContext _context;

		public EmployeeRepo(DBContext context)
		{
            this._context = context;
		}

        public async Task<APIResponse<Employee>> Create(Employee employee)
        {
            try
            {
                if (string.IsNullOrEmpty(employee.name) || string.IsNullOrEmpty(employee.phone))
                {
                    return new APIResponse<Employee>
                    {
                        ResponseCode = 400,
                        Message = "Name and phone are required."
                    };
                }

                using (IDbConnection connection = _context.CreateConnection())
                {
                    string query = "INSERT INTO employee (name, email, phone, designation) VALUES (@Name, @Email, @Phone, @Designation);";

                    int rowsAffected = await connection.ExecuteAsync(query, employee);

                    if (rowsAffected > 0)
                    {
                        return new APIResponse<Employee>
                        {
                            ResponseCode = 200,
                            Message = "Success!"
                        };
                    }
                    else
                    {
                        return new APIResponse<Employee>
                        {
                            ResponseCode = 400,
                            Message = "Failed!"
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                return new APIResponse<Employee>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<APIResponse<List<Employee>>> GetAll()
        {
            try
            {
                using (IDbConnection connection = _context.CreateConnection())
                {
                    string query = "SELECT * FROM employee";
                    var employeeList = await connection.QueryAsync<Employee>(query);

                    var result = employeeList.ToList();
                    if (result != null)
                    {
                        return new APIResponse<List<Employee>>
                        {
                            ResponseCode = 200,
                            Result = result,
                            Message = "Success!"
                        };
                    }
                    else
                    {
                        return new APIResponse<List<Employee>>
                        {
                            ResponseCode = 404,
                            Message = "Not found!"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new APIResponse<List<Employee>>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<APIResponse<Employee>> Getbycode(int code)
        {
            try
            {
                if (code == 0)
                {
                    return new APIResponse<Employee>
                    {
                        ResponseCode = 400,
                        Message = "Code are required."
                    };
                }

                using (IDbConnection connection = _context.CreateConnection())
                {
                    string query = "SELECT * FROM employee WHERE code = @Code";

                    var result = await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Code = code });

                    if (result != null)
                    {
                        return new APIResponse<Employee>
                        {
                            ResponseCode = 200,
                            Message = "Success!",
                            Result = result
                        };
                    }
                    else
                    {
                        return new APIResponse<Employee>
                        {
                            ResponseCode = 404,
                            Message = "Not found!"
                        };
                    }
                }

            }
            catch ( Exception ex)
            {
                return new APIResponse<Employee>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<APIResponse<string>> Remove(int code)
        {
            try
            {
                if (code == 0)
                {
                    return new APIResponse<string>
                    {
                        ResponseCode = 400,
                        Message = "Code are required."
                    };
                }

                using (IDbConnection connection = _context.CreateConnection())
                {
                    string query = "DELETE FROM employee WHERE code = @Code";
                    int rowsAffected = await connection.ExecuteAsync(query, new { Code = code});
                    if (rowsAffected > 0)
                    {
                        return new APIResponse<string>
                        {
                            ResponseCode = 200,
                            Message = "Success!",
                        };              
                    }
                    else
                    {
                        return new APIResponse<string>
                        { 
                            ResponseCode = 404,
                            Message = "Not found!"
                        };
                    }
                }
            }
            catch (Exception ex)
            {

                return new APIResponse<string>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<APIResponse<Employee>> Update(Employee employee, int code)
        {
            try
            {
                using (IDbConnection connection = _context.CreateConnection())
                {
                    string updateQuery = "UPDATE employee SET name = @Name, email = @Email, phone = @Phone, designation = @Designation WHERE code = @Code";

                    int rowsAffected = await connection.ExecuteAsync(updateQuery, new
                    {
                        Name = employee.name,
                        Email = employee.email,
                        Phone = employee.phone,
                        Designation = employee.designation,
                        Code = code
                    });

                    if (rowsAffected > 0)
                    {
                        return new APIResponse<Employee>
                        {
                            ResponseCode = 200,
                            Message = "Success!"
                        };
                    }
                    else
                    {
                        return new APIResponse<Employee>
                        {
                            ResponseCode = 404,
                            Message = "Not found!"
                        };
                    }
                }
            }
            catch (Exception ex)
            {

                return new APIResponse<Employee>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }
    }
}

