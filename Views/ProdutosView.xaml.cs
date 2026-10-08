using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PapJoaoSimoes.Views
{
    public partial class ProdutosView : UserControl
    {
        private LayoutProdutos _layoutAtual = LayoutProdutos.Nenhum;
        private bool _ajusteColunasAgendado;

        public ProdutosView()
        {
            InitializeComponent();
            SizeChanged += ProdutosView_SizeChanged;
            ProdutosDataGrid.SizeChanged += ProdutosDataGrid_SizeChanged;
        }

        private void ProdutosView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutProdutos novoLayout;

            if (e.NewSize.Width < 1020)
            {
                novoLayout = LayoutProdutos.Compacto;
            }
            else if (e.NewSize.Width < 1280)
            {
                novoLayout = LayoutProdutos.Medio;
            }
            else
            {
                novoLayout = LayoutProdutos.Grande;
            }

            if (novoLayout != _layoutAtual)
            {
                _layoutAtual = novoLayout;
                AplicarLayout(novoLayout);
            }

            AgendarAjusteColunas();
        }

        private void AplicarLayout(LayoutProdutos layout)
        {
            bool layoutCompacto = layout == LayoutProdutos.Compacto;
            bool layoutGrande = layout == LayoutProdutos.Grande;

            CodigoColumn.Visibility = layoutCompacto ? Visibility.Collapsed : Visibility.Visible;
            FornecedorColumn.Visibility = layoutGrande ? Visibility.Visible : Visibility.Collapsed;
            PrecoVendaColumn.Visibility = layoutGrande ? Visibility.Visible : Visibility.Collapsed;

            PesquisaContainer.Width = layoutCompacto ? 190 : 250;
            FormularioColumn.Width = new GridLength(layoutCompacto ? 300 : 326);

            CodigoColumn.Width = new DataGridLength(72);
            FornecedorColumn.Width = new DataGridLength(220);
            PrecoVendaColumn.Width = new DataGridLength(100);
            StockColumn.Width = new DataGridLength(82);
            EstadoColumn.Width = new DataGridLength(92);
            AcoesColumn.Width = new DataGridLength(88);
        }

        private void ProdutosDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
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
            if (_layoutAtual == LayoutProdutos.Nenhum || ProdutosDataGrid.ActualWidth <= 0)
            {
                return;
            }

            double larguraDisponivel = Math.Max(0, ProdutosDataGrid.ActualWidth - 4);

            if (_layoutAtual == LayoutProdutos.Compacto)
            {
                ProdutoColumn.Width = new DataGridLength(
                    Math.Max(170, larguraDisponivel
                                  - StockColumn.Width.Value
                                  - EstadoColumn.Width.Value
                                  - AcoesColumn.Width.Value));
            }
            else if (_layoutAtual == LayoutProdutos.Medio)
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + StockColumn.Width.Value
                                     + EstadoColumn.Width.Value
                                     + AcoesColumn.Width.Value;

                ProdutoColumn.Width = new DataGridLength(
                    Math.Max(170, larguraDisponivel - larguraFixa));
            }
            else
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + PrecoVendaColumn.Width.Value
                                     + StockColumn.Width.Value
                                     + EstadoColumn.Width.Value
                                     + AcoesColumn.Width.Value;
                double larguraVariavel = Math.Max(360, larguraDisponivel - larguraFixa);
                double larguraProduto = Math.Max(170, larguraVariavel * 0.45);
                double larguraFornecedor = Math.Max(160, larguraVariavel - larguraProduto);

                ProdutoColumn.Width = new DataGridLength(larguraProduto);
                FornecedorColumn.Width = new DataGridLength(larguraFornecedor);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProdutosDataGrid.InvalidateMeasure();
            }), DispatcherPriority.Background);
        }

        private enum LayoutProdutos
        {
            Nenhum,
            Compacto,
            Medio,
            Grande
        }
    }
}




