using System;
using PapJoaoSimoes.Models;

namespace PapJoaoSimoes.Services
{
    public static class SessaoAtual
    {
        public static Utilizador Utilizador { get; set; }

        public static event EventHandler UtilizadorAlterado;

        public static void Atualizar(Utilizador utilizador)
        {
            Utilizador = utilizador;
            NotificarAlteracao();
        }

        public static void Terminar()
        {
            Utilizador = null;
            NotificarAlteracao();
        }

        private static void NotificarAlteracao()
        {
            EventHandler handler = UtilizadorAlterado;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }
    }
}
