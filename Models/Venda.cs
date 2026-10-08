using System;

namespace PapJoaoSimoes.Models
{
    public class Venda
    {
        public int VendaId { get; set; }
        public DateTime DataVenda { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; }
        public decimal Total { get; set; }
    }
}
