using ClassLibrary2.Models;

public interface IStudentService
{
    ServiceResult Create(Student student);
    ServiceResult Update(Student student);
    ServiceResult Delete(int studentId);
    IEnumerable<Student> GetAll();
}
