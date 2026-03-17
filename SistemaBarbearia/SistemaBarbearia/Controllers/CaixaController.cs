using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;

namespace SistemaBarbearia.Controllers
{
    public class CaixaController : Controller
    {
        private readonly BancoContext _bancoContext;

        public CaixaController(BancoContext bancoContext)
        {
            _bancoContext = bancoContext;
        }

        // TELA PRINCIPAL DO CAIXA COM CÁLCULOS
        public IActionResult Index()
        {
            var caixaAberto = _bancoContext.Caixas.FirstOrDefault(c => c.Status == "Aberto");

            if (caixaAberto != null)
            {
                // Calcula todas as entradas do dia atual
                var totalEntradas = _bancoContext.LancamentosFinanceiros
                    .Where(l => l.CaixaId == caixaAberto.Id && l.Tipo == "Entrada")
                    .Sum(l => l.Valor);

                ViewBag.EntradasDoDia = totalEntradas;
            }

            return View(caixaAberto);
        }

        [HttpPost]
        public IActionResult Abrir(decimal saldoInicial)
        {
            var caixaAberto = _bancoContext.Caixas.FirstOrDefault(c => c.Status == "Aberto");
            if (caixaAberto != null)
            {
                TempData["Erro"] = "Já existe um caixa aberto!";
                return RedirectToAction("Index");
            }

            var novoCaixa = new CaixaModel
            {
                DataAbertura = DateTime.Now,
                SaldoInicial = saldoInicial,
                SaldoFinal = saldoInicial,
                Status = "Aberto"
            };

            _bancoContext.Caixas.Add(novoCaixa);
            _bancoContext.SaveChanges();

            TempData["Sucesso"] = "Caixa aberto com sucesso! Bom dia de trabalho!";
            return RedirectToAction("Index");
        }

        public IActionResult NovoLancamento()
        {
            var caixaAberto = _bancoContext.Caixas.FirstOrDefault(c => c.Status == "Aberto");
            if (caixaAberto == null)
            {
                TempData["Erro"] = "Você precisa abrir o caixa primeiro!";
                return RedirectToAction("Index");
            }

            ViewBag.CaixaId = caixaAberto.Id;
            return View();
        }

        [HttpPost]
        public IActionResult NovoLancamento(LancamentoFinanceiroModel lancamento)
        {
            lancamento.DataLancamento = DateTime.Now;
            _bancoContext.LancamentosFinanceiros.Add(lancamento);

            var caixa = _bancoContext.Caixas.Find(lancamento.CaixaId);

            if (caixa != null)
            {
                if (lancamento.Tipo == "Entrada")
                {
                    caixa.SaldoFinal += lancamento.Valor;
                }
                else
                {
                    caixa.SaldoFinal -= lancamento.Valor;
                }
            }

            _bancoContext.SaveChanges();

            TempData["Sucesso"] = "Lançamento registrado com sucesso!";
            return RedirectToAction("Index");
        }

        // AÇÃO DO BOTÃO VERMELHO (FECHAR CAIXA)
        [HttpPost]
        public IActionResult Fechar()
        {
            var caixaAberto = _bancoContext.Caixas.FirstOrDefault(c => c.Status == "Aberto");
            if (caixaAberto != null)
            {
                caixaAberto.Status = "Fechado";
                caixaAberto.DataFechamento = DateTime.Now;
                _bancoContext.SaveChanges();

                TempData["Sucesso"] = "Caixa fechado com sucesso! Excelente dia de trabalho.";
            }
            return RedirectToAction("Index");
        }
    }
}