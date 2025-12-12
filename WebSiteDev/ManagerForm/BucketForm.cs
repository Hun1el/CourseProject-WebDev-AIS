using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;
using WebSiteDev.ManagerForm;

namespace WebSiteDev
{
    /// <summary>
    /// Форма оформления заказа - управление корзиной скидками и надбавками
    /// </summary>
    public partial class BucketForm : Form
    {
        private DataManipulation dataManipulation;

        public BucketForm(DataManipulation dm, int userID = 0, string userName = "")
        {
            InitializeComponent();

            ProductControl.CurrentUserID = userID;
            ProductControl.CurrentUserName = userName;

            dataManipulation = dm;
            dataManipulation.FillComboBoxWithClients(comboBox1, "Клиент не выбран");
            dataManipulation.FillComboBoxWithUsers(comboBox2, "Сотрудник не выбран");
        }

        private void BucketForm_Load(object sender, EventArgs e)
        {
            Inactivity.OnFormLoad(this);

            DateTime dateTimeNow = DateTime.Now;
            textBox1.Text = dateTimeNow.ToString("yyyy.MM.dd");
            dateTimePicker1.Value = dateTimeNow.AddDays(7);
            dateTimePicker1.MinDate = dateTimeNow.AddDays(3);

            // Отключение автоматическое изменение состояния чекбоксов (управляем вручную)
            checkbox1.AutoCheck = false;
            checkbox2.AutoCheck = false;
            checkbox3.AutoCheck = false;
            SelectCurrentUser();
            LoadCartItems();
        }

        /// <summary>
        /// Выбирает текущего пользователя если он передан
        /// </summary>
        private void SelectCurrentUser()
        {
            if (ProductControl.CurrentUserID > 0)
            {
                comboBox2.SelectedValue = ProductControl.CurrentUserID;
                comboBox2.Enabled = false;
            }
        }

        /// <summary>
        /// Загружает и отображает товары из корзины в панели
        /// </summary>
        private void LoadCartItems()
        {
            flowLayoutPanel1.Controls.Clear();

            if (ProductControl.CurrentOrder.Items.Count == 0)
            {
                Label emptyLabel = new Label
                {
                    Text = "Корзина пуста",
                    Font = new Font("Microsoft Sans Serif", 18),
                    ForeColor = Color.Gray,
                    Size = new Size(1000, 350),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                flowLayoutPanel1.Controls.Add(emptyLabel);
                button2.Enabled = false;
                button3.Enabled = false;
            }
            else
            {
                button2.Enabled = true;
                button3.Enabled = true;

                // Создаём панель для каждого товара в корзине
                foreach (var item in ProductControl.CurrentOrder.Items)
                {
                    Panel itemPanel = CreateCartItemPanel(item);
                    flowLayoutPanel1.Controls.Add(itemPanel);
                }

                Panel spacer = new Panel
                {
                    Size = new Size(0, 10),
                    BackColor = Color.Transparent
                };
                flowLayoutPanel1.Controls.Add(spacer);
            }

            CheckQuantityDiscount();
        }

        /// <summary>
        /// Создаёт панель с информацией о товаре в корзине (фото, цена, скидки)
        /// </summary>
        private Panel CreateCartItemPanel(ProductControl.OrderItem item)
        {
            Panel panel = new Panel
            {
                Size = new Size(1000, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(5),
                ForeColor = Color.Black
            };

            // Загружаем изображение товара
            PictureBox pic = new PictureBox
            {
                Size = new Size(135, 135),
                Location = new Point(20, 5),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.no_image,
                BorderStyle = BorderStyle.FixedSingle
            };

            if (!string.IsNullOrEmpty(item.ProductPhoto))
            {
                string imagePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebShop", "Images", item.ProductPhoto);
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        using (var fs = new System.IO.FileStream(imagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            using (var img = Image.FromStream(fs))
                            {
                                pic.Image = img.GetThumbnailImage(110, 110, null, IntPtr.Zero);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }

            panel.Controls.Add(pic);

            // Название товара
            Label labelName = new Label
            {
                Text = item.ProductName,
                Font = new Font("Comic Sans Serif", 18, FontStyle.Bold),
                Location = new Point(175, 10),
                Size = new Size(380, 25),
                ForeColor = Color.Black,
                AutoSize = false
            };
            panel.Controls.Add(labelName);

            // Категория
            Label labelCategory = new Label
            {
                Text = $"Категория: {item.CategoryName}",
                Font = new Font("Comic Sans Serif", 14),
                Location = new Point(175, 40),
                Size = new Size(380, 25),
                ForeColor = Color.Black,
                AutoSize = false
            };
            panel.Controls.Add(labelCategory);

            // Количество
            Label labelQuantity = new Label
            {
                Text = $"Кол-во: {item.Quantity}",
                Font = new Font("Comic Sans Serif", 12),
                Location = new Point(175, 80),
                Size = new Size(150, 20),
                ForeColor = Color.Black
            };
            panel.Controls.Add(labelQuantity);

            // Рассчитываем скидки и надбавки
            decimal itemBasePrice = item.BasePrice * item.Quantity;
            decimal itemDiscount = 0;
            decimal itemSurcharge = 0;
            string priceInfo = "";

            if (checkbox1.Checked)
            {
                itemDiscount += itemBasePrice * 0.05m;
            }

            if (checkbox3.Checked)
            {
                itemDiscount += itemBasePrice * 0.07m;
            }

            if (checkbox2.Checked)
            {
                itemSurcharge += itemBasePrice * 0.15m;
            }

            decimal itemFinalPrice = itemBasePrice - itemDiscount + itemSurcharge;

            // Отображаем цену с учётом скидок/надбавок
            if (itemDiscount > 0 || itemSurcharge > 0)
            {
                Label labelOldPrice = new Label
                {
                    Text = $"Было: {itemBasePrice:F2} руб.",
                    Font = new Font("Comic Sans Serif", 11, FontStyle.Strikeout),
                    Location = new Point(175, 110),
                    Size = new Size(200, 20),
                    ForeColor = Color.Gray,
                    AutoSize = false
                };

                panel.Controls.Add(labelOldPrice);

                Label labelNewPrice = new Label
                {
                    Text = $"Сейчас: {itemFinalPrice:F2} руб.",
                    Font = new Font("Comic Sans Serif", 12, FontStyle.Bold),
                    Location = new Point(175, 130),
                    Size = new Size(200, 20),
                    ForeColor = Color.Red,
                    AutoSize = false
                };

                if (itemDiscount > 0)
                {
                    labelNewPrice.ForeColor = Color.Green;
                }

                panel.Controls.Add(labelNewPrice);

                if (itemDiscount > 0)
                {
                    priceInfo += $"Скидка: -{itemDiscount:F2} руб.\n";
                }
                if (itemSurcharge > 0)
                {
                    priceInfo += $"Надбавка: +{itemSurcharge:F2} руб.\n";
                }

                Label labelPriceInfo = new Label
                {
                    Text = priceInfo.TrimEnd('\n'),
                    Font = new Font("Comic Sans Serif", 9),
                    Location = new Point(400, 110),
                    Size = new Size(220, 40),
                    ForeColor = Color.DarkBlue,
                    AutoSize = false
                };
                panel.Controls.Add(labelPriceInfo);
            }
            else
            {
                Label labelPrice = new Label
                {
                    Text = $"Цена: {item.BasePrice} руб.",
                    Font = new Font("Comic Sans Serif", 12),
                    Location = new Point(175, 110),
                    Size = new Size(180, 20),
                    ForeColor = Color.Black
                };
                panel.Controls.Add(labelPrice);
            }

            // Итоговая сумма за товар
            decimal subtotalPrice;
            if (itemDiscount > 0 || itemSurcharge > 0)
            {
                subtotalPrice = itemFinalPrice;
            }
            else
            {
                subtotalPrice = itemBasePrice;
            }

            Label labelSubtotal = new Label
            {
                Text = $"Сумма: {subtotalPrice:F2} руб.",
                Font = new Font("Comic Sans Serif", 12, FontStyle.Bold),
                Location = new Point(650, 90),
                Size = new Size(210, 25),
                ForeColor = Color.DarkGreen,
                AutoSize = false
            };
            panel.Controls.Add(labelSubtotal);

            // Кнопка удалить товар из корзины
            Button buttonRemove = new Button
            {
                Text = "Удалить",
                Location = new Point(860, 90),
                Size = new Size(125, 50),
                BackColor = Color.FromArgb(220, 20, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Comic Sans Serif", 12),
                Name = "buttonRemove" + item.ProductID,
                Tag = item.ProductID,
                Cursor = Cursors.Hand
            };
            buttonRemove.Click += button4_Click;
            panel.Controls.Add(buttonRemove);

            return panel;
        }

        /// <summary>
        /// Удаляет товар из корзины по ID
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            if (button.Tag is int == false)
            {
                return;
            }

            int productID = Convert.ToInt32(button.Tag);

            string productName = "";

            for (int i = 0; i < ProductControl.CurrentOrder.Items.Count; i++)
            {
                if (ProductControl.CurrentOrder.Items[i].ProductID == productID)
                {
                    productName = ProductControl.CurrentOrder.Items[i].ProductName;
                    break;
                }
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить услугу \"" + productName + "\" из корзины?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                RemoveItem(productID);
            }
        }

        private void RemoveItem(int productID)
        {
            for (int i = ProductControl.CurrentOrder.Items.Count - 1; i >= 0; i--)
            {
                if (ProductControl.CurrentOrder.Items[i].ProductID == productID)
                {
                    ProductControl.CurrentOrder.Items.RemoveAt(i);
                    break;
                }
            }
            LoadCartItems();
            UpdateTotal();
        }

        /// <summary>
        /// Проверяет постоянного ли клиента (более 3 заказов) - если да, применяет скидку 5%
        /// </summary>
        private void CheckLoyalClient()
        {
            if (comboBox1.SelectedIndex > 0 && comboBox1.SelectedValue != null)
            {
                int clientID = Convert.ToInt32(comboBox1.SelectedValue);
                int orderCount = GetClientOrderCount(clientID);

                if (orderCount > 3)
                {
                    checkbox1.Enabled = true;
                    checkbox1.Checked = true;
                }
                else
                {
                    checkbox1.Enabled = false;
                    checkbox1.Checked = false;
                }
            }
            else
            {
                checkbox1.Enabled = false;
                checkbox1.Checked = false;
            }

            checkbox1.AutoCheck = false;
        }

        /// <summary>
        /// Проверяет срочность заказа (менее 7 дней) - если срочно, применяет надбавку 15%
        /// </summary>
        private void CheckUrgency()
        {
            DateTime selectedDate = dateTimePicker1.Value.Date;
            DateTime standardDate = DateTime.Now.AddDays(7).Date;

            if (selectedDate < standardDate)
            {
                checkbox2.Enabled = true;
                checkbox2.Checked = true;
            }
            else
            {
                checkbox2.Enabled = false;
                checkbox2.Checked = false;
            }

            checkbox2.AutoCheck = false;
        }

        /// <summary>
        /// Проверяет количество товаров (3+) - если много, применяет скидку 7%
        /// </summary>
        private void CheckQuantityDiscount()
        {
            int uniqueProductCount = ProductControl.CurrentOrder.Items.Count;

            if (uniqueProductCount >= 3)
            {
                checkbox3.Enabled = true;
                checkbox3.Checked = true;
            }
            else
            {
                checkbox3.Enabled = false;
                checkbox3.Checked = false;
            }

            checkbox3.AutoCheck = false;
        }

        /// <summary>
        /// Получает количество заказов клиента из БД
        /// </summary>
        private int GetClientOrderCount(int clientID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM `Order` WHERE ClientID = " + clientID;
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Рассчитывает и обновляет итоговую сумму с учётом всех скидок и надбавок
        /// </summary>
        private void UpdateTotal()
        {
            decimal total = 0;
            int totalQuantity = 0;

            foreach (var item in ProductControl.CurrentOrder.Items)
            {
                total += item.BasePrice * item.Quantity;
                totalQuantity += item.Quantity;
            }

            decimal discount = 0;
            decimal surcharge = 0;
            string discountInfo = "";

            // Скидка постоянного клиента 5%
            if (checkbox1.Checked)
            {
                decimal loyalDiscount = total * 0.05m;
                discount += loyalDiscount;
                discountInfo += $"Скидка постоянного клиента: -{loyalDiscount:F2} руб. (5%)\n";
            }

            // Скидка за количество товаров 7%
            if (checkbox3.Checked)
            {
                decimal quantityDiscount = total * 0.07m;
                discount += quantityDiscount;
                discountInfo += $"Скидка за количество: -{quantityDiscount:F2} руб. (7%)\n";
            }

            // Надбавка за срочность 15%
            if (checkbox2.Checked)
            {
                decimal urgencySurcharge = total * 0.15m;
                surcharge += urgencySurcharge;
                discountInfo += $"Надбавка за срочность: +{urgencySurcharge:F2} руб. (15%)\n";
            }

            decimal finalTotal = total - discount + surcharge;

            label1.Text = $"Итого: {finalTotal:F2} руб.";
            label2.Text = $"Кол-во товаров: {totalQuantity}";
            label7.Text = discountInfo.TrimEnd('\n');
        }

        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTotal();
            LoadCartItems();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckLoyalClient();
            UpdateTotal();
            LoadCartItems();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            CheckUrgency();
            UpdateTotal();
            LoadCartItems();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Оформить заказ - создаёт заказ в БД и опционально печатает чек
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите сотрудника!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ProductControl.CurrentOrder.Items.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newOrderID = 0;

            if (CreateOrderWithProducts(out newOrderID))
            {
                MessageBox.Show("Заказ успешно оформлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var result = MessageBox.Show("Напечатать чек для этого заказа?", "Печать чека", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Doc.CheckWord.CreateCheck(newOrderID);
                }

                ProductControl.CurrentOrder.Clear();
                LoadCartItems();
                comboBox1.SelectedIndex = 0;
                dateTimePicker1.Value = DateTime.Now.AddDays(7);
                UpdateTotal();
            }
            else
            {
                MessageBox.Show("Ошибка при оформлении заказа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Создаёт заказ и добавляет товары в БД с использованием транзакции
        /// </summary>
        private bool CreateOrderWithProducts(out int createdOrderID)
        {
            createdOrderID = 0;

            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();
                    MySqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null)
                        {
                            MessageBox.Show("Не выбран клиент или сотрудник", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        int clientID = Convert.ToInt32(comboBox1.SelectedValue);
                        int userID = Convert.ToInt32(comboBox2.SelectedValue);
                        DateTime orderDate = DateTime.Now;
                        DateTime orderCompDate = dateTimePicker1.Value;

                        // Рассчитываем общую стоимость
                        decimal totalCost = 0;
                        foreach (var item in ProductControl.CurrentOrder.Items)
                        {
                            totalCost += item.BasePrice * item.Quantity;
                        }

                        decimal discount = 0;
                        decimal surcharge = 0;

                        if (checkbox1.Checked)
                        {
                            discount += totalCost * 0.05m;
                        }
                        if (checkbox3.Checked)
                        {
                            discount += totalCost * 0.07m;
                        }
                        if (checkbox2.Checked)
                        {
                            surcharge += totalCost * 0.15m;
                        }

                        decimal finalTotal = totalCost - discount + surcharge;

                        // Создаём заказ в БД
                        string insertOrderQuery = @"INSERT INTO `Order` 
                    (UserID, ClientID, OrderDate, OrderCompDate, StatusID, OrderCost, Discount, Surcharge) 
                    VALUES 
                    (@UserID, @ClientID, @OrderDate, @OrderCompDate, @StatusID, @OrderCost, @Discount, @Surcharge)";

                        MySqlCommand cmdOrder = new MySqlCommand(insertOrderQuery, con, transaction);
                        cmdOrder.Parameters.AddWithValue("@UserID", userID);
                        cmdOrder.Parameters.AddWithValue("@ClientID", clientID);
                        cmdOrder.Parameters.AddWithValue("@OrderDate", orderDate.Date);
                        cmdOrder.Parameters.AddWithValue("@OrderCompDate", orderCompDate.Date);
                        cmdOrder.Parameters.AddWithValue("@StatusID", 1);
                        cmdOrder.Parameters.AddWithValue("@OrderCost", finalTotal);
                        cmdOrder.Parameters.AddWithValue("@Discount", discount);
                        cmdOrder.Parameters.AddWithValue("@Surcharge", surcharge);
                        cmdOrder.ExecuteNonQuery();

                        // Получаем ID созданного заказа
                        MySqlCommand cmdGetOrderID = new MySqlCommand("SELECT LAST_INSERT_ID()", con, transaction);
                        createdOrderID = Convert.ToInt32(cmdGetOrderID.ExecuteScalar());

                        // Добавляем товары в заказ
                        string insertProductQuery = @"INSERT INTO orderproduct 
                    (OrderID, ProductID, ProductCount, ProductPrice) 
                    VALUES 
                    (@OrderID, @ProductID, @ProductCount, @ProductPrice)";

                        foreach (var item in ProductControl.CurrentOrder.Items)
                        {
                            MySqlCommand cmdProduct = new MySqlCommand(insertProductQuery, con, transaction);
                            cmdProduct.Parameters.AddWithValue("@OrderID", createdOrderID);
                            cmdProduct.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmdProduct.Parameters.AddWithValue("@ProductCount", item.Quantity);
                            cmdProduct.Parameters.AddWithValue("@ProductPrice", item.BasePrice);
                            cmdProduct.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка БД: {ex.Message}", "Детали ошибки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}", "Детали ошибки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Кнопка очистить корзину
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Очистить корзину?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ProductControl.CurrentOrder.Clear();
                LoadCartItems();
                UpdateTotal();
            }
        }

        private void BucketForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Inactivity.OnFormClosing(this);
        }
    }
}
