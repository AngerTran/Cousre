using ClassLibrary2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Service
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public ServiceResult Create(Student student)
        {
            // BR05: StudentCode must be unique
            if (_unitOfWork.Students.Find(s => s.StudentCode == student.StudentCode).Any())
                return ServiceResult.Failure("BR05: StudentCode must be unique");

            // BR06: Must belong to exactly one department
            if (student.DepartmentId == null || student.DepartmentId == 0 || _unitOfWork.Departments.GetById(student.DepartmentId.Value) == null)
                return ServiceResult.Failure("BR06: Student must belong to exactly one department");

            // BR07-08: Full name validation
            if (string.IsNullOrWhiteSpace(student.FullName) || student.FullName.Length < 3)
                return ServiceResult.Failure("BR07-08: Full name invalid");

            // BR09: Email unique if provided
            if (!string.IsNullOrEmpty(student.Email) &&
                _unitOfWork.Students.Find(s => s.Email == student.Email).Any())
                return ServiceResult.Failure("BR09: Email must be unique");

            _unitOfWork.Students.Add(student);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public ServiceResult Delete(int studentId)
        {
            // BR10: Cannot delete if has enrollments
            if (_unitOfWork.Enrollments.Find(e => e.StudentId == studentId).Any())
                return ServiceResult.Failure("BR10: Cannot delete student with enrollments");

            _unitOfWork.Students.Delete(studentId);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public IEnumerable<Student> GetAll() => _unitOfWork.Students.GetAll();
        public ServiceResult Update(Student student)
        {
            _unitOfWork.Students.Update(student);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }
    }
}
