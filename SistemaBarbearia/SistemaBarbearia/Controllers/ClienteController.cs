using Microsoft.AspNetCore.Mvc;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;

namespace SistemaBarbearia.Controllers
{
    public class ClienteController : Controller
    {
        private readonly BancoContext _context;

        public ClienteController(BancoContext context) { _context = context; }

        public IActionResult Cadastro() => View();

        [HttpPost]
        public IActionResult Salvar(ClienteModel cliente)
        {
            
            _context.SaveChanges();

            
            return RedirectToAction("Criar", "Agendamento");
        }
    }
}