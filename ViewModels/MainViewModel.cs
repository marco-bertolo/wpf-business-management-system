using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using PapJoaoSimoes.Models;
using PapJoaoSimoes.Services;

namespace PapJoaoSimoes.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly Dictionary<string, PageViewModel> pages;
        private PageViewModel currentPage;

        public event EventHandler TerminarSessaoPedido;

        public MainViewModel()
        {
            pages = new Dictionary<string, PageViewModel>
            {
                { "Dashboard", new DashboardViewModel() },
                { "Vendas", new VendasViewModel() },
                { "Produtos", new ProdutosViewModel() },
                { "Clientes", new ClientesViewModel() },
                { "Fornecedores", new FornecedoresViewModel() },
                { "Funcionarios", new FuncionariosViewModel() },
                { "Contas", new ContasViewModel() }
            };

            NavigateCommand = new RelayCommand(Navigate);
            TerminarSessaoCommand = new RelayCommand(TerminarSessao);
            SessaoAtual.UtilizadorAlterado += SessaoAtual_UtilizadorAlterado;
            CurrentDate = DateTime.Now.ToString("dddd, d 'de' MMMM", new CultureInfo("pt-PT"));
            CurrentPage = pages["Dashboard"];
        }

        public ICommand NavigateCommand { get; private set; }
        public ICommand TerminarSessaoCommand { get; private set; }
        public string CurrentDate { get; private set; }

        public string NomeUtilizadorAtual
        {
            get
            {
                Utilizador utilizador = SessaoAtual.Utilizador;
                if (utilizador == null)
                {
                    return "Sem utilizador";
                }

                return string.IsNullOrWhiteSpace(utilizador.FuncionarioNome)
                    ? utilizador.NomeUtilizador
                    : utilizador.FuncionarioNome;
            }
        }

        public string DetalheUtilizadorAtual
        {
            get
            {
                Utilizador utilizador = SessaoAtual.Utilizador;
                if (utilizador == null || string.IsNullOrWhiteSpace(utilizador.FuncionarioNome))
                {
                    return "Sessão iniciada";
                }

                return "@" + utilizador.NomeUtilizador;
            }
        }

        public string InicialUtilizadorAtual
        {
            get
            {
                string nome = NomeUtilizadorAtual;
                return string.IsNullOrWhiteSpace(nome) ? "?" : nome.Substring(0, 1).ToUpperInvariant();
            }
        }

        public PageViewModel CurrentPage
        {
            get { return currentPage; }
            private set
            {
                currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged("PageTitle");
                OnPropertyChanged("PageSubtitle");
            }
        }

        public string PageTitle
        {
            get { return CurrentPage == null ? string.Empty : CurrentPage.Title; }
        }

        public string PageSubtitle
        {
            get { return CurrentPage == null ? string.Empty : CurrentPage.Subtitle; }
        }

        private void Navigate(object parameter)
        {
            string pageKey = parameter as string;
            PageViewModel page;
            if (pageKey != null && pages.TryGetValue(pageKey, out page))
            {
                DashboardViewModel dashboard = page as DashboardViewModel;
                if (dashboard != null)
                {
                    dashboard.Atualizar();
                }

                CurrentPage = page;
            }
        }

        private void TerminarSessao(object parameter)
        {
            EventHandler handler = TerminarSessaoPedido;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void SessaoAtual_UtilizadorAlterado(object sender, EventArgs e)
        {
            OnPropertyChanged("NomeUtilizadorAtual");
            OnPropertyChanged("DetalheUtilizadorAtual");
            OnPropertyChanged("InicialUtilizadorAtual");
        }

        public void Dispose()
        {
            SessaoAtual.UtilizadorAlterado -= SessaoAtual_UtilizadorAlterado;
        }
    }
}
