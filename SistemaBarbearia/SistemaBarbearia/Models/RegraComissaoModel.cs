using System.ComponentModel.DataAnnotations;

namespace SuaBarbearia.Models
{
    public class RegraComissaoModel
    {
        [Key]
        public int Id { get; set; }

        public int BarbeiroId { get; set; }

        public decimal PorcentagemServico { get; set; } 
        public decimal PorcentagemProduto { get; set; } 

        public bool DescontaTaxaCartao { get; set; } = true; 
    }
}