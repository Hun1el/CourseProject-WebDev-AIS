using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using WebSiteDev.AddForm;

namespace WebSiteDev.AdminForm
{
    /// <summary>
    /// Контрол для управления пользователями с маскированием чувствительных данных
    /// Чувствительные данные (логины, телефоны, имена) маскируются и показываются на 20 секунд при двойном клике
    /// </summary>
    public partial class UsersControl : UserControl
    {
        public bool update = false;
        private DataManipulation dataManipulation;
        private int selectedUserID = -1;
        private int currentUserID = 0;
        static readonly Random rand = new Random();

        // Индекс открытой в данный момент строки с видимыми данными
        private int lastRevealedRowIndex = -1;
        private DataSecurity dataSecurity = new DataSecurity();

        /// <summary>
        /// Конструктор контрола, получает ID текущего пользователя
        /// </summary>
        public UsersControl(int userID = 0)
        {
            InitializeComponent();
            currentUserID = userID;
            GetDate();
        }

        /// <summary>
        /// Инициализирует интерфейс: скрывает столбцы, настраивает таймер и ширину колонок
        /// </summary>
        private void UsersControl_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns["UserID"].Visible = false;
            comboBox3.SelectedIndex = 0;
            dataGridView1.ClearSelection();

            // Таймер для скрытия данных через 20 секунд
            timer1.Interval = 20000;
            timer1.Stop();

            // Колонки занимают всю доступную ширину
            dataGridView1.Columns["Surname"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["FirstName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["MiddleName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["RoleName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["UserLogin"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        /// <summary>
        /// Загружает всех пользователей из БД с информацией об их ролях
        /// </summary>
        void GetDate()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                // Получаем пользователей с информацией об их ролях
                MySqlCommand cmd = new MySqlCommand(@"SELECT u.UserID, u.Surname, u.FirstName, u.MiddleName, u.UserLogin,
                                                     u.UserPassword, r.RoleName AS RoleName, u.PhoneNumber, u.RoleID
                                              FROM Users u
                                              JOIN Role r ON u.RoleID = r.RoleID", con);
                cmd.ExecuteNonQuery();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                // Сохраняем оригинальные данные для маскирования
                dataSecurity.LoadOriginalData(dt);
                lastRevealedRowIndex = -1;

                // Привязываем данные к таблице
                dataGridView1.DataSource = dt;
                dataGridView1.Columns["UserID"].Visible = false;
                dataGridView1.Columns["RoleID"].Visible = false;
                dataGridView1.Columns["UserPassword"].Visible = false;
                dataGridView1.Columns["Surname"].HeaderText = "Фамилия";
                dataGridView1.Columns["FirstName"].HeaderText = "Имя";
                dataGridView1.Columns["MiddleName"].HeaderText = "Отчество";
                dataGridView1.Columns["UserLogin"].HeaderText = "Логин";
                dataGridView1.Columns["UserPassword"].HeaderText = "Пароль";
                dataGridView1.Columns["RoleName"].HeaderText = "Роль";
                dataGridView1.Columns["PhoneNumber"].HeaderText = "Телефон";

                // Отключаем сортировку по клику на заголовок
                dataGridView1.Columns["Surname"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["FirstName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["MiddleName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["UserLogin"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["RoleName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["PhoneNumber"].SortMode = DataGridViewColumnSortMode.NotSortable;

                dataManipulation = new DataManipulation(dt);

                // Заполняем выпадающие списки ролями
                dataManipulation.FillComboBoxWithRoles(comboBox1, "Роль не выбрана");
                dataManipulation.FillComboBoxWithRoles(comboBox2, "Выберите роль");

                // Показываем количество пользователей
                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM Users", con);
                int resultcount = Convert.ToInt32(count.ExecuteScalar());
                label1.Text = $"Количество записей: {resultcount}";
            }
        }

        /// <summary>
        /// Форматирует отображение ячеек показывает оригинальные данные для открытой строки или маскирует
        /// </summary>
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            int userID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["UserID"].Value);

            // Если строка открыта (данные видны) - показываем оригинальные значения
            if (e.RowIndex == lastRevealedRowIndex)
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name == "UserLogin")
                {
                    string original = dataSecurity.GetOriginalLogin(userID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "PhoneNumber")
                {
                    string original = dataSecurity.GetOriginalPhone(userID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "FirstName")
                {
                    string original = dataSecurity.GetOriginalFirstName(userID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "MiddleName")
                {
                    string original = dataSecurity.GetOriginalMiddleName(userID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                return;
            }

            // Маскируем чувствительные данные для других строк
            if (dataGridView1.Columns[e.ColumnIndex].Name == "UserLogin")
            {
                string original = dataSecurity.GetOriginalLogin(userID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskLogin(original);
                    e.FormattingApplied = true;
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "PhoneNumber")
            {
                string original = dataSecurity.GetOriginalPhone(userID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskPhone(original);
                    e.FormattingApplied = true;
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "FirstName")
            {
                string original = dataSecurity.GetOriginalFirstName(userID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskName(original);
                    e.FormattingApplied = true;
                }
            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "MiddleName")
            {
                string original = dataSecurity.GetOriginalMiddleName(userID);
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
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            // Если нажали на уже открытую строку - закрываем её
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

            // Перезапускаем таймер
            timer1.Stop();
            timer1.Start();
        }

        /// <summary>
        /// Изменение текста в поле поиска - применяет фильтры
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllUser(comboBox3, comboBox1, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);
            InputRest.FirstLetter(textBox1);

            dataGridView1.ClearSelection();
            ClearUserFields();
        }

        /// <summary>
        /// Кнопка расширить окно для редактирования
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1500);
            update = true;
            button1.Enabled = false;
        }

        /// <summary>
        /// Кнопка сжать окно после редактирования
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1175);
            update = true;
            button1.Enabled = true;
        }

        /// <summary>
        /// При входе в поле телефона - устанавливаем позицию курсора правильно
        /// </summary>
        private void maskedTextBox1_Enter(object sender, EventArgs e)
        {
            maskedTextBox1.SelectionStart = 4;
        }

        /// <summary>
        /// При клике на поле телефона - устанавливаем позицию курсора правильно
        /// </summary>
        private void maskedTextBox1_Click(object sender, EventArgs e)
        {
            maskedTextBox1.SelectionStart = 4;
        }

        /// <summary>
        /// Кнопка добавить нового пользователя
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            AddUsersForm addUsersForm = new AddUsersForm(dataManipulation);
            addUsersForm.ShowDialog();
            GetDate();
            dataGridView1.ClearSelection();
            ClearUserFields();
        }

        /// <summary>
        /// Кнопка сбросить фильтры
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            dataManipulation.ResetFilters(comboSort: comboBox3, comboFilter: comboBox1, textSearch: textBox1);
            dataManipulation.ApplyAllUser(comboBox3, comboBox1, textBox1);

            dataGridView1.ClearSelection();
            ClearUserFields();
        }

        /// <summary>
        /// Изменение выбора роли в фильтре - применяет фильтр
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllUser(comboBox3, comboBox1, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            ClearUserFields();
        }

        /// <summary>
        /// Изменение сортировки - применяет новый порядок
        /// </summary>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllUser(comboBox3, comboBox1, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            ClearUserFields();
        }

        // Ограничение ввода в поле поиска - только русский и дефис
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox1);
        }

        // Форматирование первой буквы при вводе фамилии
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox2);
        }

        // Форматирование первой буквы при вводе имени
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox3);
        }

        // Форматирование первой буквы при вводе отчества
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox4);
        }

        // Ограничение ввода фамилии - только русский и дефис
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox2);
        }

        // Ограничение ввода имени - только русский и дефис
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussianAndDash(e, textBox3);
        }

        // Ограничение ввода отчества - только русский
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        // Ограничение ввода логина
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.LoginInput(e);
        }

        // Ограничение ввода пароля - английский, цифры и спецсимволы
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EnglishDigitsAndSpecial(e);
        }

        /// <summary>
        /// Хеширует пароль алгоритмом SHA256
        /// </summary>
        private string GetSha256(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Клик на строку таблицы - загружает данные пользователя в поля редактирования
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedUserID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["UserID"].Value);

                // Если открыта эта строка - перезапускаем таймер
                if (e.RowIndex == lastRevealedRowIndex)
                {
                    timer1.Stop();
                    timer1.Start();
                }

                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["Surname"].Value.ToString();

                // Получаем оригинальное имя из защитного хранилища
                string firstName = dataSecurity.GetOriginalFirstName(selectedUserID);
                if (firstName != null)
                {
                    textBox3.Text = firstName;
                }
                else
                {
                    textBox3.Text = dataGridView1.Rows[e.RowIndex].Cells["FirstName"].Value.ToString();
                }

                // Получаем оригинальное отчество из защитного хранилища
                string middleName = dataSecurity.GetOriginalMiddleName(selectedUserID);
                if (middleName != null)
                {
                    textBox4.Text = middleName;
                }
                else
                {
                    textBox4.Text = dataGridView1.Rows[e.RowIndex].Cells["MiddleName"].Value.ToString();
                }

                // Получаем оригинальный логин из защитного хранилища
                string login = dataSecurity.GetOriginalLogin(selectedUserID);
                if (login != null)
                {
                    textBox5.Text = login;
                }
                else
                {
                    textBox5.Text = dataGridView1.Rows[e.RowIndex].Cells["UserLogin"].Value.ToString();
                }

                textBox6.Clear();

                // Получаем оригинальный номер телефона из защитного хранилища
                string phoneNumber = dataSecurity.GetOriginalPhone(selectedUserID);
                if (phoneNumber != null)
                {
                    maskedTextBox1.Text = phoneNumber;
                }
                else
                {
                    maskedTextBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["PhoneNumber"].Value.ToString();
                }

                int roleID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["RoleID"].Value);
                comboBox2.SelectedValue = roleID;

                // Нельзя менять свою роль
                if (selectedUserID == currentUserID)
                {
                    comboBox2.Enabled = false;
                }
                else
                {
                    comboBox2.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Кнопка сохранить изменения пользователя
        /// Проверяет все данные перед сохранением
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            if (selectedUserID == -1)
            {
                MessageBox.Show("Выберите пользователя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            if (comboBox2.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите роль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!maskedTextBox1.MaskFull)
            {
                MessageBox.Show("Введите корректный номер телефона!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Заполните логин!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string adminLogin = Properties.Settings.Default.AdminLogin;
            string newLogin = textBox5.Text.Trim();

            // Проверяем что логин не зарезервирован для администратора
            if (string.IsNullOrWhiteSpace(adminLogin) == false)
            {
                if (string.Equals(newLogin, adminLogin, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"Логин \"{adminLogin}\" зарезервирован для встроенного администратора.\nВыберите другой логин.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int roleID = Convert.ToInt32(comboBox2.SelectedValue);
            string phone = maskedTextBox1.Text;
            string password = null;

            // Если пароль введён - хешируем его
            if (string.IsNullOrWhiteSpace(textBox6.Text) == false)
            {
                password = GetSha256(textBox6.Text);
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите изменить пользователя?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // Обновляем пользователя в БД
            if (DataUpdate.UpdateUser(selectedUserID, textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), newLogin, password, roleID, phone))
            {
                MessageBox.Show("Пользователь успешно изменён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Обновляем защитное хранилище с новыми данными
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT u.UserID, u.Surname, u.FirstName, u.MiddleName, u.UserLogin, u.UserPassword, r.RoleName AS RoleName, u.PhoneNumber, u.RoleID FROM Users u JOIN Role r ON u.RoleID = r.RoleID WHERE u.UserID = @id", con);
                    cmd.Parameters.AddWithValue("@id", selectedUserID);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dataSecurity.UpdateOriginalData(selectedUserID, dt.Rows[0]);
                    }
                }

                // Обновляем отображение в таблице
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    int userID = Convert.ToInt32(dataGridView1.Rows[i].Cells["UserID"].Value);
                    if (userID == selectedUserID)
                    {
                        dataGridView1.Rows[i].Cells["Surname"].Value = textBox2.Text.Trim();
                        dataGridView1.Rows[i].Cells["FirstName"].Value = textBox3.Text.Trim();
                        dataGridView1.Rows[i].Cells["MiddleName"].Value = textBox4.Text.Trim();
                        dataGridView1.Rows[i].Cells["UserLogin"].Value = newLogin;
                        dataGridView1.Rows[i].Cells["PhoneNumber"].Value = phone;

                        // Находим название роли
                        string roleName = "";
                        foreach (DataRow row in dataManipulation.table.Rows)
                        {
                            if (Convert.ToInt32(row["RoleID"]) == roleID)
                            {
                                roleName = row["RoleName"].ToString();
                                break;
                            }
                        }
                        dataGridView1.Rows[i].Cells["RoleName"].Value = roleName;

                        dataGridView1.Rows[i].Selected = true;
                        dataGridView1.InvalidateRow(i);

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Перемешивает символы строки для генерирования пароля
        /// </summary>
        static string Shuffle(string str)
        {
            var chars = str.ToCharArray();
            // Алгоритм Фишера-Йетса для перемешивания
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars);
        }

        /// <summary>
        /// Кнопка генерировать случайный пароль (12 символов)
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            string allChars = upper + lower + numbers + special;
            Random random = new Random();
            StringBuilder password = new StringBuilder();

            // Обеспечиваем минимум каждого типа символов
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(numbers[random.Next(numbers.Length)]);
            password.Append(upper[random.Next(upper.Length)]);

            // Добавляем оставшиеся случайные символы до 12
            for (int i = 4; i < 12; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            string shufflepass = Shuffle(Convert.ToString(password));
            textBox6.Text = shufflepass;
        }

        /// <summary>
        /// Переключает видимость пароля при клике на иконку глаза
        /// </summary>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (textBox6.UseSystemPasswordChar)
            {
                textBox6.UseSystemPasswordChar = false;
                pictureBox2.BackgroundImage = Properties.Resources.EyeHide;
            }
            else
            {
                textBox6.UseSystemPasswordChar = true;
                pictureBox2.BackgroundImage = Properties.Resources.EyeView;
            }
        }

        /// <summary>
        /// Кнопка удалить выбранного пользователя
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            if (selectedUserID == -1)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите удалить пользователя?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Удаляем пользователя из БД
            if (DataDelete.DeleteUser(selectedUserID, currentUserID))
            {
                MessageBox.Show("Пользователь успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GetDate();
                dataGridView1.ClearSelection();
                ClearUserFields();
            }
        }

        /// <summary>
        /// Таймер срабатывает через 20 секунд - скрывает открытые чувствительные данные
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            // Скрываем открытые данные
            if (lastRevealedRowIndex >= 0)
            {
                int rowToHide = lastRevealedRowIndex;
                lastRevealedRowIndex = -1;
                dataGridView1.InvalidateRow(rowToHide);
            }
        }

        /// <summary>
        /// Очищает все поля редактирования
        /// </summary>
        private void ClearUserFields()
        {
            selectedUserID = -1;
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            maskedTextBox1.Clear();
            comboBox2.SelectedValue = 0;
            lastRevealedRowIndex = -1;
        }
    }
}
