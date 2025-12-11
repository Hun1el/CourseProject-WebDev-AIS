using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace WebSiteDev.AddForm
{
    /// <summary>
    /// Форма для добавления новой категории товаров в базу данных
    /// </summary>
    public partial class AddCategoryForm : Form
    {
        public AddCategoryForm()
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
        /// Применяет форматирование первой буквы при вводе текста
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox1);
        }

        /// <summary>
        /// Ограничивает ввод только допустимыми символами для названия категории
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.CategoryInput(e);
        }

        /// <summary>
        /// Обработчик кнопки добавления новой категории
        /// Проверяет введенные данные, проверяет уникальность и добавляет в БД
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Получаем и очищаем название категории от пробелов
            string categoryName = textBox1.Text.Trim();

            // Проверяем, что поле не пусто
            if (categoryName == "")
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();

                    // Проверяем, существует ли уже такая категория в базе
                    string checkQuery = "SELECT COUNT(*) FROM Category WHERE CategoryName = '" + categoryName + "'";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        // Если категория найдена, показываем ошибку
                        if (count > 0)
                        {
                            MessageBox.Show("Такая категория уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Добавляем новую категорию в базу данных
                    string insertQuery = "INSERT INTO Category (CategoryName) VALUES ('" + categoryName + "')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.ExecuteNonQuery();
                    }

                    // Показываем сообщение об успехе и очищаем поле ввода
                    MessageBox.Show("Категория успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear();
                }
            }
            catch (Exception Ex)
            {
                // Обработка исключений при работе с БД
                MessageBox.Show("Ошибка при добавлении категории:\n" + Ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}