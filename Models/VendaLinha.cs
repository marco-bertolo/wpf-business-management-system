namespace PapJoaoSimoes.Models
{
    public class VendaLinha
    {
        public int VendaLinhaId { get; set; }
        public int VendaId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoDescricao { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int StockDisponivel { get; set; }

        public decimal Subtotal
        {
            get { return Quantidade * PrecoUnitario; }
        }
    }
}
