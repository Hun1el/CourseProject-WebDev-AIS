using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace WebSiteDev.AddForm
{
    /// <summary>
    /// Форма для добавления нового статуса заказа в систему
    /// </summary>
    public partial class AddStatusForm : Form
    {
        /// <summary>
        /// Инициализирует форму добавления статуса
        /// </summary>
        public AddStatusForm()
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
        /// Применяет форматирование первой буквы при вводе названия статуса
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox1);
        }

        /// <summary>
        /// Ограничивает ввод только русскими буквами в поле названия статуса
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        /// <summary>
        /// Обработчик кнопки добавления нового статуса
        /// Проверяет введённые данные, проверяет уникальность и добавляет в БД
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Получаем и очищаем название статуса от пробелов
            string statusName = textBox1.Text.Trim();

            // Проверяем что поле не пусто
            if (statusName == "")
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();

                    // Проверяем существует ли уже такой статус в базе
                    string checkQuery = "SELECT COUNT(*) FROM `Status` WHERE StatusName = '" + statusName + "'";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        // Если статус найден - показываем ошибку
                        if (count > 0)
                        {
                            MessageBox.Show("Такой статус уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Добавляем новый статус в таблицу
                    string insertQuery = "INSERT INTO `Status` (StatusName) VALUES ('" + statusName + "')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.ExecuteNonQuery();
                    }

                    // Показываем сообщение об успехе и очищаем поле ввода
                    MessageBox.Show("Статус успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear();
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с БД
                MessageBox.Show("Ошибка при добавлении статуса:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddStatusForm_Load(object sender, EventArgs e)
        {
            Inactivity.OnFormLoad(this);
        }

        private void AddStatusForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Inactivity.OnFormClosing(this);
        }
    }
}