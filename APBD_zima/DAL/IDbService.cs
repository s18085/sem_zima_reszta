using System;
using System.Collections.Generic;
using APBD_zima.Model;

namespace APBD_zima.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}
