using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PapJoaoSimoes.Views
{
    public partial class ClientesView : UserControl
    {
        private LayoutClientes _layoutAtual = LayoutClientes.Nenhum;
        private bool _ajusteColunasAgendado;

        public ClientesView()
        {
            InitializeComponent();
            SizeChanged += ClientesView_SizeChanged;
            ClientesDataGrid.SizeChanged += ClientesDataGrid_SizeChanged;
        }

        private void ClientesView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutClientes novoLayout;

            if (e.NewSize.Width < 1020)
            {
                novoLayout = LayoutClientes.Compacto;
            }
            else if (e.NewSize.Width < 1280)
            {
                novoLayout = LayoutClientes.Medio;
            }
            else
            {
                novoLayout = LayoutClientes.Grande;
            }

            if (novoLayout != _layoutAtual)
            {
                _layoutAtual = novoLayout;
                AplicarLayout(novoLayout);
            }

            AgendarAjusteColunas();
        }

        private void AplicarLayout(LayoutClientes layout)
        {
            bool layoutCompacto = layout == LayoutClientes.Compacto;
            bool layoutGrande = layout == LayoutClientes.Grande;

            CodigoColumn.Visibility = layoutCompacto ? Visibility.Collapsed : Visibility.Visible;
            NifColumn.Visibility = layoutGrande ? Visibility.Visible : Visibility.Collapsed;
            TelemovelColumn.Visibility = layoutCompacto ? Visibility.Collapsed : Visibility.Visible;
            EmailColumn.Visibility = layoutGrande ? Visibility.Visible : Visibility.Collapsed;

            PesquisaContainer.Width = layoutCompacto ? 190 : 250;
            FormularioColumn.Width = new GridLength(layoutCompacto ? 300 : 326);

            CodigoColumn.Width = new DataGridLength(72);
            NifColumn.Width = new DataGridLength(100);
            TelemovelColumn.Width = new DataGridLength(115);
            EstadoColumn.Width = new DataGridLength(92);
            AcoesColumn.Width = new DataGridLength(88);
        }

        private void ClientesDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AgendarAjusteColunas();
        }

        private void AgendarAjusteColunas()
        {
            if (_ajusteColunasAgendado)
            {
                return;
            }

            _ajusteColunasAgendado = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _ajusteColunasAgendado = false;
                AjustarLargurasColunas();
            }), DispatcherPriority.Render);
        }

        private void AjustarLargurasColunas()
        {
            if (_layoutAtual == LayoutClientes.Nenhum || ClientesDataGrid.ActualWidth <= 0)
            {
                return;
            }

            double larguraDisponivel = Math.Max(0, ClientesDataGrid.ActualWidth - 4);

            if (_layoutAtual == LayoutClientes.Compacto)
            {
                ClienteColumn.Width = new DataGridLength(
                    Math.Max(170, larguraDisponivel - EstadoColumn.Width.Value - AcoesColumn.Width.Value));
            }
            else if (_layoutAtual == LayoutClientes.Medio)
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + TelemovelColumn.Width.Value
                                     + EstadoColumn.Width.Value
                                     + AcoesColumn.Width.Value;

                ClienteColumn.Width = new DataGridLength(
                    Math.Max(170, larguraDisponivel - larguraFixa));
            }
            else
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + NifColumn.Width.Value
                                     + TelemovelColumn.Width.Value
                                     + EstadoColumn.Width.Value
                                     + AcoesColumn.Width.Value;
                double larguraVariavel = Math.Max(360, larguraDisponivel - larguraFixa);
                double larguraCliente = Math.Max(170, larguraVariavel * 0.45);
                double larguraEmail = Math.Max(190, larguraVariavel - larguraCliente);

                ClienteColumn.Width = new DataGridLength(larguraCliente);
                EmailColumn.Width = new DataGridLength(larguraEmail);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ClientesDataGrid.InvalidateMeasure();
            }), DispatcherPriority.Background);
        }

        private enum LayoutClientes
        {
            Nenhum,
            Compacto,
            Medio,
            Grande
        }
    }
}
