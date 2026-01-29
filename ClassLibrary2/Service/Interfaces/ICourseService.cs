using ClassLibrary2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Service.Interfaces
{
    public interface ICourseService
    {
        ServiceResult Create(Course course);
        ServiceResult Update(Course course);
        ServiceResult Delete(int courseId);
        IEnumerable<Course> GetAll();
    }
}
