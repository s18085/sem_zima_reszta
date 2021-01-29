﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using APBD_zima.Controllers;
using APBD_zima.Model;
using Microsoft.AspNetCore.Mvc;

namespace APBD_zima.Services
{
    public class ServerDbService : IStudentsDbService
    {
        private string dbConString = "Data Source=/Users/DamianGoraj/Documents/DBs/sqlite-tools-osx-x86-3310100/s18085.db";
        public ServerDbService()
        {
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
    }
}
