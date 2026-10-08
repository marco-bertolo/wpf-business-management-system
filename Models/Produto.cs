namespace PapJoaoSimoes.Models
{
    public class Produto
    {
        public int ProdutoId { get; set; }
        public string Descricao { get; set; }
        public decimal PrecoCompra { get; set; }
        public decimal PrecoVenda { get; set; }
        public int StockAtual { get; set; }
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; }
        public bool Ativo { get; set; }
    }
}
