using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace WebSiteDev.ManagerForm
{
    public partial class DirectorOrderControl : UserControl
    {
        private DataManipulation dataManipulation;
        private DataSecurity dataSecurity = new DataSecurity();
        private int lastRevealedRowIndex = -1;
        private Timer timer1 = new Timer();

        public DirectorOrderControl()
        {
            InitializeComponent();
            timer1.Interval = 20000;
            timer1.Tick += Timer1_Tick;
            GetDate();
        }

        private void DirectorOrderControl_Load(object sender, EventArgs e)
        {
            comboBox3.SelectedIndex = 0;
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            SetDatePickerRange();
            dataManipulation.FillComboBoxWithStatuses(comboBox1, "Выберите статус");
            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// Устанавливает минимальную и максимальную дату в календарях на основе первого и последнего заказа
        /// </summary>
        private void SetDatePickerRange()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                // Получаем первую и последнюю дату заказов из БД
                MySqlCommand cmd = new MySqlCommand("SELECT MIN(OrderDate) AS FirstDate, MAX(OrderDate) AS LastDate FROM `Order`", con);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime firstDate = DateTime.Now;
                        DateTime lastDate = DateTime.Now;

                        // Если дата не null - используем её, иначе используем текущую дату
                        if (reader["FirstDate"] != DBNull.Value)
                        {
                            firstDate = Convert.ToDateTime(reader["FirstDate"]);
                        }

                        if (reader["LastDate"] != DBNull.Value)
                        {
                            lastDate = Convert.ToDateTime(reader["LastDate"]);
                        }

                        // Устанавливаем диапазон для первого календаря (начальная дата)
                        dateTimePicker1.MinDate = firstDate;
                        dateTimePicker1.MaxDate = lastDate;
                        dateTimePicker1.Value = firstDate;
                        dateTimePicker1.CustomFormat = "dd.MM.yyyy";

                        // Устанавливаем диапазон для второго календаря (конечная дата)
                        dateTimePicker2.MinDate = firstDate;
                        dateTimePicker2.MaxDate = lastDate;
                        dateTimePicker2.Value = lastDate;
                        dateTimePicker2.CustomFormat = "dd.MM.yyyy";
                    }
                }
            }
        }

        /// <summary>
        /// Загружает заказы за выбранный период из БД и отображает их в таблице
        /// </summary>
        private void GetDate()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                // Формируем даты для фильтрации
                string dateFromStr = dateTimePicker1.Value.Date.ToString("yyyy-MM-dd");
                string dateToStr = dateTimePicker2.Value.Date.ToString("yyyy-MM-dd");

                // Получаем заказы за выбранный период
                MySqlCommand cmd = new MySqlCommand($@"
                    SELECT 
                        o.OrderID,
                        CONCAT(c.Surname, ' ', c.FirstName, ' ', COALESCE(c.MiddleName, '')) AS ClientName,
                        CONCAT(u.Surname, ' ', u.FirstName, ' ', COALESCE(u.MiddleName, '')) AS UserName,
                        o.OrderDate,
                        o.OrderCompDate,
                        GROUP_CONCAT(DISTINCT p.ProductName SEPARATOR ', ') AS ProductName,
                        s.StatusName,
                        o.OrderCost
                    FROM `Order` o
                    LEFT JOIN Clients c ON o.ClientID = c.ClientID
                    LEFT JOIN Users u ON o.UserID = u.UserID
                    LEFT JOIN orderproduct op ON o.OrderID = op.OrderID
                    LEFT JOIN Product p ON op.ProductID = p.ProductID
                    LEFT JOIN Status s ON o.StatusID = s.StatusID
                    WHERE DATE(o.OrderDate) BETWEEN '{dateFromStr}' AND '{dateToStr}'
                    GROUP BY o.OrderID 
                    ORDER BY o.OrderDate ASC", con);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Сохраняем оригинальные имена для маскирования
                dataSecurity.LoadOriginalClientNames(dt, "ClientName");
                dataSecurity.LoadOriginalUserNames(dt, "UserName");
                lastRevealedRowIndex = -1;

                dataGridView1.DataSource = dt;

                // Устанавливаем заголовки колонок
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

                // Показываем количество заказов за период
                MySqlCommand count = new MySqlCommand($"SELECT COUNT(*) FROM `Order` WHERE DATE(OrderDate) BETWEEN '{dateFromStr}' AND '{dateToStr}'", con);
                int resultcount = Convert.ToInt32(count.ExecuteScalar());
                label1.Text = "Количество записей: " + resultcount;

                // Применяем фильтры и сортировку
                dataManipulation.ApplyAllDirector(comboBox3, comboBox1, textBox1);
                dataManipulation.UpdateRecordCountLabel(label1);

                // Окрашиваем строки в зависимости от статуса
                ColorizeRowsByStatus();
            }
        }

        /// <summary>
        /// При вводе номера заказа - фильтрует таблицу
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (dataManipulation == null)
            {
                return;
            }

            dataManipulation.ApplyAllDirector(comboBox3, comboBox1, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);
            InputRest.FirstLetter(textBox1);

            dataGridView1.ClearSelection();
            dataGridView1.Refresh();
        }

        /// <summary>
        /// Кнопка "Создать отчёт" - экспортирует данные в Excel файл
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверяем есть ли данные
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для формирования отчёта!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем установлен ли Microsoft Excel
            if (!IsExcelInstalled())
            {
                MessageBox.Show(
                    "Microsoft Excel не установлен на вашем компьютере!\n\n" +
                    "Для создания отчёта требуется установленное приложение Microsoft Office Excel.\n\n" +
                    "Пожалуйста, установите Microsoft Office и повторите попытку.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Формируем сообщение подтверждения с параметрами отчёта
            string message = "Вы хотите создать отчёт со следующими параметрами:\n\n";
            message = message + "Период: с " + dateTimePicker1.Value.ToString("dd.MM.yyyy") + " по " + dateTimePicker2.Value.ToString("dd.MM.yyyy") + "\n";

            if (string.IsNullOrWhiteSpace(textBox1.Text) == false)
            {
                message = message + "Поиск по номеру заказа: " + textBox1.Text + "\n";
            }

            // Получаем выбранный статус
            string selectedStatus = "";
            if (comboBox1.SelectedIndex > 0)
            {
                object selected = comboBox1.SelectedItem;
                if (selected != null)
                {
                    if (selected is DataRowView row)
                    {
                        selectedStatus = row["StatusName"].ToString();
                        message = message + "Фильтр по статусу: " + selectedStatus + "\n";
                    }
                    else
                    {
                        selectedStatus = selected.ToString();
                        message = message + "Фильтр по статусу: " + selectedStatus + "\n";
                    }
                }
            }

            // Получаем выбранную сортировку
            string selectedSort = "";
            if (comboBox3.SelectedIndex > 0)
            {
                selectedSort = comboBox3.SelectedItem.ToString();
                message = message + "Сортировка: " + selectedSort + "\n";
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text) && comboBox1.SelectedIndex <= 0 && comboBox3.SelectedIndex <= 0)
            {
                message = message + "\nВсе заказы без поиска, фильтров и сортировки\n";
            }

            message = message + "\nВсего записей: " + dataGridView1.Rows.Count + "\n\nПродолжить?";

            var result = MessageBox.Show(message, "Подтверждение создания отчёта", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Собираем стоимости всех заказов
            List<decimal> orderCosts = new List<decimal>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["OrderCost"].Value != null)
                {
                    if (decimal.TryParse(row.Cells["OrderCost"].Value.ToString(), out decimal cost))
                    {
                        orderCosts.Add(cost);
                    }
                }
            }

            // Экспортируем в Excel
            ExcelReport.ExportToExcel(
                dataGridView1,
                orderCosts,
                dateTimePicker1.Value,
                dateTimePicker2.Value,
                textBox1.Text,
                selectedStatus,
                selectedSort
            );
        }

        /// <summary>
        /// Проверяет установлен ли Microsoft Excel на компьютере
        /// </summary>
        private bool IsExcelInstalled()
        {
            try
            {
                Type excelType = Type.GetTypeFromProgID("Excel.Application");
                if (excelType == null)
                {
                    return false;
                }

                object excelApp = Activator.CreateInstance(excelType);
                if (excelApp != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
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
        /// При изменении сортировки - применяет фильтры
        /// </summary>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataManipulation == null)
            {
                return;
            }

            dataManipulation.ApplyAllDirector(comboBox3, comboBox1, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            dataGridView1.Refresh();
        }

        /// <summary>
        /// При изменении фильтра по статусу - применяет фильтры
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataManipulation == null)
            {
                return;
            }

            dataManipulation.ApplyAllDirector(comboBox3, comboBox1, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            dataGridView1.ClearSelection();
            dataGridView1.Refresh();
        }

        /// <summary>
        /// Кнопка "Сброс фильтров" - очищает все фильтры и загружает заново
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            dataManipulation.ResetFilters(comboBox3, comboBox1, textBox1);
            SetDatePickerRange();
            GetDate();

            dataGridView1.ClearSelection();
            dataGridView1.Refresh();
        }

        /// <summary>
        /// Ограничивает ввод в поле поиска только цифрами
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyNumbers(e);
        }

        /// <summary>
        /// Двойной клик на строку таблицы - открывает форму со составом заказа
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
        /// Правый клик на таблице - выделяет строку для контекстного меню
        /// </summary>
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
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
        /// При изменении начальной даты - обновляет минимальную дату конечной и перезагружает данные
        /// </summary>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.MinDate = dateTimePicker1.Value;
            GetDate();

            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// При изменении конечной даты - перезагружает данные
        /// </summary>
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            GetDate();

            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// Форматирует отображение ячеек - окрашивает по статусу, маскирует/показывает имена
        /// </summary>
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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