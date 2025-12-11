using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WebSiteDev.AddForm
{
    /// <summary>
    /// Форма для добавления новой роли пользователя в систему
    /// </summary>
    public partial class AddRoleForm : Form
    {
        /// <summary>
        /// Инициализирует форму добавления роли
        /// </summary>
        public AddRoleForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки закрытия формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Применяет форматирование первой буквы при вводе названия роли
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox1);
        }

        /// <summary>
        /// Ограничивает ввод только русскими буквами в поле названия роли
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        /// <summary>
        /// Обработчик кнопки добавления новой роли
        /// Проверяет введённые данные, проверяет уникальность и добавляет в БД
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Получаем и очищаем название роли от пробелов
            string roleName = textBox1.Text.Trim();

            // Проверяем что поле не пусто
            if (roleName == "")
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();

                    // Проверяем существует ли уже такая роль в базе
                    string checkQuery = "SELECT COUNT(*) FROM Role WHERE RoleName = '" + roleName + "'";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        // Если роль найдена - показываем ошибку
                        if (count > 0)
                        {
                            MessageBox.Show("Такая роль уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Добавляем новую роль в таблицу
                    string insertQuery = "INSERT INTO Role (RoleName) VALUES ('" + roleName + "')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.ExecuteNonQuery();
                    }

                    // Показываем сообщение об успехе и очищаем поле ввода
                    MessageBox.Show("Роль успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear();
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с БД
                MessageBox.Show("Ошибка при добавлении роли:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}