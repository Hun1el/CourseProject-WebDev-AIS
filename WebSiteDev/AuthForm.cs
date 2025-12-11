using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using WebSiteDev.ManagerForm;

namespace WebSiteDev
{
    /// <summary>
    /// Форма авторизации пользователей в систему
    /// Проверяет учётные данные против встроенного администратора или БД пользователей
    /// </summary>
    public partial class AuthForm : Form
    {
        /// <summary>
        /// Инициализирует форму авторизации и папку для изображений
        /// </summary>
        public AuthForm()
        {
            InitializeComponent();

            // Создаём папку для хранения изображений товаров если её нет
            FolderPermissions.InitializeImagesFolder();
        }

        /// <summary>
        /// Обработчик кнопки входа
        /// Проверяет учётные данные и открывает соответствующую форму
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем введённые логин и пароль
            string login = textBox1.Text.Trim();
            string password = textBox2.Text;
            string hashedPassword = GetSha256(password);

            // Проверяем, что оба поля заполнены
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем учётные данные встроенного администратора из настроек
            string adminLogin = Properties.Settings.Default.AdminLogin;
            string adminPassword = Properties.Settings.Default.AdminPassword;

            // Проверяем вход администратора
            if (login == adminLogin && password == adminPassword)
            {
                // Открываем главную форму администратора
                MainForm adminForm = new MainForm("Администратор", "Администратор", 0);
                this.Hide();
                adminForm.ShowDialog();
                this.Show();

                // Очищаем поля после выхода
                textBox1.Text = "";
                textBox2.Text = "";
                return;
            }

            // Проверяем учётные данные в БД для обычных пользователей
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // SQL запрос для получения данных пользователя
                    string query = @"SELECT u.UserID, u.FirstName, u.Surname, u.MiddleName, r.RoleName 
                             FROM Users u JOIN Role r ON u.RoleID = r.RoleID 
                             WHERE u.UserLogin = @login AND u.UserPassword = @password
                             LIMIT 1;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    // Параметризованный запрос для защиты от SQL инъекций
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        // Если пользователь найден
                        if (reader.Read())
                        {
                            int userID = Convert.ToInt32(reader["UserID"]);
                            // Формируем полное имя пользователя
                            string fullName = $"{reader["Surname"]} {reader["FirstName"]} {reader["MiddleName"]}";
                            string role = reader["RoleName"].ToString();

                            // Открываем нужную форму в зависимости от роли пользователя
                            if (role == "Администратор")
                            {
                                MainForm adminForm = new MainForm(fullName, role, userID);
                                this.Hide();
                                adminForm.ShowDialog();
                                this.Show();
                            }
                            else if (role == "Менеджер")
                            {
                                ManagerMainForm managerForm = new ManagerMainForm(fullName, role, userID);
                                this.Hide();
                                managerForm.ShowDialog();
                                this.Show();
                            }
                            else if (role == "Директор")
                            {
                                DirectorMainForm directorForm = new DirectorMainForm(fullName, role);
                                this.Hide();
                                directorForm.ShowDialog();
                                this.Show();
                            }

                            // Очищаем поля после успешного входа
                            textBox1.Text = "";
                            textBox2.Text = "";
                        }
                        else
                        {
                            // Пользователь не найден или неверные учётные данные
                            MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textBox1.Text = "";
                            textBox2.Text = "";
                        }
                    }
                }
                catch (MySqlException Ex)
                {
                    // Обработка ошибок базы данных
                    HandleDatabaseError(Ex);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
                catch (Exception ex)
                {
                    // Обработка неожиданных ошибок
                    MessageBox.Show("Неожиданная ошибка:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
        }

        /// <summary>
        /// Обрабатывает ошибки подключения к БД и выводит понятные сообщения
        /// </summary>
        private void HandleDatabaseError(MySqlException ex)
        {
            string errorMessage = "";

            // Определяем тип ошибки по коду и выводим соответствующее сообщение
            switch (ex.Number)
            {
                case 0:
                    errorMessage = "Не удаётся подключиться к серверу базы данных.\n\nПроверьте:\n• Адрес хоста\n• Доступность сервера";
                    break;

                case 1045:
                    errorMessage = "Ошибка доступа отклонена!\n\nПроверьте:\n• Имя пользователя\n• Пароль";
                    break;

                case 1049:
                    errorMessage = "База данных не найдена!\n\nПроверьте имя базы данных в настройках.";
                    break;

                case 2003:
                    errorMessage = "Не удаётся подключиться к MySQL серверу.\n\nПроверьте:\n• IP адрес хоста\n• Работает ли сервер MySQL";
                    break;

                case 2006:
                    errorMessage = "MySQL сервер отключен.\n\nПожалуйста, проверьте состояние сервера.";
                    break;

                default:
                    // Для неизвестных ошибок выводим код и описание
                    errorMessage = $"Ошибка базы данных (код: {ex.Number}):\n{ex.Message}";
                    break;
            }

            MessageBox.Show(errorMessage, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Обработчик кнопки выхода из приложения
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Запрашиваем подтверждение у пользователя
            var result = MessageBox.Show("Вы действительно хотите выйти из приложения?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Если пользователь согласен - закрываем приложение
            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Переключает видимость пароля при нажатии на иконку глаза
        /// </summary>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // Если пароль скрыт - показываем его и меняем иконку
            if (textBox2.UseSystemPasswordChar)
            {
                textBox2.UseSystemPasswordChar = false;
                pictureBox2.BackgroundImage = Properties.Resources.EyeHide;
            }
            else
            {
                // Иначе скрываем пароль обратно
                textBox2.UseSystemPasswordChar = true;
                pictureBox2.BackgroundImage = Properties.Resources.EyeView;
            }
        }

        /// <summary>
        /// Хеширует пароль алгоритмом SHA256
        /// </summary>
        private string GetSha256(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                // Преобразуем текст в массив байт
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                // Создаём хеш
                byte[] hash = sha.ComputeHash(bytes);
                // Конвертируем хеш в строку шестнадцатеричного формата
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Открывает форму настроек приложения
        /// </summary>
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            // Скрываем форму авторизации на время открытия настроек
            this.Visible = false;
            settingsForm.ShowDialog();
            // Показываем форму авторизации обратно
            this.Visible = true;
        }

        /// <summary>
        /// Ограничивает ввод в поле логина только английским буквам, цифрам и спецсимволам
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EnglishDigitsAndSpecial(e);
        }

        /// <summary>
        /// Ограничивает ввод в поле пароля только английским буквам, цифрам и спецсимволам
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EnglishDigitsAndSpecial(e);
        }
    }
}