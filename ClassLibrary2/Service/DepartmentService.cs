using ClassLibrary2.Models;
using ClassLibrary2.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Service
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public ServiceResult Create(Department department)
        {
            // BR01: Department name must be unique
            if (_unitOfWork.Departments.Find(d => d.Name == department.Name).Any())
                return ServiceResult.Failure("BR01: Department name must be unique");

            // BR02: Department name cannot be empty or shorter than 3 characters
            if (string.IsNullOrWhiteSpace(department.Name) || department.Name.Length < 3)
                return ServiceResult.Failure("BR02: Name cannot be empty or shorter than 3 characters");

            _unitOfWork.Departments.Add(department);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public ServiceResult Delete(int departmentId)
        {
            // BR03: Cannot delete if has students
            if (_unitOfWork.Students.Find(s => s.DepartmentId == departmentId).Any())
                return ServiceResult.Failure("BR03: Cannot delete department with students");

            // BR04: Cannot delete if has courses
            if (_unitOfWork.Courses.Find(c => c.DepartmentId == departmentId).Any())
                return ServiceResult.Failure("BR04: Cannot delete department with courses");

            _unitOfWork.Departments.Delete(departmentId);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public IEnumerable<Department> GetAll() => _unitOfWork.Departments.GetAll();
        public ServiceResult Update(Department department)
        {
            _unitOfWork.Departments.Update(department);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }
    }
}
