using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WebSiteDev.AddForm;

namespace WebSiteDev.AdminForm
{
    /// <summary>
    /// Пользовательский контрол для управления ролями пользователей
    /// Позволяет просматривать, добавлять, редактировать и удалять роли
    /// </summary>
    public partial class RoleControl : UserControl
    {
        private DataManipulation dataManipulation;
        public bool update = false;
        private int selectedRoleID = -1;
        private int selectedRowIndex = -1;

        /// <summary>
        /// Инициализирует контрол и загружает данные ролей
        /// </summary>
        public RoleControl()
        {
            InitializeComponent();
            GetDate();
        }

        /// <summary>
        /// Обработчик загрузки контрола
        /// Очищает выделение в таблице
        /// </summary>
        private void RoleControl_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// Загружает все роли из БД и отображает их в таблице
        /// </summary>
        void GetDate()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                // Получаем все роли из БД
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `Role`", con);
                cmd.ExecuteNonQuery();

                // Заполняем таблицу данными
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                dataGridView1.DataSource = dt;
                // Скрываем столбец ID
                dataGridView1.Columns["RoleID"].Visible = false;
                // Устанавливаем текст заголовка столбца
                dataGridView1.Columns["RoleName"].HeaderText = "Наименование роли";
                // Отключаем сортировку при клике на заголовок
                dataGridView1.Columns["RoleName"].SortMode = DataGridViewColumnSortMode.NotSortable;

                dataManipulation = new DataManipulation(dt);

                // Получаем количество ролей и выводим в метку
                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM Role", con);
                int resultcount = Convert.ToInt32(count.ExecuteScalar());
                label1.Text = $"Количество записей: {resultcount}";
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// Применяет фильтр и обновляет таблицу
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Применяем фильтр по названию роли
            dataManipulation.ApplySearchRole(textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);
            // Форматируем первую букву
            InputRest.FirstLetter(textBox1);

            // Очищаем выделение и поле редактирования
            dataGridView1.ClearSelection();
            textBox2.Clear();
            selectedRoleID = -1;
            selectedRowIndex = -1;
        }

        /// <summary>
        /// Обработчик кнопки расширения окна для редактирования
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1500);
            update = true;
            button1.Enabled = false;
        }

        /// <summary>
        /// Обработчик кнопки сжатия окна после редактирования
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            FormControl.Resize(this.FindForm(), 1175);
            update = true;
            button1.Enabled = true;
        }

        /// <summary>
        /// Обработчик кнопки добавления новой роли
        /// Открывает диалоговую форму и перезагружает данные
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            AddRoleForm addRoleForm = new AddRoleForm();
            addRoleForm.ShowDialog();
            // Перезагружаем данные после добавления
            GetDate();
            dataGridView1.ClearSelection();
            ClearRoleFields();
        }

        /// <summary>
        /// Обработчик кнопки сброса фильтров
        /// Возвращает таблицу в исходное состояние
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            // Сбрасываем все фильтры
            dataManipulation.ResetFilters(textSearch: textBox1);

            // Очищаем выделение
            dataGridView1.ClearSelection();
            textBox2.Clear();
            selectedRoleID = -1;
            selectedRowIndex = -1;
        }

        /// <summary>
        /// Применяет форматирование первой буквы при вводе названия роли
        /// </summary>
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox2);
        }

        /// <summary>
        /// Ограничивает ввод в поле поиска только русскими буквами
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        /// <summary>
        /// Ограничивает ввод в поле редактирования только русскими буквами
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyRussian(e);
        }

        /// <summary>
        /// Обработчик клика по ячейке в таблице
        /// Загружает данные выбранной роли в поле редактирования
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Сохраняем индекс выбранной строки
                selectedRowIndex = e.RowIndex;
                // Получаем ID роли из первого столбца
                selectedRoleID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["RoleID"].Value);
                // Выводим название роли в поле редактирования
                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["RoleName"].Value.ToString();
            }
        }

        /// <summary>
        /// Обработчик кнопки редактирования выбранной роли
        /// Проверяет выбор, запрашивает подтверждение и обновляет в БД
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            // Проверяем что роль выбрана
            if (selectedRoleID == -1 || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Выберите роль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите изменить роль?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Обновляем роль в БД
            if (DataUpdate.UpdateRole(selectedRoleID, textBox2.Text.Trim()))
            {
                MessageBox.Show("Роль успешно изменена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GetDate();

                // Восстанавливаем выделение строки после перезагрузки
                if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.Rows.Count)
                {
                    dataGridView1.Rows[selectedRowIndex].Selected = true;
                    textBox2.Text = dataGridView1.Rows[selectedRowIndex].Cells["RoleName"].Value.ToString();
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки удаления выбранной роли
        /// Проверяет выбор, запрашивает подтверждение и удаляет из БД
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            // Проверяем что роль выбрана
            if (selectedRoleID == -1)
            {
                MessageBox.Show("Выберите роль для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите удалить роль?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Удаляем роль из БД
            if (DataDelete.DeleteRole(selectedRoleID))
            {
                MessageBox.Show("Роль успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedRoleID = -1;
                selectedRowIndex = -1;
                textBox2.Clear();
                // Перезагружаем данные
                GetDate();
                dataGridView1.ClearSelection();
            }
        }

        /// <summary>
        /// Очищает поля редактирования и сбрасывает ID выбранной роли
        /// </summary>
        private void ClearRoleFields()
        {
            selectedRoleID = -1;
            textBox2.Clear();
        }
    }
}