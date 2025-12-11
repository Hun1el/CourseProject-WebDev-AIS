using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WebSiteDev.Doc
{
    public static class CheckWord
    {
        /// <summary>
        /// Создаёт и сохраняет чек заказа в Word файл
        /// </summary>
        public static void CreateCheck(int orderID)
        {
            // Диалог сохранения файла
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Word 2007-2025 (*.docx)|*.docx|Word 97-2003 (*.doc)|*.doc";
            sfd.FilterIndex = 1;
            sfd.Title = "Сохранить чек заказа";
            sfd.FileName = "Чек_Заказа_№" + orderID;

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            dynamic app = null;
            dynamic doc = null;

            try
            {
                // Создаём Word приложение и новый документ
                Type wordAppType = Type.GetTypeFromProgID("Word.Application");
                app = Activator.CreateInstance(wordAppType);
                app.Visible = false;

                doc = app.Documents.Add();

                // Базовая высота страницы для одного товара
                float baseHeight = 236.8f;

                // Получаем количество товаров в заказе для расчёта размера страницы
                int productCount = 0;
                using (MySqlConnection conTemp = new MySqlConnection(Data.GetConnectionString()))
                {
                    conTemp.Open();
                    string countQuery = "SELECT COUNT(*) FROM orderproduct WHERE OrderID = " + orderID;
                    MySqlCommand countCmd = new MySqlCommand(countQuery, conTemp);
                    productCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                // Если товаров больше одного - добавляем высоту за каждый товар
                float additionalHeight = (productCount > 1) ? (productCount - 1) * 5f : 0f;
                float totalHeight = baseHeight + additionalHeight;

                // Настраиваем размер страницы как кассовая лента (узкий чек)
                doc.PageSetup.PageWidth = 226.8f;
                doc.PageSetup.PageHeight = totalHeight;
                doc.PageSetup.TopMargin = 20;
                doc.PageSetup.BottomMargin = 20;
                doc.PageSetup.LeftMargin = 14;
                doc.PageSetup.RightMargin = 14;

                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();

                    // Получаем основную информацию о заказе
                    string query = @"SELECT o.OrderID, CONCAT(c.Surname, ' ', c.FirstName, ' ', COALESCE(c.MiddleName, '')) AS ClientName, " +
                        "CONCAT(u.Surname, ' ', u.FirstName, ' ', COALESCE(u.MiddleName, '')) AS UserName, u.PhoneNumber AS UserPhone, " +
                        "o.OrderDate, o.OrderCompDate, o.OrderCost, o.Discount, o.Surcharge FROM `Order` o LEFT JOIN Clients c ON o.ClientID = c.ClientID " +
                        "LEFT JOIN Users u ON o.UserID = u.UserID WHERE o.OrderID = " + orderID;

                    MySqlCommand cmd = new MySqlCommand(query, con);

                    decimal discount = 0;
                    decimal surcharge = 0;
                    bool hasDiscount = false;
                    bool hasSurcharge = false;

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            // Проверяем наличие скидки
                            if (r["Discount"] != DBNull.Value)
                            {
                                discount = Convert.ToDecimal(r["Discount"]);
                                if (discount > 0) hasDiscount = true;
                            }

                            // Проверяем наличие надбавки
                            if (r["Surcharge"] != DBNull.Value)
                            {
                                surcharge = Convert.ToDecimal(r["Surcharge"]);
                                if (surcharge > 0) hasSurcharge = true;
                            }

                            // Заголовок чека - номер заказа
                            dynamic p1 = doc.Paragraphs.Add();
                            p1.Range.Text = "ЧЕК ЗАКАЗА № " + r["OrderID"].ToString();
                            p1.Range.Font.Name = "Times New Roman";
                            p1.Range.Font.Size = 9;
                            p1.Range.Font.Bold = 1;
                            p1.Alignment = GetWdAlignParagraphCenter();
                            p1.SpaceBefore = 2;
                            p1.SpaceAfter = 3;
                            p1.Range.InsertParagraphAfter();

                            // Дата заказа и срок выполнения
                            DateTime dt1 = Convert.ToDateTime(r["OrderDate"]);
                            DateTime dt2 = Convert.ToDateTime(r["OrderCompDate"]);

                            dynamic p2 = doc.Paragraphs.Add();
                            p2.Range.Text = "Дата заказа: " + dt1.ToString("dd.MM.yyyy") + "         " + "Срок выполнения: " + dt2.ToString("dd.MM.yyyy");
                            p2.Range.Font.Name = "Times New Roman";
                            p2.Range.Font.Size = 6;
                            p2.Range.Font.Bold = 0;
                            p2.Alignment = GetWdAlignParagraphCenter();
                            p2.SpaceBefore = 6;
                            p2.SpaceAfter = 0;
                            p2.Range.InsertParagraphAfter();

                            // Имя клиента
                            dynamic p3 = doc.Paragraphs.Add();
                            p3.Range.Text = "Клиент: " + r["ClientName"].ToString();
                            p3.Range.Font.Name = "Times New Roman";
                            p3.Range.Font.Size = 6;
                            p3.Range.Font.Bold = 0;
                            p3.Alignment = GetWdAlignParagraphLeft();
                            p3.SpaceBefore = 10;
                            p3.SpaceAfter = 2;
                            p3.Range.InsertParagraphAfter();

                            // Разделительная линия
                            dynamic line2 = doc.Paragraphs.Add();
                            line2.Range.Text = "__________________________________________________________";
                            line2.Range.Font.Name = "Times New Roman";
                            line2.Range.Font.Size = 6;
                            line2.Alignment = GetWdAlignParagraphCenter();
                            line2.SpaceBefore = 0;
                            line2.SpaceAfter = 0;
                            line2.Range.InsertParagraphAfter();

                            // Имя сотрудника
                            dynamic p4 = doc.Paragraphs.Add();
                            p4.Range.Text = "Сотрудник: " + r["UserName"].ToString();
                            p4.Range.Font.Name = "Times New Roman";
                            p4.Range.Font.Size = 6;
                            p4.Range.Font.Bold = 0;
                            p4.Alignment = GetWdAlignParagraphLeft();
                            p4.SpaceBefore = 0;
                            p4.SpaceAfter = 0;
                            p4.Range.InsertParagraphAfter();

                            // Телефон сотрудника
                            dynamic p5 = doc.Paragraphs.Add();
                            p5.Range.Text = "Тел: + 7 (900) 111-22-33";
                            p5.Range.Font.Name = "Times New Roman";
                            p5.Range.Font.Size = 6;
                            p5.Range.Font.Bold = 0;
                            p5.Alignment = GetWdAlignParagraphLeft();
                            p5.SpaceBefore = 0;
                            p5.SpaceAfter = 2;
                            p5.Range.InsertParagraphAfter();

                            // Разделительная линия
                            dynamic line3 = doc.Paragraphs.Add();
                            line3.Range.Text = "__________________________________________________________";
                            line3.Range.Font.Name = "Times New Roman";
                            line3.Range.Font.Size = 6;
                            line3.Alignment = GetWdAlignParagraphCenter();
                            line3.SpaceBefore = 0;
                            line3.SpaceAfter = 0;
                            line3.Range.InsertParagraphAfter();
                        }
                    }

                    // Получаем список товаров в заказе
                    string query2 = "SELECT p.ProductName, op.ProductCount, p.BasePrice AS ProductCost " +
                        "FROM orderproduct op LEFT JOIN Product p ON op.ProductID = p.ProductID " +
                        "WHERE op.OrderID = " + orderID;

                    MySqlCommand cmd2 = new MySqlCommand(query2, con);

                    // Заголовок состава заказа
                    dynamic p6 = doc.Paragraphs.Add();
                    p6.Range.Text = "СОСТАВ ЗАКАЗА";
                    p6.Range.Font.Name = "Times New Roman";
                    p6.Range.Font.Size = 6;
                    p6.Range.Font.Bold = 1;
                    p6.Alignment = GetWdAlignParagraphCenter();
                    p6.SpaceBefore = 2;
                    p6.SpaceAfter = 2;
                    p6.Range.InsertParagraphAfter();

                    using (MySqlDataReader r2 = cmd2.ExecuteReader())
                    {
                        // Считаем количество товаров
                        int count = 0;
                        while (r2.Read()) count = count + 1;

                        if (count > 0)
                        {
                            r2.Close();
                            MySqlDataReader r3 = cmd2.ExecuteReader();

                            // Создаём таблицу для товаров (строка для заголовка + строки для товаров)
                            dynamic tbl = doc.Tables.Add(doc.Paragraphs[doc.Paragraphs.Count].Range, count + 1, 3);
                            tbl.Borders.Enable = 1;

                            // Заголовки таблицы
                            tbl.Cell(1, 1).Range.Text = "Товар";
                            tbl.Cell(1, 2).Range.Text = "Количество";
                            tbl.Cell(1, 3).Range.Text = "Цена";

                            // Форматируем строку заголовка
                            tbl.Rows[1].Range.Font.Name = "Times New Roman";
                            tbl.Rows[1].Range.Font.Bold = 1;
                            tbl.Rows[1].Range.Font.Size = 5;
                            tbl.Rows[1].Shading.BackgroundPatternColor = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(45, 156, 219));
                            tbl.Rows[1].Range.Font.Color = GetWdColorWhite();

                            // Заполняем таблицу товарами из БД
                            int i = 2;
                            while (r3.Read())
                            {
                                tbl.Cell(i, 1).Range.Text = r3["ProductName"].ToString();
                                tbl.Cell(i, 2).Range.Text = r3["ProductCount"].ToString();

                                decimal price = 0;
                                decimal.TryParse(r3["ProductCost"].ToString(), out price);
                                tbl.Cell(i, 3).Range.Text = price.ToString("0.00") + " ₽";

                                tbl.Rows[i].Range.Font.Name = "Times New Roman";
                                tbl.Rows[i].Range.Font.Size = 4;
                                tbl.Rows[i].Range.Font.Bold = 0;

                                i = i + 1;
                            }
                            r3.Close();
                        }
                    }

                    // Если есть скидка - добавляем информацию о ней
                    if (hasDiscount == true)
                    {
                        // Получаем сумму товаров для определения типа скидки
                        string query3 = "SELECT SUM(op.ProductCount * p.BasePrice) AS Total " +
                            "FROM orderproduct op " +
                            "LEFT JOIN Product p ON op.ProductID = p.ProductID WHERE op.OrderID = " + orderID;
                        MySqlCommand cmd3 = new MySqlCommand(query3, con);

                        decimal total = 0;
                        object obj = cmd3.ExecuteScalar();
                        if (obj != null && obj != DBNull.Value) total = Convert.ToDecimal(obj);

                        // Определяем причину скидки по проценту
                        string reason = "";
                        if (total > 0)
                        {
                            decimal percent = (discount / total) * 100;
                            if (percent >= 11 && percent <= 13)
                            {
                                reason = "Скидка клиента + товары";
                            }
                            else if (percent >= 6 && percent <= 8)
                            {
                                reason = "Скидка за товары (7%)";
                            }
                            else if (percent >= 4 && percent <= 6)
                            {
                                reason = "Скидка клиента (5%)";
                            }
                            else
                            {
                                reason = "Скидка";
                            }
                        }

                        // Выводим скидку с причиной
                        dynamic p7 = doc.Paragraphs.Add();
                        p7.Range.Text = reason + ": ";
                        p7.Range.Font.Name = "Times New Roman";
                        p7.Range.Font.Size = 6;
                        p7.Range.Font.Bold = 0;
                        p7.Range.Font.Color = GetWdColorBlack();

                        dynamic rng = p7.Range.Duplicate;
                        rng.Start = p7.Range.End;
                        rng.Text = "-" + discount.ToString("0.00") + " ₽";
                        rng.Font.Bold = 1;
                        rng.Font.Name = "Times New Roman";
                        rng.Font.Size = 6;

                        p7.Range.End = rng.End;
                        p7.Alignment = GetWdAlignParagraphRight();
                        p7.SpaceBefore = 2;
                        p7.SpaceAfter = 0;
                        p7.Range.InsertParagraphAfter();
                    }

                    // Если есть надбавка - добавляем информацию о ней
                    if (hasSurcharge == true)
                    {
                        dynamic p8 = doc.Paragraphs.Add();
                        p8.Range.Text = "Срочность (15%): ";
                        p8.Range.Font.Name = "Times New Roman";
                        p8.Range.Font.Size = 6;
                        p8.Range.Font.Bold = 0;
                        p8.Range.Font.Color = GetWdColorBlack();

                        dynamic rng = p8.Range.Duplicate;
                        rng.Start = p8.Range.End;
                        rng.Text = "+" + surcharge.ToString("0.00") + " ₽";
                        rng.Font.Bold = 1;
                        rng.Font.Name = "Times New Roman";
                        rng.Font.Size = 6;

                        p8.Range.End = rng.End;
                        p8.Alignment = GetWdAlignParagraphRight();
                        p8.SpaceBefore = 0;
                        p8.SpaceAfter = 2;
                        p8.Range.InsertParagraphAfter();
                    }

                    // Получаем итоговую стоимость заказа
                    string query4 = "SELECT OrderCost FROM `Order` WHERE OrderID = " + orderID;
                    MySqlCommand cmd4 = new MySqlCommand(query4, con);

                    string totalCost = "0";
                    object obj2 = cmd4.ExecuteScalar();
                    if (obj2 != null)
                    {
                        decimal val = Convert.ToDecimal(obj2);
                        totalCost = val.ToString("0.00");
                    }

                    // Выводим итого
                    dynamic p9 = doc.Paragraphs.Add();
                    p9.Range.Text = "ИТОГО: ";
                    p9.Range.Font.Name = "Times New Roman";
                    p9.Range.Font.Size = 7;
                    p9.Range.Font.Bold = 0;

                    dynamic rng2 = p9.Range.Duplicate;
                    rng2.Start = p9.Range.End;
                    rng2.Text = totalCost + " ₽";
                    rng2.Font.Bold = 1;
                    rng2.Font.Size = 7;
                    rng2.Font.Name = "Times New Roman";

                    p9.Range.End = rng2.End;
                    p9.Alignment = GetWdAlignParagraphRight();
                    p9.SpaceBefore = 0;
                    p9.SpaceAfter = 2;
                    p9.Range.InsertParagraphAfter();

                    // Спасибо за покупку
                    dynamic p10 = doc.Paragraphs.Add();
                    p10.Range.Text = "Спасибо за покупку!";
                    p10.Range.Font.Name = "Times New Roman";
                    p10.Range.Font.Size = 7;
                    p10.Range.Font.Bold = 1;
                    p10.Alignment = GetWdAlignParagraphCenter();
                    p10.SpaceBefore = 16;
                    p10.SpaceAfter = 2;
                    p10.Range.InsertParagraphAfter();

                    // Время печати чека
                    dynamic p11 = doc.Paragraphs.Add();
                    p11.Range.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    p11.Range.Font.Name = "Times New Roman";
                    p11.Range.Font.Size = 5;
                    p11.Range.Font.Bold = 0;
                    p11.Alignment = GetWdAlignParagraphRight();
                    p11.SpaceBefore = 4;
                    p11.SpaceAfter = 0;
                }

                // Определяем формат для сохранения
                string ext = System.IO.Path.GetExtension(sfd.FileName).ToLower();

                if (ext == ".doc")
                {
                    // Сохраняем в старом формате Word 97-2003
                    doc.SaveAs(sfd.FileName, GetWdFormatDocument97());
                }
                else
                {
                    // Сохраняем в новом формате Word 2007+
                    doc.SaveAs(sfd.FileName, GetWdFormatDocumentDefault());
                }

                doc.Close();
                app.Quit();

                // Показываем сообщение об успехе
                MessageBox.Show("Чек успешно сформирован!\n\nПуть сохранения:\n" + sfd.FileName, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Ошибка при создании документа
                MessageBox.Show("Ошибка при создании чека:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    // Освобождаем объект документа
                    if (doc != null)
                    {
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(doc);
                        doc = null;
                    }
                }
                catch { }

                try
                {
                    // Освобождаем объект приложения Word
                    if (app != null)
                    {
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(app);
                        app = null;
                    }
                }
                catch { }

                // Принудительно вызываем сборку мусора
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // Константа для выравнивания по центру
        private static int GetWdAlignParagraphCenter()
        {
            return 1;
        }

        // Константа для выравнивания влево
        private static int GetWdAlignParagraphLeft()
        {
            return 0;
        }

        // Константа для выравнивания вправо
        private static int GetWdAlignParagraphRight()
        {
            return 2;
        }

        // Константа для белого цвета
        private static int GetWdColorWhite()
        {
            return 16777215;
        }

        // Константа для чёрного цвета
        private static int GetWdColorBlack()
        {
            return 0;
        }

        // Константа для формата Word 97-2003
        private static int GetWdFormatDocument97()
        {
            return 0;
        }

        // Константа для формата Word 2007+
        private static int GetWdFormatDocumentDefault()
        {
            return 16;
        }
    }
}