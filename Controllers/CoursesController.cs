using System.Threading.Tasks;
using AzureEducation.Models;
using AzureEducation.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureEducation.Controllers
{
    public class CoursesController : Controller
    {
        private readonly CourseStore _courseStore;

        public CoursesController(CourseStore courseStore)
        {
            _courseStore = courseStore;
        }

        public IActionResult Index()
        {
            var model = _courseStore.GetAllCourses();
            return View("Index", model);
        }

        public async Task<IActionResult> Generate()
        {
            var data = new SampleData().GetCourses();
            await _courseStore.InsertCourses(data);
            return RedirectToAction(nameof(Index));
        }
    }
}
