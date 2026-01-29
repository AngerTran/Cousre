using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ClassLibrary2.Models;
using ClassLibrary2.Service;
using ClassLibrary2.Service.Interfaces;
using System;

namespace ClassLibrary2.Tests
{
    public class TestBase
    {
        protected CourseManagementContext Context;
        protected IUnitOfWork UnitOfWork;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new CourseManagementContext(options);
            Context.Database.EnsureCreated();
            UnitOfWork = new UnitOfWork(Context); // Thay bằng constructor thật của bạn
        }

        [TearDown]
        public void TearDown()
        {
            Context?.Database.EnsureDeleted();
            Context?.Dispose();
        }

        protected IDepartmentService DepartmentService => new DepartmentService(UnitOfWork);
        protected IStudentService StudentService => new StudentService(UnitOfWork);
        protected ICourseService CourseService => new CourseService(UnitOfWork);
        protected IEnrollmentService EnrollmentService => new EnrollmentService(UnitOfWork);
    }
}
