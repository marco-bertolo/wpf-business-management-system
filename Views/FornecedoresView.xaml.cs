using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PapJoaoSimoes.Views
{
    public partial class FornecedoresView : UserControl
    {
        private LayoutFornecedores _layoutAtual = LayoutFornecedores.Nenhum;
        private bool _ajusteColunasAgendado;

        public FornecedoresView()
        {
            InitializeComponent();
            SizeChanged += FornecedoresView_SizeChanged;
            FornecedoresDataGrid.SizeChanged += FornecedoresDataGrid_SizeChanged;
        }

        private void FornecedoresView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutFornecedores novoLayout;

            if (e.NewSize.Width < 1020)
            {
                novoLayout = LayoutFornecedores.Compacto;
            }
            else if (e.NewSize.Width < 1280)
            {
                novoLayout = LayoutFornecedores.Medio;
            }
            else
            {
                novoLayout = LayoutFornecedores.Grande;
            }

            if (novoLayout != _layoutAtual)
            {
                _layoutAtual = novoLayout;
                AplicarLayout(novoLayout);
            }

            AgendarAjusteColunas();
        }

        private void AplicarLayout(LayoutFornecedores layout)
        {
            bool layoutCompacto = layout == LayoutFornecedores.Compacto;
            bool layoutGrande = layout == LayoutFornecedores.Grande;

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

        private void FornecedoresDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
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
            if (_layoutAtual == LayoutFornecedores.Nenhum || FornecedoresDataGrid.ActualWidth <= 0)
            {
                return;
            }

            double larguraDisponivel = Math.Max(0, FornecedoresDataGrid.ActualWidth - 4);

            if (_layoutAtual == LayoutFornecedores.Compacto)
            {
                FornecedorColumn.Width = new DataGridLength(
                    Math.Max(170, larguraDisponivel - EstadoColumn.Width.Value - AcoesColumn.Width.Value));
            }
            else if (_layoutAtual == LayoutFornecedores.Medio)
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + TelemovelColumn.Width.Value
                                     + EstadoColumn.Width.Value
                                     + AcoesColumn.Width.Value;

                FornecedorColumn.Width = new DataGridLength(
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
                double larguraFornecedor = Math.Max(170, larguraVariavel * 0.45);
                double larguraEmail = Math.Max(190, larguraVariavel - larguraFornecedor);

                FornecedorColumn.Width = new DataGridLength(larguraFornecedor);
                EmailColumn.Width = new DataGridLength(larguraEmail);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                FornecedoresDataGrid.InvalidateMeasure();
            }), DispatcherPriority.Background);
        }

        private enum LayoutFornecedores
        {
            Nenhum,
            Compacto,
            Medio,
            Grande
        }
    }
}


