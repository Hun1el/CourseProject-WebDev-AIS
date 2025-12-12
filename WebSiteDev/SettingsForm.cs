using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSiteDev
{
    /// <summary>
    /// Форма настроек подключения к базе данных
    /// Позволяет указать хост, пользователя, пароль и имя БД
    /// </summary>
    public partial class SettingsForm : Form
    {
        /// <summary>
        /// Инициализирует форму и загружает сохранённые настройки
        /// </summary>
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        /// <summary>
        /// Загружает настройки подключения из application settings
        /// </summary>
        private void LoadSettings()
        {
            textBox1.Text = Properties.Settings.Default.DbHost;
            textBox2.Text = Properties.Settings.Default.DbUser;
            textBox3.Text = Properties.Settings.Default.DbPassword;
            textBox4.Text = Properties.Settings.Default.DbName;
        }

        /// <summary>
        /// Кнопка "Отмена" - закрывает форму без сохранения
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти? Несохранённые изменения не будут применены.", "Выход из настроек", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Кнопка "Сохранить" - сохраняет все параметры подключения
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверяем что все обязательные поля заполнены
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Сохраняем настройки в application settings
            Properties.Settings.Default.DbHost = textBox1.Text;
            Properties.Settings.Default.DbUser = textBox2.Text;
            Properties.Settings.Default.DbPassword = textBox3.Text;
            Properties.Settings.Default.DbName = textBox4.Text;
            Properties.Settings.Default.Save();

            MessageBox.Show("Все настройки сохранены!", "Сохранение настроек", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        /// <summary>
        /// Переключает видимость пароля при нажатии на иконку глаза
        /// </summary>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Если пароль скрыт - показываем его и меняем иконку
            if (textBox3.UseSystemPasswordChar)
            {
                textBox3.UseSystemPasswordChar = false;
                pictureBox1.BackgroundImage = Properties.Resources.EyeHide;
            }
            else
            {
                // Иначе скрываем пароль обратно
                textBox3.UseSystemPasswordChar = true;
                pictureBox1.BackgroundImage = Properties.Resources.EyeView;
            }
        }

        /// <summary>
        /// Кнопка "Тест подключения" - проверяет подключение к БД с введёнными параметрами
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            string host = textBox1.Text;
            string user = textBox2.Text;
            string password = textBox3.Text;
            string dbname = textBox4.Text;

            // Проверяем что обязательные поля заполнены
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(dbname))
            {
                MessageBox.Show("Заполните обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Формируем строку подключения
            string connStr = $"host={host};database={dbname};uid={user};pwd={password};";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    // Пытаемся подключиться к БД
                    conn.Open();
                    MessageBox.Show("Подключение успешно!", "Проверка подключения", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (MySqlException mySqlEx)
                {
                    // Обработка ошибок MySQL
                    HandleDatabaseError(mySqlEx);
                }
                catch (Exception ex)
                {
                    // Обработка неожиданных ошибок
                    MessageBox.Show("Неожиданная ошибка:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Обрабатывает ошибки БД и выводит понятные сообщения
        /// </summary>
        private void HandleDatabaseError(MySqlException ex)
        {
            string errorMessage = "";

            // Определяем тип ошибки по коду и выводим подходящее сообщение
            switch (ex.Number)
            {
                case 0:
                    errorMessage = "Не удаётся подключиться к серверу базы данных.\n\nПроверьте:\n• Адрес хоста (может быть localhost или IP)\n• Доступность сервера";
                    break;

                case 1045:
                    errorMessage = "Ошибка доступа отклонена!\n\nПроверьте:\n• Имя пользователя\n• Пароль";
                    break;

                case 1049:
                    errorMessage = "База данных не найдена!\n\nПроверьте имя базы данных.";
                    break;

                case 2003:
                    errorMessage = "Не удаётся подключиться к MySQL серверу.\n\nПроверьте:\n• IP адрес хоста\n• Работает ли сервер MySQL";
                    break;

                case 2006:
                    errorMessage = "MySQL сервер отключен или потеряна связь.\n\nПожалуйста, проверьте состояние сервера.";
                    break;

                default:
                    // Для неизвестных ошибок выводим код и описание
                    errorMessage = $"Ошибка базы данных (код: {ex.Number}):\n{ex.Message}";
                    break;
            }

            MessageBox.Show(errorMessage, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            Inactivity.OnFormLoad(this);
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Inactivity.OnFormClosing(this);
        }
    }
}