using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaBarbearia.Controllers
{
    [Authorize]
    public class AgendamentoController : Controller
    {
        private readonly BancoContext _context;

        public AgendamentoController(BancoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Criar(DateTime? dataSelecionada, string nome = "", string whatsapp = "", string servico = "", string profissional = "")
        {
            DateTime data = dataSelecionada ?? DateTime.Today;
            ViewBag.DataSelecionada = data;

            // BUSCA OS BARBEIROS ATIVOS NO BANCO DE DADOS
            var barbeirosAtivos = await _context.Profissionais.Where(p => p.Ativo).ToListAsync();
            ViewBag.Profissionais = barbeirosAtivos;

            // Se o cliente não escolheu, pega o primeiro da lista
            if (string.IsNullOrEmpty(profissional) && barbeirosAtivos.Any())
            {
                profissional = barbeirosAtivos.First().Nome;
            }

            var bloqueio = await _context.Bloqueios.FirstOrDefaultAsync(b => b.Data.Date == data.Date);

            if (bloqueio != null)
            {
                ViewBag.HorariosDisponiveis = new List<string>();
                ViewBag.MotivoBloqueio = bloqueio.Motivo;
            }
            else
            {
                // AGORA BUSCA OS HORÁRIOS FILTRADOS POR BARBEIRO E POR HORA ATUAL
                ViewBag.HorariosDisponiveis = await ObterHorariosDisponiveis(data, profissional);
            }

            var userWhatsapp = User?.Claims?.FirstOrDefault(c => c.Type == "UserWhatsapp")?.Value ?? "";
            var userName = User?.Identity?.Name ?? "";

            var modeloMemoria = new AgendamentoModel
            {
                NomeCliente = string.IsNullOrEmpty(nome) ? userName : nome,
                Whatsapp = string.IsNullOrEmpty(whatsapp) ? userWhatsapp : whatsapp,
                Servico = servico,
                Profissional = profissional
            };

            return View(modeloMemoria);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromForm] AgendamentoModel agendamento)
        {
            if (agendamento == null) return RedirectToAction("Criar");

            var userWhatsapp = User?.Claims?.FirstOrDefault(c => c.Type == "UserWhatsapp")?.Value;
            if (!string.IsNullOrEmpty(userWhatsapp)) agendamento.Whatsapp = userWhatsapp;

            if (string.IsNullOrEmpty(agendamento.Profissional))
            {
                var primeiroProfissional = await _context.Profissionais.FirstOrDefaultAsync(p => p.Ativo);
                agendamento.Profissional = primeiroProfissional?.Nome ?? "Equipe";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Agendamentos.Add(agendamento);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Sucesso");
                }
                catch (Exception) { }
            }

            ViewBag.DataSelecionada = agendamento.Data;
            ViewBag.Profissionais = await _context.Profissionais.Where(p => p.Ativo).ToListAsync();

            var bloqueio = await _context.Bloqueios.FirstOrDefaultAsync(b => b.Data.Date == agendamento.Data.Date);
            if (bloqueio != null)
            {
                ViewBag.HorariosDisponiveis = new List<string>();
                ViewBag.MotivoBloqueio = bloqueio.Motivo;
            }
            else
            {
                ViewBag.HorariosDisponiveis = await ObterHorariosDisponiveis(agendamento.Data, agendamento.Profissional);
            }

            return View(agendamento);
        }

        [HttpPost]
        public async Task<IActionResult> Finalizar(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento != null)
            {
                agendamento.IsConcluido = true;
                agendamento.IsFalta = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Listagem));
        }

        [HttpPost]
        public async Task<IActionResult> Falta(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento != null)
            {
                agendamento.IsFalta = true;
                agendamento.IsConcluido = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Listagem));
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento != null)
            {
                _context.Agendamentos.Remove(agendamento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Listagem));
        }

        [AllowAnonymous]
        public IActionResult Listagem()
        {
            var hoje = DateTime.Today;

            // Busca no banco os agendamentos de hoje e dos dias futuros (>=)
            var agendamentos = _context.Agendamentos
                .Where(a => a.Data.Date >= hoje)
                .OrderBy(a => a.Data)    // 1º Organiza pelos dias (do mais próximo ao mais distante)
                .ThenBy(a => a.Horario)  // 2º Organiza pelas horas dentro de cada dia
                .ToList();

            return View(agendamentos);
        }

        public IActionResult Sucesso()
        {
            return View();
        }

        
        // NOVO MOTOR DE HORÁRIOS (Filtra Passados e Ocupados por Barbeiro)
        
        private async Task<List<string>> ObterHorariosDisponiveis(DateTime data, string profissional)
        {
            var horarios = new List<string>();
            TimeSpan horaAtual = new TimeSpan(9, 0, 0); // Inicia as 09:00
            TimeSpan horaFim = new TimeSpan(18, 0, 0);  // Termina as 18:00

            // 1. Busca no banco todos os horários agendados PARA ESTE BARBEIRO nesta data
            var agendamentosOcupados = await _context.Agendamentos
                .Where(a => a.Data.Date == data.Date && a.Profissional == profissional)
                .Select(a => a.Horario)
                .ToListAsync();

            var agora = DateTime.Now;

            while (horaAtual <= horaFim)
            {
                string horaFormatada = horaAtual.ToString(@"hh\:mm");

                // Verifica se este barbeiro já está ocupado nesta hora
                bool isOcupado = agendamentosOcupados.Contains(horaFormatada);

                // Verifica se a hora já passou no relógio
                bool isPassado = false;
                if (data.Date == agora.Date && horaAtual <= agora.TimeOfDay)
                {
                    isPassado = true; // Hoje: corta as horas que já passaram
                }
                else if (data.Date < agora.Date)
                {
                    isPassado = true; // Dias anteriores: corta tudo
                }

                // Se o barbeiro NÃO está ocupado E o horário NÃO passou, adiciona na tela!
                if (!isOcupado && !isPassado)
                {
                    horarios.Add(horaFormatada);
                }

                // Pula de 20 em 20 minutos
                horaAtual = horaAtual.Add(TimeSpan.FromMinutes(20));
            }

            return horarios;
        }
    }
}