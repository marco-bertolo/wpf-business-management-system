using PapJoaoSimoes.Models;
using PapJoaoSimoes.Services;
using System;
using System.Linq;

namespace PapJoaoSimoes.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AutenticacaoService autenticacaoService;
        private string nomeUtilizador;
        private string mensagem;
        private bool mensagemEhErro;
        private bool modoCriarAdministrador;
        private bool baseDadosDisponivel;

        public LoginViewModel()
        {
            autenticacaoService = new AutenticacaoService();
            Inicializar();
        }

        public string NomeUtilizador
        {
            get { return nomeUtilizador; }
            set
            {
                nomeUtilizador = value;
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

        public bool ModoCriarAdministrador
        {
            get { return modoCriarAdministrador; }
            private set
            {
                modoCriarAdministrador = value;
                OnPropertyChanged();
                OnPropertyChanged("Titulo");
                OnPropertyChanged("Subtitulo");
                OnPropertyChanged("TextoBotao");
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

        public string Titulo
        {
            get { return ModoCriarAdministrador ? "Criar administrador" : "Iniciar sessão"; }
        }

        public string Subtitulo
        {
            get
            {
                return ModoCriarAdministrador
                    ? "Configuração inicial do acesso"
                    : "Introduza os seus dados de acesso";
            }
        }

        public string TextoBotao
        {
            get { return ModoCriarAdministrador ? "Criar e entrar" : "Entrar"; }
        }

        public bool Submeter(string password, string confirmarPassword)
        {
            LimparMensagem();

            if (!BaseDadosDisponivel)
            {
                MostrarErro("Não existe ligação ao MySQL.");
                return false;
            }

            string erro = ValidarDados(password, confirmarPassword);
            if (erro != null)
            {
                MostrarErro(erro);
                return false;
            }

            try
            {
                Utilizador utilizador;

                if (ModoCriarAdministrador)
                {
                    utilizador = autenticacaoService.CriarAdministrador(NomeUtilizador, password);
                }
                else
                {
                    utilizador = autenticacaoService.Autenticar(NomeUtilizador, password);
                    if (utilizador == null)
                    {
                        MostrarErro("Nome de utilizador ou password incorretos.");
                        return false;
                    }
                }

                SessaoAtual.Atualizar(utilizador);
                return true;
            }
            catch (Exception)
            {
                MostrarErro("Não foi possível concluir o acesso. Tente novamente.");
                return false;
            }
        }

        private void Inicializar()
        {
            try
            {
                autenticacaoService.GarantirTabela();
                ModoCriarAdministrador = !autenticacaoService.ExisteUtilizador();
                BaseDadosDisponivel = true;
                Mensagem = string.Empty;
            }
            catch (Exception)
            {
                BaseDadosDisponivel = false;
                MostrarErro("Não foi possível ligar ao MySQL. Confirme se o servidor está ligado.");
            }
        }

        private string ValidarDados(string password, string confirmarPassword)
        {
            string utilizador = NomeUtilizador == null ? string.Empty : NomeUtilizador.Trim();

            if (utilizador.Length < 3 || utilizador.Length > 50)
            {
                return "O nome de utilizador deve ter entre 3 e 50 caracteres.";
            }

            if (!utilizador.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-'))
            {
                return "O nome de utilizador só pode conter letras, números, ponto, hífen e underscore.";
            }

            if (string.IsNullOrEmpty(password))
            {
                return "Introduza a password.";
            }

            if (ModoCriarAdministrador)
            {
                if (password.Length < 8 ||
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

        private void MostrarErro(string texto)
        {
            MensagemEhErro = true;
            Mensagem = texto;
        }

        private void LimparMensagem()
        {
            Mensagem = string.Empty;
            MensagemEhErro = false;
        }
    }
}
