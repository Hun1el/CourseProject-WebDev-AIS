using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebSiteDev
{
    /// <summary>
    /// Форма для просмотра состава заказа - отображает все товары в заказе с их характеристиками
    /// </summary>
    public partial class OrderProductForm : Form
    {
        private int orderID;

        /// <summary>
        /// Конструктор - инициализирует форму с ID заказа
        /// </summary>
        public OrderProductForm(int orderID)
        {
            InitializeComponent();
            this.orderID = orderID;
            label1.Text = $"Состав заказа №{orderID}";
        }

        private void OrderProductForm_Load(object sender, EventArgs e)
        {
            Inactivity.OnFormLoad(this);

            LoadOrderProducts();
        }

        private void LoadOrderProducts()
        {
            flowLayoutPanel1.Controls.Clear();

            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Получаем основную информацию о заказе
                    string orderDataQuery = @"SELECT 
                        o.OrderDate, 
                        o.OrderCompDate,
                        CONCAT(u.Surname, ' ', u.FirstName, ' ', u.MiddleName) AS UserName,
                        CONCAT(c.Surname, ' ', c.FirstName, ' ', c.MiddleName) AS ClientName
                        FROM `Order` o
                        LEFT JOIN Users u ON o.UserID = u.UserID
                        LEFT JOIN Clients c ON o.ClientID = c.ClientID
                        WHERE o.OrderID = @OrderID";

                    MySqlCommand orderDataCmd = new MySqlCommand(orderDataQuery, con);
                    orderDataCmd.Parameters.AddWithValue("@OrderID", orderID);
                    MySqlDataReader orderDataReader = orderDataCmd.ExecuteReader();

                    // Выводим информацию о дате, сотруднике и клиенте
                    if (orderDataReader.Read())
                    {
                        label8.Text = $"Дата заказа: {Convert.ToDateTime(orderDataReader["OrderDate"]):dd.MM.yyyy}";
                        label7.Text = $"Срок выполнения: {Convert.ToDateTime(orderDataReader["OrderCompDate"]):dd.MM.yyyy}";
                        label6.Text = $"Сотрудник: {orderDataReader["UserName"]}";
                        label9.Text = $"Клиент: {orderDataReader["ClientName"]}";
                    }
                    orderDataReader.Close();

                    // Получаем скидку и надбавку для заказа
                    string discountQuery = "SELECT Discount, Surcharge FROM `Order` WHERE OrderID = @OrderID";
                    MySqlCommand discountCmd = new MySqlCommand(discountQuery, con);
                    discountCmd.Parameters.AddWithValue("@OrderID", orderID);
                    MySqlDataReader discountReader = discountCmd.ExecuteReader();

                    decimal totalDiscount = 0;
                    decimal totalSurcharge = 0;

                    if (discountReader.Read())
                    {
                        object discountVal = discountReader["Discount"];
                        object surchargeVal = discountReader["Surcharge"];

                        if (discountVal != DBNull.Value)
                        {
                            totalDiscount = Convert.ToDecimal(discountVal);
                        }

                        if (surchargeVal != DBNull.Value)
                        {
                            totalSurcharge = Convert.ToDecimal(surchargeVal);
                        }
                    }
                    discountReader.Close();

                    // Получаем все товары в заказе
                    string query = @"SELECT p.ProductID, p.ProductName, p.ProductPhoto, 
                                    c.CategoryName, op.ProductCount, op.ProductPrice
                                    FROM orderproduct op
                                    INNER JOIN product p ON op.ProductID = p.ProductID
                                    INNER JOIN category c ON p.CategoryID = c.CategoryID
                                    WHERE op.OrderID = @OrderID";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    decimal totalBasePrice = 0;
                    int itemCount = 0;

                    // Создаём панели для каждого товара
                    while (reader.Read())
                    {
                        itemCount++;

                        // Получаем цену товара
                        decimal productPrice = 0;
                        if (reader["ProductPrice"] != DBNull.Value)
                        {
                            productPrice = Convert.ToDecimal(reader["ProductPrice"]);
                        }
                        else
                        {
                            productPrice = 0;
                        }

                        int quantity = Convert.ToInt32(reader["ProductCount"]);
                        totalBasePrice += productPrice * quantity;

                        // Создаём панель с информацией о товаре
                        Panel productPanel = CreateProductPanel(
                            reader["ProductName"].ToString(),
                            reader["CategoryName"].ToString(),
                            reader["ProductPhoto"].ToString(),
                            productPrice,
                            quantity
                        );
                        flowLayoutPanel1.Controls.Add(productPanel);
                    }

                    reader.Close();

                    // Обновляем итоговую информацию
                    UpdateSummaryPanel(totalBasePrice, totalDiscount, totalSurcharge);

                    // Если товаров нет - показываем сообщение
                    if (itemCount == 0)
                    {
                        Label emptyLabel = new Label
                        {
                            Text = "Товары не найдены",
                            Font = new Font("Microsoft Sans Serif", 16),
                            ForeColor = Color.Gray,
                            Size = new Size(950, 300),
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        flowLayoutPanel1.Controls.Add(emptyLabel);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Обновляет итоговую информацию о стоимости - сумму, скидку, надбавку
        /// </summary>
        private void UpdateSummaryPanel(decimal totalBasePrice, decimal discount, decimal surcharge)
        {
            // Выводим сумму товаров
            label2.Text = $"Сумма товаров: {totalBasePrice:F2} руб.";

            // Показываем скидку если она есть
            if (discount > 0)
            {
                label3.Text = $"Скидка: -{discount:F2} руб.";
                label3.Visible = true;
            }
            else
            {
                label3.Visible = false;
            }

            // Показываем надбавку если она есть
            if (surcharge > 0)
            {
                label4.Text = $"Надбавка: +{surcharge:F2} руб.";
                label4.Visible = true;
            }
            else
            {
                label4.Visible = false;
            }

            // Рассчитываем и выводим итоговую сумму
            decimal finalTotal = totalBasePrice - discount + surcharge;
            label5.Text = $"Итого: {finalTotal:F2} руб.";
        }

        /// <summary>
        /// Создаёт панель с информацией о товаре - название, категория, цена, количество, фото
        /// </summary>
        private Panel CreateProductPanel(string productName, string categoryName, string photoPath, decimal price, int quantity)
        {
            Panel panel = new Panel
            {
                Size = new Size(970, 120),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(5)
            };

            // Загружаем и выводим фото товара
            PictureBox pic = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.no_image,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Пытаемся загрузить изображение товара из папки
            if (!string.IsNullOrEmpty(photoPath))
            {
                string imagePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebShop", "Images", photoPath);
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        using (var fs = new System.IO.FileStream(imagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            using (var img = Image.FromStream(fs))
                            {
                                pic.Image = new Bitmap(img, 100, 100);
                            }
                        }
                    }
                    catch { }
                }
            }

            panel.Controls.Add(pic);

            // Название товара
            Label labelName = new Label
            {
                Text = productName,
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                Location = new Point(120, 10),
                Size = new Size(500, 25),
                ForeColor = Color.Black,
                AutoSize = false
            };
            panel.Controls.Add(labelName);

            // Категория
            Label labelCategory = new Label
            {
                Text = $"Категория: {categoryName}",
                Font = new Font("Microsoft Sans Serif", 11),
                Location = new Point(120, 40),
                Size = new Size(400, 20),
                ForeColor = Color.Gray,
                AutoSize = false
            };
            panel.Controls.Add(labelCategory);

            // Цена за единицу
            Label labelPrice = new Label
            {
                Text = $"Цена: {price:F2} руб.",
                Font = new Font("Microsoft Sans Serif", 11),
                Location = new Point(120, 65),
                Size = new Size(180, 20),
                ForeColor = Color.Black,
                AutoSize = false
            };
            panel.Controls.Add(labelPrice);

            // Количество товара
            Label labelQuantity = new Label
            {
                Text = $"Количество: {quantity} шт.",
                Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold),
                Location = new Point(310, 65),
                Size = new Size(200, 20),
                ForeColor = Color.DarkGreen,
                AutoSize = false
            };
            panel.Controls.Add(labelQuantity);

            // Рассчитываем сумму товара
            decimal baseTotal = price * quantity;

            // Итоговая сумма за товар
            Label labelSubtotal = new Label
            {
                Text = $"Сумма: {baseTotal:F2} руб.",
                Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                Location = new Point(700, 45),
                Size = new Size(230, 25),
                ForeColor = Color.DarkGreen,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight
            };
            panel.Controls.Add(labelSubtotal);

            return panel;
        }

        /// <summary>
        /// Кнопка "Закрыть" - закрывает форму
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OrderProductForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Inactivity.OnFormClosing(this);
        }
    }
}
