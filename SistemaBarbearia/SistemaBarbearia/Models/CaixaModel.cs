using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class CaixaModel
    {
        [Key]
        public int Id { get; set; }

        public DateTime DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }

        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }

        public string Status { get; set; } = "Aberto";
    }
}