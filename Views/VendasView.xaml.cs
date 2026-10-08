using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PapJoaoSimoes.Views
{
    public partial class VendasView : UserControl
    {
        private LayoutVendas _layoutAtual = LayoutVendas.Nenhum;
        private bool _ajusteColunasAgendado;

        public VendasView()
        {
            InitializeComponent();
            SizeChanged += VendasView_SizeChanged;
            VendasDataGrid.SizeChanged += VendasDataGrid_SizeChanged;
        }

        private void VendasView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutVendas novoLayout;

            if (e.NewSize.Width < 1050)
            {
                novoLayout = LayoutVendas.Compacto;
            }
            else if (e.NewSize.Width < 1350)
            {
                novoLayout = LayoutVendas.Medio;
            }
            else
            {
                novoLayout = LayoutVendas.Grande;
            }

            if (novoLayout != _layoutAtual)
            {
                _layoutAtual = novoLayout;
                AplicarLayout(novoLayout);
            }

            AgendarAjusteColunas();
        }

        private void AplicarLayout(LayoutVendas layout)
        {
            bool layoutCompacto = layout == LayoutVendas.Compacto;
            bool layoutGrande = layout == LayoutVendas.Grande;

            CodigoColumn.Visibility = layoutCompacto ? Visibility.Collapsed : Visibility.Visible;
            FuncionarioColumn.Visibility = layoutGrande ? Visibility.Visible : Visibility.Collapsed;

            PesquisaContainer.Width = layoutCompacto ? 190 : 250;
            FormularioColumn.Width = new GridLength(layoutCompacto ? 360 : 400);

            CodigoColumn.Width = new DataGridLength(72);
            DataColumn.Width = new DataGridLength(126);
            FuncionarioColumn.Width = new DataGridLength(200);
            TotalColumn.Width = new DataGridLength(100);
        }

        private void VendasDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
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
            if (_layoutAtual == LayoutVendas.Nenhum || VendasDataGrid.ActualWidth <= 0)
            {
                return;
            }

            double larguraDisponivel = Math.Max(0, VendasDataGrid.ActualWidth - 4);

            if (_layoutAtual == LayoutVendas.Compacto)
            {
                ClienteColumn.Width = new DataGridLength(
                    Math.Max(150, larguraDisponivel - DataColumn.Width.Value - TotalColumn.Width.Value));
            }
            else if (_layoutAtual == LayoutVendas.Medio)
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + DataColumn.Width.Value
                                     + TotalColumn.Width.Value;

                ClienteColumn.Width = new DataGridLength(
                    Math.Max(150, larguraDisponivel - larguraFixa));
            }
            else
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + DataColumn.Width.Value
                                     + TotalColumn.Width.Value;
                double larguraVariavel = Math.Max(320, larguraDisponivel - larguraFixa);
                double larguraCliente = Math.Max(150, larguraVariavel * 0.55);
                double larguraFuncionario = Math.Max(150, larguraVariavel - larguraCliente);

                ClienteColumn.Width = new DataGridLength(larguraCliente);
                FuncionarioColumn.Width = new DataGridLength(larguraFuncionario);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                VendasDataGrid.InvalidateMeasure();
            }), DispatcherPriority.Background);
        }

        private enum LayoutVendas
        {
            Nenhum,
            Compacto,
            Medio,
            Grande
        }
    }
}






