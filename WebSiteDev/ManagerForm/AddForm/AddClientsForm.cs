using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace WebSiteDev.AddForm
{
    /// <summary>
    /// Форма для добавления нового клиента в систему
    /// Включает заполнение ФИО, email и номера телефона
    /// </summary>
    public partial class AddClientsForm : Form
    {
        public AddClientsForm()
        {
            InitializeComponent();
        }

        private void AddClientsForm_Load(object sender, EventArgs e)
        {
            Inactivity.OnFormLoad(this);

            comboBox1.SelectedIndex = 0;
        }

        /// <summary>
        /// Кнопка отмена - закрывает форму
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
        /// Ограничивает ввод фамилии только русскими буквами и дефисом
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox2);
        }

        /// <summary>
        /// Ограничивает ввод имени только русскими буквами и дефисом
        /// </summary>
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox3);
        }

        /// <summary>
        /// Ограничивает ввод отчества только русскими буквами
        /// </summary>
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        /// <summary>
        /// Ограничивает ввод email допустимыми символами
        /// </summary>
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EmailInput(e);
        }

        /// <summary>
        /// Кнопка добавить - проверяет данные и добавляет клиента в БД
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Получаем все введённые данные
            string SurName = textBox2.Text.Trim();
            string FirstName = textBox3.Text.Trim();
            string MiddleName = textBox4.Text.Trim();
            string EmailName = textBox5.Text.Trim();

            string PhoneNumber = maskedTextBox1.Text.Trim();

            // Проверяем что обязательные поля заполнены
            if (SurName == "" || FirstName == "" || comboBox1.SelectedIndex <= 0 || PhoneNumber == "")
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем выбор домена email
            if (comboBox1.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите домен электронной почты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем корректность номера телефона
            if (!maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Введите корректный номер телефона!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Формируем полный email из введённой части и выбранного домена
            string EmailDomain = comboBox1.SelectedItem.ToString();
            string FullEmail = EmailName + EmailDomain;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();

                    // Проверяем что клиент с таким email не существует
                    string checkEmailQuery = "SELECT COUNT(*) FROM Clients WHERE Email = '" + FullEmail + "'";
                    using (MySqlCommand checkEmailCmd = new MySqlCommand(checkEmailQuery, con))
                    {
                        int emailCount = Convert.ToInt32(checkEmailCmd.ExecuteScalar());

                        if (emailCount > 0)
                        {
                            MessageBox.Show("Клиент с таким email уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Проверяем что клиент с таким номером телефона не существует
                    string checkPhoneQuery = "SELECT COUNT(*) FROM Clients WHERE PhoneNumber = '" + PhoneNumber + "'";
                    using (MySqlCommand checkPhoneCmd = new MySqlCommand(checkPhoneQuery, con))
                    {
                        int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                        if (phoneCount > 0)
                        {
                            MessageBox.Show("Клиент с таким номером телефона уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Добавляем нового клиента в таблицу
                    string insertQuery = "INSERT INTO Clients (Surname, FirstName, MiddleName, Email, PhoneNumber) " +
                                         "VALUES ('" + SurName + "', '" + FirstName + "', '" + MiddleName + "', '" + FullEmail + "', '" + PhoneNumber + "')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, con))
                    {
                        insertCmd.ExecuteNonQuery();
                    }

                    // Показываем сообщение об успехе и очищаем все поля
                    MessageBox.Show("Клиент успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    textBox5.Clear();
                    maskedTextBox1.Clear();
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с БД
                MessageBox.Show("Ошибка при добавлении клиента:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddClientsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Inactivity.OnFormClosing(this);
        }
    }
}