using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using APBD_zima.Controllers;
using APBD_zima.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace APBD_zima.Services
{
    public class ServerDbService : IStudentsDbService
    {
        private string dbConString = "Data Source=/Users/DamianGoraj/Documents/DBs/sqlite-tools-osx-x86-3310100/s18085.db";
        public IConfiguration Configuration { get; set; }
        public ServerDbService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public bool AuthenticateStudent(string index, string password)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                var salt = GetUserSalt(index);
                var hashedUserPass = GetUserPassword(index);
                var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt + Configuration["Pepper"]), 100);
                var hash = rfc2898DeriveBytes.GetBytes(24);
                var res = hash.SequenceEqual(hash);

                return res;
            }
        }
        void AddPassword(string index, string password)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                com.Transaction = trans;
                var random = new RNGCryptoServiceProvider();
                int max_length = 32;
                byte[] salt = new byte[max_length];
                random.GetNonZeroBytes(salt);
                var genSalt = Convert.ToBase64String(salt);
                var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(genSalt + Configuration["Pepper"]), 100);
                var hash = rfc2898DeriveBytes.GetBytes(24);
                SQLiteParameter[] pars = new SQLiteParameter[3];
                var dataParameter = new SQLiteParameter("Data", DbType.Binary) { Value = hash };
                pars[0] = new SQLiteParameter("Salt", genSalt);
                pars[1] = new SQLiteParameter("Index", index);
                pars[2] = dataParameter;
                com.CommandText = "update student set Password = @Data, Salt = @Salt where IndexNumber = @Index";
                com.Parameters.AddRange(pars);
                com.ExecuteNonQuery();
                trans.Commit();
            }
        }

        public Enrollment EnrollStudent(Student st)
        {
            if(st.AllNotEmpty())
            {
                string semester = st.Semester.ToString();
                using (var con = new SQLiteConnection(dbConString))
                using (var com = con.CreateCommand())
                {
                    try
                    {
                        con.Open();

                        com.CommandText = "select IdStudy from Studies where Name = @Name";
                        com.Parameters.AddWithValue("Name", st.StudiesName);
                        var dr = com.ExecuteReader();
                        if (dr.Read())
                        {
                            //Student
                            string idStudent = st.IdStudent;
                            bool studentExst = false;
                            bool enrollmentExst = false;
                            string firstName = st.FirstName;
                            string lastName = st.LastName;
                            string birthD = st.BirthDate;
                            string idStudy = dr["IdStudy"].ToString();
                            com.Reset();
                            com.CommandText = "Select * from Student where IndexNumber = @idStudent";
                            com.Parameters.AddWithValue("idStudent", idStudent);
                            dr = com.ExecuteReader();
                            studentExst = dr.Read();
                            com.Reset();
                            com.CommandText = "Select * from Enrollment where Semester = @semester and idStudy = @idStudy";
                            com.Parameters.AddWithValue("semester", semester);
                            com.Parameters.AddWithValue("idStudy", idStudy);
                            dr = com.ExecuteReader();
                            enrollmentExst = dr.Read();
                            if (studentExst)
                            {
                                dr.Close();
                                return null;
                            }
                            else
                            {
                                com.Reset();
                                com.CommandText = "Select Max(IdEnrollment) as Max from Enrollment";
                                dr = com.ExecuteReader();
                                dr.Read();
                                //Console.WriteLine(dr["Max(IdEnrollment)"]);
                                int newIdEnroll = int.Parse(dr["Max"].ToString()) + 1;
                                //---------------
                                com.Reset();
                                SQLiteTransaction trans = con.BeginTransaction();
                                try
                                {
                                    if (!enrollmentExst)
                                    {
                                        com.Transaction = trans;
                                        com.CommandText = "Insert into Enrollment Values (@IdEnr,@sem,@IdStud,@startDate)";
                                        com.Parameters.AddWithValue("IdEnr", newIdEnroll);
                                        com.Parameters.AddWithValue("sem", semester);
                                        com.Parameters.AddWithValue("IdStud", idStudy);
                                        com.Parameters.AddWithValue("startDate", DateTime.Today);
                                        com.ExecuteNonQuery();
                                    }
                                    //--------------------
                                    com.CommandText = "Insert into Student Values (@indexNo,@FirstName,@LastName,@BirthD,@IdEnroll)";
                                    com.Parameters.AddWithValue("indexNo", idStudent);
                                    com.Parameters.AddWithValue("FirstName", firstName);
                                    com.Parameters.AddWithValue("LastName", lastName);
                                    com.Parameters.AddWithValue("BirthD", birthD);
                                    com.Parameters.AddWithValue("IdEnroll", newIdEnroll);
                                    com.ExecuteNonQuery();
                                    //----------------------
                                    trans.Commit();
                                    Enrollment enrollment = new Enrollment();
                                    enrollment.IdEnrollment = newIdEnroll.ToString();
                                    enrollment.Semester = int.Parse(semester);
                                    enrollment.IdStudy = idStudy;
                                    enrollment.StartDate = DateTime.Today.ToString();
                                    return enrollment;
                                }
                                catch (Exception e)
                                {
                                    trans.Rollback();
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    finally
                    {
                        con.Close();
                    }

                }
            }
            else
            {
                return null;
            }

        }

        public Student FindStudentById(string id)
        {
            Student st1 = null;
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                com.CommandText = "select S.IndexNumber, S.FirstName, S.LastName, S.BirthDate, St.Name, E.Semester " +
                                    "from Student S inner join Enrollment E on E.IdEnrollment = S.IdEnrollment " +
                                    "inner join Studies St on St.IdStudy = E.IdStudy where S.IndexNumber = @id";
                com.Parameters.AddWithValue("id", id);
                con.Open();
                var dr = com.ExecuteReader();
                if (dr.Read())
                {
                    st1 = new Student();
                    st1.IdStudent = dr["IndexNumber"].ToString();
                    st1.FirstName = dr["FirstName"].ToString();
                    st1.LastName = dr["LastName"].ToString();
                    st1.BirthDate = dr["BirthDate"].ToString();
                    st1.StudiesName = dr["Name"].ToString();
                    st1.Semester = int.Parse(dr["Semester"].ToString());
                }
            }
            return st1;
        }

        public Enrollment PromoteStudents(string studies, int semester)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                com.Transaction = trans;
                com.CommandText = "select * from Studies St where Name = @studies";
                com.Parameters.AddWithValue("studies", studies);
                var dr = com.ExecuteReader();

                if (!dr.Read())
                {
                    return null;
                }
                var idStudy = dr["IdStudy"].ToString();
                dr.Close();
                com.CommandText = "select E.IdEnrollment, E.StartDate from Enrollment E where E.IdStudy = @idStudy AND E.Semester = @semester";

                com.Parameters.AddWithValue("idStudy", idStudy);
                com.Parameters.AddWithValue("semester", semester + 1);
                var recEx = com.ExecuteReader();
                int newIdEnroll;
                string stDate = DateTime.Today.ToString();
                if (!recEx.Read())
                {
                    recEx.Close();
                    com.CommandText = "Select Max(IdEnrollment) as Max from Enrollment";
                    var dread = com.ExecuteReader();
                    dread.Read();
                    newIdEnroll = int.Parse(dread["Max"].ToString()) + 1;
                    dread.Close();
                    com.CommandText = "Insert into Enrollment Values (@IdEnr,@sem,@IdStud,@startDate)";
                    com.Parameters.AddWithValue("IdEnr", newIdEnroll);
                    com.Parameters.AddWithValue("sem", semester + 1);
                    com.Parameters.AddWithValue("IdStud", idStudy);
                    com.Parameters.AddWithValue("startDate", stDate);
                    com.ExecuteNonQuery();

                }
                else
                {
                    newIdEnroll = int.Parse(recEx["IdEnrollment"].ToString());
                    //stDate = recEx["StartDate"].ToString().Split(" ")[0];
                }
                recEx.Close();
                com.CommandText = "select St.IndexNumber " +
                                    "FROM Studies S " +
                                    "inner join Enrollment E on E.IdStudy = S.IdStudy " +
                                    "inner join Student St on St.IdEnrollment = E.IdEnrollment " +
                                    "WHERE S.Name = @StudiesName AND E.Semester = @semester;";
                //com.CommandText = "Select * from Enrollment";
                com.Parameters.AddWithValue("StudiesName", studies);
                com.Parameters.AddWithValue("semester", semester);
                dr = com.ExecuteReader();
                List<String> stList = new List<string>();

                while (dr.Read())
                {
                    stList.Add(dr["IndexNumber"].ToString());
                }
                dr.Close();
                foreach (string indN in stList)
                {
                    com.CommandText = "UPDATE Student set IdEnrollment = @IdEnr WHERE IndexNumber=@IndNo";
                    com.Parameters.AddWithValue("IdEnr", newIdEnroll);
                    com.Parameters.AddWithValue("IndNo", indN);
                    com.ExecuteNonQuery();
                }
                trans.Commit();
                return new Enrollment
                {
                    IdEnrollment = newIdEnroll.ToString(),
                    Semester = semester + 1,
                    Studies = studies,
                    StartDate = stDate,
                    IdStudy = idStudy
                };
            }
        }

        public void SaveRefreshToken(string index, string refreshToken)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                com.Transaction = trans;
                com.CommandText = "update student set RefreshToken = @refreshToken where IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);
                com.Parameters.AddWithValue("refreshToken", refreshToken);
                var dr = com.ExecuteNonQuery();
                trans.Commit();
            }
        }
        public string GetRefreshToken(string index)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                com.Transaction = trans;
                com.CommandText = "select RefreshToken from student where IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);
                using (var dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return dr["RefreshToken"].ToString();
                    }
                }
                return null;
            }
        }

        public string GetUserSalt(string index)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                com.Transaction = trans;
                com.CommandText = "select Salt from student where IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);
                using (var dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return dr["Salt"].ToString();
                    }
                }
                return null;
            }
        }

       public byte[] GetUserPassword(string index)
        {
            using (var con = new SQLiteConnection(dbConString))
            using (var com = con.CreateCommand())
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                com.Transaction = trans;
                com.CommandText = "select Password from student where IndexNumber = @index";
                com.Parameters.AddWithValue("index", index);
                byte[] res = new byte[24];
                using (var dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        dr.GetBytes(0, 0, res, 0, 24);
                        return res;
                    }
                }
                return null;
            }
        }
    }
}
