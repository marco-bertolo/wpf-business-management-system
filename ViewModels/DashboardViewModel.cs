using PapJoaoSimoes.Models;
using PapJoaoSimoes.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PapJoaoSimoes.ViewModels
{
    public class DashboardViewModel : PageViewModel
    {
        private readonly DashboardService dashboardService;
        private int clientesAtivos;
        private int produtosAtivos;
        private int totalVendas;
        private decimal totalVendido;
        private string estadoBaseDados;
        private string ultimaAtualizacao;
        private bool baseDadosDisponivel;

        public DashboardViewModel()
            : base("Painel geral", "Resumo da atividade comercial")
        {
            dashboardService = new DashboardService();
            VendasRecentes = new ObservableCollection<Venda>();
            ProdutosStockBaixo = new ObservableCollection<Produto>();
            AtualizarDashboardCommand = new RelayCommand(AtualizarDashboard);
            Atualizar();
        }

        public ObservableCollection<Venda> VendasRecentes { get; private set; }
        public ObservableCollection<Produto> ProdutosStockBaixo { get; private set; }
        public ICommand AtualizarDashboardCommand { get; private set; }

        public int ClientesAtivos
        {
            get { return clientesAtivos; }
            private set
            {
                clientesAtivos = value;
                OnPropertyChanged();
            }
        }

        public int ProdutosAtivos
        {
            get { return produtosAtivos; }
            private set
            {
                produtosAtivos = value;
                OnPropertyChanged();
            }
        }

        public int TotalVendas
        {
            get { return totalVendas; }
            private set
            {
                totalVendas = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalVendido
        {
            get { return totalVendido; }
            private set
            {
                totalVendido = value;
                OnPropertyChanged();
            }
        }

        public string EstadoBaseDados
        {
            get { return estadoBaseDados; }
            private set
            {
                estadoBaseDados = value;
                OnPropertyChanged();
            }
        }

        public string UltimaAtualizacao
        {
            get { return ultimaAtualizacao; }
            private set
            {
                ultimaAtualizacao = value;
                OnPropertyChanged();
            }
        }

        public bool BaseDadosDisponivel
        {
            get { return baseDadosDisponivel; }
            private set
            {
                baseDadosDisponivel = value;
                OnPropertyChanged();
            }
        }

        public void Atualizar()
        {
            try
            {
                DashboardResumo resumo = dashboardService.ObterResumo();

                ClientesAtivos = resumo.ClientesAtivos;
                ProdutosAtivos = resumo.ProdutosAtivos;
                TotalVendas = resumo.TotalVendas;
                TotalVendido = resumo.TotalVendido;
                SubstituirColecao(VendasRecentes, resumo.VendasRecentes);
                SubstituirColecao(ProdutosStockBaixo, resumo.ProdutosStockBaixo);

                BaseDadosDisponivel = true;
                EstadoBaseDados = "Dados sincronizados com o MySQL";
                UltimaAtualizacao = "Atualizado às " + DateTime.Now.ToString("HH:mm");
            }
            catch (Exception)
            {
                BaseDadosDisponivel = false;
                EstadoBaseDados = "Não foi possível ligar ao MySQL";
                UltimaAtualizacao = "Verifica se o servidor está ligado";
            }
        }

        private void AtualizarDashboard(object parameter)
        {
            Atualizar();
        }

        private static void SubstituirColecao<T>(ObservableCollection<T> destino, System.Collections.Generic.IEnumerable<T> valores)
        {
            destino.Clear();
            foreach (T valor in valores)
            {
                destino.Add(valor);
            }
        }
    }
}
