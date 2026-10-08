using PapJoaoSimoes.Models;
using PapJoaoSimoes.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PapJoaoSimoes.Views
{
    public partial class ContasView : UserControl
    {
        public ContasView()
        {
            InitializeComponent();
        }

        private void NovaContaButton_Click(object sender, RoutedEventArgs e)
        {
            LimparPasswords();
        }

        private void EditarContaButton_Click(object sender, RoutedEventArgs e)
        {
            LimparPasswords();
        }

        private void GuardarContaButton_Click(object sender, RoutedEventArgs e)
        {
            ContasViewModel viewModel = DataContext as ContasViewModel;
            if (viewModel != null && viewModel.Guardar(PasswordInput.Password, ConfirmarPasswordInput.Password))
            {
                LimparPasswords();
            }
        }

        private void EliminarContaButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Utilizador conta = button == null ? null : button.Tag as Utilizador;
            ContasViewModel viewModel = DataContext as ContasViewModel;
            if (conta == null || viewModel == null)
            {
                return;
            }

            MessageBoxResult resultado = MessageBox.Show(
                "Pretendes eliminar definitivamente a conta \"" + conta.NomeUtilizador + "\"?",
                "Eliminar conta",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes && viewModel.Eliminar(conta))
            {
                LimparPasswords();
            }
        }

        private void LimparPasswords()
        {
            PasswordInput.Clear();
            ConfirmarPasswordInput.Clear();
        }
    }
}
