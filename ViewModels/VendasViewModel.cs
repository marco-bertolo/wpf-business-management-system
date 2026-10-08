using PapJoaoSimoes.Models;
using PapJoaoSimoes.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace PapJoaoSimoes.ViewModels
{
    public class VendasViewModel : PageViewModel
    {
        private readonly VendaService vendaService;
        private readonly ClienteService clienteService;
        private readonly FuncionarioService funcionarioService;
        private readonly ProdutoService produtoService;
        private string textoPesquisa;
        private Cliente clienteSelecionado;
        private Funcionario funcionarioSelecionado;
        private Produto produtoSelecionado;
        private int quantidade;
        private string mensagemVenda;
        private bool mensagemEhErro;

        public VendasViewModel()
            : base("Vendas", "Consulta e registo das vendas")
        {
            vendaService = new VendaService();
            clienteService = new ClienteService();
            funcionarioService = new FuncionarioService();
            produtoService = new ProdutoService();

            Vendas = new ObservableCollection<Venda>();
            ClientesAtivos = new ObservableCollection<Cliente>();
            FuncionariosAtivos = new ObservableCollection<Funcionario>();
            ProdutosDisponiveis = new ObservableCollection<Produto>();
            LinhasVenda = new ObservableCollection<VendaLinha>();
            VendasFiltradas = CollectionViewSource.GetDefaultView(Vendas);
            VendasFiltradas.Filter = FiltrarVenda;

            NovaVendaCommand = new RelayCommand(NovaVenda);
            LimparVendaCommand = new RelayCommand(LimparVenda);
            AdicionarProdutoCommand = new RelayCommand(AdicionarProduto);
            RemoverLinhaCommand = new RelayCommand(RemoverLinha);
            RegistarVendaCommand = new RelayCommand(RegistarVenda);

            PrepararNovaVenda();
            CarregarDados();
        }

        public ObservableCollection<Venda> Vendas { get; private set; }
        public ObservableCollection<Cliente> ClientesAtivos { get; private set; }
        public ObservableCollection<Funcionario> FuncionariosAtivos { get; private set; }
        public ObservableCollection<Produto> ProdutosDisponiveis { get; private set; }
        public ObservableCollection<VendaLinha> LinhasVenda { get; private set; }
        public ICollectionView VendasFiltradas { get; private set; }

        public ICommand NovaVendaCommand { get; private set; }
        public ICommand LimparVendaCommand { get; private set; }
        public ICommand AdicionarProdutoCommand { get; private set; }
        public ICommand RemoverLinhaCommand { get; private set; }
        public ICommand RegistarVendaCommand { get; private set; }

        public string TextoPesquisa
        {
            get { return textoPesquisa; }
            set
            {
                textoPesquisa = value;
                OnPropertyChanged();
                AtualizarFiltro();
            }
        }

        public Cliente ClienteSelecionado
        {
            get { return clienteSelecionado; }
            set
            {
                clienteSelecionado = value;
                OnPropertyChanged();
            }
        }

        public Funcionario FuncionarioSelecionado
        {
            get { return funcionarioSelecionado; }
            set
            {
                funcionarioSelecionado = value;
                OnPropertyChanged();
            }
        }

        public Produto ProdutoSelecionado
        {
            get { return produtoSelecionado; }
            set
            {
                produtoSelecionado = value;
                OnPropertyChanged();
            }
        }

        public int Quantidade
        {
            get { return quantidade; }
            set
            {
                quantidade = value;
                OnPropertyChanged();
            }
        }

        public string MensagemVenda
        {
            get { return mensagemVenda; }
            set
            {
                mensagemVenda = value;
                OnPropertyChanged();
            }
        }

        public bool MensagemEhErro
        {
            get { return mensagemEhErro; }
            set
            {
                mensagemEhErro = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalVenda
        {
            get { return LinhasVenda.Sum(l => l.Subtotal); }
        }

        public string TotalVendas
        {
            get
            {
                int total = VendasFiltradas.Cast<object>().Count();
                return total == 1 ? "1 venda registada" : total + " vendas registadas";
            }
        }

        private bool FiltrarVenda(object item)
        {
            Venda venda = item as Venda;
            if (venda == null)
            {
                return false;
            }

            string pesquisa = TextoPesquisa == null ? string.Empty : TextoPesquisa.Trim();
            return pesquisa.Length == 0 ||
                   venda.VendaId.ToString().Contains(pesquisa) ||
                   Contem(venda.ClienteNome, pesquisa) ||
                   Contem(venda.FuncionarioNome, pesquisa) ||
                   venda.DataVenda.ToString("dd/MM/yyyy").Contains(pesquisa);
        }

        private static bool Contem(string valor, string pesquisa)
        {
            return !string.IsNullOrEmpty(valor) &&
                   valor.IndexOf(pesquisa, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void AtualizarFiltro()
        {
            if (VendasFiltradas != null)
            {
                VendasFiltradas.Refresh();
                OnPropertyChanged("TotalVendas");
            }
        }

        private bool CarregarDados()
        {
            try
            {
                List<Venda> vendas = vendaService.ObterTodas();
                List<Cliente> clientes = clienteService.ObterTodos();
                List<Funcionario> funcionarios = funcionarioService.ObterTodos();
                List<Produto> produtos = produtoService.ObterTodos();

                SubstituirColecao(Vendas, vendas);
                SubstituirColecao(ClientesAtivos, clientes.Where(c => c.Ativo));
                SubstituirColecao(FuncionariosAtivos, funcionarios.Where(f => f.Ativo));
                SubstituirColecao(ProdutosDisponiveis, produtos.Where(p => p.Ativo && p.StockAtual > 0));
                AtualizarFiltro();
                return true;
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível carregar as vendas. Confirma se o MySQL está ligado.", true);
                return false;
            }
        }

        private static void SubstituirColecao<T>(ObservableCollection<T> destino, IEnumerable<T> valores)
        {
            destino.Clear();
            foreach (T valor in valores)
            {
                destino.Add(valor);
            }
        }

        private void NovaVenda(object parameter)
        {
            PrepararNovaVenda();
        }

        private void LimparVenda(object parameter)
        {
            PrepararNovaVenda();
        }

        private void PrepararNovaVenda()
        {
            ClienteSelecionado = null;
            FuncionarioSelecionado = null;
            ProdutoSelecionado = null;
            Quantidade = 1;
            LinhasVenda.Clear();
            OnPropertyChanged("TotalVenda");
            LimparMensagem();
        }

        private void AdicionarProduto(object parameter)
        {
            if (ProdutoSelecionado == null)
            {
                MostrarMensagem("Seleciona um produto.", true);
                return;
            }

            if (Quantidade <= 0)
            {
                MostrarMensagem("A quantidade deve ser superior a zero.", true);
                return;
            }

            VendaLinha existente = LinhasVenda.FirstOrDefault(l => l.ProdutoId == ProdutoSelecionado.ProdutoId);
            int quantidadeTotal = Quantidade + (existente == null ? 0 : existente.Quantidade);

            if (quantidadeTotal > ProdutoSelecionado.StockAtual)
            {
                MostrarMensagem("Stock insuficiente. Disponível: " + ProdutoSelecionado.StockAtual + ".", true);
                return;
            }

            VendaLinha novaLinha = new VendaLinha
            {
                ProdutoId = ProdutoSelecionado.ProdutoId,
                ProdutoDescricao = ProdutoSelecionado.Descricao,
                Quantidade = quantidadeTotal,
                PrecoUnitario = ProdutoSelecionado.PrecoVenda,
                StockDisponivel = ProdutoSelecionado.StockAtual
            };

            if (existente == null)
            {
                LinhasVenda.Add(novaLinha);
            }
            else
            {
                int indice = LinhasVenda.IndexOf(existente);
                LinhasVenda[indice] = novaLinha;
            }

            ProdutoSelecionado = null;
            Quantidade = 1;
            OnPropertyChanged("TotalVenda");
            LimparMensagem();
        }

        private void RemoverLinha(object parameter)
        {
            VendaLinha linha = parameter as VendaLinha;
            if (linha != null)
            {
                LinhasVenda.Remove(linha);
                OnPropertyChanged("TotalVenda");
            }
        }

        private void RegistarVenda(object parameter)
        {
            if (ClienteSelecionado == null)
            {
                MostrarMensagem("Seleciona o cliente da venda.", true);
                return;
            }

            if (FuncionarioSelecionado == null)
            {
                MostrarMensagem("Seleciona o funcionário responsável.", true);
                return;
            }

            if (LinhasVenda.Count == 0)
            {
                MostrarMensagem("Adiciona pelo menos um produto à venda.", true);
                return;
            }

            Venda venda = new Venda
            {
                ClienteId = ClienteSelecionado.ClienteId,
                FuncionarioId = FuncionarioSelecionado.FuncionarioId
            };

            try
            {
                vendaService.Registar(venda, LinhasVenda.ToList());
                int vendaId = venda.VendaId;

                if (CarregarDados())
                {
                    PrepararNovaVenda();
                    MostrarMensagem("Venda #" + vendaId.ToString("D4") + " registada com sucesso.", false);
                }
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensagem(ex.Message, true);
                CarregarDados();
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível registar a venda. Tenta novamente.", true);
            }
        }

        private void MostrarMensagem(string mensagem, bool ehErro)
        {
            MensagemEhErro = ehErro;
            MensagemVenda = mensagem;
        }

        private void LimparMensagem()
        {
            MensagemVenda = string.Empty;
            MensagemEhErro = false;
        }
    }
}
