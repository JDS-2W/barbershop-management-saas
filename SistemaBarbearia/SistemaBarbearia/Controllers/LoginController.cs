using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaBarbearia.Controllers
{
    public class LoginController : Controller
    {
        private readonly BancoContext _context;

        public LoginController(BancoContext context)
        {
            _context = context;
        }

        //  Rota normal para o CLIENTE
        [HttpGet]
        public IActionResult Index()
        {
            return View(new UsuarioModel { IsAdmin = false });
        }

        //  NOVA ROTA para o BARBEIRO (Admin)
        [HttpGet]
        public IActionResult Admin()
        {
            return View("Index", new UsuarioModel { IsAdmin = true });
        }

        [HttpPost]
        public async Task<IActionResult> Entrar(string whatsapp, string senha)
        {
            //  Cria o seu usuário Admin automaticamente na primeira vez
            if (whatsapp == "admin" && senha == "123")
            {
                var adminExiste = await _context.Usuarios.AnyAsync(x => x.Whatsapp == "admin");
                if (!adminExiste)
                {
                    _context.Usuarios.Add(new UsuarioModel
                    {
                        Nome = "Jadson (Barbeiro)",
                        Whatsapp = "admin",
                        Senha = "123",
                        IsAdmin = true
                    });
                    await _context.SaveChangesAsync();
                }
            }

            //  Busca o usuário no banco de dados
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Whatsapp == whatsapp && x.Senha == senha);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nome ?? "Usuário"),
                    new Claim("UserWhatsapp", usuario.Whatsapp)
                };

                //  SE FOR O BARBEIRO (ADMIN), VAI PARA O SEU PAINEL!
                if (usuario.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));

                    return RedirectToAction("PainelPrincipal", "Admin");
                }

                //  SE FOR CLIENTE, VAI PARA O AGENDAMENTO
                var identityCliente = new ClaimsIdentity(claims, "CookieAuth");
                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identityCliente));

                return RedirectToAction("Criar", "Agendamento");
            }

            TempData["MensagemErro"] = "Senha ou usuário incorretos!";

            if (whatsapp == "admin")
            {
                return RedirectToAction("Admin");
            }

            return RedirectToAction("Index");
        }

        // --- MÉTODOS DE CADASTRO PARA NOVOS CLIENTES ---

        [HttpGet]
        public IActionResult Cadastrar()
        {
            return View(new UsuarioModel());
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(UsuarioModel usuario)
        {
            if (ModelState.IsValid)
            {
                // Verifica se o WhatsApp já existe no banco
                var existe = await _context.Usuarios.AnyAsync(x => x.Whatsapp == usuario.Whatsapp);
                if (existe)
                {
                    TempData["MensagemErro"] = "Este WhatsApp já está cadastrado!";
                    return View(usuario);
                }

                // Garante que o cliente NÃO seja admin
                usuario.IsAdmin = false;

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Faz o login automático do cliente logo após o cadastro
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nome ?? "Cliente"),
                    new Claim("UserWhatsapp", usuario.Whatsapp)
                };
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));

                // Manda o cliente direto para agendar o horário
                return RedirectToAction("Criar", "Agendamento");
            }
            return View(usuario);
        }

        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }
    }
}