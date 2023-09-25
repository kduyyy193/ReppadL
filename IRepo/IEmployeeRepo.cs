using System;
using ReppadL.Common;
using ReppadL.Model;

namespace ReppadL.IRepo
{
    public interface IEmployeeRepo
    {
        Task<APIResponse<List<Employee>>> GetAll();
        Task<APIResponse<Employee>> Getbycode(int code);
        Task<APIResponse<Employee>> Create(Employee employee);
        Task<APIResponse<Employee>> Update(Employee employee, int code);
        Task<APIResponse<string>> Remove(int code);
    }
}

