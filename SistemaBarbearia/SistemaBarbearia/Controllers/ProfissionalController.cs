using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;
using System.Threading.Tasks;

namespace SistemaBarbearia.Controllers
{
    [Authorize]
    public class ProfissionalController : Controller
    {
        private readonly BancoContext _context;

        public ProfissionalController(BancoContext context)
        {
            _context = context;
        }

        // TELA PRINCIPAL DE GERENCIAMENTO
        public async Task<IActionResult> Index()
        {
            var profissionais = await _context.Profissionais.ToListAsync();
            return View(profissionais);
        }

        // ADICIONAR NOVO BARBEIRO RAPIDAMENTE
        [HttpPost]
        public async Task<IActionResult> Adicionar(string nome, string telefone)
        {
            if (!string.IsNullOrEmpty(nome))
            {
                var novoBarbeiro = new ProfissionalModel
                {
                    Nome = nome,
                    Telefone = telefone,
                    Ativo = true // Já entra disponível
                };
                _context.Profissionais.Add(novoBarbeiro);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // BOTÃO PARA DEIXAR DISPONÍVEL / INDISPONÍVEL
        [HttpPost]
        public async Task<IActionResult> AlternarStatus(int id)
        {
            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional != null)
            {
                profissional.Ativo = !profissional.Ativo; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}