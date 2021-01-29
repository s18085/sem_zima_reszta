using System;
using APBD_zima.DAL;
using APBD_zima.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace CW4.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private List<Student> stList;
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

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18085;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
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
                    st1.IdStudent = id; // dr["IndexNumber"].ToString();
                    st1.FirstName = dr["FirstName"].ToString();
                    st1.LastName = dr["LastName"].ToString();
                    st1.BirthDate = dr["BirthDate"].ToString();
                    st1.StudiesName = dr["Name"].ToString();
                    st1.Semester = dr["Semester"].ToString();
                    stList.Add(st1);
                }
            }
            return Ok(stList);
        }
        // POST api/values
        [HttpPost]
        public IActionResult CreateStudent(Student st)
        {
            st.IdStudent = $"s{new Random().Next(1, 20000)}";
            return Ok(st);
        }

        // PUT api/values/5
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok($"Usuwanie ukończone dla Id: {id}");
        }

        // DELETE api/values/5
        [HttpPut("{id}")]
        public IActionResult PutStudent(int id)
        {
            return Ok($"Aktualizacja dokończona dla Id: {id}");
        }
    }
}

