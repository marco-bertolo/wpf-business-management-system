using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PapJoaoSimoes.Views
{
    public partial class FuncionariosView : UserControl
    {
        private LayoutFuncionarios _layoutAtual = LayoutFuncionarios.Nenhum;
        private bool _ajusteColunasAgendado;

        public FuncionariosView()
        {
            InitializeComponent();
            SizeChanged += FuncionariosView_SizeChanged;
            FuncionariosDataGrid.SizeChanged += FuncionariosDataGrid_SizeChanged;
        }

        private void FuncionariosView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutFuncionarios novoLayout;

            if (e.NewSize.Width < 1020)
            {
                novoLayout = LayoutFuncionarios.Compacto;
            }
            else if (e.NewSize.Width < 1280)
            {
                novoLayout = LayoutFuncionarios.Medio;
            }
            else
            {
                novoLayout = LayoutFuncionarios.Grande;
            }

            if (novoLayout != _layoutAtual)
            {
                _layoutAtual = novoLayout;
                AplicarLayout(novoLayout);
            }

            AgendarAjusteColunas();
        }

        private void AplicarLayout(LayoutFuncionarios layout)
        {
            bool layoutCompacto = layout == LayoutFuncionarios.Compacto;
            bool layoutGrande = layout == LayoutFuncionarios.Grande;

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

        private void FuncionariosDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
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
            if (_layoutAtual == LayoutFuncionarios.Nenhum || FuncionariosDataGrid.ActualWidth <= 0)
            {
                return;
            }

            double larguraDisponivel = Math.Max(0, FuncionariosDataGrid.ActualWidth - 4);

            if (_layoutAtual == LayoutFuncionarios.Compacto)
            {
                FuncionarioColumn.Width = new DataGridLength(
                    Math.Max(170, larguraDisponivel - EstadoColumn.Width.Value - AcoesColumn.Width.Value));
            }
            else if (_layoutAtual == LayoutFuncionarios.Medio)
            {
                double larguraFixa = CodigoColumn.Width.Value
                                     + TelemovelColumn.Width.Value
                                     + EstadoColumn.Width.Value
                                     + AcoesColumn.Width.Value;

                FuncionarioColumn.Width = new DataGridLength(
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
                double larguraFuncionario = Math.Max(170, larguraVariavel * 0.45);
                double larguraEmail = Math.Max(190, larguraVariavel - larguraFuncionario);

                FuncionarioColumn.Width = new DataGridLength(larguraFuncionario);
                EmailColumn.Width = new DataGridLength(larguraEmail);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                FuncionariosDataGrid.InvalidateMeasure();
            }), DispatcherPriority.Background);
        }

        private enum LayoutFuncionarios
        {
            Nenhum,
            Compacto,
            Medio,
            Grande
        }
    }
}




