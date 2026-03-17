using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class ServicoModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório")]
        public string Nome { get; set; } 

        [Required(ErrorMessage = "O preço é obrigatório")]
        public decimal Preco { get; set; } 

        public bool Ativo { get; set; } = true; 
    }
}