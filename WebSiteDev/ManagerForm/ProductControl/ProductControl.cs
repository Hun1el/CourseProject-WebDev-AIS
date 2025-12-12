using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WebSiteDev.AddForm;

namespace WebSiteDev.ManagerForm
{
    public partial class ProductControl : UserControl
    {
        private DataManipulation dataManipulation;
        private string userRole;
        public bool update = false;
        private ProductCard selectedCard;
        private int editingProductID = -1;
        private int batchSize = 10;
        private int currentIndex = 0;

        public static int CurrentUserID { get; set; } = 0;
        public static string CurrentUserName { get; set; } = "";

        /// <summary>
        /// Класс для хранения информации о товаре в корзине
        /// </summary>
        public class OrderItem
        {
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public decimal BasePrice { get; set; }
            public int Quantity { get; set; }
            public string ProductPhoto { get; set; }
        }

        /// <summary>
        /// Статический класс для управления текущим заказом
        /// </summary>
        public static class CurrentOrder
        {
            public static BindingList<OrderItem> Items { get; set; } = new BindingList<OrderItem>();
            public static void Clear() { Items.Clear(); }
        }

        /// <summary>
        /// Конструктор - инициализирует контрол и загружает данные
        /// </summary>
        public ProductControl(string role, int userID = 0, string userName = "")
        {
            InitializeComponent();
            userRole = role;
            CurrentUserID = userID;
            CurrentUserName = userName;

            GetData();
            EnableLazyLoading();
            flowPanel.MouseWheel += flowPanel_MouseWheel;
        }

        /// <summary>
        /// При загрузке контрола - скрывает кнопки в зависимости от роли
        /// </summary>
        private void ProductControl_Load(object sender, EventArgs e)
        {
            // Менеджеры не могут добавлять новые товары
            if (userRole == "Менеджер")
            {
                button2.Visible = false;
            }
            else
            {
                // Если это администратор и находится в режиме просмотра услуг, скрываем кнопку просмотра заказа
                Form parentForm = this.FindForm();
                if (parentForm != null && parentForm.Text == "Список услуг")
                {
                    button1.Visible = false;
                }
            }

            comboBox3.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            CurrentOrder.Clear();
            RefreshProductCardStates();
            UpdateOrderButtonVisibility();
        }

        /// <summary>
        /// Загружает все товары из БД в DataTable
        /// </summary>
        private void GetData()
        {
            // Очищаем старый DataManipulation если он есть
            if (dataManipulation != null)
            {
                if (dataManipulation.table != null)
                {
                    dataManipulation.table.Clear();
                    dataManipulation.table.Dispose();
                }
                dataManipulation = null;
            }

            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(@"SELECT p.ProductID, p.ProductName, p.ProductDescription, p.ProductPhoto,
                    c.CategoryName AS Category, p.BasePrice, p.CategoryID FROM Product p JOIN Category c ON p.CategoryID = c.CategoryID", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataManipulation = new DataManipulation(dt);
                dataManipulation.FillComboBoxWithCategories(comboBox1, "Все категории");

                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM Product", con);
                label1.Text = "Количество записей: " + count.ExecuteScalar();
            }
        }

        private void RefreshData()
        {
            // Отписываем события перед удалением карточек
            foreach (Control control in flowPanel.Controls)
            {
                if (control is ProductCard card)
                {
                    card.EditButtonClicked -= Card_EditButtonClicked;
                    card.DeleteButtonClicked -= Card_DeleteButtonClicked;
                    card.AddToCartClicked -= Card_AddToCartClicked;
                    card.CancelEditClicked -= Card_CancelEditClicked;
                    card.Dispose();
                }
            }
            flowPanel.Controls.Clear();

            // Очищаем старый DataManipulation
            if (dataManipulation != null)
            {
                if (dataManipulation.table != null)
                {
                    dataManipulation.table.Clear();
                    dataManipulation.table.Dispose();
                }
                dataManipulation = null;
            }

            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                con.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(@"SELECT p.ProductID, p.ProductName, p.ProductDescription, p.ProductPhoto,
            c.CategoryName AS Category, p.BasePrice, p.CategoryID FROM Product p JOIN Category c ON p.CategoryID = c.CategoryID", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataManipulation = new DataManipulation(dt);

                MySqlCommand count = new MySqlCommand("SELECT COUNT(*) FROM Product", con);
                label1.Text = "Количество записей: " + count.ExecuteScalar();
            }

            currentIndex = 0;
            flowPanel.Controls.Clear();
            LoadNextBatch();
        }

        private ProductCard CreateProductCard(DataRowView row)
        {
            ProductCard card = new ProductCard();
            card.RowData = row;
            card.Margin = new Padding(10);

            // Менеджеры видят контекстное меню для добавления в корзину
            if (userRole == "Менеджер")
            {
                card.ContextMenuStrip = contextMenuStrip1;
            }

            card.InitializeCard(row, userRole);

            // Проверяем находится ли товар уже в корзине
            int productID = Convert.ToInt32(row["ProductID"]);
            bool isInCart = IsProductInCart(productID);
            card.UpdateAddToCartButtonState(isInCart, userRole);

            card.EditButtonClicked += Card_EditButtonClicked;
            card.DeleteButtonClicked += Card_DeleteButtonClicked;
            card.AddToCartClicked += Card_AddToCartClicked;
            card.CancelEditClicked += Card_CancelEditClicked;

            return card;
        }

        private bool IsProductInCart(int productID)
        {
            foreach (var item in CurrentOrder.Items)
            {
                if (item.ProductID == productID)
                {
                    return true;
                }
            }
            return false;
        }

        private void Card_EditButtonClicked(object sender, EventArgs e)
        {
            StartEdit(sender as ProductCard);
        }

        private void Card_DeleteButtonClicked(object sender, EventArgs e)
        {
            DeleteProduct(sender as ProductCard);
        }

        private void Card_AddToCartClicked(object sender, EventArgs e)
        {
            ProductCard card = sender as ProductCard;

            if (card == null)
            {
                return;
            }

            // Только менеджеры могут добавлять в корзину
            if (userRole != "Менеджер")
            {
                return;
            }

            MouseEventArgs me = e as MouseEventArgs;
            selectedCard = card;

            // Левый клик - добавляем сразу, правый клик - открываем контекстное меню
            if (me != null && me.Button == MouseButtons.Left)
            {
                AddToCartDirect(card);
                return;
            }

            if (me != null && me.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(Control.MousePosition);
            }
        }

        private void AddToCartDirect(ProductCard card)
        {
            if (card == null)
            {
                return;
            }

            DataRowView row = card.RowData;
            int productID = Convert.ToInt32(row["ProductID"]);
            string productName = row["ProductName"].ToString();
            decimal basePrice = Convert.ToDecimal(row["BasePrice"]);

            // Проверяем нет ли товара уже в корзине
            foreach (OrderItem item in CurrentOrder.Items)
            {
                if (item.ProductID == productID)
                {
                    MessageBox.Show("Товар \"" + productName + "\" уже в корзине.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Рассчитываем текущую сумму заказа
            decimal currentTotal = 0;
            foreach (OrderItem item in CurrentOrder.Items)
            {
                currentTotal += item.BasePrice * item.Quantity;
            }

            decimal newTotal = currentTotal + basePrice;
            decimal newTotalWithSurcharge = Math.Round(newTotal * 1.15m, 2);
            decimal maxLimit = 9999999999.99m;

            // Проверяем не превышены ли лимиты суммы (с учётом надбавки 15%)
            if (newTotalWithSurcharge > maxLimit)
            {
                MessageBox.Show(
                    "Невозможно добавить товар!\n\n" +
                    "Сумма заказа с учётом надбавки 15% превысит допустимый лимит заказа (9 999 999 999.99 руб.).\n" +
                    "Текущая сумма: " + currentTotal.ToString("N2") + " руб.\n" +
                    "Будет после добавления: " + newTotal.ToString("N2") + " руб. (без надбавки)\n" +
                    "С учётом надбавки: " + newTotalWithSurcharge.ToString("N2") + " руб.",
                    "Превышение лимита",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Создаём новый товар и добавляем в корзину
            OrderItem newItem = new OrderItem();
            newItem.ProductID = productID;
            newItem.ProductName = productName;
            newItem.BasePrice = basePrice;
            newItem.CategoryName = row["Category"].ToString();
            newItem.Quantity = 1;
            newItem.ProductPhoto = row["ProductPhoto"].ToString();

            CurrentOrder.Items.Add(newItem);

            // Обновляем состояние кнопки карточки
            card.UpdateAddToCartButtonState(true, "Менеджер");
            UpdateOrderButtonVisibility();
        }

        private void Card_CancelEditClicked(object sender, EventArgs e)
        {
            ProductCard card = sender as ProductCard;
            editingProductID = -1;
            card.button3.Click -= SaveProduct;

            // Восстанавливаем оригинальное изображение
            ImageControl img = card.GetImageControl();
            if (img != null)
            {
                img.ShowChangeButton(false);
                img.CancelEdit();
                img.InitializeImage(card.RowData["ProductPhoto"].ToString());
            }

            card.HideEditMode();
        }

        private void StartEdit(ProductCard card)
        {
            if (card == null)
            {
                return;
            }

            DataRowView row = card.RowData;
            int productID = Convert.ToInt32(row["ProductID"]);

            // Не позволяем редактировать два товара одновременно
            if (editingProductID != -1 && editingProductID != productID)
            {
                MessageBox.Show("Уже редактируется другой товар! Завершите редактирование.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            editingProductID = productID;

            // Инициализируем контрол изображения
            ImageControl imageControl = card.GetImageControl();

            if (imageControl != null)
            {
                imageControl.InitializeImage(row["ProductPhoto"].ToString());
                imageControl.ShowChangeButton(true);
            }

            // Переводим карточку в режим редактирования
            card.ShowEditMode(dataManipulation);
            card.button3.Tag = new object[] { productID, card.textBox1, card.textBox2, card.comboBox1, card.textBox3, card };
            card.button3.Click -= SaveProduct;
            card.button3.Click += SaveProduct;
        }

        private void SaveProduct(object sender, EventArgs e)
        {
            object[] data = (sender as Button).Tag as object[];

            if (data == null)
            {
                return;
            }

            int productID = Convert.ToInt32(data[0]);
            TextBox textBox1 = data[1] as TextBox;
            TextBox textBox2 = data[2] as TextBox;
            ComboBox comboBox = data[3] as ComboBox;
            TextBox textBox3 = data[4] as TextBox;
            ProductCard card = data[5] as ProductCard;

            // Проверяем корректность введённых данных
            if (!ValidateProductData(textBox1, textBox2, textBox3, card.numericUpDown1, comboBox))
            {
                return;
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите изменить услугу?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                // Отменяем редактирование если пользователь отказал
                ImageControl imgCancel = card.GetImageControl();
                if (imgCancel != null)
                {
                    imgCancel.ShowChangeButton(false);
                    imgCancel.CancelEdit();
                    imgCancel.InitializeImage(card.RowData["ProductPhoto"].ToString());
                }

                editingProductID = -1;
                card.button3.Click -= SaveProduct;
                card.HideEditMode();
                return;
            }

            // Получаем ID категории по названию
            string categoryName = comboBox.SelectedItem.ToString();
            int categoryID = GetCategoryID(categoryName);

            if (categoryID == 0)
            {
                MessageBox.Show("Категория не найдена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Сохраняем изображение товара
            ImageControl imageControl = card.GetImageControl();

            if (imageControl != null)
            {
                imageControl.SaveImage(productID);
            }

            // Собираем цену из рублей и копеек
            int rubles = 0;
            int kopecks = 0;

            int.TryParse(textBox3.Text, out rubles);
            kopecks = Convert.ToInt32(card.numericUpDown1.Value);

            decimal price = rubles + (kopecks / 100.0m);

            // Обновляем товар в БД
            if (DataUpdate.UpdateProduct(productID, textBox1.Text.Trim(), textBox2.Text.Trim(), categoryID, price))
            {
                MessageBox.Show("Услуга успешно изменена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                editingProductID = -1;
                card.button3.Click -= SaveProduct;

                ImageControl img = card.GetImageControl();

                if (img != null)
                {
                    img.ShowChangeButton(false);
                }

                card.HideEditMode();

                // Сохраняем текущие фильтры и применяем их заново после обновления
                int savedFilterIndex = comboBox1.SelectedIndex;
                string savedSearchText = textBox1.Text;
                int savedSortIndex = comboBox3.SelectedIndex;

                RefreshData();

                comboBox1.SelectedIndex = savedFilterIndex;
                textBox1.Text = savedSearchText;
                comboBox3.SelectedIndex = savedSortIndex;

                ApplyFilters();
            }
        }

        /// <summary>
        /// Удаляет товар из БД
        /// </summary>
        private void DeleteProduct(ProductCard card)
        {
            if (card == null)
            {
                return;
            }

            // Запрашиваем подтверждение удаления
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить услугу?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            int productID = Convert.ToInt32(card.RowData["ProductID"]);

            // Удаляем товар из БД
            if (DataDelete.DeleteProduct(productID))
            {
                MessageBox.Show("Услуга удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Сохраняем текущие фильтры и применяем их заново
                int savedFilterIndex = comboBox1.SelectedIndex;
                string savedSearchText = textBox1.Text;
                int savedSortIndex = comboBox3.SelectedIndex;

                RefreshData();

                comboBox1.SelectedIndex = savedFilterIndex;
                textBox1.Text = savedSearchText;
                comboBox3.SelectedIndex = savedSortIndex;

                ApplyFilters();
            }
        }

        private bool ValidateProductData(TextBox name, TextBox description, TextBox rubles, NumericUpDown kopecks, ComboBox category)
        {
            // Проверяем что все элементы переданы
            if (name == null || description == null || rubles == null || kopecks == null || category == null)
            {
                return false;
            }

            // Проверяем что все поля заполнены
            if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(description.Text) ||
                string.IsNullOrWhiteSpace(rubles.Text) || category.SelectedIndex < 0)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверяем минимальную длину названия
            if (name.Text.Length < 3)
            {
                MessageBox.Show("Название услуги должно быть минимум 3 символа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверяем минимальную длину описания
            if (description.Text.Length < 10)
            {
                MessageBox.Show("Описание должно быть минимум 10 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверяем что рубли - это число
            if (!int.TryParse(rubles.Text, out int rublesValue))
            {
                MessageBox.Show("Рубли должны быть числом!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверяем что рубли не отрицательные
            if (rublesValue < 0)
            {
                MessageBox.Show("Рубли не могут быть отрицательными!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Проверяем что цена больше нуля
            if (rublesValue == 0 && kopecks.Value == 0)
            {
                MessageBox.Show("Цена должна быть больше нуля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private int GetCategoryID(string categoryName)
        {
            if (dataManipulation == null || string.IsNullOrEmpty(categoryName))
            {
                return 0;
            }

            // Ищем категорию в таблице
            foreach (DataRow row in dataManipulation.table.Rows)
            {
                if (row["Category"].ToString() == categoryName)
                {
                    return Convert.ToInt32(row["CategoryID"]);
                }
            }

            return 0;
        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            if (selectedCard == null)
            {
                return;
            }

            DataRowView row = selectedCard.RowData;
            int productID = Convert.ToInt32(row["ProductID"]);
            string productName = row["ProductName"].ToString();
            decimal basePrice = Convert.ToDecimal(row["BasePrice"]);

            // Проверяем нет ли товара уже в корзине
            foreach (OrderItem item in CurrentOrder.Items)
            {
                if (item.ProductID == productID)
                {
                    contextMenuStrip1.Close();
                    MessageBox.Show("Товар \"" + productName + "\" уже в корзине.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Рассчитываем текущую сумму заказа
            decimal currentTotal = 0;
            foreach (OrderItem item in CurrentOrder.Items)
            {
                currentTotal += item.BasePrice * item.Quantity;
            }

            decimal newTotal = currentTotal + basePrice;
            decimal newTotalWithSurcharge = Math.Round(newTotal * 1.15m, 2);
            decimal maxLimit = 9999999999.99m;

            // Проверяем не превышены ли лимиты суммы
            if (newTotalWithSurcharge > maxLimit)
            {
                contextMenuStrip1.Close();
                MessageBox.Show(
                    "Невозможно добавить товар!\n\n" +
                    "Сумма заказа с учётом надбавки 15% превысит допустимый лимит заказа (9 999 999 999.99 руб.).\n" +
                    "Текущая сумма: " + currentTotal.ToString("N2") + " руб.\n" +
                    "Будет после добавления: " + newTotal.ToString("N2") + " руб. (без надбавки)\n" +
                    "С учётом надбавки: " + newTotalWithSurcharge.ToString("N2") + " руб.",
                    "Превышение лимита",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Добавляем товар в корзину
            OrderItem newItem = new OrderItem();
            newItem.ProductID = productID;
            newItem.ProductName = productName;
            newItem.BasePrice = basePrice;
            newItem.CategoryName = row["Category"].ToString();
            newItem.Quantity = 1;
            newItem.ProductPhoto = row["ProductPhoto"].ToString();

            CurrentOrder.Items.Add(newItem);

            selectedCard.UpdateAddToCartButtonState(true, "Менеджер");
            UpdateOrderButtonVisibility();
            contextMenuStrip1.Close();
        }

        /// <summary>
        /// Ленивая загрузка
        /// </summary>
        private void LoadNextBatch()
        {
            flowPanel.SuspendLayout();

            int count = 0;
            while (currentIndex < dataManipulation.view.Count && count < batchSize)
            {
                flowPanel.Controls.Add(CreateProductCard(dataManipulation.view[currentIndex]));
                currentIndex++;
                count++;
            }

            flowPanel.ResumeLayout();
        }

        private void EnableLazyLoading()
        {
            currentIndex = 0;
            flowPanel.Controls.Clear();
            LoadNextBatch();
        }

        /// <summary>
        /// Возвращает правильное окончание слова в зависимости от количества
        /// </summary>
        private string GetWordEnding(int count)
        {
            int mod = count % 100;
            if (mod >= 11 && mod <= 19)
            {
                return "товаров";
            }

            int last = count % 10;
            if (last == 1)
            {
                return "товар";
            }
            if (last == 2 || last == 3 || last == 4)
            {
                return "товара";
            }
            return "товаров";
        }

        public void UpdateOrderButtonVisibility()
        {
            if (button1 == null)
            {
                return;
            }

            button1.ForeColor = Color.White;
            button1.BackColor = Color.FromArgb(45, 156, 219);

            // Рассчитываем общее количество товаров в корзине
            int totalQuantity = 0;
            foreach (OrderItem item in CurrentOrder.Items)
            {
                totalQuantity += item.Quantity;
            }

            // Если в корзине есть товары - показываем кнопку с количеством
            if (totalQuantity > 0)
            {
                button1.Visible = true;
                button1.Text = "Просмотр заказа\n(" + totalQuantity + " " + GetWordEnding(totalQuantity) + ")";
                button1.Enabled = true;
            }
            else
            {
                button1.Visible = false;
                button1.Text = "Просмотр заказа";
                button1.Enabled = false;
            }
        }

        /// <summary>
        /// Применяет фильтры и сортировку
        /// </summary>
        private void ApplyFilters()
        {
            dataManipulation.ApplyAllProduct(comboBox3, comboBox1, textBox1);
            currentIndex = 0;
            flowPanel.Controls.Clear();
            dataManipulation.UpdateRecordCountLabel(label1);
            LoadNextBatch();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            InputRest.FirstLetter(textBox1);
            ApplyFilters();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.AllowAll(e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BucketForm bucketForm = new BucketForm(dataManipulation, CurrentUserID, CurrentUserName);
            bucketForm.ShowDialog();

            RefreshProductCardStates();
            UpdateOrderButtonVisibility();
        }

        /// <summary>
        /// Кнопка "Добавить товар" - открывает форму для добавления нового товара
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            AddProductForm addProductForm = new AddProductForm(dataManipulation);
            addProductForm.ShowDialog();
            GetData();
            EnableLazyLoading();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                FormControl.Resize(parentForm, 1175);
            }
            update = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dataManipulation.ResetFilters(comboSort: comboBox3, comboFilter: comboBox1, textSearch: textBox1);
            dataManipulation.ApplyAllProduct(comboBox3, comboBox1, textBox1);
            ApplyFilters();
        }

        /// <summary>
        /// При скроллинге таблицы - загружает ещё товары если достигнут конец
        /// </summary>
        private void flowPanel_Scroll(object sender, ScrollEventArgs e)
        {
            if (flowPanel.VerticalScroll.Value + flowPanel.ClientSize.Height >= flowPanel.VerticalScroll.Maximum - 50)
            {
                LoadNextBatch();
            }
        }

        /// <summary>
        /// При прокрутке колёсико мыши - загружает ещё товары если достигнут конец
        /// </summary>
        private void flowPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (flowPanel.VerticalScroll.Value + flowPanel.ClientSize.Height >= flowPanel.VerticalScroll.Maximum - 50)
            {
                LoadNextBatch();
            }
        }

        public void RefreshProductCardStates()
        {
            foreach (Control control in flowPanel.Controls)
            {
                ProductCard card = control as ProductCard;

                if (card == null)
                {
                    continue;
                }

                int productID = Convert.ToInt32(card.RowData["ProductID"]);
                bool isInCart = false;

                // Ищем товар в корзине
                for (int i = 0; i < CurrentOrder.Items.Count; i++)
                {
                    if (CurrentOrder.Items[i].ProductID == productID)
                    {
                        isInCart = true;
                        break;
                    }
                }

                card.UpdateAddToCartButtonState(isInCart, userRole);
            }
        }
    }
}
