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
    public class FuncionariosViewModel : PageViewModel
    {
        private string textoPesquisa;
        private string estadoSelecionado;
        private Funcionario funcionarioSelecionado;
        private Funcionario funcionarioEmEdicao;
        private string formTitle;
        private string mensagemFuncionario;
        private bool mensagemEhErro;
        private readonly FuncionarioService funcionarioService;
        private readonly DialogService dialogService;

        public FuncionariosViewModel()
            : base("Funcionários", "Equipa e responsabilidades")
        {
            Funcionarios = new ObservableCollection<Funcionario>();
            EstadosFiltro = new ObservableCollection<string>
            {
                "Todos",
                "Ativos",
                "Inativos"
            };

            FuncionariosFiltrados = CollectionViewSource.GetDefaultView(Funcionarios);
            FuncionariosFiltrados.Filter = FiltrarFuncionario;

            NovoFuncionarioCommand = new RelayCommand(NovoFuncionario);
            LimparFormularioCommand = new RelayCommand(LimparFormulario);
            GuardarFuncionarioCommand = new RelayCommand(GuardarFuncionario);
            EditarFuncionarioCommand = new RelayCommand(EditarFuncionario);
            DesativarFuncionarioCommand = new RelayCommand(DesativarFuncionario);

            EstadoSelecionado = "Todos";
            PrepararNovoFuncionario();

            funcionarioService = new FuncionarioService();
            dialogService = new DialogService();
            CarregarFuncionarios();
        }

        public ObservableCollection<Funcionario> Funcionarios { get; private set; }
        public ObservableCollection<string> EstadosFiltro { get; private set; }
        public ICollectionView FuncionariosFiltrados { get; private set; }

        public ICommand NovoFuncionarioCommand { get; private set; }
        public ICommand LimparFormularioCommand { get; private set; }
        public ICommand GuardarFuncionarioCommand { get; private set; }
        public ICommand EditarFuncionarioCommand { get; private set; }
        public ICommand DesativarFuncionarioCommand { get; private set; }

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

        public Funcionario FuncionarioSelecionado
        {
            get { return funcionarioSelecionado; }
            set
            {
                funcionarioSelecionado = value;
                OnPropertyChanged();
            }
        }

        public Funcionario FuncionarioEmEdicao
        {
            get { return funcionarioEmEdicao; }
            set
            {
                funcionarioEmEdicao = value;
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

        public string MensagemFuncionario
        {
            get { return mensagemFuncionario; }
            set
            {
                mensagemFuncionario = value;
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

        public string TotalFuncionarios
        {
            get
            {
                int total = FuncionariosFiltrados.Cast<object>().Count();
                return total == 1 ? "1 funcionário registado" : total + " funcionários registados";
            }
        }

        private bool FiltrarFuncionario(object item)
        {
            Funcionario funcionario = item as Funcionario;
            if (funcionario == null)
            {
                return false;
            }

            bool correspondeAoEstado =
                EstadoSelecionado == "Todos" ||
                EstadoSelecionado == "Ativos" && funcionario.Ativo ||
                EstadoSelecionado == "Inativos" && !funcionario.Ativo;

            string pesquisa = TextoPesquisa == null ? string.Empty : TextoPesquisa.Trim();
            bool correspondeAPesquisa =
                pesquisa.Length == 0 ||
                Contem(funcionario.Nome, pesquisa) ||
                Contem(funcionario.Nif, pesquisa) ||
                Contem(funcionario.Telemovel, pesquisa) ||
                Contem(funcionario.Informacoes, pesquisa);

            return correspondeAoEstado && correspondeAPesquisa;
        }

        private static bool Contem(string valor, string pesquisa)
        {
            return !string.IsNullOrEmpty(valor) &&
                   valor.IndexOf(pesquisa, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void AtualizarFiltro()
        {
            if (FuncionariosFiltrados != null)
            {
                FuncionariosFiltrados.Refresh();
                OnPropertyChanged("TotalFuncionarios");
            }
        }

        private void NovoFuncionario(object parameter)
        {
            PrepararNovoFuncionario();
        }

        private void LimparFormulario(object parameter)
        {
            PrepararNovoFuncionario();
        }

        private void PrepararNovoFuncionario()
        {
            FuncionarioSelecionado = null;
            FuncionarioEmEdicao = new Funcionario { Ativo = true };
            FormTitle = "Novo funcionário";
            LimparMensagem();
        }

        private void EditarFuncionario(object parameter)
        {
            Funcionario funcionario = parameter as Funcionario;
            if (funcionario == null)
            {
                return;
            }

            FuncionarioSelecionado = funcionario;
            LimparMensagem();
            FuncionarioEmEdicao = new Funcionario
            {
                FuncionarioId = funcionario.FuncionarioId,
                Nome = funcionario.Nome,
                Nif = funcionario.Nif,
                Morada = funcionario.Morada,
                Telemovel = funcionario.Telemovel,
                Email = funcionario.Email,
                Informacoes = funcionario.Informacoes,
                Ativo = funcionario.Ativo
            };
            FormTitle = "Editar funcionário";
        }

        private bool CarregarFuncionarios()
        {
            try
            {
                Funcionario[] funcionarios = funcionarioService.ObterTodos().ToArray();
                Funcionarios.Clear();

                foreach (Funcionario funcionario in funcionarios)
                {
                    Funcionarios.Add(funcionario);
                }

                AtualizarFiltro();
                return true;
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível carregar os funcionários. Confirma se o MySQL está ligado.", true);
                return false;
            }
        }

        private void GuardarFuncionario(object parameter)
        {
            string erroValidacao = ValidarFuncionario();
            if (erroValidacao != null)
            {
                MostrarMensagem(erroValidacao, true);
                return;
            }

            NormalizarFuncionario();

            try
            {
                if (funcionarioService.NifExiste(FuncionarioEmEdicao.Nif, FuncionarioEmEdicao.FuncionarioId))
                {
                    MostrarMensagem("Já existe um funcionário registado com este NIF.", true);
                    return;
                }

                bool novoFuncionario = FuncionarioEmEdicao.FuncionarioId == 0;

                if (novoFuncionario)
                {
                    funcionarioService.Inserir(FuncionarioEmEdicao);
                }
                else
                {
                    funcionarioService.Atualizar(FuncionarioEmEdicao);
                }

                if (CarregarFuncionarios())
                {
                    PrepararNovoFuncionario();
                    MostrarMensagem(novoFuncionario
                        ? "Funcionário registado com sucesso."
                        : "Alterações guardadas com sucesso.", false);
                }
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível guardar o funcionário. Tenta novamente.", true);
            }
        }

        private void DesativarFuncionario(object parameter)
        {
            Funcionario funcionario = parameter as Funcionario;

            if (funcionario == null)
            {
                return;
            }

            if (!funcionario.Ativo || !dialogService.ConfirmarDesativacao("funcionário", funcionario.Nome))
            {
                return;
            }

            try
            {
                funcionarioService.Desativar(funcionario.FuncionarioId);
                if (!CarregarFuncionarios())
                {
                    return;
                }

                if (FuncionarioEmEdicao != null && FuncionarioEmEdicao.FuncionarioId == funcionario.FuncionarioId)
                {
                    PrepararNovoFuncionario();
                }

                MostrarMensagem("Funcionário desativado com sucesso.", false);
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível desativar o funcionário. Tenta novamente.", true);
            }
        }

        private string ValidarFuncionario()
        {
            if (FuncionarioEmEdicao == null || string.IsNullOrWhiteSpace(FuncionarioEmEdicao.Nome))
            {
                return "Preenche o nome do funcionário.";
            }

            if (FuncionarioEmEdicao.Nome.Trim().Length < 2)
            {
                return "O nome deve ter pelo menos 2 caracteres.";
            }

            string nif = FuncionarioEmEdicao.Nif == null ? string.Empty : FuncionarioEmEdicao.Nif.Trim();
            if (!Regex.IsMatch(nif, "^[0-9]{9}$"))
            {
                return "O NIF deve ter exatamente 9 algarismos.";
            }

            string telemovel = FuncionarioEmEdicao.Telemovel == null
                ? string.Empty
                : FuncionarioEmEdicao.Telemovel.Trim();

            if (telemovel.Length > 0)
            {
                int totalAlgarismos = telemovel.Count(char.IsDigit);
                if (totalAlgarismos < 9 || totalAlgarismos > 15)
                {
                    return "O telemóvel deve ter entre 9 e 15 algarismos.";
                }
            }

            string email = FuncionarioEmEdicao.Email == null ? string.Empty : FuncionarioEmEdicao.Email.Trim();
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

        private void NormalizarFuncionario()
        {
            FuncionarioEmEdicao.Nome = FuncionarioEmEdicao.Nome.Trim();
            FuncionarioEmEdicao.Nif = FuncionarioEmEdicao.Nif.Trim();
            FuncionarioEmEdicao.Morada = NormalizarTextoOpcional(FuncionarioEmEdicao.Morada);
            FuncionarioEmEdicao.Telemovel = NormalizarTextoOpcional(FuncionarioEmEdicao.Telemovel);
            FuncionarioEmEdicao.Email = NormalizarTextoOpcional(FuncionarioEmEdicao.Email);
            FuncionarioEmEdicao.Informacoes = NormalizarTextoOpcional(FuncionarioEmEdicao.Informacoes);
        }

        private static string NormalizarTextoOpcional(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private void MostrarMensagem(string mensagem, bool ehErro)
        {
            MensagemEhErro = ehErro;
            MensagemFuncionario = mensagem;
        }

        private void LimparMensagem()
        {
            MensagemFuncionario = string.Empty;
            MensagemEhErro = false;
        }
    }
}




