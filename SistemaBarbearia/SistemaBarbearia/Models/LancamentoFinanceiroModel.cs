using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class LancamentoFinanceiroModel
    {
        [Key]
        public int Id { get; set; }

        public int CaixaId { get; set; }

        [Required]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        public string Categoria { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public decimal Valor { get; set; }

        public string FormaPagamento { get; set; } = string.Empty;

        public DateTime DataLancamento { get; set; } = DateTime.Now;

        public int? BarbeiroId { get; set; }
    }
}