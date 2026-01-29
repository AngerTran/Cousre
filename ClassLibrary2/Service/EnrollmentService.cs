using ClassLibrary2.Models;
using ClassLibrary2.Service.Interfaces;

namespace ClassLibrary2.Service
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int MIN_AGE = 18;
        private const int MIN_CREDITS = 1;
        private const int GRADING_PERIOD_DAYS = 30;

        public EnrollmentService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public ServiceResult Enroll(int studentId, int courseId, DateTime enrollDate)
        {
            // BR24: Transaction (sửa: dùng SaveChanges() thay vì _context private)
            _unitOfWork.Save(); // Auto-transaction qua Save()

            try
            {
                var student = _unitOfWork.Students.Find(s => s.StudentId == studentId).FirstOrDefault();
                var course = _unitOfWork.Courses.Find(c => c.CourseId == courseId).FirstOrDefault();

                // BR20: Existing student & course
                if (_unitOfWork.Students.GetById(studentId) == null ||
                    _unitOfWork.Courses.GetById(courseId) == null)
                    return ServiceResult.Failure("BR20: Invalid student/course");


                // BR26: Student age >= 18
                if (student.DateOfBirth.AddYears(MIN_AGE) > DateTime.Today)
                    return ServiceResult.Failure("BR26: Student must be at least 18 years old");

                // BR27: Course credits >= 1
                if (course.Credits < MIN_CREDITS)
                    return ServiceResult.Failure("BR27: Course must have at least 1 credit");

                // BR28: Course IsActive = true
                if (!course.IsActive)
                    return ServiceResult.Failure("BR28: Cannot enroll in inactive courses");

                // BR29: Student IsActive = true
                if (!student.IsActive)
                    return ServiceResult.Failure("BR29: Inactive student cannot enroll");

                // BR30 - Grade can only be assigned within 30 days of enrollment
               

                // BR16: No duplicate (composite PK tự handle)
                if (_unitOfWork.Enrollments.Find(e => e.StudentId == studentId && e.CourseId == courseId).Any())
                    return ServiceResult.Failure("BR16: Already enrolled");

                // BR17: Max 5 courses/student
                if (_unitOfWork.Enrollments.Find(e => e.StudentId == studentId).Count() >= 5)
                    return ServiceResult.Failure("BR17: Max 5 courses");

                // BR18: Not past
                if (enrollDate < DateTime.Today)
                    return ServiceResult.Failure("BR18: Future date only");

                // BR19: Same department
                
                if (student?.DepartmentId != course?.DepartmentId)
                    return ServiceResult.Failure("BR19: Same department required");

                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollDate = enrollDate
                };

                _unitOfWork.Enrollments.Add(enrollment);
                _unitOfWork.Save();
                return ServiceResult.Success("Enrolled successfully");
            }
            catch (Exception)
            {
                // BR24: Auto-rollback nếu Save() fail
                return ServiceResult.Failure("BR24: Enrollment failed - transaction rolled back");
            }
        }

        // ✅ SINGLE AssignGrade: dùng StudentId+CourseId (composite PK)
        // Grade là decimal theo DB schema
        public ServiceResult AssignGrade(int studentId, int courseId, decimal grade)
        {
            var enrollment = _unitOfWork.Enrollments
                .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                .FirstOrDefault();

            if (enrollment == null)
                return ServiceResult.Failure("BR21: Enrollment not found");

            // BR30: Grade chỉ trong 30 ngày ✅ ĐÚNG VỊ TRÍ
            var daysSinceEnrollment = (DateTime.Now - enrollment.EnrollDate).Days;
            if (daysSinceEnrollment > GRADING_PERIOD_DAYS)
                return ServiceResult.Failure("BR30: Grade can only be assigned within 30 days");

            // BR22: Grade 0-10
            if (grade < 0 || grade > 10)
                return ServiceResult.Failure("BR22: Grade must be 0-10");

            enrollment.Grade = grade;
            _unitOfWork.Enrollments.Update(enrollment);
            _unitOfWork.Save();
            return ServiceResult.Success("Grade assigned");
        }

        public ServiceResult UpdateGrade(int studentId, int courseId, decimal grade)
        {
            // BR23: Cannot update if finalized (thêm IsFinalized field sau)
            var enrollment = _unitOfWork.Enrollments
                .Find(e => e.StudentId == studentId && e.CourseId == courseId)
                .FirstOrDefault();

            if (enrollment == null)
                return ServiceResult.Failure("Enrollment not found");

            // BR23: Cannot update if finalized
            if (enrollment.IsFinalized)
                 return ServiceResult.Failure("BR23: Cannot update finalized grade");

            if (grade < 0 || grade > 10)
                return ServiceResult.Failure("BR22: Invalid grade");

            enrollment.Grade = grade;
            _unitOfWork.Enrollments.Update(enrollment);
            _unitOfWork.Save();
            return ServiceResult.Success();
        }

        public IEnumerable<Enrollment> GetAll() => _unitOfWork.Enrollments.GetAll();

      
    }
}
