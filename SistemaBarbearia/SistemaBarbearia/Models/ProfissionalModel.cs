using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class ProfissionalModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do barbeiro é obrigatório")]
        public string Nome { get; set; } 
        public string Telefone { get; set; } 

        public bool Ativo { get; set; } = true;
    }
}