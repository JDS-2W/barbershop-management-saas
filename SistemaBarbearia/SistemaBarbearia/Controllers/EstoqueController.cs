using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;

namespace SistemaBarbearia.Controllers
{
    public class EstoqueController : Controller
    {
        private readonly BancoContext _bancoContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        
        public EstoqueController(BancoContext bancoContext, IWebHostEnvironment webHostEnvironment)
        {
            _bancoContext = bancoContext;
            _webHostEnvironment = webHostEnvironment;
        }

        // TELA PRINCIPAL DO ESTOQUE
        public IActionResult Index()
        {
            var produtos = _bancoContext.Produtos.OrderBy(p => p.QuantidadeEstoque).ToList();
            return View(produtos);
        }

        
        public IActionResult Criar()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Criar(ProdutoModel produto, IFormFile? fotoProduto)
        {
            
            if (fotoProduto != null && fotoProduto.Length > 0)
            {
                
                string pastaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "images", "produtos");
                if (!Directory.Exists(pastaDestino))
                {
                    Directory.CreateDirectory(pastaDestino);
                }

                
                string nomeArquivo = Guid.NewGuid().ToString() + "_" + fotoProduto.FileName;
                string caminhoCompleto = Path.Combine(pastaDestino, nomeArquivo);

                
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await fotoProduto.CopyToAsync(stream);
                }

                
                produto.FotoCaminho = "/images/produtos/" + nomeArquivo;
            }
            else
            {
                
                produto.FotoCaminho = "/images/sem-foto.png";
            }

            _bancoContext.Produtos.Add(produto);
            _bancoContext.SaveChanges();

            TempData["Sucesso"] = "Produto cadastrado com sucesso!";
            return RedirectToAction("Index");
        }

        
        public IActionResult Vender(int id)
        {
            // Verifica se tem caixa aberto primeiro! Não pode vender sem caixa.
            var caixaAberto = _bancoContext.Caixas.FirstOrDefault(c => c.Status == "Aberto");
            if (caixaAberto == null)
            {
                TempData["Erro"] = "Você precisa Abrir o Caixa do dia antes de vender produtos!";
                return RedirectToAction("Index");
            }

            var produto = _bancoContext.Produtos.Find(id);
            if (produto == null || produto.QuantidadeEstoque <= 0)
            {
                TempData["Erro"] = "Produto não encontrado ou esgotado!";
                return RedirectToAction("Index");
            }

            ViewBag.CaixaId = caixaAberto.Id;
            return View(produto);
        }

        
        [HttpPost]
        public IActionResult ConfirmarVenda(int id, int quantidade, string formaPagamento, int caixaId)
        {
            var produto = _bancoContext.Produtos.Find(id);
            var caixa = _bancoContext.Caixas.Find(caixaId);

            if (produto != null && caixa != null)
            {
                if (quantidade > produto.QuantidadeEstoque)
                {
                    TempData["Erro"] = "Quantidade maior que o estoque disponível!";
                    return RedirectToAction("Vender", new { id = id });
                }

                // 1. Desconta do estoque
                produto.QuantidadeEstoque -= quantidade;

                // 2. Calcula o valor total
                decimal valorTotal = produto.PrecoVenda * quantidade;

                // 3. Registra a entrada no financeiro
                var lancamento = new LancamentoFinanceiroModel
                {
                    CaixaId = caixa.Id,
                    Tipo = "Entrada",
                    Categoria = "Produto",
                    Descricao = $"Venda: {quantidade}x {produto.Nome}",
                    Valor = valorTotal,
                    FormaPagamento = formaPagamento,
                    DataLancamento = DateTime.Now
                };

                _bancoContext.LancamentosFinanceiros.Add(lancamento);

                
                caixa.SaldoFinal += valorTotal;

                
                _bancoContext.SaveChanges();

                TempData["Sucesso"] = $"Venda de {produto.Nome} realizada! R$ {valorTotal:F2} adicionados ao caixa.";
            }

            return RedirectToAction("Index");
        }
        
        public IActionResult Reabastecer(int id)
        {
            var caixaAberto = _bancoContext.Caixas.FirstOrDefault(c => c.Status == "Aberto");
            if (caixaAberto == null)
            {
                TempData["Erro"] = "Você precisa Abrir o Caixa para poder registrar uma despesa de compra de estoque!";
                return RedirectToAction("Index");
            }

            var produto = _bancoContext.Produtos.Find(id);
            if (produto == null) return RedirectToAction("Index");

            ViewBag.CaixaId = caixaAberto.Id;
            return View(produto);
        }

        // CONFIRMA A COMPRA, AUMENTA ESTOQUE E REGISTRA O GASTO
        [HttpPost]
        public IActionResult ConfirmarReabastecimento(int id, int quantidadeComprada, decimal custoTotal, int caixaId)
        {
            var produto = _bancoContext.Produtos.Find(id);
            var caixa = _bancoContext.Caixas.Find(caixaId);

            if (produto != null && caixa != null)
            {
                //  Aumenta a quantidade no estoque
                produto.QuantidadeEstoque += quantidadeComprada;

                //  Cria a SAÍDA (Despesa) no financeiro
                var lancamento = new LancamentoFinanceiroModel
                {
                    CaixaId = caixa.Id,
                    Tipo = "Saida",
                    Categoria = "Despesa", // Gasto com estoque
                    Descricao = $"Compra de Estoque: {quantidadeComprada}x {produto.Nome}",
                    Valor = custoTotal,
                    FormaPagamento = "Dinheiro", // Ou como ele tirou do caixa
                    DataLancamento = DateTime.Now
                };

                _bancoContext.LancamentosFinanceiros.Add(lancamento);

                //  Tira o dinheiro da gaveta (Saldo Atual do dia)
                caixa.SaldoFinal -= custoTotal;

                _bancoContext.SaveChanges();

                TempData["Sucesso"] = $"Estoque reabastecido! Despesa de R$ {custoTotal:F2} registrada no caixa.";
            }

            return RedirectToAction("Index");
        }


    }
}