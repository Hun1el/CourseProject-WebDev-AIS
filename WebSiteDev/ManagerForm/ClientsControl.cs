using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using WebSiteDev.AddForm;

namespace WebSiteDev.ManagerForm
{
    /// <summary>
    /// Контрол для управления клиентами с маскированием личных данных
    /// </summary>
    public partial class ClientsControl : UserControl
    {
        private DataManipulation dataManipulation;
        public bool update = false;
        private int selectedClientID = -1;
        private int lastRevealedRowIndex = -1;
        private DataSecurity dataSecurity = new DataSecurity();

        public ClientsControl()
        {
            InitializeComponent();
            GetDate();
        }

        private void ClientsControl_Load(object sender, EventArgs e)
        {
            comboBox3.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            dataGridView1.ClearSelection();

            // Таймер для скрытия данных через 20 секунд
            timer1.Interval = 20000;
            timer1.Stop();
        }

        /// <summary>
        /// Загружает всех клиентов из БД и отображает их в таблице
        /// </summary>
        void GetDate()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `Clients`", con);
                cmd.ExecuteNonQuery();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                // Сохраняем оригинальные данные для маскирования
                dataSecurity.LoadOriginalData(dt, "", "PhoneNumber", "FirstName", "MiddleName");
                lastRevealedRowIndex = -1;

                dataGridView1.DataSource = dt;
                dataGridView1.Columns["ClientID"].Visible = false;
                dataGridView1.Columns["Surname"].HeaderText = "Фамилия";
                dataGridView1.Columns["FirstName"].HeaderText = "Имя";
                dataGridView1.Columns["MiddleName"].HeaderText = "Отчество";
                dataGridView1.Columns["PhoneNumber"].HeaderText = "Телефон";
                dataGridView1.Columns["Email"].HeaderText = "Эл. почта";

                // Отключаем сортировку по клику на заголовок
                dataGridView1.Columns["Surname"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["FirstName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["MiddleName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["PhoneNumber"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["Email"].SortMode = DataGridViewColumnSortMode.NotSortable;

                dataManipulation = new DataManipulation(dt);

                // Показываем количество клиентов
                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM Clients", con);
                int resultcount = Convert.ToInt32(count.ExecuteScalar());
                label1.Text = $"Количество записей: {resultcount}";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllClient(comboBox3, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);
            InputRest.FirstLetter(textBox1);

            dataGridView1.ClearSelection();
            ClearClientFields();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1500);
            update = true;
            button1.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1175);
            update = true;
            button1.Enabled = true;
        }

        private void maskedTextBox1_Enter(object sender, EventArgs e)
        {
            maskedTextBox1.SelectionStart = 4;
        }

        private void maskedTextBox1_Click(object sender, EventArgs e)
        {
            maskedTextBox1.SelectionStart = 4;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddClientsForm addClientsForm = new AddClientsForm();
            addClientsForm.ShowDialog();
            GetDate();
            dataGridView1.ClearSelection();
            ClearClientFields();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllClient(comboBox3, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            ClearClientFields();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dataManipulation.ResetFilters(comboSort: comboBox3, textSearch: textBox1);
            dataManipulation.ApplyAllClient(comboBox3, textBox1);

            dataGridView1.ClearSelection();
            ClearClientFields();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox2);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox3);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox4);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox2);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox3);
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EmailInput(e);
        }

        /// <summary>
        /// При клике на строку - загружает данные клиента в поля редактирования
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedClientID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ClientID"].Value);

                if (e.RowIndex == lastRevealedRowIndex)
                {
                    timer1.Stop();
                    timer1.Start();
                }

                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["Surname"].Value.ToString();

                // Получаем оригинальное имя из защитного хранилища
                string firstName = dataSecurity.GetOriginalFirstName(selectedClientID);
                if (firstName != null)
                {
                    textBox3.Text = firstName;
                }
                else
                {
                    textBox3.Text = dataGridView1.Rows[e.RowIndex].Cells["FirstName"].Value.ToString();
                }

                // Получаем оригинальное отчество из защитного хранилища
                string middleName = dataSecurity.GetOriginalMiddleName(selectedClientID);
                if (middleName != null)
                {
                    textBox4.Text = middleName;
                }
                else
                {
                    textBox4.Text = dataGridView1.Rows[e.RowIndex].Cells["MiddleName"].Value.ToString();
                }

                // Получаем оригинальный номер телефона из защитного хранилища
                string phoneNumber = dataSecurity.GetOriginalPhone(selectedClientID);
                if (phoneNumber != null)
                {
                    maskedTextBox1.Text = phoneNumber;
                }
                else
                {
                    maskedTextBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["PhoneNumber"].Value.ToString();
                }

                string email = dataGridView1.Rows[e.RowIndex].Cells["Email"].Value.ToString();

                // Разбиваем email на часть и домен
                if (email.Contains("@"))
                {
                    string[] emailParts = email.Split('@');
                    string emailWithoutDomain = emailParts[0];
                    string domainWithAt = "@" + emailParts[1];

                    textBox5.Text = emailWithoutDomain;

                    int domainIndex = comboBox2.FindString(domainWithAt);

                    if (domainIndex >= 0)
                    {
                        comboBox2.SelectedIndex = domainIndex;
                    }
                    else
                    {
                        comboBox2.Items.Add(domainWithAt);
                        comboBox2.SelectedItem = domainWithAt;
                    }
                }
                else
                {
                    textBox5.Text = email;
                    comboBox2.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Редактирует выбранного клиента
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            if (selectedClientID == -1)
            {
                MessageBox.Show("Клиент не выбран!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Заполните фамилию!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Заполните имя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Введите корректный номер телефона!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Заполните логин электронную почту!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите домен электронной почты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string phone = maskedTextBox1.Text;
            string domain = comboBox2.SelectedItem.ToString();

            // Формируем полный email
            string fullEmail;
            if (textBox5.Text.Contains("@"))
            {
                fullEmail = textBox5.Text;
            }
            else
            {
                fullEmail = $"{textBox5.Text}{domain}";
            }

            var result = MessageBox.Show("Вы действительно хотите изменить клиента?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            if (DataUpdate.UpdateClient(selectedClientID, textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), phone, fullEmail))
            {
                MessageBox.Show("Клиент успешно изменён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Обновляем защитное хранилище с новыми данными
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM `Clients` WHERE ClientID = @id", con);
                    cmd.Parameters.AddWithValue("@id", selectedClientID);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dataSecurity.UpdateOriginalData(selectedClientID, dt.Rows[0]);
                    }
                }

                // Обновляем таблицу
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    int clientID = Convert.ToInt32(dataGridView1.Rows[i].Cells["ClientID"].Value);
                    if (clientID == selectedClientID)
                    {
                        dataGridView1.Rows[i].Cells["Surname"].Value = textBox2.Text.Trim();
                        dataGridView1.Rows[i].Cells["FirstName"].Value = textBox3.Text.Trim();
                        dataGridView1.Rows[i].Cells["MiddleName"].Value = textBox4.Text.Trim();
                        dataGridView1.Rows[i].Cells["PhoneNumber"].Value = maskedTextBox1.Text;
                        dataGridView1.Rows[i].Cells["Email"].Value = fullEmail;

                        dataGridView1.Rows[i].Selected = true;
                        dataGridView1.InvalidateRow(i);

                        textBox2.Text = dataGridView1.Rows[i].Cells["Surname"].Value.ToString();

                        string firstName = dataSecurity.GetOriginalFirstName(selectedClientID);
                        if (firstName != null)
                        {
                            textBox3.Text = firstName;
                        }
                        else
                        {
                            textBox3.Text = dataGridView1.Rows[i].Cells["FirstName"].Value.ToString();
                        }

                        string middleName = dataSecurity.GetOriginalMiddleName(selectedClientID);
                        if (middleName != null)
                        {
                            textBox4.Text = middleName;
                        }
                        else
                        {
                            textBox4.Text = dataGridView1.Rows[i].Cells["MiddleName"].Value.ToString();
                        }

                        string phoneNumber = dataSecurity.GetOriginalPhone(selectedClientID);
                        if (phoneNumber != null)
                        {
                            maskedTextBox1.Text = phoneNumber;
                        }
                        else
                        {
                            maskedTextBox1.Text = dataGridView1.Rows[i].Cells["PhoneNumber"].Value.ToString();
                        }

                        string email = dataGridView1.Rows[i].Cells["Email"].Value.ToString();

                        if (email.Contains("@"))
                        {
                            string[] emailParts = email.Split('@');
                            string emailWithoutDomain = emailParts[0];
                            string domainWithAt = "@" + emailParts[1];

                            textBox5.Text = emailWithoutDomain;

                            int domainIndex = comboBox2.FindString(domainWithAt);

                            if (domainIndex >= 0)
                            {
                                comboBox2.SelectedIndex = domainIndex;
                            }
                            else
                            {
                                if (!comboBox2.Items.Contains(domainWithAt))
                                {
                                    comboBox2.Items.Add(domainWithAt);
                                }
                                comboBox2.SelectedItem = domainWithAt;
                            }
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Удаляет выбранного клиента
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            if (selectedClientID == -1)
            {
                MessageBox.Show("Выберите клиента для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы действительно хотите удалить клиента?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            if (DataDelete.DeleteClient(selectedClientID))
            {
                MessageBox.Show("Клиент успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedClientID = -1;
                GetDate();
                dataGridView1.ClearSelection();
                ClearClientFields();
            }
        }

        /// <summary>
        /// Форматирует отображение ячеек: показывает оригинальные данные для открытой строки или маскирует
        /// </summary>
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            int clientID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ClientID"].Value);

            // Если строка открыта - показываем оригинальные значения
            if (e.RowIndex == lastRevealedRowIndex)
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name == "PhoneNumber")
                {
                    string original = dataSecurity.GetOriginalPhone(clientID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "FirstName")
                {
                    string original = dataSecurity.GetOriginalFirstName(clientID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "MiddleName")
                {
                    string original = dataSecurity.GetOriginalMiddleName(clientID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                return;
            }

            // Маскируем чувствительные данные для других строк
            if (dataGridView1.Columns[e.ColumnIndex].Name == "PhoneNumber")
            {
                string original = dataSecurity.GetOriginalPhone(clientID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskPhone(original);
                    e.FormattingApplied = true;
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "FirstName")
            {
                string original = dataSecurity.GetOriginalFirstName(clientID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskName(original);
                    e.FormattingApplied = true;
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "MiddleName")
            {
                string original = dataSecurity.GetOriginalMiddleName(clientID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskName(original);
                    e.FormattingApplied = true;
                }
            }
        }

        /// <summary>
        /// Двойной клик на ячейку - показывает/скрывает чувствительные данные на 20 секунд
        /// </summary>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (e.RowIndex == lastRevealedRowIndex)
            {
                lastRevealedRowIndex = -1;
                dataGridView1.InvalidateRow(e.RowIndex);
                timer1.Stop();
                return;
            }

            // Закрываем предыдущую открытую строку если она была
            if (lastRevealedRowIndex >= 0)
            {
                int previousRow = lastRevealedRowIndex;
                lastRevealedRowIndex = -1;
                dataGridView1.InvalidateRow(previousRow);
            }

            // Открываем новую строку
            lastRevealedRowIndex = e.RowIndex;
            dataGridView1.InvalidateRow(e.RowIndex);

            timer1.Stop();
            timer1.Start();
        }

        /// <summary>
        /// Таймер срабатывает через 20 секунд - скрывает открытые чувствительные данные
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            if (lastRevealedRowIndex >= 0)
            {
                int rowToHide = lastRevealedRowIndex;
                lastRevealedRowIndex = -1;
                dataGridView1.InvalidateRow(rowToHide);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox1);
        }

        private void ClearClientFields()
        {
            selectedClientID = -1;
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            maskedTextBox1.Clear();

            if (comboBox2.Items.Count > 0)
            {
                comboBox2.SelectedIndex = 0;
            }

            lastRevealedRowIndex = -1;
        }
    }
}
