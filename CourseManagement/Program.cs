using ClassLibrary2.Models;
using ClassLibrary2.Service;
using ClassLibrary2.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PRN222.CourseManagement.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ================= DI CONFIG =================
            var services = new ServiceCollection();

            services.AddDbContext<CourseManagementContext>(options =>
                options.UseSqlServer(
                    "Server=.;Database=CourseManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
                ));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Register Services
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();

            var serviceProvider = services.BuildServiceProvider();

            // Resolve Services
            var studentService = serviceProvider.GetRequiredService<IStudentService>();
            var courseService = serviceProvider.GetRequiredService<ICourseService>();
            var enrollmentService = serviceProvider.GetRequiredService<IEnrollmentService>();
            var departmentService = serviceProvider.GetRequiredService<IDepartmentService>();

            // ================= MENU LOOP =================
            while (true)
            {
                Console.Clear();
                Console.WriteLine("===================================");
                Console.WriteLine("   COURSE MANAGEMENT SYSTEM");
                Console.WriteLine("===================================");
                Console.WriteLine("1. Display all students");
                Console.WriteLine("2. Display courses by department");
                Console.WriteLine("3. Display courses of a student");
                Console.WriteLine("4. Enroll student into course");
                Console.WriteLine("5. Update student information");
                Console.WriteLine("6. Delete a course");
                Console.WriteLine("7. Display enrollment report");
                Console.WriteLine("8. Assign Grade"); 
                Console.WriteLine("0. Exit");
                Console.WriteLine("-----------------------------------");
                Console.Write("Select an option: ");

                var choice = Console.ReadLine();

                try 
                {
                    switch (choice)
                    {
                        case "1": DisplayAllStudents(studentService); break;
                        case "2": DisplayCoursesByDepartment(courseService); break;
                        case "3": DisplayCoursesOfStudent(enrollmentService, courseService); break;
                        case "4": EnrollStudent(enrollmentService); break;
                        case "5": UpdateStudent(studentService); break;
                        case "6": DeleteCourse(courseService); break;
                        case "7": DisplayEnrollmentReport(enrollmentService, studentService, courseService); break;
                        case "8": AssignGrade(enrollmentService); break;
                        case "0": return;
                        default:
                            Console.WriteLine("Invalid choice!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        // ================= OPTION 1 =================
        static void DisplayAllStudents(IStudentService service)
        {
            var students = service.GetAll();

            Console.WriteLine("\nID | Code | Full Name | Email");
            foreach (var s in students)
            {
                Console.WriteLine($"{s.StudentId} | {s.StudentCode} | {s.FullName} | {s.Email}");
            }
        }

        // ================= OPTION 2 =================
        static void DisplayCoursesByDepartment(ICourseService service)
        {
            Console.Write("Enter Department ID: ");
            if (!int.TryParse(Console.ReadLine(), out int deptId)) return;

            var courses = service.GetAll()
                .Where(c => c.DepartmentId == deptId);

            foreach (var c in courses)
            {
                Console.WriteLine($"{c.CourseCode} - {c.Title} ({c.Credits} credits)");
            }
        }

        // ================= OPTION 3 =================
        static void DisplayCoursesOfStudent(IEnrollmentService enrollmentService, ICourseService courseService)
        {
            Console.Write("Enter Student ID: ");
            if (!int.TryParse(Console.ReadLine(), out int studentId)) return;

            var enrollments = enrollmentService.GetAll().Where(e => e.StudentId == studentId);
            var courses = courseService.GetAll();

            var result = from e in enrollments
                         join c in courses on e.CourseId equals c.CourseId
                         select new { c.CourseCode, c.Title, e.Grade };

            Console.WriteLine("\nCourse Code | Course Title | Grade");
            foreach (var r in result)
            {
                Console.WriteLine($"{r.CourseCode,-10} | {r.Title,-30} | {r.Grade}");
            }
        }

        // ================= OPTION 4 =================
        static void EnrollStudent(IEnrollmentService service)
        {
            Console.Write("Student ID: ");
            int studentId = int.Parse(Console.ReadLine()!);

            Console.Write("Course ID: ");
            int courseId = int.Parse(Console.ReadLine()!);

            var result = service.Enroll(studentId, courseId, DateTime.Now);
            
            if (result.IsSuccess)
                Console.WriteLine("Student enrolled successfully!");
            else
                Console.WriteLine($"Failed: {result.Message}");
        }

        // ================= OPTION 5 =================
        static void UpdateStudent(IStudentService service)
        {
            Console.Write("Enter Student ID: ");
            int id = int.Parse(Console.ReadLine()!);

            var students = service.GetAll(); 
            var student = students.FirstOrDefault(s => s.StudentId == id); // Service doesn't have GetById, so filter

            if (student == null)
            {
                Console.WriteLine("Student not found!");
                return;
            }

            Console.Write("New Full Name: ");
            student.FullName = Console.ReadLine()!;

            Console.Write("New Email: ");
            student.Email = Console.ReadLine()!;

            var result = service.Update(student);
             if (result.IsSuccess)
                Console.WriteLine("Student updated!");
            else
                Console.WriteLine($"Failed: {result.Message}");
        }

        // ================= OPTION 6 =================
        static void DeleteCourse(ICourseService service)
        {
            Console.Write("Enter Course ID: ");
            int id = int.Parse(Console.ReadLine()!);

            var result = service.Delete(id);
             if (result.IsSuccess)
                Console.WriteLine("Course deleted!");
            else
                Console.WriteLine($"Failed: {result.Message}");
        }

        // ================= OPTION 7 =================
        static void DisplayEnrollmentReport(IEnrollmentService enrollmentService, IStudentService studentService, ICourseService courseService)
        {
            var enrollments = enrollmentService.GetAll();
            var students = studentService.GetAll();
            var courses = courseService.GetAll();

            var report =
                from e in enrollments
                join s in students
                    on e.StudentId equals s.StudentId
                join c in courses
                    on e.CourseId equals c.CourseId
                select new
                {
                    StudentName = s.FullName,
                    CourseTitle = c.Title,
                    e.Grade
                };

            Console.WriteLine("\nStudent | Course | Grade");
            foreach (var r in report)
            {
                Console.WriteLine(
                    $"{r.StudentName,-25} | {r.CourseTitle,-30} | {r.Grade}"
                );
            }
        }
        
        // ================= OPTION 8 =================
        static void AssignGrade(IEnrollmentService service)
        {
             Console.Write("Student ID: ");
            int studentId = int.Parse(Console.ReadLine()!);

            Console.Write("Course ID: ");
            int courseId = int.Parse(Console.ReadLine()!);
            
            Console.Write("Grade (0-10): ");
            decimal grade = decimal.Parse(Console.ReadLine()!);

            var result = service.AssignGrade(studentId, courseId, grade);
             if (result.IsSuccess)
                Console.WriteLine("Grade assigned successfully!");
            else
                Console.WriteLine($"Failed: {result.Message}");
        }
    }
}
