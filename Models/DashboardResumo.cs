using System.Collections.Generic;

namespace PapJoaoSimoes.Models
{
    public class DashboardResumo
    {
        public int ClientesAtivos { get; set; }
        public int ProdutosAtivos { get; set; }
        public int TotalVendas { get; set; }
        public decimal TotalVendido { get; set; }
        public List<Venda> VendasRecentes { get; set; }
        public List<Produto> ProdutosStockBaixo { get; set; }
    }
}
