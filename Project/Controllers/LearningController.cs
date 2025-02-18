﻿using Project.Filters;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{

    public class WatchLessonTable
    {
        public int lessonID { get; set; }
        public string name { get; set; }
        public string video { get; set; }
        public string description { get; set; }
        public int courseID { get; set; }
        public int userID { get; set; }
        public bool watched { get; set; }
        public string time { get; set; }
    }
    public class LearningController : Controller
    {
        private ProjectEntities db = new ProjectEntities();
        // GET: Learning
        [HttpGet]
        [CheckSession]
        public ActionResult Index(string courseId, string lessonId)
        {
            if ( courseId == null || lessonId == null) {
                return RedirectToAction( "Index", "Error" );
            }

            if ( courseId.Trim() == "" || lessonId.Trim() == "") {
                return RedirectToAction( "Index" , "Error" );
            }

            int userId = Convert.ToInt32(Session["user_id"].ToString());

            var temp = (from unc in db.UserAndCourses
                where unc.CourseID.ToString() == courseId.Trim() && unc.UserID == userId
                select unc).ToList();

            if (temp.Count == 0) {
                return Redirect($"/course/{courseId.Trim()}");
            }

            var checkUserLearnLesson = from e in db.UserAndLessions
                                       where e.UserID == userId && e.LessionID.ToString() == lessonId
                                       select e;

            //case if user have not learn this lesson before
            if (checkUserLearnLesson.ToList().Count == 0)
            {
                try
                {
                    //user learn this lesson
                    UserAndLession newUserAndLesson = new UserAndLession()
                    {
                        UserID = userId,
                        LessionID = int.Parse(lessonId),
                        Watched = true
                    };
                    db.UserAndLessions.Add(newUserAndLesson);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    //direct to error page if any error occurs
                    return RedirectToAction("Index", "Error");
                }
            }

            //select watched lesson of user in course to a table
            var watchedLesson = from l in db.Lessions
                                join ul in db.UserAndLessions on l.LessionID equals ul.LessionID
                                where l.CourseID.ToString() == courseId && ul.UserID == userId
                                select new WatchLessonTable
                                {
                                    lessonID = l.LessionID,
                                    name = l.Name,
                                    video = l.Video,
                                    description = l.Description,
                                    courseID = l.CourseID,
                                    userID = ul.UserID,
                                    watched = (bool)ul.Watched,
                                    time = l.Time
                                };

            //select unWatched lesson of user in course to another table
            var unWatchedLesson = from l in db.Lessions
                                  where l.CourseID.ToString() == courseId && !(from t in watchedLesson select t.lessonID).Contains(l.LessionID)
                                  select new WatchLessonTable
                                  {
                                      lessonID = l.LessionID,
                                      name = l.Name,
                                      video = l.Video,
                                      description = l.Description,
                                      courseID = l.CourseID,
                                      userID = -1,
                                      watched = false,
                                      time = l.Time
                                  };
            //union 2 table above we have result is a type of WatchLessonTable
            var result = watchedLesson.Union(unWatchedLesson).ToList();

            //get data in WatchLessonTable
            var lessonInfor = from l in result where l.lessonID.ToString() == lessonId select l;

            //case when lesson not existed
            if (lessonInfor.ToList().Count == 0)
            {
                return RedirectToAction("Index", "Error");
            }

            //set ViewBag for lession infomation
            ViewBag.srcVideo = lessonInfor.ToList()[0].video;
            ViewBag.description = lessonInfor.ToList()[0].description;
            ViewBag.title = lessonInfor.ToList()[0].name;

            //get course name
            var courseName = from c in db.Courses where c.CourseID.ToString() == courseId select c;
            ViewBag.courseName = courseName.ToList()[0].Name;
            ViewBag.courseID = courseId;
            ViewBag.Time = lessonInfor.ToList()[0].time;
            //if all lesson in 1 course of 1 user is true
            //chuyen den trang quiz
            if (unWatchedLesson.ToList().Count == 0)
            {
                ViewBag.doneCourse = true;
            }
            else { 
                ViewBag.doneCourse = false; 
            }

            /*UserAndCourse temp = new UserAndCourse();
            temp.CourseID = int.Parse(courseId);
            temp.UserID = userId;

            var selectTest = from userCourse in db.UserAndCourses
                where userCourse.CourseID.ToString() == courseId && userCourse.UserID == userId
                select userCourse;

            if ( selectTest.ToList().Count == 0) {
                try {
                    db.UserAndCourses.Add( temp );
                    db.SaveChanges();
                } catch ( Exception e ) {
                }
            }*/

            var checkQuiz = (from quiz in db.Quizs where quiz.CourseID.ToString() == courseId.Trim() select quiz).ToList();

            ViewBag.hasQuiz = checkQuiz.Count != 0;

            return View(result);
        }

    }
}