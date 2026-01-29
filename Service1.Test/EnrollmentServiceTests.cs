using NUnit.Framework;
using ClassLibrary2.Models;
using ClassLibrary2.Service;
using ClassLibrary2.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ClassLibrary2.Tests
{
    [TestFixture]
    public class EnrollmentServiceTests
    {
        private CourseManagementContext _context;
        private IEnrollmentService _service;
        private Department _dept1, _dept2;
        private Student _student;
        private Course _course1, _course2;

        [SetUp]
        public void SetUp()
        {
            // Setup In-Memory DB
            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new CourseManagementContext(options);
            _context.Database.EnsureCreated();

            // Setup UnitOfWork & Service
            var uow = new UnitOfWork(_context);
            _service = new EnrollmentService(uow);

            // Setup Departments
            _dept1 = new Department { DepartmentId = 1, Name = "IT" };
            _dept2 = new Department { DepartmentId = 2, Name = "HR" };
            _context.Departments.AddRange(_dept1, _dept2);
            _context.SaveChanges();

            // Setup Student
            _student = new Student
            {
                StudentId = 1,
                StudentCode = "S001",
                FullName = "Test Student",
                Email = "test@test.com",
                DepartmentId = _dept1.DepartmentId
            };
            _context.Students.Add(_student);

            // Setup Courses
            _course1 = new Course
            {
                CourseId = 1,
                CourseCode = "IT101",
                Title = "Course 1",
                Credits = 3,
                DepartmentId = _dept1.DepartmentId
            };
            _course2 = new Course
            {
                CourseId = 2,
                CourseCode = "IT102",
                Title = "Course 2",
                Credits = 3,
                DepartmentId = _dept1.DepartmentId
            };
            _context.Courses.AddRange(_course1, _course2);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        // TC16 - BR16: No duplicate enrollment
        [Test]
        public void TC16_Enroll_Same_Course_Twice_Fail_BR16()
        {
            // Given
            _context.Enrollments.Add(new Enrollment
            {
                StudentId = _student.StudentId,
                CourseId = _course1.CourseId,
                EnrollDate = DateTime.Today
            });
            _context.SaveChanges();

            // When
            var result = _service.Enroll(_student.StudentId, _course1.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR16"));
        }

        // TC17 - BR17: Max 5 courses
        [Test]
        public void TC17_Enroll_Exceeding_Max_5_Courses_Fail_BR17()
        {
            // Given: Create 5 enrollments
            for (int i = 1; i <= 5; i++)
            {
                var c = new Course
                {
                    CourseId = 10 + i,
                    CourseCode = $"C{i}",
                    Title = $"Course {i}",
                    Credits = 3,
                    DepartmentId = _dept1.DepartmentId
                };
                _context.Courses.Add(c);
                _context.SaveChanges();

                _context.Enrollments.Add(new Enrollment
                {
                    StudentId = _student.StudentId,
                    CourseId = c.CourseId,
                    EnrollDate = DateTime.Today
                });
                _context.SaveChanges();
            }

            var course6 = new Course
            {
                CourseId = 99,
                CourseCode = "C6",
                Title = "Course 6",
                Credits = 3,
                DepartmentId = _dept1.DepartmentId
            };
            _context.Courses.Add(course6);
            _context.SaveChanges();

            // When
            var result = _service.Enroll(_student.StudentId, course6.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR17"));
        }

        // TC18 - BR18: No past date
        [Test]
        public void TC18_Enroll_Past_Date_Fail_BR18()
        {
            // Given
            var pastDate = DateTime.Today.AddDays(-1);

            // When
            var result = _service.Enroll(_student.StudentId, _course1.CourseId, pastDate);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR18"));
        }

        // TC19 - BR19: Same department only
        [Test]
        public void TC19_Enroll_Different_Department_Fail_BR19()
        {
            // Given: Create course in different department
            var courseOtherDept = new Course
            {
                CourseId = 100,
                CourseCode = "HR101",
                Title = "HR Course",
                Credits = 3,
                DepartmentId = _dept2.DepartmentId
            };
            _context.Courses.Add(courseOtherDept);
            _context.SaveChanges();

            // When
            var result = _service.Enroll(_student.StudentId, courseOtherDept.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR19"));
        }

        // TC20 - BR20: Valid student & course
        [Test]
        public void TC20_Enroll_Invalid_StudentOrCourse_Fail_BR20()
        {
            // When
            var result1 = _service.Enroll(9999, _course1.CourseId, DateTime.Today);
            var result2 = _service.Enroll(_student.StudentId, 9999, DateTime.Today);

            // Then
            Assert.That(result1.IsSuccess, Is.False);
            Assert.That(result1.Message, Contains.Substring("BR20"));

            Assert.That(result2.IsSuccess, Is.False);
            Assert.That(result2.Message, Contains.Substring("BR20"));
        }

        // TC21 - BR21: Enrollment must exist
        [Test]
        public void TC21_AssignGrade_No_Enrollment_Fail_BR21()
        {
            // When
            var result = _service.AssignGrade(_student.StudentId, _course1.CourseId, 8.5m);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR21"));
        }

        // TC22 - BR22: Grade 0-10
        [Test]
        public void TC22_AssignGrade_Invalid_Value_Fail_BR22()
        {
            // Given
            _context.Enrollments.Add(new Enrollment
            {
                StudentId = _student.StudentId,
                CourseId = _course1.CourseId,
                EnrollDate = DateTime.Today
            });
            _context.SaveChanges();

            // When
            var result = _service.AssignGrade(_student.StudentId, _course1.CourseId, 11m);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR22"));
        }

        // TC23 - BR23: No update finalized grade (TODO)
        [Test]
        public void TC23_Update_Finalized_Grade_Fail_BR23()
        {
            // Given
            var enrollment = new Enrollment
            {
                StudentId = _student.StudentId,
                CourseId = _course1.CourseId,
                EnrollDate = DateTime.Today,
                Grade = 7.5m,
                IsFinalized = true // Finalized
            };
            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            // When
            var result = _service.UpdateGrade(_student.StudentId, _course1.CourseId, 8.5m);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR23"));
        }

        // TC24 - BR24: Transaction rollback
        [Test]
        public void TC24_Enrollment_Fails_No_Persist_BR24()
        {
            // Given: dùng dept khác để fail
            var courseOtherDept = new Course
            {
                CourseId = 101,
                CourseCode = "HR102",
                Title = "HR",
                Credits = 3,
                DepartmentId = _dept2.DepartmentId
            };
            _context.Courses.Add(courseOtherDept);
            _context.SaveChanges();

            var countBefore = _context.Enrollments.Count();

            // When
            var result = _service.Enroll(_student.StudentId, courseOtherDept.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(_context.Enrollments.Count(), Is.EqualTo(countBefore));
        }

        // TC25 - BR25: Service returns result, not exception
        [Test]
        public void TC25_Service_Returns_Failure_Not_Exception_BR25()
        {
            // When
            var result = _service.Enroll(9999, 9999, DateTime.Today);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void TC26_Enroll_StudentUnder18_Fail_BR26()
        {
            // Given: Student 17 years old
            var youngStudent = new Student
            {
                StudentId = 2,
                StudentCode = "S002",
                FullName = "Young Student",
                Email = "young@test.com",
                DateOfBirth = DateTime.Today.AddYears(-17), // Under 18
                DepartmentId = _dept1.DepartmentId
            };
            _context.Students.Add(youngStudent);
            _context.SaveChanges();

            // When
            var result = _service.Enroll(youngStudent.StudentId, _course1.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR26"));
        }

        // TC27 - BR27: Course must have at least 1 credit
        [Test]
        public void TC27_Enroll_ZeroCreditCourse_Fail_BR27()
        {
            // Given: Course 0 credits
            var zeroCreditCourse = new Course
            {
                CourseId = 3,
                CourseCode = "ZERO001",
                Title = "Zero Credit",
                Credits = 0, // 0 credits
                DepartmentId = _dept1.DepartmentId
            };
            _context.Courses.Add(zeroCreditCourse);
            _context.SaveChanges();

            // When
            var result = _service.Enroll(_student.StudentId, zeroCreditCourse.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR27"));
        }

        // TC28 - BR28: Cannot enroll inactive course
        [Test]
        public void TC28_Enroll_InactiveCourse_Fail_BR28()
        {
            // Given: Inactive course
            var inactiveCourse = new Course
            {
                CourseId = 4,
                CourseCode = "INACTIVE001",
                Title = "Inactive Course",
                Credits = 3,
                IsActive = false, // Inactive
                DepartmentId = _dept1.DepartmentId
            };
            _context.Courses.Add(inactiveCourse);
            _context.SaveChanges();

            // When
            var result = _service.Enroll(_student.StudentId, inactiveCourse.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR28"));
        }

        // TC29 - BR29: Inactive student cannot enroll
        [Test]
        public void TC29_Enroll_InactiveStudent_Fail_BR29()
        {
            // Given: Inactive student
            var inactiveStudent = new Student
            {
                StudentId = 3,
                StudentCode = "S003",
                FullName = "Inactive Student",
                Email = "inactive@test.com",
                IsActive = false, // Inactive
                DepartmentId = _dept1.DepartmentId
            };
            _context.Students.Add(inactiveStudent);
            _context.SaveChanges();

            // When
            var result = _service.Enroll(inactiveStudent.StudentId, _course1.CourseId, DateTime.Today);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR29"));
        }

        // TC30 - BR30: Grade only within 30 days
        [Test]
        public void TC30_AssignGrade_Outside30Days_Fail_BR30()
        {
            // Given: Enrollment 31 days ago
            var oldEnrollment = new Enrollment
            {
                StudentId = _student.StudentId,
                CourseId = _course1.CourseId,
                EnrollDate = DateTime.Today.AddDays(-31)
            };
            _context.Enrollments.Add(oldEnrollment);
            _context.SaveChanges();

            // When
            var result = _service.AssignGrade(_student.StudentId, _course1.CourseId, 8.5m);

            // Then - BÂY GIỜ PASS ✅
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Contains.Substring("BR30"));
        }
    }
}
