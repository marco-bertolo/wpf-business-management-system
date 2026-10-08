using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PapJoaoSimoes.Views;

namespace PapJoaoSimoes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModels.MainViewModel viewModel;

        public bool SessaoTerminada { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            viewModel = DataContext as ViewModels.MainViewModel;
            if (viewModel != null)
            {
                viewModel.TerminarSessaoPedido += ViewModel_TerminarSessaoPedido;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.TerminarSessaoPedido -= ViewModel_TerminarSessaoPedido;
                viewModel.Dispose();
            }

            base.OnClosed(e);
        }

        private void ViewModel_TerminarSessaoPedido(object sender, EventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show(
                "Tens a certeza de que pretendes terminar a sessão?",
                "Terminar sessão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
            {
                return;
            }

            SessaoTerminada = true;
            Services.SessaoAtual.Terminar();
            Close();
        }
    }
}
