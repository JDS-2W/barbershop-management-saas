using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class AgendamentoModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do cliente é obrigatório")]
        public string NomeCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "O WhatsApp é obrigatório")]
        public string Whatsapp { get; set; } = string.Empty;

        public string Servico { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string Horario { get; set; } = string.Empty;

        
        public string Profissional { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public bool IsConcluido { get; set; } = false;

        
        public bool IsFalta { get; set; } = false;
    }
}