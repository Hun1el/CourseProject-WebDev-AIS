using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WebSiteDev.ManagerForm
{
    /// <summary>
    /// Контрол для управления заказами - отслеживание статусов, редактирование дат выполнения, статусов, печать чеков
    /// </summary>
    public partial class OrderControl : UserControl
    {
        private DataManipulation dataManipulation;
        private string userRole;
        public bool update = false;
        private int selectedOrderID = -1;
        private string currentStatus = "";
        private int lastRevealedRowIndex = -1;
        private Timer timer1 = new Timer();

        public static int CurrentUserID { get; set; } = 0;
        public static string CurrentUserName { get; set; } = "";

        private DataSecurity dataSecurity = new DataSecurity();

        /// <summary>
        /// Инициализирует контрол и загружает данные заказов
        /// </summary>
        public OrderControl(string role, int userID = 0, string userName = "")
        {
            InitializeComponent();
            userRole = role;
            CurrentUserID = userID;
            CurrentUserName = userName;
            timer1.Interval = 20000;
            timer1.Tick += Timer1_Tick;
            GetDate();
        }

        /// <summary>
        /// При загрузке контрола - инициализирует UI элементы и скрывает кнопки для администраторов
        /// </summary>
        private void OrderControl_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            comboBox1.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            DateTime dateTimeNow = DateTime.Now;
            dateTimePicker1.CustomFormat = "yyyy.MM.dd";

            dataGridView1.ContextMenuStrip = contextMenuStrip1;

            // Администраторы не могут создавать заказы, только просматривать
            if (userRole == "Администратор")
            {
                button5.Visible = false;
                button1.Visible = false;
                button2.Visible = false;
            }
        }

        /// <summary>
        /// Загружает все заказы из БД с информацией о клиентах, сотрудниках и товарах
        /// </summary>
        void GetDate()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                // Сложный запрос с несколькими JOIN для получения полной информации о заказах
                MySqlCommand cmd = new MySqlCommand(@"
        SELECT o.OrderID,
            CONCAT(c.Surname, ' ', c.FirstName, ' ', COALESCE(c.MiddleName, '')) AS ClientName,
            CONCAT(u.Surname, ' ', u.FirstName, ' ', COALESCE(u.MiddleName, '')) AS UserName,
            o.OrderDate, o.OrderCompDate,
        GROUP_CONCAT(DISTINCT p.ProductName SEPARATOR ', ') AS ProductName,
            s.StatusName, o.OrderCost
        FROM `Order` o
        LEFT JOIN Clients c ON o.ClientID = c.ClientID
        LEFT JOIN Users u ON o.UserID = u.UserID
        LEFT JOIN orderproduct op ON o.OrderID = op.OrderID
        LEFT JOIN Product p ON op.ProductID = p.ProductID
        LEFT JOIN Status s ON o.StatusID = s.StatusID
        GROUP BY o.OrderID
        ORDER BY o.OrderDate ASC", con);

                cmd.ExecuteNonQuery();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                // Сохраняем оригинальные имена для маскирования
                dataSecurity.LoadOriginalClientNames(dt, "ClientName");
                dataSecurity.LoadOriginalUserNames(dt, "UserName");
                lastRevealedRowIndex = -1;

                dataGridView1.DataSource = dt;
                dataGridView1.Columns["OrderID"].HeaderText = "№ заказа";
                dataGridView1.Columns["ClientName"].HeaderText = "Клиент";
                dataGridView1.Columns["UserName"].HeaderText = "Сотрудник";
                dataGridView1.Columns["OrderDate"].HeaderText = "Дата заказа";
                dataGridView1.Columns["OrderCompDate"].HeaderText = "Срок выполнения заказа";
                dataGridView1.Columns["ProductName"].Visible = false;
                dataGridView1.Columns["StatusName"].HeaderText = "Статус";
                dataGridView1.Columns["OrderCost"].HeaderText = "Итоговая цена";

                // Отключаем сортировку по клику
                dataGridView1.Columns["OrderID"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["ClientName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["UserName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["OrderDate"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["OrderCompDate"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["ProductName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["StatusName"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns["OrderCost"].SortMode = DataGridViewColumnSortMode.NotSortable;

                dataManipulation = new DataManipulation(dt);

                // Заполняем выпадающие списки статусов
                dataManipulation.FillComboBoxWithStatuses(comboBox6, "Статус не выбран");
                dataManipulation.FillComboBoxWithStatuses(comboBox5, "Выберите статус");

                // Показываем общее количество заказов
                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM `Order`", con);
                int resultcount = Convert.ToInt32(count.ExecuteScalar());
                label1.Text = $"Количество записей: {resultcount}";

                // Окрашиваем строки в зависимости от статуса
                ColorizeRowsByStatus();
            }
        }

        /// <summary>
        /// Форматирует отображение ячеек - окрашивает по статусу, маскирует/показывает имена
        /// </summary>
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            int orderID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["OrderID"].Value);
            string status = dataGridView1.Rows[e.RowIndex].Cells["StatusName"].Value?.ToString();

            // Окрашиваем строки в зависимости от статуса
            if (status == "Завершён")
            {
                e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
            }
            else if (status == "Отменён")
            {
                e.CellStyle.BackColor = System.Drawing.Color.IndianRed;
            }

            // Если строка открыта (двойной клик) - показываем оригинальные данные
            if (e.RowIndex == lastRevealedRowIndex)
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name == "ClientName")
                {
                    string original = dataSecurity.GetOriginalClientName(orderID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "UserName")
                {
                    string original = dataSecurity.GetOriginalUserName(orderID);
                    if (original != null)
                    {
                        e.Value = original;
                        e.FormattingApplied = true;
                    }
                }
                return;
            }

            // Маскируем имена для защиты данных
            if (dataGridView1.Columns[e.ColumnIndex].Name == "ClientName")
            {
                string original = dataSecurity.GetOriginalClientName(orderID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskClientName(original);
                    e.FormattingApplied = true;
                }
            }

            if (dataGridView1.Columns[e.ColumnIndex].Name == "UserName")
            {
                string original = dataSecurity.GetOriginalUserName(orderID);
                if (e.Value != null && original != null)
                {
                    e.Value = DataSecurity.MaskUserName(original);
                    e.FormattingApplied = true;
                }
            }
        }

        /// <summary>
        /// Обработка правого клика на таблице - выделение строки для контекстного меню
        /// </summary>
        private void DataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo hit = dataGridView1.HitTest(e.X, e.Y);

                if (hit.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hit.RowIndex].Selected = true;
                }
            }
        }

        /// <summary>
        /// При вводе номера заказа - фильтрует таблицу по данному номеру
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllOrder(comboBox1, comboBox6, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);
            InputRest.FirstLetter(textBox1);

            dataGridView1.ClearSelection();
            selectedOrderID = -1;
            dataGridView1.Refresh();
        }

        /// <summary>
        /// Кнопка расширить панель - показывает дополнительные элементы управления
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1500);
            update = true;
            button1.Enabled = false;
        }

        /// <summary>
        /// Кнопка свернуть панель - скрывает дополнительные элементы управления
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1175);
            update = true;
            button1.Enabled = true;
        }

        /// <summary>
        /// Кнопка создать новый заказ - переходит к форме выбора товаров
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            ProductControl.CurrentOrder.Clear();

            ManagerMainForm managerForm = (ManagerMainForm)this.FindForm();
            managerForm.LoadControl(new ProductControl(userRole, CurrentUserID, CurrentUserName));
            managerForm.Text = "Оформление заказа";

            managerForm.SelectButtonPublic(managerForm.Button2);
        }

        /// <summary>
        /// При изменении фильтра по дате - обновляет отображение таблицы
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllOrder(comboBox1, comboBox6, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            selectedOrderID = -1;
            dataGridView1.Refresh();
        }

        /// <summary>
        /// При изменении фильтра по статусу - обновляет отображение таблицы
        /// </summary>
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataManipulation.ApplyAllOrder(comboBox1, comboBox6, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            selectedOrderID = -1;
            dataGridView1.Refresh();
        }

        /// <summary>
        /// Кнопка сброс фильтров - отображает все заказы
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            dataManipulation.ResetFilters(comboBox1, comboBox6, textBox1);

            dataGridView1.ClearSelection();
            selectedOrderID = -1;
            dataGridView1.Refresh();
        }

        /// <summary>
        /// Ограничивает ввод только цифрами в поле поиска
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyNumbers(e);
        }

        /// <summary>
        /// Двойной клик на ячейку таблицы - открывает форму со списком товаров в заказе
        /// </summary>
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Rows[e.RowIndex].Cells["OrderID"].Value != null)
            {
                int orderID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["OrderID"].Value);
                OrderProductForm form = new OrderProductForm(orderID);
                form.ShowDialog();
            }
        }

        /// <summary>
        /// Пункт контекстного меню - просмотр состава заказа
        /// </summary>
        private void просмотрСоставаЗаказаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["OrderID"].Value);
                OrderProductForm form = new OrderProductForm(orderID);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// При клике на строку таблицы - загружает данные заказа и доступные статусы для редактирования
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedOrderID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["OrderID"].Value);
                currentStatus = dataGridView1.Rows[e.RowIndex].Cells["StatusName"].Value.ToString();

                button5.Enabled = true;

                // Загружаем дату выполнения заказа
                DateTime orderDate = DateTime.Now;
                bool isValidDate = false;

                try
                {
                    object compDateObj = dataGridView1.Rows[e.RowIndex].Cells["OrderCompDate"].Value;
                    if (compDateObj != null && compDateObj != DBNull.Value)
                    {
                        orderDate = Convert.ToDateTime(compDateObj);
                        isValidDate = true;
                    }
                }
                catch { }

                // Устанавливаем минимальную дату для выпадающего календаря
                if (isValidDate)
                {
                    if (orderDate < DateTime.Now.Date)
                    {
                        dateTimePicker1.MinDate = DateTime.Now.Date;
                        dateTimePicker1.Value = DateTime.Now.Date;
                    }
                    else
                    {
                        dateTimePicker1.MinDate = orderDate;
                        dateTimePicker1.Value = orderDate;
                    }
                }
                else
                {
                    dateTimePicker1.MinDate = DateTime.Now.Date;
                    dateTimePicker1.Value = DateTime.Now.Date;
                }

                // Определяем доступные статусы в зависимости от текущего статуса
                string[] statuses = null;

                if (currentStatus == "Новый")
                {
                    statuses = new string[] { "Новый", "В работе", "Отменён" };
                }
                else if (currentStatus == "В работе")
                {
                    statuses = new string[] { "В работе", "Завершён", "Отменён" };
                }
                else if (currentStatus == "Завершён" || currentStatus == "Отменён")
                {
                    statuses = new string[] { currentStatus };
                }

                if (statuses != null && statuses.Length > 0)
                {
                    comboBox5.DataSource = null;
                    comboBox5.DataSource = statuses;
                    comboBox5.SelectedIndex = 0;
                }

                // Заказы со статусом "Завершён" или "Отменён" нельзя редактировать
                bool isEditable = !(currentStatus == "Завершён" || currentStatus == "Отменён");
                comboBox5.Enabled = isEditable;
                dateTimePicker1.Enabled = isEditable;
                button6.Enabled = isEditable;
            }
        }

        /// <summary>
        /// Кнопка сохранить - обновляет статус и дату выполнения заказа в БД
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            if (selectedOrderID == -1)
            {
                MessageBox.Show("Выберите заказ!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newStatus = comboBox5.SelectedItem.ToString();
            DateTime newDate = dateTimePicker1.Value;

            // Если выбрана дата в прошлом - переносим на завтра
            if (newDate < DateTime.Now)
            {
                newDate = DateTime.Now.AddDays(1);
                dateTimePicker1.Value = newDate;
            }

            // Завершённые и отменённые заказы нельзя редактировать
            if (currentStatus == "Отменён" || currentStatus == "Завершён")
            {
                MessageBox.Show("Данный заказ нельзя редактировать!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем логику переходов между статусами
            if ((currentStatus == "Новый" && (newStatus != "В работе" && newStatus != "Отменён" && newStatus != "Новый")) ||
                (currentStatus == "В работе" && (newStatus != "Завершён" && newStatus != "Отменён" && newStatus != "В работе")))
            {
                MessageBox.Show("Недопустимый переход статуса!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы действительно хотите изменить заказ?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            if (DataUpdate.UpdateOrderStatusAndDate(selectedOrderID, newStatus, newDate))
            {
                MessageBox.Show("Заказ успешно обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GetDate();

                // Находим обновленный заказ и выделяем его
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    int orderID = Convert.ToInt32(dataGridView1.Rows[i].Cells["OrderID"].Value);
                    if (orderID == selectedOrderID)
                    {
                        dataGridView1.Rows[i].Selected = true;

                        currentStatus = dataGridView1.Rows[i].Cells["StatusName"].Value.ToString();

                        try
                        {
                            DateTime orderDate = Convert.ToDateTime(dataGridView1.Rows[i].Cells["OrderCompDate"].Value);
                            dateTimePicker1.MinDate = orderDate;
                            dateTimePicker1.Value = orderDate;
                        }
                        catch
                        {
                            dateTimePicker1.MinDate = DateTime.Now;
                            dateTimePicker1.Value = DateTime.Now;
                        }

                        string[] statuses = null;

                        if (currentStatus == "Новый")
                        {
                            statuses = new string[] { "Новый", "В работе", "Отменён" };
                        }
                        else if (currentStatus == "В работе")
                        {
                            statuses = new string[] { "В работе", "Завершён", "Отменён" };
                        }
                        else if (currentStatus == "Завершён" || currentStatus == "Отменён")
                        {
                            statuses = new string[] { currentStatus };
                        }

                        if (statuses != null && statuses.Length > 0)
                        {
                            comboBox5.DataSource = null;
                            comboBox5.DataSource = statuses;
                            comboBox5.SelectedIndex = 0;
                        }

                        bool isEditable = !(currentStatus == "Завершён" || currentStatus == "Отменён");
                        comboBox5.Enabled = isEditable;
                        dateTimePicker1.Enabled = isEditable;
                        button6.Enabled = isEditable;

                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Ошибка обновления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Кнопка печать чека - создаёт документ Word с информацией о заказе
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ для создания чека!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем установлен ли Microsoft Word на компьютере
            if (!IsWordInstalled())
            {
                MessageBox.Show(
                    "Microsoft Word не установлен на вашем компьютере!\n\n" +
                    "Для создания чека требуется установленное приложение Microsoft Office Word.\n\n" +
                    "Пожалуйста, установите Microsoft Office и повторите попытку.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            int orderID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["OrderID"].Value);

            // Получаем оригинальное имя клиента из защитного хранилища
            string clientName = dataSecurity.GetOriginalClientName(orderID);

            if (clientName == null)
            {
                clientName = dataGridView1.SelectedRows[0].Cells["ClientName"].Value.ToString();
            }

            string orderDate = Convert.ToDateTime(dataGridView1.SelectedRows[0].Cells["OrderDate"].Value).ToString("dd.MM.yyyy");
            string statusName = dataGridView1.SelectedRows[0].Cells["StatusName"].Value.ToString();
            string orderCost = dataGridView1.SelectedRows[0].Cells["OrderCost"].Value.ToString();

            // Формируем сообщение подтверждения
            string message = "Вы хотите создать чек для следующего заказа?\n\n";
            message = message + "Номер заказа: " + orderID + "\n";
            message = message + "Клиент: " + clientName + "\n";
            message = message + "Дата заказа: " + orderDate + "\n";
            message = message + "Сумма: " + orderCost + " руб.\n\n";
            message = message + "Продолжить?";

            var result = MessageBox.Show(message, "Подтверждение создания чека", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            Doc.CheckWord.CreateCheck(orderID);
        }

        /// <summary>
        /// Проверяет установлен ли Microsoft Word через COM объекты
        /// </summary>
        private bool IsWordInstalled()
        {
            try
            {
                Type wordType = Type.GetTypeFromProgID("Word.Application");
                if (wordType == null)
                {
                    return false;
                }

                object wordApp = Activator.CreateInstance(wordType);
                if (wordApp != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Двойной клик на ячейку - показывает/скрывает оригинальные имена на 20 секунд
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

            // Закрываем предыдущую открытую строку
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
        /// Таймер - срабатывает через 20 секунд и скрывает открытые данные
        /// </summary>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            if (lastRevealedRowIndex >= 0)
            {
                int rowToHide = lastRevealedRowIndex;
                lastRevealedRowIndex = -1;
                dataGridView1.InvalidateRow(rowToHide);
            }
        }

        /// <summary>
        /// Окрашивает строки таблицы в зависимости от статуса заказа
        /// Зелёный - Завершён, Красный - Отменён
        /// </summary>
        private void ColorizeRowsByStatus()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["StatusName"].Value != null)
                {
                    string status = row.Cells["StatusName"].Value.ToString();

                    if (status == "Завершён")
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.BackColor = System.Drawing.Color.LightGreen;
                        }
                    }
                    else if (status == "Отменён")
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.BackColor = System.Drawing.Color.IndianRed;
                        }
                    }
                }
            }
        }
    }
}