using NUnit.Framework;
using ClassLibrary2.Models;
using ClassLibrary2.Service.Interfaces;

namespace ClassLibrary2.Tests
{
    [TestFixture]
    public class DepartmentServiceTests : TestBase
    {
        [Test]
        public void TC01_Create_Department_Duplicate_Name_Fail_BR01()
        {
            // Given
            Context.Departments.Add(new Department { DepartmentId = 1, Name = "IT" });
            Context.SaveChanges();

            var duplicateDept = new Department { DepartmentId = 2, Name = "IT", Description = "Duplicate" };

            // When
            var result = DepartmentService.Create(duplicateDept);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR01"));
        }

        [Test]
        public void TC02_Create_Department_Short_Name_Fail_BR02()
        {
            // Given
            var shortDept = new Department { DepartmentId = 3, Name = "AB" };

            // When
            var result = DepartmentService.Create(shortDept);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR02"));
        }

        [Test]
        public void TC03_Delete_Department_With_Students_Fail_BR03()
        {
            // Given
            var dept = new Department { DepartmentId = 4, Name = "CS" };
            Context.Departments.Add(dept);
            Context.SaveChanges();

            var student = new Student
            {
                StudentId = 1,
                StudentCode = "S001",
                FullName = "John",
                Email = "john@test.com",
                DepartmentId = dept.DepartmentId
            };
            Context.Students.Add(student);
            Context.SaveChanges();

            // When
            var result = DepartmentService.Delete(dept.DepartmentId);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR03"));
        }

        [Test]
        public void TC04_Delete_Department_With_Courses_Fail_BR04()
        {
            // Given
            var dept = new Department { DepartmentId = 5, Name = "Math" };
            Context.Departments.Add(dept);
            Context.SaveChanges();

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "M101",
                Title = "Math 101",
                Credits = 3,
                DepartmentId = dept.DepartmentId
            };
            Context.Courses.Add(course);
            Context.SaveChanges();

            // When
            var result = DepartmentService.Delete(dept.DepartmentId);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR04"));
        }
    }
}
