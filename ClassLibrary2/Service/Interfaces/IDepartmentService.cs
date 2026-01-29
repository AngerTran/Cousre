using ClassLibrary2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Service.Interfaces
{
    public interface IDepartmentService
    {
        ServiceResult Create(Department department);
        ServiceResult Update(Department department);
        ServiceResult Delete(int departmentId);
        IEnumerable<Department> GetAll();
    }
}
