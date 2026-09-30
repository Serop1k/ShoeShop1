using System.Windows;
using ShoeShop.Services;

namespace ShoeShop.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow() { InitializeComponent(); }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (AuthService.TryLogin(TxtLogin.Text, TxtPassword.Password, out var err))
                OpenMain();
            else
                TxtError.Text = err;
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            AuthService.LoginAsGuest();
            OpenMain();
        }

        private void OpenMain()
        {
            new MainWindow().Show();
            Close();
        }
    }
}