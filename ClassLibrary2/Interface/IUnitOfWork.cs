using ClassLibrary2.Models;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Student> Students { get; }
    IGenericRepository<Course> Courses { get; }
    IGenericRepository<Department> Departments { get; }
    IGenericRepository<Enrollment> Enrollments { get; }
    int Save();
}
