using MasterDetailsCore.Models;
using MasterDetailsCore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MasterDetailsCore.Controllers
{
    public class StudentController : Controller
    {
        private readonly StudentDbContext _db;
        private readonly IWebHostEnvironment _webHost;

        public StudentController(StudentDbContext db, IWebHostEnvironment webHost)
        {
            _db = db;
            _webHost = webHost;
        }

        public IActionResult Index()
        {
            var applicants = _db.Students.Include(i => i.Modules).Include(x=>x.Course).ToList();    
           // applicants = _db.Students.Include(a => a.Course).ToList();
            return View(applicants);
        }
        public JsonResult GetCourses()
        {
            List<SelectListItem> cors = (from cor in _db.Courses
                                         select new SelectListItem
                                         {
                                             Value = cor.CourseId.ToString(),
                                             Text = cor.CourseName
                                         }).ToList();
            return Json(cors);
        }
        public IActionResult Create()
        {
            StudentViewModel student = new StudentViewModel();
            student.Courses = _db.Courses.ToList();
            student.Modules.Add(new Module() { ModuleId = 1 });

            return View(student);
        }
        [HttpPost]

        public IActionResult Create(StudentViewModel student)
        {
            string uniqueFileName = GetUploadedFileName(student);
            student.ImageUrl = uniqueFileName;
            Student obj = new Student();
            obj.StudentName = student.StudentName;
            obj.CourseId = student.CourseId;
            obj.MobileNo = student.MobileNo;
            obj.IsEnrolled = student.IsEnrolled;
            obj.Dob = student.Dob;
            obj.ImageUrl = student.ImageUrl;
            _db.Add(obj);
            _db.SaveChanges();
            var user = _db.Students.FirstOrDefault(x => x.MobileNo == student.MobileNo);
            if (user != null)
            {
                if (student.Modules.Count > 0)
                {
                    foreach (var item in student.Modules)
                    {
                        Module objM = new Module();
                        objM.StudentId = user.StudentId;
                        objM.Duration = item.Duration;
                        objM.ModuleName = item.ModuleName;
                        _db.Add(objM);
                    }
                }
            }
            _db.SaveChanges();
            return RedirectToAction("index");
        }


        public ActionResult Delete(int? id)
        {
            var app = _db.Students.Find(id);
            var existsModules = _db.Modules.Where(e => e.StudentId == id).ToList();
            foreach (var exp in existsModules)
            {
                _db.Modules.Remove(exp);
            }
            _db.Entry(app).State = EntityState.Deleted;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }



        private string GetUploadedFileName(StudentViewModel student)
        {
            string uniqueFileName = null;

            if (student.ProfileFile != null)
            {
                string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + student.ProfileFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    student.ProfileFile.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            Student app = _db.Students.Include(a => a.Modules).FirstOrDefault(x => x.StudentId == id);

            if (app != null)
            {
                StudentViewModel aps = new StudentViewModel()
                {
                    StudentId = app.StudentId,
                    StudentName = app.StudentName,
                    MobileNo = app.MobileNo,
                    Dob = app.Dob,
                    ImageUrl = app.ImageUrl,
                    CourseId = app.CourseId,
                    IsEnrolled = app.IsEnrolled,
                    Courses = _db.Courses.ToList(),
                    Modules = app.Modules.ToList()
                };

                return View("Edit", aps);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentViewModel student)
        {
            try
            {
                Student existingStudent = _db.Students
                    .Include(a => a.Modules)
                    .FirstOrDefault(x => x.StudentId == student.StudentId);

                if (existingStudent != null)
                {
                    existingStudent.StudentName = student.StudentName;
                    existingStudent.CourseId = student.CourseId;
                    existingStudent.MobileNo = student.MobileNo;
                    existingStudent.IsEnrolled = student.IsEnrolled;
                    existingStudent.Dob = student.Dob;

                    if (student.ProfileFile != null)
                    {
                        string uniqueFileName = GetUploadedFileName(student);
                        existingStudent.ImageUrl = uniqueFileName;
                    }

                    existingStudent.Modules.Clear();
                    foreach (var item in student.Modules)
                    {
                        var newModule = new Module
                        {
                            StudentId = existingStudent.StudentId,
                            ModuleName = item.ModuleName,
                            Duration = item.Duration
                        };

                        existingStudent.Modules.Add(newModule);
                    }

                    _db.SaveChanges();

                    return RedirectToAction("Index");
                }

                return NotFound();
            }
            catch (DbUpdateConcurrencyException ex)
            {

                var entry = ex.Entries.FirstOrDefault();
                if (entry != null)
                {
                    var databaseValues = entry.GetDatabaseValues();
                    if (databaseValues != null)
                    {
                        var databaseStudent = (Student)databaseValues.ToObject();

                        ModelState.AddModelError("", "The entity you are trying to edit has been modified by another user. Please refresh the page and try again.");


                        entry.OriginalValues.SetValues(databaseValues);


                        student.Courses = _db.Courses.ToList();
                        student.Modules = databaseStudent.Modules.ToList();

                        return View("Edit", student);
                    }
                }

                ModelState.AddModelError("", "The entity you are trying to edit has been deleted by another user.");

            }

            return RedirectToAction("Index");
        }


    }
}
