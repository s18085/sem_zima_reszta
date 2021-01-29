using System;
namespace APBD_zima.Model
{
    public class Student
    {
        public string IdStudent { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        //public string IndexNb { get; set; }
        public string StudiesName { get; set; }
        public int Semester { get; set; }

        public Student()
        {
            Semester = -1;
        }
        internal bool AllNotEmpty()
        {
            return IdStudent != null && FirstName != null && LastName != null && BirthDate != null && StudiesName != null && Semester != -1;
        }
    }
}
