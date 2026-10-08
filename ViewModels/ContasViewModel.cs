using PapJoaoSimoes.Models;
using PapJoaoSimoes.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PapJoaoSimoes.ViewModels
{
    public class ContasViewModel : PageViewModel
    {
        private readonly AutenticacaoService autenticacaoService;
        private Utilizador contaSelecionada;
        private Utilizador contaEmEdicao;
        private string tituloFormulario;
        private string mensagem;
        private bool mensagemEhErro;

        public ContasViewModel()
            : base("Contas", "Administradores com acesso à aplicação")
        {
            autenticacaoService = new AutenticacaoService();
            Contas = new ObservableCollection<Utilizador>();
            NovaContaCommand = new RelayCommand(NovaConta);
            EditarContaCommand = new RelayCommand(EditarConta);

            PrepararNovaConta();
            CarregarContas();
        }

        public ObservableCollection<Utilizador> Contas { get; private set; }
        public ICommand NovaContaCommand { get; private set; }
        public ICommand EditarContaCommand { get; private set; }

        public Utilizador ContaSelecionada
        {
            get { return contaSelecionada; }
            set
            {
                contaSelecionada = value;
                OnPropertyChanged();
            }
        }

        public Utilizador ContaEmEdicao
        {
            get { return contaEmEdicao; }
            private set
            {
                contaEmEdicao = value;
                OnPropertyChanged();
                OnPropertyChanged("EstaAEditar");
                OnPropertyChanged("AjudaPassword");
                OnPropertyChanged("PodeAlterarEstado");
            }
        }

        public string TituloFormulario
        {
            get { return tituloFormulario; }
            private set
            {
                tituloFormulario = value;
                OnPropertyChanged();
            }
        }

        public string Mensagem
        {
            get { return mensagem; }
            private set
            {
                mensagem = value;
                OnPropertyChanged();
            }
        }

        public bool MensagemEhErro
        {
            get { return mensagemEhErro; }
            private set
            {
                mensagemEhErro = value;
                OnPropertyChanged();
            }
        }

        public bool EstaAEditar
        {
            get { return ContaEmEdicao != null && ContaEmEdicao.UtilizadorId > 0; }
        }

        public string AjudaPassword
        {
            get
            {
                return EstaAEditar
                    ? "Deixa em branco para manter a password atual."
                    : "Mínimo de 8 caracteres, com maiúscula, minúscula e número.";
            }
        }

        public bool PodeAlterarEstado
        {
            get
            {
                return ContaEmEdicao == null ||
                       ContaEmEdicao.UtilizadorId == 0 ||
                       !EhContaAtual(ContaEmEdicao.UtilizadorId);
            }
        }

        public string TotalContas
        {
            get
            {
                int total = Contas.Count;
                return total == 1 ? "1 conta administradora" : total + " contas administradoras";
            }
        }

        public bool Guardar(string password, string confirmarPassword)
        {
            LimparMensagem();

            string erro = Validar(password, confirmarPassword);
            if (erro != null)
            {
                MostrarMensagem(erro, true);
                return false;
            }

            ContaEmEdicao.NomeUtilizador = ContaEmEdicao.NomeUtilizador.Trim();

            try
            {
                if (autenticacaoService.NomeUtilizadorExiste(
                    ContaEmEdicao.NomeUtilizador,
                    ContaEmEdicao.UtilizadorId))
                {
                    MostrarMensagem("Já existe uma conta com esse nome de utilizador.", true);
                    return false;
                }

                bool criada = ContaEmEdicao.UtilizadorId == 0;
                if (criada)
                {
                    autenticacaoService.CriarAdministrador(ContaEmEdicao.NomeUtilizador, password);
                }
                else
                {
                    autenticacaoService.AtualizarAdministrador(ContaEmEdicao, password);
                    AtualizarSessaoSeNecessario();
                }

                CarregarContas();
                PrepararNovaConta(false);
                MostrarMensagem(criada
                    ? "Conta administradora criada com sucesso."
                    : "Conta atualizada com sucesso.", false);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensagem(ex.Message, true);
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível guardar a conta. Tenta novamente.", true);
            }

            return false;
        }

        public bool Eliminar(Utilizador conta)
        {
            if (conta == null)
            {
                return false;
            }

            if (EhContaAtual(conta.UtilizadorId))
            {
                MostrarMensagem("Não podes eliminar a conta que está com sessão iniciada.", true);
                return false;
            }

            try
            {
                autenticacaoService.EliminarAdministrador(conta.UtilizadorId);
                CarregarContas();

                if (ContaEmEdicao != null && ContaEmEdicao.UtilizadorId == conta.UtilizadorId)
                {
                    PrepararNovaConta(false);
                }

                MostrarMensagem("Conta eliminada com sucesso.", false);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensagem(ex.Message, true);
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível eliminar a conta. Tenta novamente.", true);
            }

            return false;
        }

        private void NovaConta(object parameter)
        {
            PrepararNovaConta();
        }

        private void EditarConta(object parameter)
        {
            Utilizador conta = parameter as Utilizador;
            if (conta == null)
            {
                return;
            }

            ContaSelecionada = conta;
            ContaEmEdicao = new Utilizador
            {
                UtilizadorId = conta.UtilizadorId,
                NomeUtilizador = conta.NomeUtilizador,
                FuncionarioId = conta.FuncionarioId,
                FuncionarioNome = conta.FuncionarioNome,
                UltimoAcesso = conta.UltimoAcesso,
                CriadoEm = conta.CriadoEm,
                Ativo = conta.Ativo
            };
            TituloFormulario = "Editar conta";
            LimparMensagem();
        }

        private void PrepararNovaConta(bool limparMensagem = true)
        {
            ContaSelecionada = null;
            ContaEmEdicao = new Utilizador { Ativo = true };
            TituloFormulario = "Nova conta";

            if (limparMensagem)
            {
                LimparMensagem();
            }
        }

        private void CarregarContas()
        {
            try
            {
                Utilizador[] contas = autenticacaoService.ObterUtilizadores().ToArray();
                Contas.Clear();

                foreach (Utilizador conta in contas)
                {
                    Contas.Add(conta);
                }

                OnPropertyChanged("TotalContas");
            }
            catch (Exception)
            {
                MostrarMensagem("Não foi possível carregar as contas. Confirma a ligação ao MySQL.", true);
            }
        }

        private string Validar(string password, string confirmarPassword)
        {
            string nome = ContaEmEdicao == null || ContaEmEdicao.NomeUtilizador == null
                ? string.Empty
                : ContaEmEdicao.NomeUtilizador.Trim();

            if (nome.Length < 3 || nome.Length > 50)
            {
                return "O nome de utilizador deve ter entre 3 e 50 caracteres.";
            }

            if (!nome.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-'))
            {
                return "O nome só pode conter letras, números, ponto, hífen e underscore.";
            }

            if (EhContaAtual(ContaEmEdicao.UtilizadorId) && !ContaEmEdicao.Ativo)
            {
                return "Não podes desativar a conta que está com sessão iniciada.";
            }

            bool passwordObrigatoria = ContaEmEdicao.UtilizadorId == 0;
            bool querAlterarPassword = !string.IsNullOrEmpty(password) || !string.IsNullOrEmpty(confirmarPassword);

            if (passwordObrigatoria || querAlterarPassword)
            {
                if (string.IsNullOrEmpty(password) ||
                    password.Length < 8 ||
                    !password.Any(char.IsUpper) ||
                    !password.Any(char.IsLower) ||
                    !password.Any(char.IsDigit))
                {
                    return "A password deve ter 8 caracteres, incluindo maiúscula, minúscula e número.";
                }

                if (password != confirmarPassword)
                {
                    return "As passwords não são iguais.";
                }
            }

            return null;
        }

        private void AtualizarSessaoSeNecessario()
        {
            if (!EhContaAtual(ContaEmEdicao.UtilizadorId))
            {
                return;
            }

            Utilizador atual = SessaoAtual.Utilizador;
            atual.NomeUtilizador = ContaEmEdicao.NomeUtilizador;
            atual.Ativo = ContaEmEdicao.Ativo;
            SessaoAtual.Atualizar(atual);
        }

        private static bool EhContaAtual(int utilizadorId)
        {
            return SessaoAtual.Utilizador != null &&
                   SessaoAtual.Utilizador.UtilizadorId == utilizadorId;
        }

        private void MostrarMensagem(string texto, bool erro)
        {
            MensagemEhErro = erro;
            Mensagem = texto;
        }

        private void LimparMensagem()
        {
            Mensagem = string.Empty;
            MensagemEhErro = false;
        }
    }
}
