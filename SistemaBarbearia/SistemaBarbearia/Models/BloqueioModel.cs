using System;

namespace SistemaBarbearia.Models
{
    public class BloqueioModel
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string? Motivo { get; set; } 
    }
}