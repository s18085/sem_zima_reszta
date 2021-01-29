using System;
using System.Collections;
using System.Collections.Generic;
using APBD_zima.Model;

namespace APBD_zima.DAL
{
    
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> _students;

        public MockDbService()
        {
            _students = new List<Student>
            {
                new Student {IdStudent=1, FirstName = "Antoni", LastName="Kowalski" },
                new Student {IdStudent=2, FirstName = "Maciej", LastName="Wojtkowski" },
                new Student {IdStudent=3, FirstName = "Szymon", LastName="Malinowski" }
            };
        }

        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
