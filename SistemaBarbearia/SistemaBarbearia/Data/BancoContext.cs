using Microsoft.EntityFrameworkCore;
using SistemaBarbearia.Models;
using SuaBarbearia.Models;

namespace SistemaBarbearia.Data
{
    public class BancoContext : DbContext
    {
        public BancoContext(DbContextOptions<BancoContext> options) : base(options) { }

        public DbSet<AgendamentoModel> Agendamentos { get; set; }
        public DbSet<BloqueioModel> Bloqueios { get; set; }
        public DbSet<ClienteModel> Clientes { get; set; }

        
        public DbSet<UsuarioModel> Usuarios { get; set; }

        
        public DbSet<ServicoModel> Servicos { get; set; }
        public DbSet<ProfissionalModel> Profissionais { get; set; }

        public DbSet<CaixaModel> Caixas { get; set; }
        public DbSet<LancamentoFinanceiroModel> LancamentosFinanceiros { get; set; }
        public DbSet<RegraComissaoModel> RegrasComissao { get; set; }
        public DbSet<ProdutoModel> Produtos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AgendamentoModel>()
                .Property(a => a.Valor)
                .HasPrecision(10, 2); 

            
            modelBuilder.Entity<ServicoModel>()
                .Property(s => s.Preco)
                .HasPrecision(10, 2);
        }
    }
}