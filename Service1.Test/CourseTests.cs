using ClassLibrary2.Models;
using ClassLibrary2.Service;
using ClassLibrary2.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ClassLibrary2.Tests;

[TestFixture]
public class CourseServiceTests
{
    private CourseManagementContext _context;
    private ICourseService _service;
    private Department _department;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CourseManagementContext(options);
        _context.Database.EnsureCreated();

        // ✅ SETUP DEPARTMENT TRƯỚC
        _department = new Department { DepartmentId = 100, Name = "IT" };
        _context.Departments.Add(_department);
        _context.SaveChanges();  // ← Persist trước

        // ✅ UnitOfWork SAU Department (clean context)
        var uow = new UnitOfWork(_context);
        _service = new CourseService(uow);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    public void TC11_Create_Course_Duplicate_Code_Fail_BR11()
    {
        // Given
        var firstCourse = new Course
        {
            CourseId = 1,
            CourseCode = "IT101",
            Title = "Intro",
            Credits = 3,
            DepartmentId = _department.DepartmentId
        };
        _context.Courses.Add(firstCourse);
        _context.SaveChanges();

        var duplicate = new Course
        {
            CourseId = 2,
            CourseCode = "IT101",
            Title = "Duplicate",
            Credits = 3,
            DepartmentId = _department.DepartmentId
        };

        // When
        var result = _service.Create(duplicate);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("BR11"));
    }

    [Test]
    public void TC12_Create_Course_No_Department_Fail_BR12()
    {
        var noDept = new Course
        {
            CourseId = 3,
            CourseCode = "IT999",
            Title = "No Dept",
            Credits = 3,
            DepartmentId = null  // null → BR12
        };

        var result = _service.Create(noDept);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("BR12"));
    }

    [Test]
    public void TC13_Create_Course_Invalid_Credits_Fail_BR13()
    {
        // Given
        var invalidCredits = new Course
        {
            CourseId = 4,
            CourseCode = "IT-ZERO-CREDITS",  // unique
            Title = "Zero Credits Test",
            Credits = 9,  // < 1 → BR13 chính xác
            DepartmentId = _department.DepartmentId  // pass BR12
        };

        // When
        var result = _service.Create(invalidCredits);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("BR13"));  // match service message
    }

    [Test]
    public void TC14_Delete_Course_With_Enrollments_Fail_BR14()
    {
        // Given
        var course = new Course
        {
            CourseId = 5,
            CourseCode = "IT777",
            Title = "Enrolled",
            Credits = 3,
            DepartmentId = _department.DepartmentId
        };
        _context.Courses.Add(course);

        var student = new Student
        {
            StudentId = 1,
            StudentCode = "S001",
            FullName = "Test",
            Email = "test@test.com",
            DepartmentId = _department.DepartmentId
        };
        _context.Students.Add(student);

        _context.Enrollments.Add(new Enrollment
        {
            StudentId = student.StudentId,
            CourseId = course.CourseId,
            EnrollDate = DateTime.Now
        });
        _context.SaveChanges();  // Save ALL together

        // When
        var result = _service.Delete(course.CourseId);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("BR14"));
    }

    [Test]
    public void TC15_Update_Inactive_Course_Fail_BR15()
    {
        // Given: Tạo course ACTIVE trước
        var activeCourse = new Course
        {
            CourseCode = "IT666",
            Title = "Active",
            Credits = 3,
            DepartmentId = _department.DepartmentId
        };
        _context.Courses.Add(activeCourse);
        _context.SaveChanges();

        // Update thành INACTIVE
        activeCourse.Credits = 0;
        activeCourse.Title = "Inactive";

        // When
        var result = _service.Update(activeCourse);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Does.Contain("BR15"));
    }
}
