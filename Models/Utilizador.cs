using System;

namespace PapJoaoSimoes.Models
{
    public class Utilizador
    {
        public int UtilizadorId { get; set; }
        public string NomeUtilizador { get; set; }
        public int? FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public DateTime CriadoEm { get; set; }
        public bool Ativo { get; set; }
    }
}
