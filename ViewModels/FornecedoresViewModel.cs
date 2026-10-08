using PapJoaoSimoes.Models;
using PapJoaoSimoes.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;

namespace PapJoaoSimoes.ViewModels
{
    public class FornecedoresViewModel : PageViewModel
    {
        private string textoPesquisa;
        private string estadoSelecionado;
        private Fornecedor fornecedorSelecionado;
        private Fornecedor fornecedorEmEdicao;
        private string formTitle;
        private string mensagemFornecedor;
        private bool mensagemEhErro;
        private readonly FornecedorService fornecedorService;
        private readonly DialogService dialogService;

        public FornecedoresViewModel()
            : base("Fornecedores", "Fornecedores de artigos de plástico")
        {
            Fornecedores = new ObservableCollection<Fornecedor>();
            EstadosFiltro = new ObservableCollection<string>
            {
                "Todos",
                "Ativos",
                "Inativos"
            };

            FornecedoresFiltrados = CollectionViewSource.GetDefaultView(Fornecedores);
            FornecedoresFiltrados.Filter = FiltrarFornecedor;

            NovoFornecedorCommand = new RelayCommand(NovoFornecedor);
            LimparFormularioCommand = new RelayCommand(LimparFormulario);
            GuardarFornecedorCommand = new RelayCommand(GuardarFornecedor);
            EditarFornecedorCommand = new RelayCommand(EditarFornecedor);
            DesativarFornecedorCommand = new RelayCommand(DesativarFornecedor);

            EstadoSelecionado = "Todos";
            PrepararNovoFornecedor();

            fornecedorService = new FornecedorService();
            dialogService = new DialogService();
            CarregarFornecedores();
        }

        public ObservableCollection<Fornecedor> Fornecedores { get; private set; }
        public ObservableCollection<string> EstadosFiltro { get; private set; }
        public ICollectionView FornecedoresFiltrados { get; private set; }

        public ICommand NovoFornecedorCommand { get; private set; }
        public ICommand LimparFormularioCommand { get; private set; }
        public ICommand GuardarFornecedorCommand { get; private set; }
        public ICommand EditarFornecedorCommand { get; private set; }
        public ICommand DesativarFornecedorCommand { get; private set; }

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

        public Fornecedor FornecedorSelecionado
        {
            get { return fornecedorSelecionado; }
            set
            {
                fornecedorSelecionado = value;
                OnPropertyChanged();
            }
        }

        public Fornecedor FornecedorEmEdicao
        {
            get { return fornecedorEmEdicao; }
            set
            {
                fornecedorEmEdicao = value;
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

        public string MensagemFornecedor
        {
            get { return mensagemFornecedor; }
            set
            {
                mensagemFornecedor = value;
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

        public string TotalFornecedores
        {
            get
            {
                int total = FornecedoresFiltrados.Cast<object>().Count();
                return total == 1 ? "1 fornecedor registado" : total + " fornecedores registados";
            }
        }

        private bool FiltrarFornecedor(object item)
        {
            Fornecedor fornecedor = item as Fornecedor;
            if (fornecedor == null)
            {
                return false;
            }

            bool correspondeAoEstado =
                EstadoSelecionado == "Todos" ||
                EstadoSelecionado == "Ativos" && fornecedor.Ativo ||
                EstadoSelecionado == "Inativos" && !fornecedor.Ativo;

            string pesquisa = TextoPesquisa == null ? string.Empty : TextoPesquisa.Trim();
            bool correspondeAPesquisa =
                pesquisa.Length == 0 ||
                Contem(fornecedor.Nome, pesquisa) ||
                Contem(fornecedor.Nif, pesquisa) ||
                Contem(fornecedor.Telemovel, pesquisa);

            return correspondeAoEstado && correspondeAPesquisa;
        }

        private static bool Contem(string valor, string pesquisa)
        {
            return !string.IsNullOrEmpty(valor) &&
                   valor.IndexOf(pesquisa, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void AtualizarFiltro()
        {
            if (FornecedoresFiltrados != null)
            {
                FornecedoresFiltrados.Refresh();
                OnPropertyChanged("TotalFornecedores");
            }
        }

        private void NovoFornecedor(object parameter)
        {
            PrepararNovoFornecedor();
        }

        private void LimparFormulario(object parameter)
        {
            PrepararNovoFornecedor();
        }

        private void PrepararNovoFornecedor()
        {
            FornecedorSelecionado = null;
            FornecedorEmEdicao = new Fornecedor { Ativo = true };
            FormTitle = "Novo fornecedor";
            LimparMensagem();
        }

        private void EditarFornecedor(object parameter)
        {
            Fornecedor fornecedor = parameter as Fornecedor;
            if (fornecedor == null)
            {
                return;
            }

            FornecedorSelecionado = fornecedor;
            LimparMensagem();
            FornecedorEmEdicao = new Fornecedor
            {
                FornecedorId = fornecedor.FornecedorId,
                Nome = fornecedor.Nome,
                Nif = fornecedor.Nif,
                Morada = fornecedor.Morada,
                Telemovel = fornecedor.Telemovel,
                Email = fornecedor.Email,
                Ativo = fornecedor.Ativo
            };
            FormTitle = "Editar fornecedor";
        }

        private bool CarregarFornecedores()
        {
            try
            {
                Fornecedor[] fornecedores = fornecedorService.ObterTodos().ToArray();
                Fornecedores.Clear();

                foreach (Fornecedor fornecedor in fornecedores)
                {
                    Fornecedores.Add(fornecedor);
                }

                AtualizarFiltro();
                return true;
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível carregar os fornecedores. Confirma se o MySQL está ligado.", true);
                return false;
            }
        }

        private void GuardarFornecedor(object parameter)
        {
            string erroValidacao = ValidarFornecedor();
            if (erroValidacao != null)
            {
                MostrarMensagem(erroValidacao, true);
                return;
            }

            NormalizarFornecedor();

            try
            {
                if (fornecedorService.NifExiste(FornecedorEmEdicao.Nif, FornecedorEmEdicao.FornecedorId))
                {
                    MostrarMensagem("Já existe um fornecedor registado com este NIF.", true);
                    return;
                }

                bool novoFornecedor = FornecedorEmEdicao.FornecedorId == 0;

                if (novoFornecedor)
                {
                    fornecedorService.Inserir(FornecedorEmEdicao);
                }
                else
                {
                    fornecedorService.Atualizar(FornecedorEmEdicao);
                }

                if (CarregarFornecedores())
                {
                    PrepararNovoFornecedor();
                    MostrarMensagem(novoFornecedor
                        ? "Fornecedor registado com sucesso."
                        : "Alterações guardadas com sucesso.", false);
                }
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível guardar o fornecedor. Tenta novamente.", true);
            }
        }

        private void DesativarFornecedor(object parameter)
        {
            Fornecedor fornecedor = parameter as Fornecedor;

            if (fornecedor == null)
            {
                return;
            }

            if (!fornecedor.Ativo || !dialogService.ConfirmarDesativacao("fornecedor", fornecedor.Nome))
            {
                return;
            }

            try
            {
                fornecedorService.Desativar(fornecedor.FornecedorId);
                if (!CarregarFornecedores())
                {
                    return;
                }

                if (FornecedorEmEdicao != null && FornecedorEmEdicao.FornecedorId == fornecedor.FornecedorId)
                {
                    PrepararNovoFornecedor();
                }

                MostrarMensagem("Fornecedor desativado com sucesso.", false);
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível desativar o fornecedor. Tenta novamente.", true);
            }
        }

        private string ValidarFornecedor()
        {
            if (FornecedorEmEdicao == null || string.IsNullOrWhiteSpace(FornecedorEmEdicao.Nome))
            {
                return "Preenche o nome do fornecedor.";
            }

            if (FornecedorEmEdicao.Nome.Trim().Length < 2)
            {
                return "O nome deve ter pelo menos 2 caracteres.";
            }

            string nif = FornecedorEmEdicao.Nif == null ? string.Empty : FornecedorEmEdicao.Nif.Trim();
            if (!Regex.IsMatch(nif, "^[0-9]{9}$"))
            {
                return "O NIF deve ter exatamente 9 algarismos.";
            }

            string telemovel = FornecedorEmEdicao.Telemovel == null
                ? string.Empty
                : FornecedorEmEdicao.Telemovel.Trim();

            if (telemovel.Length > 0)
            {
                int totalAlgarismos = telemovel.Count(char.IsDigit);
                if (totalAlgarismos < 9 || totalAlgarismos > 15)
                {
                    return "O telemóvel deve ter entre 9 e 15 algarismos.";
                }
            }

            string email = FornecedorEmEdicao.Email == null ? string.Empty : FornecedorEmEdicao.Email.Trim();
            if (email.Length > 0 && !EmailValido(email))
            {
                return "Introduz um endereço de email válido.";
            }

            return null;
        }

        private static bool EmailValido(string email)
        {
            try
            {
                MailAddress endereco = new MailAddress(email);
                return endereco.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void NormalizarFornecedor()
        {
            FornecedorEmEdicao.Nome = FornecedorEmEdicao.Nome.Trim();
            FornecedorEmEdicao.Nif = FornecedorEmEdicao.Nif.Trim();
            FornecedorEmEdicao.Morada = NormalizarTextoOpcional(FornecedorEmEdicao.Morada);
            FornecedorEmEdicao.Telemovel = NormalizarTextoOpcional(FornecedorEmEdicao.Telemovel);
            FornecedorEmEdicao.Email = NormalizarTextoOpcional(FornecedorEmEdicao.Email);
        }

        private static string NormalizarTextoOpcional(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private void MostrarMensagem(string mensagem, bool ehErro)
        {
            MensagemEhErro = ehErro;
            MensagemFornecedor = mensagem;
        }

        private void LimparMensagem()
        {
            MensagemFornecedor = string.Empty;
            MensagemEhErro = false;
        }
    }
}


