using Microsoft.AspNetCore.Mvc;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SistemaBarbearia.Controllers
{
    // Classe auxiliar para mandar os dados de ganhos dinâmicos para a tela
    public class FaturamentoProfissional
    {
        public string Nome { get; set; }
        public double TotalGanhos { get; set; }
        public int QuantidadeCortes { get; set; }
    }

    [Authorize]
    public class AdminController : Controller
    {
        private readonly BancoContext _context;

        public AdminController(BancoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PainelPrincipal(string filtro, DateTime? dataFiltro)
        {
            IQueryable<AgendamentoModel> query = _context.Agendamentos;

            // Se você não clicar em nenhuma data, ele puxa automaticamente o dia de hoje
            DateTime dataConsulta = dataFiltro ?? DateTime.Today;

            if (filtro == "concluidos")
                query = query.Where(a => a.IsConcluido && a.Data.Date == dataConsulta.Date);
            else if (filtro == "faltas")
                query = query.Where(a => a.IsFalta && a.Data.Date == dataConsulta.Date);
            else
            {
                filtro = "futuros"; // Garante que o botão fique aceso
                // Mostra os próximos agendamentos apenas do dia selecionado
                query = query.Where(a => !a.IsConcluido && !a.IsFalta && a.Data.Date == dataConsulta.Date);
            }

            var agendamentos = await query.OrderBy(a => a.Data).ThenBy(a => a.Horario).ToListAsync();

            
            // === INÍCIO DO CÁLCULO FINANCEIRO REAL (MOTOR NOVO) 
            
            var hoje = DateTime.Today;
            var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            // Calcula TUDO (Cortes + Produtos) que foi ENTRADA (LUCRO)
            ViewBag.GanhoHoje = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Entrada" && l.DataLancamento.Date == hoje)
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            ViewBag.GanhoSemanal = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Entrada" && l.DataLancamento.Date >= inicioSemana)
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            ViewBag.GanhoMensal = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Entrada" && l.DataLancamento.Date >= inicioMes)
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            ViewBag.GanhoTotal = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Entrada")
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            // Calcula TUDO (Despesas + Compras de Estoque) que foi SAÍDA (GASTO)
            ViewBag.GastoHoje = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Saida" && l.DataLancamento.Date == hoje)
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            ViewBag.GastoSemanal = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Saida" && l.DataLancamento.Date >= inicioSemana)
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            ViewBag.GastoMensal = await _context.LancamentosFinanceiros
                .Where(l => l.Tipo == "Saida" && l.DataLancamento.Date >= inicioMes)
                .SumAsync(l => (decimal?)l.Valor) ?? 0;

            ViewBag.FiltroAtual = filtro;
            // Envia a data escolhida para a tela para manter o calendário atualizado
            ViewBag.DataFiltroStr = dataConsulta.ToString("yyyy-MM-dd");

            return View(agendamentos);
        }

        
        // RELATÓRIO MENSAL 
        
        public async Task<IActionResult> RelatorioMensal(int? mes, int? ano)
        {
            var hoje = DateTime.Today;
            int mesAlvo = mes ?? hoje.Month;
            int anoAlvo = ano ?? hoje.Year;

            var inicioMes = new DateTime(anoAlvo, mesAlvo, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var agendamentosMes = await _context.Agendamentos
                .Where(a => a.IsConcluido && a.Data >= inicioMes && a.Data <= fimMes)
                .OrderBy(a => a.Data)
                .ToListAsync();

            ViewBag.MesReferencia = inicioMes.ToString("MMMM / yyyy").ToUpper();
            ViewBag.MesAtual = mesAlvo;
            ViewBag.AnoAtual = anoAlvo;

            // Agrupa dinamicamente por barbeiro
            var faturamentoEquipe = agendamentosMes
                .GroupBy(a => string.IsNullOrEmpty(a.Profissional) ? "Sem Profissional" : a.Profissional)
                .Select(g => new FaturamentoProfissional
                {
                    Nome = g.Key,
                    TotalGanhos = g.Sum(a => ExtrairValor(a)),
                    QuantidadeCortes = g.Count()
                }).ToList();

            ViewBag.FaturamentoEquipe = faturamentoEquipe;
            ViewBag.TotalMes = faturamentoEquipe.Sum(f => f.TotalGanhos);
            ViewBag.QtdServicos = agendamentosMes.Count;

            return View(agendamentosMes);
        }

        
        // RELATÓRIO SEMANAL 
        
        public async Task<IActionResult> RelatorioSemanal(DateTime? dataReferencia)
        {
            var dataBase = dataReferencia ?? DateTime.Today;

            int diff = (7 + (dataBase.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicioSemana = dataBase.AddDays(-1 * diff).Date;
            var fimSemana = inicioSemana.AddDays(6).Date;

            var agendamentosSemana = await _context.Agendamentos
                .Where(a => a.IsConcluido && a.Data >= inicioSemana && a.Data <= fimSemana)
                .OrderBy(a => a.Data)
                .ToListAsync();

            ViewBag.SemanaReferencia = $"{inicioSemana:dd/MM} a {fimSemana:dd/MM/yyyy}";
            ViewBag.DataAnterior = inicioSemana.AddDays(-7).ToString("yyyy-MM-dd");
            ViewBag.DataProxima = inicioSemana.AddDays(7).ToString("yyyy-MM-dd");

            // Agrupa dinamicamente por barbeiro
            var faturamentoEquipe = agendamentosSemana
                .GroupBy(a => string.IsNullOrEmpty(a.Profissional) ? "Sem Profissional" : a.Profissional)
                .Select(g => new FaturamentoProfissional
                {
                    Nome = g.Key,
                    TotalGanhos = g.Sum(a => ExtrairValor(a)),
                    QuantidadeCortes = g.Count()
                }).ToList();

            ViewBag.FaturamentoEquipe = faturamentoEquipe;
            ViewBag.TotalSemana = faturamentoEquipe.Sum(f => f.TotalGanhos);
            ViewBag.QtdServicos = agendamentosSemana.Count;

            return View(agendamentosSemana);
        }

        // Método de extração de valores
        private double ExtrairValor(AgendamentoModel a)
        {
            if (a.Valor > 0) return (double)a.Valor;
            if (string.IsNullOrEmpty(a.Servico)) return 0;
            if (a.Servico.Contains("15")) return 15.0;
            if (a.Servico.Contains("10")) return 10.0;
            if (a.Servico.Contains("20")) return 20.0;
            return 30.0;
        }

        [HttpPost]
        public async Task<IActionResult> MarcarComoFalta(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento != null)
            {
                if (agendamento.IsConcluido) return BadRequest("Não é possível marcar falta para um agendamento já concluído.");
                agendamento.IsFalta = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(PainelPrincipal), new { filtro = "faltas" });
        }

        [HttpPost]
        public async Task<IActionResult> Concluir(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento != null)
            {
                agendamento.IsConcluido = true;
                agendamento.IsFalta = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(PainelPrincipal));
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
            return RedirectToAction(nameof(PainelPrincipal));
        }

        public async Task<IActionResult> Configuracoes()
        {
            var bloqueios = await _context.Bloqueios.OrderBy(b => b.Data).ToListAsync();
            return View(bloqueios);
        }

        [HttpPost]
        public async Task<IActionResult> BloquearData(DateTime data, string motivo)
        {
            if (data != DateTime.MinValue)
            {
                var novoBloqueio = new BloqueioModel
                {
                    Data = data,
                    Motivo = motivo ?? "Bloqueio administrativo"
                };
                _context.Bloqueios.Add(novoBloqueio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Configuracoes");
        }

        [HttpPost]
        public async Task<IActionResult> RemoverBloqueio(int id)
        {
            var bloqueio = await _context.Bloqueios.FindAsync(id);
            if (bloqueio != null)
            {
                _context.Bloqueios.Remove(bloqueio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Configuracoes");
        }
    }
}