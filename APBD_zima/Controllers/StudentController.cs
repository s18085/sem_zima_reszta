using APBD_zima.DAL;
using APBD_zima.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CW4.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private List<Student> stList;
        private string dbConString = "Data Source=/Users/DamianGoraj/Documents/DBs/sqlite-tools-osx-x86-3310100/s18085.db";
        public StudentsController(IDbService ser)
        {
            _dbService = ser;
        }
        /*     [HttpGet]
             public ActionResult<IEnumerable<string>> Get()
             {
                 return new string[] { "value1", "value2" };
             }*/
        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {

            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                com.CommandText = "select S.IndexNumber, S.FirstName, S.LastName, S.BirthDate, St.Name, E.Semester " +
                                    "from Student S inner join Enrollment E on E.IdEnrollment = S.IdEnrollment " +
                                    "inner join Studies St on St.IdStudy = E.IdStudy where S.IndexNumber = @id";
                com.Parameters.AddWithValue("id", id);
                con.Open();
                var dr = com.ExecuteReader();
                stList = new List<Student>();
                while (dr.Read())
                {
                    var st1 = new Student();
                    st1.IdStudent = dr["IndexNumber"].ToString();
                    st1.FirstName = dr["FirstName"].ToString();
                    st1.LastName = dr["LastName"].ToString();
                    st1.BirthDate = dr["BirthDate"].ToString();
                    st1.StudiesName = dr["Name"].ToString();
                    st1.Semester = int.Parse(dr["Semester"].ToString());
                    stList.Add(st1);
                }
            }
            return Ok(stList);
        }
    }
}

