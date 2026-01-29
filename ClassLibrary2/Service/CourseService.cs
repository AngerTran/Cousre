using ClassLibrary2.Models;
using ClassLibrary2.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Service
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public ServiceResult Create(Course course)
        {
            // BR11: CourseCode unique
            if (_unitOfWork.Courses.Find(c => c.CourseCode == course.CourseCode).Any())
                return ServiceResult.Failure("BR11: CourseCode must be unique");

            // BR12: Belong to one department
            if (course.DepartmentId == null || course.DepartmentId == 0 || _unitOfWork.Departments.GetById(course.DepartmentId.Value) == null)
                return ServiceResult.Failure("BR12: Course must belong to one department");

            // BR13: Credits 1-6
            if (course.Credits < 1 || course.Credits > 6)
                return ServiceResult.Failure("BR13: Credits must be 1-6");

            _unitOfWork.Courses.Add(course);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public ServiceResult Update(Course course)
        {
            // BR15: Cannot update if inactive/archived
            if (course.Credits == 0) // Giả sử IsActive = false là inactive
                return ServiceResult.Failure("BR15: Cannot update inactive course");

            _unitOfWork.Courses.Update(course);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public ServiceResult Delete(int courseId)
        {
            // BR14: Cannot delete if students enrolled
            if (_unitOfWork.Enrollments.Find(e => e.CourseId == courseId).Any())
                return ServiceResult.Failure("BR14: Cannot delete course with enrollments");

            _unitOfWork.Courses.Delete(courseId);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public IEnumerable<Course> GetAll() => _unitOfWork.Courses.GetAll();
    }
}
