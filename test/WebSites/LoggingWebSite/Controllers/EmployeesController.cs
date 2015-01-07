using System.Threading.Tasks;
using LoggingWebSite.Models;
using Microsoft.AspNet.Mvc;

namespace LoggingWebSite.Controllers
{
    public class EmployeesController : Controller
    {
        public IActionResult Index()
        {
            return Content(nameof(EmployeesController) + ".Index");
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpBadRequest();
            }

            return Content(string.Format("id:{0}", id));
        }

        public IActionResult Create()
        {
            return Content(nameof(EmployeesController) + ".Create");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [DummyActionFilter]
        public IActionResult Create(Employee employee)
        {
            if(!ModelState.IsValid)
            {
                return HttpBadRequest();
            }

            return Content(string.Format("Id:{0},Name:{1}", employee.Id, employee.Name));
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpBadRequest();
            }

            return Content(string.Format("id:{0}", id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest();
            }

            return Content(string.Format("Id:{0},Name:{1}", employee.Id, employee.Name));
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpBadRequest();
            }

            return Content(string.Format("id:{0}", id));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return Content(string.Format("id:{0}", id));
        }
    }
}
