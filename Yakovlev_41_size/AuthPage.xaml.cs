using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Yakovlev_41_size
{
    public partial class AuthPage : Page
    {
        private string currentCaptcha;
        private int failedAttempts;

        public AuthPage()
        {
            InitializeComponent();
            CaptchaPanel.Visibility = Visibility.Collapsed;
        }

        private void GenerateCaptcha()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789qwertyuioplkjhgfdsazxcvbnm";
            currentCaptcha = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            captchaOneWord.Text = currentCaptcha[0].ToString();
            captchaTwoWord.Text = currentCaptcha[1].ToString();
            captchaThreeWord.Text = currentCaptcha[2].ToString();
            captchaFourWord.Text = currentCaptcha[3].ToString();

            CaptchaPanel.Visibility = Visibility.Visible;
            CaptchaTB.Text = "";
        }
        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;
            string password = PassTB.Password; // Используем PasswordBox

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            // Проверяем капчу после первой неудачной попытки
            if (failedAttempts > 0)
            {
                if (CaptchaTB.Text != currentCaptcha)
                {
                    MessageBox.Show("Неверная капча");
                    LoginBtn.IsEnabled = false;
                    await Task.Delay(10000);
                    LoginBtn.IsEnabled = true;
                    GenerateCaptcha();
                    return;
                }
            }

            // Проверка пользователя в БД
            User user = Yakovlev41Entities.GetContext().User
                .FirstOrDefault(u => u.UserLogin == login && u.UserPassword == password);

            if (user != null)
            {
                MessageBox.Show($"Добро пожаловать, {user.UserSurname} {user.UserName} {user.UserPatronymic}!");
                Manager.MainFrame.Navigate(new ProductPage(user));

                // Сброс состояния
                ResetAuthState();
            }
            else
            {
                failedAttempts++;
                MessageBox.Show("Неверный логин или пароль");

                if (failedAttempts == 1)
                {
                    GenerateCaptcha();
                }
                else if (failedAttempts >= 2)
                {
                    LoginBtn.IsEnabled = false;
                    await Task.Delay(10000);
                    LoginBtn.IsEnabled = true;
                    GenerateCaptcha();
                }
            }
        }

        private void ResetAuthState()
        {
            LoginTB.Text = "";
            PassTB.Password = "";
            CaptchaTB.Text = "";
            failedAttempts = 0;
            CaptchaPanel.Visibility = Visibility.Collapsed;
        }

        private void GuestLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var guest = new User
            {
                UserSurname = "Гость",
                UserName = "",
                UserPatronymic = "",
                UserRole = 1
            };
            MessageBox.Show("Вы вошли как гость");
            Manager.MainFrame.Navigate(new ProductPage(guest));
            ResetAuthState();
        }
    }
}