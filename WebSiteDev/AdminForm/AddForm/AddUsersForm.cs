using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WebSiteDev.AddForm
{
    /// <summary>
    /// Форма для добавления нового пользователя в систему
    /// Включает заполнение ФИО, логина, пароля, роли и номера телефона
    /// </summary>
    public partial class AddUsersForm : Form
    {
        private DataManipulation dataManipulation;
        static readonly Random rand = new Random();

        /// <summary>
        /// Инициализирует форму и заполняет комбо-бокс ролями
        /// </summary>
        public AddUsersForm(DataManipulation dm)
        {
            InitializeComponent();

            dataManipulation = dm;
            // Загружаем список ролей в выпадающий список
            dataManipulation.FillComboBoxWithRoles(comboBox1, "Выберите роль");
        }

        /// <summary>
        /// Обработчик кнопки закрытия формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Применяет форматирование первой буквы при вводе фамилии
        /// </summary>
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox2);
        }

        /// <summary>
        /// Применяет форматирование первой буквы при вводе имени
        /// </summary>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox3);
        }

        /// <summary>
        /// Применяет форматирование первой буквы при вводе отчества
        /// </summary>
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox4);
        }

        /// <summary>
        /// Ограничивает ввод только русскими буквами и дефисом в поле фамилии
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox2);
        }

        /// <summary>
        /// Ограничивает ввод только русскими буквами и дефисом в поле имени
        /// </summary>
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox3);
        }

        /// <summary>
        /// Ограничивает ввод только русскими буквами в поле отчества
        /// </summary>
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        /// <summary>
        /// Ограничивает ввод в поле логина только допустимыми символами для логина
        /// </summary>
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.LoginInput(e);
        }

        /// <summary>
        /// Ограничивает ввод в поле пароля только английскими буквами, цифрами и спецсимволами
        /// </summary>
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EnglishDigitsAndSpecial(e);
        }

        /// <summary>
        /// Обработчик кнопки добавления нового пользователя
        /// Проверяет все данные, проверяет уникальность и добавляет в БД
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Получаем все введённые данные
            string SurName = textBox2.Text.Trim();
            string FirstName = textBox3.Text.Trim();
            string MiddleName = textBox4.Text.Trim();
            string UserLogin = textBox5.Text.Trim();
            string UserPassword = textBox6.Text.Trim();
            int roleId = Convert.ToInt32(comboBox1.SelectedValue);
            string PhoneNumber = maskedTextBox1.Text.Trim();

            // Получаем зарезервированный логин администратора из настроек
            string adminLogin = Properties.Settings.Default.AdminLogin;

            // Проверяем что логин не совпадает с логином администратора
            if (!string.IsNullOrWhiteSpace(adminLogin) && string.Equals(UserLogin, adminLogin, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"Логин \"{adminLogin}\" зарезервирован для встроенного администратора.\nВыберите другой логин.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Хешируем пароль перед сохранением в БД
            string hashedPassword = GetSha256(UserPassword);

            // Проверяем корректность номера телефона
            if (!maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Введите корректный номер телефона!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем что все обязательные поля заполнены
            if (SurName == "" || FirstName == "" || UserLogin == "" || UserPassword == "" || comboBox1.SelectedIndex <= 0 || PhoneNumber == "")
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();

                    // Проверяем что пользователь с таким логином не существует
                    string checkQuery = "SELECT COUNT(*) FROM `Users` WHERE UserLogin = '" + UserLogin + "'";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Проверяем что пользователь с таким номером телефона не существует
                    string checkPhoneQuery = "SELECT COUNT(*) FROM `Users` WHERE PhoneNumber = '" + PhoneNumber + "'";
                    using (MySqlCommand checkPhoneCmd = new MySqlCommand(checkPhoneQuery, con))
                    {
                        int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                        if (phoneCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким номером телефона уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Добавляем нового пользователя в таблицу
                    string insertQuery = "INSERT INTO Users (Surname, FirstName, MiddleName, UserLogin, UserPassword, RoleID, PhoneNumber) " +
                                         "VALUES ('" + SurName + "', '" + FirstName + "', '" + MiddleName + "', '" + UserLogin + "', '" + hashedPassword + "', " + roleId + ", '" + PhoneNumber + "')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.ExecuteNonQuery();
                    }

                    // Показываем сообщение об успехе и очищаем все поля
                    MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    textBox5.Clear();
                    textBox6.Clear();
                    maskedTextBox1.Clear();
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с БД
                MessageBox.Show("Ошибка при добавлении пользователя:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// Переключает видимость пароля при нажатии на иконку глаза
        /// </summary>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            // Если пароль скрыт - показываем его и меняем иконку
            if (textBox6.UseSystemPasswordChar)
            {
                textBox6.UseSystemPasswordChar = false;
                pictureBox2.BackgroundImage = Properties.Resources.EyeHide;
            }
            else
            {
                // Иначе скрываем пароль обратно
                textBox6.UseSystemPasswordChar = true;
                pictureBox2.BackgroundImage = Properties.Resources.EyeView;
            }
        }

        /// <summary>
        /// Перемешивает символы строки в случайном порядке
        /// Используется для генерации пароля
        /// </summary>
        static string Shuffle(string str)
        {
            var chars = str.ToCharArray();
            // Проходим по всем символам с конца и случайно переставляем их
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars);
        }

        /// <summary>
        /// Обработчик кнопки генерации случайного пароля
        /// Создаёт пароль из 12 символов с буквами, цифрами и спецсимволами
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            // Строки с символами для каждой категории
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            // Объединяем все символы
            string allChars = upper + lower + numbers + special;
            Random random = new Random();
            StringBuilder password = new StringBuilder();

            // Обеспечиваем минимум каждого типа символов
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(numbers[random.Next(numbers.Length)]);
            password.Append(upper[random.Next(upper.Length)]);

            // Добавляем оставшиеся символы случайно (всего 12 символов)
            for (int i = 4; i < 12; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Перемешиваем пароль и выводим его в поле
            string shufflepass = Shuffle(Convert.ToString(password));
            textBox6.Text = shufflepass;
        }
    }
}