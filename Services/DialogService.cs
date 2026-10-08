using System.Windows;

namespace PapJoaoSimoes.Services
{
    public class DialogService
    {
        public bool ConfirmarDesativacao(string tipoEntidade, string nome)
        {
            MessageBoxResult resultado = MessageBox.Show(
                "Pretende desativar o " + tipoEntidade + " \"" + nome + "\"?",
                "Desativar " + tipoEntidade,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return resultado == MessageBoxResult.Yes;
        }
    }
}
