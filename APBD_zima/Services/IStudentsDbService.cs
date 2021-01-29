using System;
using APBD_zima.Controllers;
using APBD_zima.Model;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zima.Services
{
    public interface IStudentsDbService
    {
        public Enrollment PromoteStudents(string studies, int semester);
        public Enrollment EnrollStudent(Student st);
    }
}
