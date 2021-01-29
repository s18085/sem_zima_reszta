using APBD_zima.Model;
using APBD_zima.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zima.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;

        public EnrollmentsController(IStudentsDbService service)
        {
            _dbService = service;
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        [Route("promotions")]
        public IActionResult PromoteStudent(Enrollment en)
        {
            var studies = en.Studies;
            var semester = en.Semester;
            var res = _dbService.PromoteStudents(studies, semester);
            if (res == null)
            {
                return NotFound("Student Not Found");
            }
            return Ok(res);
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(Student st)
        {
            var res = _dbService.EnrollStudent(st);
            if (res == null)
            {
                return NotFound("Record required for enrollment not found");
            }
            return Ok(res);
        }
    }
}
