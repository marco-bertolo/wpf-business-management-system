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
    public class ClientesViewModel : PageViewModel
    {
        private string textoPesquisa;
        private string estadoSelecionado;
        private Cliente clienteSelecionado;
        private Cliente clienteEmEdicao;
        private string formTitle;
        private string mensagemCliente;
        private bool mensagemEhErro;
        private readonly ClienteService clienteService;
        private readonly DialogService dialogService;

        public ClientesViewModel()
            : base("Clientes", "Clientes e feirantes registados")
        {
            Clientes = new ObservableCollection<Cliente>();
            EstadosFiltro = new ObservableCollection<string>
            {
                "Todos",
                "Ativos",
                "Inativos"
            };

            ClientesFiltrados = CollectionViewSource.GetDefaultView(Clientes);
            ClientesFiltrados.Filter = FiltrarCliente;

            NovoClienteCommand = new RelayCommand(NovoCliente);
            LimparFormularioCommand = new RelayCommand(LimparFormulario);
            GuardarClienteCommand = new RelayCommand(GuardarCliente);
            EditarClienteCommand = new RelayCommand(EditarCliente);
            DesativarClienteCommand = new RelayCommand(DesativarCliente);

            EstadoSelecionado = "Todos";
            PrepararNovoCliente();

            clienteService = new ClienteService();
            dialogService = new DialogService();
            CarregarClientes();
        }

        public ObservableCollection<Cliente> Clientes { get; private set; }
        public ObservableCollection<string> EstadosFiltro { get; private set; }
        public ICollectionView ClientesFiltrados { get; private set; }

        public ICommand NovoClienteCommand { get; private set; }
        public ICommand LimparFormularioCommand { get; private set; }
        public ICommand GuardarClienteCommand { get; private set; }
        public ICommand EditarClienteCommand { get; private set; }
        public ICommand DesativarClienteCommand { get; private set; }

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

        public Cliente ClienteSelecionado
        {
            get { return clienteSelecionado; }
            set
            {
                clienteSelecionado = value;
                OnPropertyChanged();
            }
        }

        public Cliente ClienteEmEdicao
        {
            get { return clienteEmEdicao; }
            set
            {
                clienteEmEdicao = value;
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

        public string MensagemCliente
        {
            get { return mensagemCliente; }
            set
            {
                mensagemCliente = value;
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

        public string TotalClientes
        {
            get
            {
                int total = ClientesFiltrados.Cast<object>().Count();
                return total == 1 ? "1 cliente registado" : total + " clientes registados";
            }
        }

        private bool FiltrarCliente(object item)
        {
            Cliente cliente = item as Cliente;
            if (cliente == null)
            {
                return false;
            }

            bool correspondeAoEstado =
                EstadoSelecionado == "Todos" ||
                EstadoSelecionado == "Ativos" && cliente.Ativo ||
                EstadoSelecionado == "Inativos" && !cliente.Ativo;

            string pesquisa = TextoPesquisa == null ? string.Empty : TextoPesquisa.Trim();
            bool correspondeAPesquisa =
                pesquisa.Length == 0 ||
                Contem(cliente.Nome, pesquisa) ||
                Contem(cliente.Nif, pesquisa) ||
                Contem(cliente.Telemovel, pesquisa);

            return correspondeAoEstado && correspondeAPesquisa;
        }

        private static bool Contem(string valor, string pesquisa)
        {
            return !string.IsNullOrEmpty(valor) &&
                   valor.IndexOf(pesquisa, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void AtualizarFiltro()
        {
            if (ClientesFiltrados != null)
            {
                ClientesFiltrados.Refresh();
                OnPropertyChanged("TotalClientes");
            }
        }

        private void NovoCliente(object parameter)
        {
            PrepararNovoCliente();
        }

        private void LimparFormulario(object parameter)
        {
            PrepararNovoCliente();
        }

        private void PrepararNovoCliente()
        {
            ClienteSelecionado = null;
            ClienteEmEdicao = new Cliente { Ativo = true };
            FormTitle = "Novo cliente";
            LimparMensagem();
        }

        private void EditarCliente(object parameter)
        {
            Cliente cliente = parameter as Cliente;
            if (cliente == null)
            {
                return;
            }

            ClienteSelecionado = cliente;
            LimparMensagem();
            ClienteEmEdicao = new Cliente
            {
                ClienteId = cliente.ClienteId,
                Nome = cliente.Nome,
                Nif = cliente.Nif,
                Morada = cliente.Morada,
                Telemovel = cliente.Telemovel,
                Email = cliente.Email,
                Ativo = cliente.Ativo
            };
            FormTitle = "Editar cliente";
        }

        private bool CarregarClientes()
        {
            try
            {
                Cliente[] clientes = clienteService.ObterTodos().ToArray();
                Clientes.Clear();

                foreach (Cliente cliente in clientes)
                {
                    Clientes.Add(cliente);
                }

                AtualizarFiltro();
                return true;
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível carregar os clientes. Confirma se o MySQL está ligado.", true);
                return false;
            }
        }

        private void GuardarCliente(object parameter)
        {
            string erroValidacao = ValidarCliente();
            if (erroValidacao != null)
            {
                MostrarMensagem(erroValidacao, true);
                return;
            }

            NormalizarCliente();

            try
            {
                if (clienteService.NifExiste(ClienteEmEdicao.Nif, ClienteEmEdicao.ClienteId))
                {
                    MostrarMensagem("Já existe um cliente registado com este NIF.", true);
                    return;
                }

                bool novoCliente = ClienteEmEdicao.ClienteId == 0;

                if (novoCliente)
                {
                    clienteService.Inserir(ClienteEmEdicao);
                }
                else
                {
                    clienteService.Atualizar(ClienteEmEdicao);
                }

                if (CarregarClientes())
                {
                    PrepararNovoCliente();
                    MostrarMensagem(novoCliente
                        ? "Cliente registado com sucesso."
                        : "Alterações guardadas com sucesso.", false);
                }
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível guardar o cliente. Tenta novamente.", true);
            }
        }

        private void DesativarCliente(object parameter)
        {
            Cliente cliente = parameter as Cliente;

            if (cliente == null)
            {
                return;
            }

            if (!cliente.Ativo || !dialogService.ConfirmarDesativacao("cliente", cliente.Nome))
            {
                return;
            }

            try
            {
                clienteService.Desativar(cliente.ClienteId);
                if (!CarregarClientes())
                {
                    return;
                }

                if (ClienteEmEdicao != null && ClienteEmEdicao.ClienteId == cliente.ClienteId)
                {
                    PrepararNovoCliente();
                }

                MostrarMensagem("Cliente desativado com sucesso.", false);
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível desativar o cliente. Tenta novamente.", true);
            }
        }

        private string ValidarCliente()
        {
            if (ClienteEmEdicao == null || string.IsNullOrWhiteSpace(ClienteEmEdicao.Nome))
            {
                return "Preenche o nome do cliente.";
            }

            if (ClienteEmEdicao.Nome.Trim().Length < 2)
            {
                return "O nome deve ter pelo menos 2 caracteres.";
            }

            string nif = ClienteEmEdicao.Nif == null ? string.Empty : ClienteEmEdicao.Nif.Trim();
            if (!Regex.IsMatch(nif, "^[0-9]{9}$"))
            {
                return "O NIF deve ter exatamente 9 algarismos.";
            }

            string telemovel = ClienteEmEdicao.Telemovel == null
                ? string.Empty
                : ClienteEmEdicao.Telemovel.Trim();

            if (telemovel.Length > 0)
            {
                int totalAlgarismos = telemovel.Count(char.IsDigit);
                if (totalAlgarismos < 9 || totalAlgarismos > 15)
                {
                    return "O telemóvel deve ter entre 9 e 15 algarismos.";
                }
            }

            string email = ClienteEmEdicao.Email == null ? string.Empty : ClienteEmEdicao.Email.Trim();
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

        private void NormalizarCliente()
        {
            ClienteEmEdicao.Nome = ClienteEmEdicao.Nome.Trim();
            ClienteEmEdicao.Nif = ClienteEmEdicao.Nif.Trim();
            ClienteEmEdicao.Morada = NormalizarTextoOpcional(ClienteEmEdicao.Morada);
            ClienteEmEdicao.Telemovel = NormalizarTextoOpcional(ClienteEmEdicao.Telemovel);
            ClienteEmEdicao.Email = NormalizarTextoOpcional(ClienteEmEdicao.Email);
        }

        private static string NormalizarTextoOpcional(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private void MostrarMensagem(string mensagem, bool ehErro)
        {
            MensagemEhErro = ehErro;
            MensagemCliente = mensagem;
        }

        private void LimparMensagem()
        {
            MensagemCliente = string.Empty;
            MensagemEhErro = false;
        }
    }
}
