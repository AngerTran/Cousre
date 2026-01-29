using NUnit.Framework;
using ClassLibrary2.Models;
using ClassLibrary2.Service.Interfaces;

namespace ClassLibrary2.Tests
{
    [TestFixture]
    public class StudentServiceTests : TestBase
    {
        private Department _department;

        [SetUp]
        public void AdditionalSetup()
        {
            _department = new Department { DepartmentId = 100, Name = "IT" };
            Context.Departments.Add(_department);
            Context.SaveChanges();
        }

        [Test]
        public void TC05_Create_Student_Duplicate_Code_Fail_BR05()
        {
            // Given
            Context.Students.Add(new Student
            {
                StudentId = 1,
                StudentCode = "IT001",
                FullName = "Existing",
                Email = "exist@test.com",
                DepartmentId = _department.DepartmentId
            });
            Context.SaveChanges();

            var duplicate = new Student
            {
                StudentId = 2,
                StudentCode = "IT001",
                FullName = "Duplicate",
                Email = "dup@test.com",
                DepartmentId = _department.DepartmentId
            };

            // When
            var result = StudentService.Create(duplicate);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR05"));
        }

        [Test]
        public void TC06_Create_Student_No_Department_Fail_BR06()
        {
            // Given
            var noDept = new Student
            {
                StudentId = 3,
                StudentCode = "IT003",
                FullName = "NoDept",
                Email = "nodept@test.com",
                DepartmentId = null
            };

            // When
            var result = StudentService.Create(noDept);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR06"));
        }

        [Test]
        public void TC07_Create_Student_Empty_Name_Fail_BR07()
        {
            // Given
            var emptyName = new Student
            {
                StudentId = 4,
                StudentCode = "IT004",
                FullName = "",
                Email = "empty@test.com",
                DepartmentId = _department.DepartmentId
            };

            // When
            var result = StudentService.Create(emptyName);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR07"));
        }

        [Test]
        public void TC08_Create_Student_Short_Name_Fail_BR08()
        {
            // Given
            var shortName = new Student
            {
                StudentId = 5,
                StudentCode = "IT005",
                FullName = "Ab",
                Email = "short@test.com",
                DepartmentId = _department.DepartmentId
            };

            // When
            var result = StudentService.Create(shortName);

            // Then
            Assert.That(result.IsSuccess, Is.False );
            Assert.That(result.Message, Contains.Substring("BR07"));
        }

        [Test]
        public void TC09_Create_Student_Duplicate_Email_Fail_BR09()
        {
            // Given
            Context.Students.Add(new Student
            {
                StudentId = 6,
                StudentCode = "IT006",
                FullName = "Email1",
                Email = "test@test.com",
                DepartmentId = _department.DepartmentId
            });
            Context.SaveChanges();

            var dupEmail = new Student
            {
                StudentId = 7,
                StudentCode = "IT007",
                FullName = "Email2",
                Email = "test@test.com",
                DepartmentId = _department.DepartmentId
            };

            // When
            var result = StudentService.Create(dupEmail);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR09"));
        }

        [Test]
        public void TC10_Delete_Student_With_Enrollments_Fail_BR10()
        {
            // Given
            var student = new Student
            {
                StudentId = 8,
                StudentCode = "IT008",
                FullName = "Enrolled",
                Email = "enroll@test.com",
                DepartmentId = _department.DepartmentId
            };
            Context.Students.Add(student);

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "IT101",
                Title = "Intro",
                Credits = 3,
                DepartmentId = _department.DepartmentId
            };
            Context.Courses.Add(course);
            Context.SaveChanges();

            Context.Enrollments.Add(new Enrollment
            {
                StudentId = student.StudentId,
                CourseId = course.CourseId,
                EnrollDate = DateTime.Today
            });
            Context.SaveChanges();

            // When
            var result = StudentService.Delete(student.StudentId);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR10"));
        }
    }
}
