using PapJoaoSimoes.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PapJoaoSimoes
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NomeUtilizadorTextBox.Focus();
            Keyboard.Focus(NomeUtilizadorTextBox);
        }

        private void EntrarButton_Click(object sender, RoutedEventArgs e)
        {
            LoginViewModel viewModel = DataContext as LoginViewModel;
            if (viewModel == null)
            {
                return;
            }

            if (viewModel.Submeter(PasswordInput.Password, ConfirmarPasswordInput.Password))
            {
                DialogResult = true;
                Close();
                return;
            }

            PasswordInput.Clear();
            ConfirmarPasswordInput.Clear();
            PasswordInput.Focus();
        }
    }
}
