using System.ComponentModel.DataAnnotations;

namespace SistemaBarbearia.Models
{
    public class ClienteModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O WhatsApp é obrigatório")]
        public string? WhatsApp { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(4, ErrorMessage = "A senha deve ter no mínimo 4 caracteres")]
        public string? Senha { get; set; }
    }
}