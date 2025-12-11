using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using WebSiteDev.AddForm;

namespace WebSiteDev.AdminForm
{
    /// <summary>
    /// Пользовательский контрол для управления категориями товаров
    /// Позволяет просматривать, добавлять, редактировать и удалять категории
    /// </summary>
    public partial class CategoryControl : UserControl
    {
        private DataManipulation dataManipulation;
        public bool update = false;
        private int selectedCategoryID = -1;
        private int selectedRowIndex = -1;

        /// <summary>
        /// Инициализирует контрол и загружает данные категорий
        /// </summary>
        public CategoryControl()
        {
            InitializeComponent();
            GetDate();
        }

        /// <summary>
        /// Обработчик загрузки контрола
        /// Устанавливает начальные значения для сортировки и очищает выделение
        /// </summary>
        private void CategoryControl_Load(object sender, EventArgs e)
        {
            comboBox3.SelectedIndex = 0;
            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// Загружает все категории из БД и отображает их в таблице
        /// </summary>
        public void GetDate()
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                // Получаем все категории из БД
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Category", con);
                cmd.ExecuteNonQuery();

                // Заполняем таблицу данными
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                dataGridView1.DataSource = dt;
                // Скрываем столбец ID
                dataGridView1.Columns["CategoryID"].Visible = false;
                // Устанавливаем текст заголовка столбца
                dataGridView1.Columns["CategoryName"].HeaderText = "Наименование категории";
                // Отключаем сортировку при клике на заголовок
                dataGridView1.Columns["CategoryName"].SortMode = DataGridViewColumnSortMode.NotSortable;

                dataManipulation = new DataManipulation(dt);

                // Получаем количество категорий и выводим в метку
                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM Category", con);
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
            // Применяем фильтр по названию категории
            dataManipulation.ApplyAllCategory(comboBox3, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);
            InputRest.FirstLetter(textBox1);

            // Очищаем выделение и поле редактирования
            dataGridView1.ClearSelection();
            textBox2.Clear();
            selectedCategoryID = -1;
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
        /// Обработчик кнопки добавления новой категории
        /// Открывает диалоговую форму и перезагружает данные
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            AddCategoryForm addCategoryForm = new AddCategoryForm();
            addCategoryForm.ShowDialog();
            // Перезагружаем данные после добавления
            GetDate();
            dataGridView1.ClearSelection();
            ClearCategoryFields();
        }

        /// <summary>
        /// Обработчик кнопки сброса фильтров
        /// Возвращает таблицу в исходное состояние
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            // Сбрасываем все фильтры
            dataManipulation.ResetFilters(textSearch: textBox1, comboSort: comboBox3);
            dataManipulation.ApplyAllCategory(comboBox3, textBox1);

            // Очищаем выделение
            dataGridView1.ClearSelection();
            textBox2.Clear();
            selectedCategoryID = -1;
            selectedRowIndex = -1;
        }

        /// <summary>
        /// Обработчик изменения выбора в комбо-боксе сортировки
        /// Применяет новый порядок сортировки
        /// </summary>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Применяем новую сортировку
            dataManipulation.ApplyAllCategory(comboBox3, textBox1);
            dataManipulation.UpdateRecordCountLabel(label1);

            // Очищаем выделение
            dataGridView1.ClearSelection();
            textBox2.Clear();
            selectedCategoryID = -1;
            selectedRowIndex = -1;
        }

        /// <summary>
        /// Применяет форматирование первой буквы при вводе названия категории
        /// </summary>
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox2);
        }

        /// <summary>
        /// Ограничивает ввод в поле поиска только допустимыми символами
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.CategoryInput(e);
        }

        /// <summary>
        /// Ограничивает ввод в поле редактирования только допустимыми символами
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.CategoryInput(e);
        }

        /// <summary>
        /// Обработчик клика по ячейке в таблице
        /// Загружает данные выбранной категории в поле редактирования
        /// </summary>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Сохраняем индекс выбранной строки
                selectedRowIndex = e.RowIndex;
                // Получаем ID категории из первого столбца
                selectedCategoryID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["CategoryID"].Value);
                // Выводим название категории в поле редактирования
                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["CategoryName"].Value.ToString();
            }
        }

        /// <summary>
        /// Обработчик кнопки редактирования выбранной категории
        /// Проверяет выбор, запрашивает подтверждение и обновляет в БД
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            // Проверяем что категория выбрана
            if (selectedCategoryID == -1 || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Выберите категорию!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите изменить категорию?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Обновляем категорию в БД
            if (DataUpdate.UpdateCategory(selectedCategoryID, textBox2.Text.Trim()))
            {
                MessageBox.Show("Категория изменена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GetDate();

                // Восстанавливаем выделение строки после перезагрузки
                if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.Rows.Count)
                {
                    dataGridView1.Rows[selectedRowIndex].Selected = true;
                    textBox2.Text = dataGridView1.Rows[selectedRowIndex].Cells["CategoryName"].Value.ToString();
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки удаления выбранной категории
        /// Проверяет выбор, запрашивает подтверждение и удаляет из БД
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            // Проверяем что категория выбрана
            if (selectedCategoryID == -1)
            {
                MessageBox.Show("Выберите категорию для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите удалить категорию?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Удаляем категорию из БД
            if (DataDelete.DeleteCategory(selectedCategoryID))
            {
                MessageBox.Show("Категория успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedCategoryID = -1;
                selectedRowIndex = -1;
                textBox2.Clear();
                // Перезагружаем данные
                GetDate();
                dataGridView1.ClearSelection();
            }
        }

        /// <summary>
        /// Очищает поля редактирования и сбрасывает ID выбранной категории
        /// </summary>
        private void ClearCategoryFields()
        {
            selectedCategoryID = -1;
            textBox2.Clear();
        }
    }
}