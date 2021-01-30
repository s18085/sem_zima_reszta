using APBD_zima.DAL;
using APBD_zima.Model;
using APBD_zima.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CW4.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        private string dbConString = "Data Source=/Users/DamianGoraj/Documents/DBs/sqlite-tools-osx-x86-3310100/s18085.db";
        public StudentsController(IStudentsDbService ser)
        {
            _dbService = ser;
        }
      
        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {
            var st = _dbService.findStudentById(id);
            if (st == null)
            {
                return NotFound("Student not found");
            }
            return Ok(st);
        }
    }
}

