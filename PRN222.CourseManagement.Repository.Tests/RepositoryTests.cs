using System;
using System.Linq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using ClassLibrary2.Models;

namespace PRN222.CourseManagement.Repository.Tests
{
    [TestFixture]
    public class RepositoryTests
    {
        // BẮT BUỘC: mỗi test dùng 1 DB khác nhau
        private CourseManagementContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CourseManagementContext(options);
        }

        private IGenericRepository<Student> CreateStudentRepository(
            CourseManagementContext context)
        {
            return new GenericRepository<Student>(context);
        }

        private IUnitOfWork CreateUnitOfWork(CourseManagementContext context)
        {
            return new UnitOfWork(context);
        }

        private void SeedBasicData(CourseManagementContext context)
        {
            var dep = new Department
            {
                DepartmentId = 1,
                Name = "SE",
                Description = "Software Engineering"
            };
            context.Departments.Add(dep);

            var student = new Student
            {
                StudentId = 1,
                StudentCode = "ST001",        // REQUIRED
                FullName = "Alice",           // dùng FullName
                Email = "alice@example.com",  // REQUIRED
                DepartmentId = dep.DepartmentId
            };

            context.Students.Add(student);
            context.SaveChanges();
        }

        // 6.1 GetAll returns data
        [Test]
        public void GetAll_ReturnsData()
        {
            using var context = CreateInMemoryContext();
            SeedBasicData(context);

            var repo = CreateStudentRepository(context);

            var result = repo.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.True);
        }

        // 6.2 Add inserts a new entity
        [Test]
        public void Add_InsertsStudent()
        {
            using var context = CreateInMemoryContext();

            // Seed Department trước
            var dep = new Department
            {
                DepartmentId = 1,
                Name = "SE",
                Description = "Software Engineering"
            };
            context.Departments.Add(dep);
            context.SaveChanges();

            var repo = CreateStudentRepository(context);
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "ST002",        // REQUIRED
                FullName = "Bob",
                Email = "bob@example.com",    // REQUIRED
                DepartmentId = 1
            };

            var countBefore = repo.Count();

            repo.Add(student);
            context.SaveChanges();

            var countAfter = repo.Count();
            Assert.That(countAfter, Is.EqualTo(countBefore + 1));
        }

        // 6.3 GetById returns correct entity
        [Test]
        public void GetById_ReturnsCorrectStudent()
        {
            using var context = CreateInMemoryContext();
            SeedBasicData(context);
            var repo = CreateStudentRepository(context);

            var student = repo.GetById(1);

            Assert.That(student, Is.Not.Null);
            Assert.That(student!.StudentId, Is.EqualTo(1));
            Assert.That(student.StudentCode, Is.EqualTo("ST001"));
            Assert.That(student.FullName, Is.EqualTo("Alice"));
            Assert.That(student.Email, Is.EqualTo("alice@example.com"));
        }

        // 6.4 Delete removes entity
        [Test]
        public void Delete_RemovesStudent()
        {
            using var context = CreateInMemoryContext();
            SeedBasicData(context);
            var repo = CreateStudentRepository(context);

            repo.Delete(1);
            context.SaveChanges();

            var deleted = repo.GetById(1);
            Assert.That(deleted, Is.Null);
            Assert.That(repo.Count(), Is.EqualTo(0));
        }

        // 6.5 UnitOfWork.Save persists multiple entities
        [Test]
        public void UnitOfWork_Save_PersistsEntities()
        {
            using var context = CreateInMemoryContext();
            var uow = CreateUnitOfWork(context);

            var departmentRepo = uow.Departments;  // SỬA: dùng Repository<T>
            var studentRepo = uow.Students;        // SỬA: dùng Repository<T>

            departmentRepo.Add(new Department
            {
                DepartmentId = 1,
                Name = "SE",
                Description = "Software Engineering"
            });

            studentRepo.Add(new Student
            {
                StudentId = 1,
                StudentCode = "ST003",        // REQUIRED
                FullName = "Charlie",
                Email = "charlie@example.com", // REQUIRED
                DepartmentId = 1
            });

            uow.Save();

            Assert.That(context.Departments.Count(), Is.EqualTo(1));
            Assert.That(context.Students.Count(), Is.EqualTo(1));
        }

        // 6.6 (Bonus) Create Enrollment
        [Test]
        public void CreateEnrollment_Works()
        {
            using var context = CreateInMemoryContext();

            var dep = new Department
            {
                DepartmentId = 1,
                Name = "SE",
                Description = "Software Engineering"
            };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "ST001",
                FullName = "Alice",
                Email = "alice@example.com",
                DepartmentId = 1
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "PRN222",        // REQUIRED unique
                Title = "PRN222",
                DepartmentId = 1
            };

            context.Departments.Add(dep);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            var enrollmentRepo = new GenericRepository<Enrollment>(context);

            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                EnrollDate = DateTime.Now,
                Grade = 3.5m  // decimal(3,2)
            };

            enrollmentRepo.Add(enrollment);
            context.SaveChanges();

            var saved = context.Enrollments.FirstOrDefault(e => e.StudentId == 1 && e.CourseId == 1);

            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.StudentId, Is.EqualTo(1));
            Assert.That(saved.CourseId, Is.EqualTo(1));
        }
    }
}
