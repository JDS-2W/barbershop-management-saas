namespace SistemaBarbearia.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Whatsapp { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;

        
        public bool IsAdmin { get; set; } = false;
    }
}