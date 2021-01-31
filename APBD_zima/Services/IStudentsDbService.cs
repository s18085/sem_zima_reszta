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
        public Student FindStudentById(string id);
        public bool AuthenticateStudent(string index, string password);
        public void SaveRefreshToken(string index, string refreshToken);
        public string GetRefreshToken(string index);
        public string GetUserSalt(string index);
        public byte[] GetUserPassword(string index);
        
    }
}
