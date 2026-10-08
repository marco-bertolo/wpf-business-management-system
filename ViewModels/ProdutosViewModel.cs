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
    public class ProdutosViewModel : PageViewModel
    {
        private readonly ProdutoService produtoService;
        private readonly FornecedorService fornecedorService;
        private readonly DialogService dialogService;
        private List<Fornecedor> todosFornecedores;
        private string textoPesquisa;
        private string estadoSelecionado;
        private Produto produtoSelecionado;
        private Produto produtoEmEdicao;
        private string formTitle;
        private string mensagemProduto;
        private bool mensagemEhErro;

        public ProdutosViewModel()
            : base("Produtos", "Catálogo e controlo de stock")
        {
            produtoService = new ProdutoService();
            fornecedorService = new FornecedorService();
            dialogService = new DialogService();

            Produtos = new ObservableCollection<Produto>();
            FornecedoresDisponiveis = new ObservableCollection<Fornecedor>();
            EstadosFiltro = new ObservableCollection<string> { "Todos", "Ativos", "Inativos" };
            ProdutosFiltrados = CollectionViewSource.GetDefaultView(Produtos);
            ProdutosFiltrados.Filter = FiltrarProduto;

            NovoProdutoCommand = new RelayCommand(NovoProduto);
            LimparFormularioCommand = new RelayCommand(LimparFormulario);
            GuardarProdutoCommand = new RelayCommand(GuardarProduto);
            EditarProdutoCommand = new RelayCommand(EditarProduto);
            DesativarProdutoCommand = new RelayCommand(DesativarProduto);

            EstadoSelecionado = "Todos";
            PrepararNovoProduto();
            CarregarDados();
        }

        public ObservableCollection<Produto> Produtos { get; private set; }
        public ObservableCollection<Fornecedor> FornecedoresDisponiveis { get; private set; }
        public ObservableCollection<string> EstadosFiltro { get; private set; }
        public ICollectionView ProdutosFiltrados { get; private set; }

        public ICommand NovoProdutoCommand { get; private set; }
        public ICommand LimparFormularioCommand { get; private set; }
        public ICommand GuardarProdutoCommand { get; private set; }
        public ICommand EditarProdutoCommand { get; private set; }
        public ICommand DesativarProdutoCommand { get; private set; }

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

        public string EstadoSelecionado
        {
            get { return estadoSelecionado; }
            set
            {
                estadoSelecionado = value;
                OnPropertyChanged();
                AtualizarFiltro();
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

        public Produto ProdutoEmEdicao
        {
            get { return produtoEmEdicao; }
            set
            {
                produtoEmEdicao = value;
                OnPropertyChanged();
            }
        }

        public string FormTitle
        {
            get { return formTitle; }
            set
            {
                formTitle = value;
                OnPropertyChanged();
            }
        }

        public string MensagemProduto
        {
            get { return mensagemProduto; }
            set
            {
                mensagemProduto = value;
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

        public string TotalProdutos
        {
            get
            {
                int total = ProdutosFiltrados.Cast<object>().Count();
                return total == 1 ? "1 produto registado" : total + " produtos registados";
            }
        }

        private bool FiltrarProduto(object item)
        {
            Produto produto = item as Produto;
            if (produto == null)
            {
                return false;
            }

            bool correspondeAoEstado =
                EstadoSelecionado == "Todos" ||
                EstadoSelecionado == "Ativos" && produto.Ativo ||
                EstadoSelecionado == "Inativos" && !produto.Ativo;

            string pesquisa = TextoPesquisa == null ? string.Empty : TextoPesquisa.Trim();
            bool correspondeAPesquisa =
                pesquisa.Length == 0 ||
                Contem(produto.Descricao, pesquisa) ||
                Contem(produto.FornecedorNome, pesquisa) ||
                produto.ProdutoId.ToString().Contains(pesquisa);

            return correspondeAoEstado && correspondeAPesquisa;
        }

        private static bool Contem(string valor, string pesquisa)
        {
            return !string.IsNullOrEmpty(valor) &&
                   valor.IndexOf(pesquisa, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void AtualizarFiltro()
        {
            if (ProdutosFiltrados != null)
            {
                ProdutosFiltrados.Refresh();
                OnPropertyChanged("TotalProdutos");
            }
        }

        private bool CarregarDados()
        {
            try
            {
                List<Produto> produtos = produtoService.ObterTodos();
                todosFornecedores = fornecedorService.ObterTodos();

                Produtos.Clear();
                foreach (Produto produto in produtos)
                {
                    Produtos.Add(produto);
                }

                AtualizarFornecedoresDisponiveis();
                AtualizarFiltro();
                return true;
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível carregar os produtos. Confirma se o MySQL está ligado.", true);
                return false;
            }
        }

        private void AtualizarFornecedoresDisponiveis()
        {
            FornecedoresDisponiveis.Clear();

            if (todosFornecedores == null)
            {
                return;
            }

            foreach (Fornecedor fornecedor in todosFornecedores.Where(f => f.Ativo))
            {
                FornecedoresDisponiveis.Add(fornecedor);
            }
        }

        private void NovoProduto(object parameter)
        {
            PrepararNovoProduto();
        }

        private void LimparFormulario(object parameter)
        {
            PrepararNovoProduto();
        }

        private void PrepararNovoProduto()
        {
            ProdutoSelecionado = null;
            ProdutoEmEdicao = new Produto { Ativo = true };
            FormTitle = "Novo produto";
            LimparMensagem();
        }

        private void EditarProduto(object parameter)
        {
            Produto produto = parameter as Produto;
            if (produto == null)
            {
                return;
            }

            if (!FornecedoresDisponiveis.Any(f => f.FornecedorId == produto.FornecedorId) && todosFornecedores != null)
            {
                Fornecedor fornecedorAtual = todosFornecedores.FirstOrDefault(f => f.FornecedorId == produto.FornecedorId);
                if (fornecedorAtual != null)
                {
                    FornecedoresDisponiveis.Add(fornecedorAtual);
                }
            }

            ProdutoSelecionado = produto;
            ProdutoEmEdicao = new Produto
            {
                ProdutoId = produto.ProdutoId,
                Descricao = produto.Descricao,
                PrecoCompra = produto.PrecoCompra,
                PrecoVenda = produto.PrecoVenda,
                StockAtual = produto.StockAtual,
                FornecedorId = produto.FornecedorId,
                FornecedorNome = produto.FornecedorNome,
                Ativo = produto.Ativo
            };
            FormTitle = "Editar produto";
            LimparMensagem();
        }

        private void GuardarProduto(object parameter)
        {
            string erro = ValidarProduto();
            if (erro != null)
            {
                MostrarMensagem(erro, true);
                return;
            }

            ProdutoEmEdicao.Descricao = ProdutoEmEdicao.Descricao.Trim();

            try
            {
                bool novoProduto = ProdutoEmEdicao.ProdutoId == 0;
                if (novoProduto)
                {
                    produtoService.Inserir(ProdutoEmEdicao);
                }
                else
                {
                    produtoService.Atualizar(ProdutoEmEdicao);
                }

                if (CarregarDados())
                {
                    PrepararNovoProduto();
                    MostrarMensagem(novoProduto
                        ? "Produto registado com sucesso."
                        : "Alterações guardadas com sucesso.", false);
                }
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível guardar o produto. Tenta novamente.", true);
            }
        }

        private void DesativarProduto(object parameter)
        {
            Produto produto = parameter as Produto;
            if (produto == null || !produto.Ativo ||
                !dialogService.ConfirmarDesativacao("produto", produto.Descricao))
            {
                return;
            }

            try
            {
                produtoService.Desativar(produto.ProdutoId);
                if (!CarregarDados())
                {
                    return;
                }

                if (ProdutoEmEdicao != null && ProdutoEmEdicao.ProdutoId == produto.ProdutoId)
                {
                    PrepararNovoProduto();
                }

                MostrarMensagem("Produto desativado com sucesso.", false);
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível desativar o produto. Tenta novamente.", true);
            }
        }

        private string ValidarProduto()
        {
            if (ProdutoEmEdicao == null || string.IsNullOrWhiteSpace(ProdutoEmEdicao.Descricao))
            {
                return "Preenche a descrição do produto.";
            }

            if (ProdutoEmEdicao.Descricao.Trim().Length < 2)
            {
                return "A descrição deve ter pelo menos 2 caracteres.";
            }

            if (ProdutoEmEdicao.FornecedorId <= 0)
            {
                return "Seleciona o fornecedor do produto.";
            }

            if (ProdutoEmEdicao.PrecoCompra < 0 || ProdutoEmEdicao.PrecoVenda < 0)
            {
                return "Os preços não podem ser negativos.";
            }

            if (ProdutoEmEdicao.PrecoVenda < ProdutoEmEdicao.PrecoCompra)
            {
                return "O preço de venda não pode ser inferior ao preço de compra.";
            }

            if (ProdutoEmEdicao.StockAtual < 0)
            {
                return "O stock não pode ser negativo.";
            }

            return null;
        }

        private void MostrarMensagem(string mensagem, bool ehErro)
        {
            MensagemEhErro = ehErro;
            MensagemProduto = mensagem;
        }

        private void LimparMensagem()
        {
            MensagemProduto = string.Empty;
            MensagemEhErro = false;
        }
    }
}
