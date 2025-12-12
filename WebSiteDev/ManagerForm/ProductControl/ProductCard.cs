using System;
using System.Data;
using System.Windows.Forms;

namespace WebSiteDev
{
    public partial class ProductCard : UserControl
    {
        // События для взаимодействия с родительским контролом
        public event EventHandler EditButtonClicked;
        public event EventHandler DeleteButtonClicked;
        public event EventHandler AddToCartClicked;
        public event EventHandler CancelEditClicked;
        public event EventHandler SaveButtonClicked;

        private string originalImagePath;

        /// <summary>
        /// Данные товара из таблицы БД
        /// </summary>
        public DataRowView RowData { get; set; }

        public ProductCard()
        {
            InitializeComponent();
        }

        public void InitializeCard(DataRowView row, string userRole)
        {
            RowData = row;

            // Загружаем изображение товара
            imageControl1.InitializeImage(row["ProductPhoto"].ToString());

            string productName = row["ProductName"].ToString();
            string productDesc = row["ProductDescription"].ToString();

            label1.Text = productName;
            label2.Text = productDesc;

            string categoryName = row["Category"].ToString();
            label3.Text = "Категория: " + categoryName;

            decimal price = Convert.ToDecimal(row["BasePrice"]);
            label4.Text = "Цена: " + price.ToString("0.00") + " руб.";

            // Для менеджеров показываем кнопку "Добавить в корзину", для администраторов - редактировать/удалить
            if (userRole == "Менеджер")
            {
                button1.Visible = false;
                button2.Visible = false;
                button6.Visible = true;
            }
            else
            {
                button1.Text = "Редактировать";
                button2.Text = "Удалить";
                button1.Visible = true;
                button2.Visible = true;
                button6.Visible = false;
            }

            // Если описание длинное - показываем кнопку "Полное описание"
            if (productDesc.Length > 285)
            {
                button5.Visible = true;
            }
            else
            {
                button5.Visible = false;
            }

            button5.ContextMenuStrip = null;
        }

        /// <summary>
        /// Кнопка "Полное описание" - открывает отдельное окно с полным описанием товара
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            string productDesc = RowData["ProductDescription"].ToString();

            if (productDesc.Length > 285)
            {
                DescriptionProduct descForm = new DescriptionProduct();
                descForm.SetDescription(RowData["ProductName"].ToString(), productDesc);
                descForm.ShowDialog();
            }
        }

        /// <summary>
        /// Кнопка "Добавить в корзину" - вызывает событие для добавления товара в корзину
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            if (AddToCartClicked != null)
            {
                AddToCartClicked(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            }
        }

        /// <summary>
        /// Кнопка "Редактировать" - вызывает событие редактирования товара
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (EditButtonClicked != null)
            {
                EditButtonClicked(this, e);
            }
        }

        /// <summary>
        /// Кнопка "Удалить" - вызывает событие удаления товара
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (DeleteButtonClicked != null)
            {
                DeleteButtonClicked(this, e);
            }
        }

        /// <summary>
        /// Кнопка "Сохранить" - вызывает событие сохранения изменений товара
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (SaveButtonClicked != null)
            {
                SaveButtonClicked(this, e);
            }
        }

        /// <summary>
        /// Кнопка "Отмена" - отменяет редактирование и восстанавливает оригинальное изображение
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            ImageControl img = GetImageControl();
            if (img != null)
            {
                img.CancelEdit();
                img.InitializeImage(originalImagePath);
            }

            if (CancelEditClicked != null)
            {
                CancelEditClicked(this, e);
            }

            HideEditMode();
        }

        public void ShowEditMode(DataManipulation dataManipulation)
        {
            // Сохраняем оригинальный путь изображения для возможности отката
            originalImagePath = RowData["ProductPhoto"].ToString();

            // Загружаем текущие значения в поля редактирования
            textBox1.Text = label1.Text;
            textBox2.Text = label2.Text;

            // Разбиваем цену на рубли и копейки
            decimal price = Convert.ToDecimal(RowData["BasePrice"]);
            int rubles = (int)price;
            int kopecks = (int)Math.Round((price - rubles) * 100, MidpointRounding.AwayFromZero);

            textBox3.Text = rubles.ToString();
            numericUpDown1.Value = kopecks;

            // Получаем текущую категорию
            string categoryName = label3.Text.Replace("Категория: ", "");

            comboBox1.Items.Clear();

            // Заполняем список категорий из всех товаров (без дублей)
            DataTable fullTable = dataManipulation.table;

            for (int i = 0; i < fullTable.Rows.Count; i++)
            {
                string cat = fullTable.Rows[i]["Category"].ToString();

                bool categoryExists = false;
                for (int j = 0; j < comboBox1.Items.Count; j++)
                {
                    if (comboBox1.Items[j].ToString() == cat)
                    {
                        categoryExists = true;
                        break;
                    }
                }

                if (!categoryExists)
                {
                    comboBox1.Items.Add(cat);
                }
            }

            // Выбираем текущую категорию
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                if (comboBox1.Items[i].ToString() == categoryName)
                {
                    comboBox1.SelectedIndex = i;
                    break;
                }
            }

            // Скрываем элементы режима просмотра
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            button5.Visible = false;
            button6.Visible = false;

            // Показываем элементы режима редактирования
            textBox1.Visible = true;
            textBox2.Visible = true;
            comboBox1.Visible = true;
            textBox3.Visible = true;
            numericUpDown1.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
        }

        public void HideEditMode()
        {
            // Скрываем элементы режима редактирования
            textBox1.Visible = false;
            textBox2.Visible = false;
            comboBox1.Visible = false;
            textBox3.Visible = false;
            numericUpDown1.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            button3.Visible = false;
            button4.Visible = false;

            // Показываем элементы режима просмотра
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            button1.Visible = true;
            button2.Visible = true;

            // Показываем кнопку полного описания если описание длинное
            string productDesc = RowData["ProductDescription"].ToString();
            if (productDesc.Length > 150)
            {
                button5.Visible = true;
            }
        }

        public ImageControl GetImageControl()
        {
            return imageControl1;
        }

        public void UpdateAddToCartButtonState(bool isProductInCart, string userRole)
        {
            if (userRole != "Менеджер")
            {
                return;
            }

            button6.Enabled = true;
            if (isProductInCart)
            {
                button6.Enabled = false;
            }
        }

        private void ProductCard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (AddToCartClicked != null)
                {
                    AddToCartClicked(this, e);
                }
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ProductCard_MouseDown(this, e);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.OnlyNumbers(e);

            // Не позволяем вводить ноль если поле пусто или уже содержит ноль
            if ((textBox3.Text.Length == 0 || textBox3.Text == "0") && e.KeyChar == '0' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox1);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox2);
        }

        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;

            InputRest.OnlyNumbers(e);

            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            string currentText = nud.Text;

            // Не позволяем вводить более 2 символов
            if (currentText.Length >= 2)
            {
                e.Handled = true;
                return;
            }

            string newText = currentText.Insert(currentText.Length, e.KeyChar.ToString());
            if (int.TryParse(newText, out int value))
            {
                // Не позволяем значения больше 99
                if (value > 99)
                {
                    e.Handled = true;
                }
            }
        }

        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip = null;
            }
        }
    }
}
