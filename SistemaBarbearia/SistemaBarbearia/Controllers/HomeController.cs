using Microsoft.AspNetCore.Mvc;

namespace SistemaBarbearia.Controllers
{
    public class HomeController : Controller
    {
        [Route("")] // Faz com que o endereço vazio SEJA obrigatoriamente a Home
        [Route("Home/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}