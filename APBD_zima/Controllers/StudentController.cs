using APBD_zima.DAL;
using APBD_zima.Dtos;
using APBD_zima.Model;
using APBD_zima.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace CW4.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        public IConfiguration Configuration { get; set; }
        private string dbConString = "Data Source=/Users/DamianGoraj/Documents/DBs/sqlite-tools-osx-x86-3310100/s18085.db";
        public StudentsController(IStudentsDbService ser, IConfiguration configuration)
        {
            _dbService = ser;
            Configuration = configuration;
        }


       [HttpGet("{id}")]
       [Authorize(Roles = "student")]
        public IActionResult GetStudent(string id)
        {
            var st = _dbService.FindStudentById(id);
            if (st == null)
            {
                return NotFound("Student not found");
            }
            return Ok(st);
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginRequestDto loginRequest)
        {
            var index = loginRequest.Login;
            var password = loginRequest.Haslo;

            var authOk = _dbService.AuthenticateStudent(index, password);

            if (authOk)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, index),
                    new Claim(ClaimTypes.Name, index),
                    new Claim(ClaimTypes.Role, "student")
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );
                var refreshToken = Guid.NewGuid();

                _dbService.SaveRefreshToken(index, refreshToken.ToString());
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken
                });
            }
            return NotFound("Student not found");
        }

    }
}

