using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class ProdutoModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string Categoria { get; set; } = "Venda";

        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }

        public int QuantidadeEstoque { get; set; }
        public int AlertaEstoqueBaixo { get; set; } = 5;

        
        public string? FotoCaminho { get; set; }
    }
}