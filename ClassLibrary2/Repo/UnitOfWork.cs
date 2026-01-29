using ClassLibrary2.Models;

public class UnitOfWork : IUnitOfWork
{
    private readonly CourseManagementContext _context;
    public IGenericRepository<Student> Students { get; private set; }
    public IGenericRepository<Course> Courses { get; private set; }
    public IGenericRepository<Department> Departments { get; private set; }
    public IGenericRepository<Enrollment> Enrollments { get; private set; }

    public UnitOfWork(CourseManagementContext context)
    {
        _context = context;
        Students = new GenericRepository<Student>(_context);
        Courses = new GenericRepository<Course>(_context);
        Departments = new GenericRepository<Department>(_context);
        Enrollments = new GenericRepository<Enrollment>(_context);
    }

    public int Save() => _context.SaveChanges();

    public void Dispose() => _context.Dispose();
}
