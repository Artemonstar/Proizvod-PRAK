using System.Windows;

namespace DeliveryServiceManager
{
    public partial class LoginWindow : Window
    {
        private DataService dataService = new DataService();

        public LoginWindow()
        {
            InitializeComponent();
            txtLogin.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Введите логин и пароль";
                return;
            }

            var user = dataService.Authenticate(login, password);

            if (user != null)
            {
                // Открываем главное окно и передаем роль
                MainWindow mainWindow = new MainWindow(user.Role);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Неверный логин или пароль";
            }
        }
    }
}