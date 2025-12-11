using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WebSiteDev
{
    public class ExcelReport
    {
        public static void ExportToExcel(DataGridView dataGridView, List<decimal> orderCosts, DateTime dateFrom, DateTime dateTo,
            string searchText, string selectedStatus, string selectedSort)
        {
            // Диалог сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel 2007-2025 (*.xlsx)|*.xlsx|Excel 97-2003 (*.xls)|*.xls";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.Title = "Сохранить отчёт по заказам";
            saveFileDialog.FileName = "Отчет_заказы_" + dateFrom.ToString("dd.MM.yyyy") + "-" + dateTo.ToString("dd.MM.yyyy");

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            dynamic excelApp = null;
            dynamic workbook = null;
            dynamic worksheet = null;

            try
            {
                // Создаём приложение Excel
                Type excelAppType = Type.GetTypeFromProgID("Excel.Application");
                excelApp = Activator.CreateInstance(excelAppType);
                excelApp.Visible = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet;
                worksheet.Name = "Отчет";
                worksheet.Cells.Font.Name = "Times New Roman";
                worksheet.Cells.Font.Size = 14;

                int currentRow = 1;

                // Заголовок отчёта
                worksheet.Cells[currentRow, 1].Value = "ОТЧЁТ ПО ЗАКАЗАМ";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 20;
                worksheet.Cells[currentRow, 1].Font.Color = System.Drawing.Color.White;
                worksheet.Cells[currentRow, 1].Interior.Color = System.Drawing.Color.FromArgb(45, 156, 219);
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 8]].Merge();
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 8]].HorizontalAlignment = GetXlHAlignCenter();
                currentRow = currentRow + 2;

                // Период отчёта
                string periodStr = dateFrom.ToString("dd.MM.yyyy") + " - " + dateTo.ToString("dd.MM.yyyy");
                worksheet.Cells[currentRow, 1].Value = "Выбранный период отчёта: " + periodStr;
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                currentRow = currentRow + 1;

                // Дата создания
                worksheet.Cells[currentRow, 1].Value = "Дата создания отчёта: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                currentRow = currentRow + 2;

                // Заголовок фильтров
                worksheet.Cells[currentRow, 1].Value = "ПРИМЕНЁННЫЕ ФИЛЬТРЫ:";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 16;
                worksheet.Cells[currentRow, 1].Interior.Color = System.Drawing.Color.LightGray;
                currentRow = currentRow + 1;

                bool hasFilters = false;

                // Показываем активные фильтры
                if (searchText != "")
                {
                    worksheet.Cells[currentRow, 1].Value = "Поиск: " + searchText;
                    worksheet.Cells[currentRow, 1].Font.Size = 14;
                    currentRow = currentRow + 1;
                    hasFilters = true;
                }

                if (selectedStatus != "Статус не выбран" && selectedStatus != "")
                {
                    worksheet.Cells[currentRow, 1].Value = "Статус: " + selectedStatus;
                    worksheet.Cells[currentRow, 1].Font.Size = 14;
                    currentRow = currentRow + 1;
                    hasFilters = true;
                }

                if (selectedSort != "Сортировка не выбрана" && selectedSort != "")
                {
                    worksheet.Cells[currentRow, 1].Value = "Сортировка: " + selectedSort;
                    worksheet.Cells[currentRow, 1].Font.Size = 14;
                    currentRow = currentRow + 1;
                    hasFilters = true;
                }

                // Если фильтров нет - показываем сообщение
                if (hasFilters == false)
                {
                    worksheet.Cells[currentRow, 1].Value = "Все данные за выбранный период";
                    worksheet.Cells[currentRow, 1].Font.Italic = true;
                    worksheet.Cells[currentRow, 1].Font.Size = 14;
                    currentRow = currentRow + 1;
                }

                // Легенда статусов (справа)
                worksheet.Cells[3, 3].Value = "ЛЕГЕНДА СТАТУСОВ";
                worksheet.Cells[3, 3].Font.Bold = true;
                worksheet.Cells[3, 3].Font.Size = 16;
                worksheet.Cells[3, 3].Font.Color = System.Drawing.Color.White;
                worksheet.Cells[3, 3].Interior.Color = System.Drawing.Color.FromArgb(45, 156, 219);
                worksheet.Range[worksheet.Cells[3, 3], worksheet.Cells[3, 4]].Merge();

                // "Завершён" - зелёный
                worksheet.Cells[4, 3].Value = "Завершён";
                worksheet.Cells[4, 3].Interior.Color = System.Drawing.Color.FromArgb(200, 255, 200);
                worksheet.Cells[4, 3].Font.Bold = true;
                worksheet.Cells[4, 3].Font.Size = 14;
                worksheet.Cells[4, 3].HorizontalAlignment = GetXlHAlignCenter();
                worksheet.Cells[4, 4].Value = "Заказ успешно выполнен";
                worksheet.Cells[4, 4].Font.Size = 14;

                // "Отменён" - красный
                worksheet.Cells[5, 3].Value = "Отменён";
                worksheet.Cells[5, 3].Interior.Color = System.Drawing.Color.FromArgb(255, 200, 200);
                worksheet.Cells[5, 3].Font.Bold = true;
                worksheet.Cells[5, 3].Font.Size = 14;
                worksheet.Cells[5, 3].HorizontalAlignment = GetXlHAlignCenter();
                worksheet.Cells[5, 4].Value = "Заказ был отменён";
                worksheet.Cells[5, 4].Font.Size = 14;

                currentRow = currentRow + 2;

                // Заголовок таблицы
                int headerRow = currentRow;
                worksheet.Cells[headerRow, 1].Value = "№ Заказа";
                worksheet.Cells[headerRow, 2].Value = "Клиент";
                worksheet.Cells[headerRow, 3].Value = "Сотрудник";
                worksheet.Cells[headerRow, 4].Value = "Дата заказа";
                worksheet.Cells[headerRow, 5].Value = "Срок выполнения";
                worksheet.Cells[headerRow, 6].Value = "Состав заказа";
                worksheet.Cells[headerRow, 7].Value = "Статус";
                worksheet.Cells[headerRow, 8].Value = "Сумма, руб.";

                dynamic headerRange = worksheet.Range[worksheet.Cells[headerRow, 1], worksheet.Cells[headerRow, 8]];
                headerRange.Font.Bold = true;
                headerRange.Font.Size = 16;
                headerRange.Font.Color = System.Drawing.Color.White;
                headerRange.Interior.Color = System.Drawing.Color.FromArgb(45, 156, 219);
                headerRange.HorizontalAlignment = GetXlHAlignCenter();
                headerRange.VerticalAlignment = GetXlVAlignCenter();

                currentRow = currentRow + 1;
                int dataStartRow = currentRow;

                // Счётчики для статистики
                decimal totalSum = 0;
                int newCount = 0;
                int inProgressCount = 0;
                int completedCount = 0;
                int cancelledCount = 0;

                // Заполняем данные заказов
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    // Получаем номер заказа
                    object orderIDObj = dataGridView.Rows[i].Cells["OrderID"].Value;
                    string orderID = "";
                    if (orderIDObj != null)
                    {
                        orderID = orderIDObj.ToString();
                    }

                    // Получаем имя клиента
                    object clientNameObj = dataGridView.Rows[i].Cells["ClientName"].Value;
                    string clientName = "";
                    if (clientNameObj != null)
                    {
                        clientName = clientNameObj.ToString();
                    }

                    // Получаем имя сотрудника
                    object userNameObj = dataGridView.Rows[i].Cells["UserName"].Value;
                    string userName = "";
                    if (userNameObj != null)
                    {
                        userName = userNameObj.ToString();
                    }

                    // Получаем дату заказа
                    object orderDateObj = dataGridView.Rows[i].Cells["OrderDate"].Value;
                    string orderDate = "";
                    if (orderDateObj != null)
                    {
                        DateTime dt = Convert.ToDateTime(orderDateObj);
                        orderDate = dt.ToString("dd.MM.yy");
                    }

                    // Получаем срок выполнения
                    object compDateObj = dataGridView.Rows[i].Cells["OrderCompDate"].Value;
                    string compDate = "";
                    if (compDateObj != null)
                    {
                        DateTime dt = Convert.ToDateTime(compDateObj);
                        compDate = dt.ToString("dd.MM.yy");
                    }

                    // Получаем список товаров
                    object productNameObj = dataGridView.Rows[i].Cells["ProductName"].Value;
                    string productName = "";
                    if (productNameObj != null)
                    {
                        productName = productNameObj.ToString();
                    }

                    // Разделяем товары на отдельные строки
                    if (productName != "")
                    {
                        string[] products = productName.Split(new string[] { ", " }, System.StringSplitOptions.None);
                        productName = string.Join("\n", products);
                    }

                    // Получаем статус
                    object statusNameObj = dataGridView.Rows[i].Cells["StatusName"].Value;
                    string statusName = "";
                    if (statusNameObj != null)
                    {
                        statusName = statusNameObj.ToString();
                    }

                    // Получаем стоимость
                    object orderCostObj = dataGridView.Rows[i].Cells["OrderCost"].Value;
                    string orderCostStr = "0";
                    if (orderCostObj != null)
                    {
                        orderCostStr = orderCostObj.ToString();
                    }

                    decimal orderCost = 0;
                    bool parsed = decimal.TryParse(orderCostStr, out orderCost);
                    if (parsed == true)
                    {
                        totalSum = totalSum + orderCost;
                    }

                    // Считаем статусы для статистики
                    if (statusName == "Новый")
                    {
                        newCount = newCount + 1;
                    }

                    if (statusName == "В работе")
                    {
                        inProgressCount = inProgressCount + 1;
                    }

                    if (statusName == "Завершён")
                    {
                        completedCount = completedCount + 1;
                    }

                    if (statusName == "Отменён")
                    {
                        cancelledCount = cancelledCount + 1;
                    }

                    // Заполняем ячейки
                    worksheet.Cells[currentRow, 1].Value = orderID;
                    worksheet.Cells[currentRow, 2].Value = clientName;
                    worksheet.Cells[currentRow, 3].Value = userName;
                    worksheet.Cells[currentRow, 4].Value = orderDate;
                    worksheet.Cells[currentRow, 5].Value = compDate;
                    worksheet.Cells[currentRow, 6].Value = productName;
                    worksheet.Cells[currentRow, 7].Value = statusName;
                    worksheet.Cells[currentRow, 8].Value = orderCost + " руб.";

                    // Форматируем строку данных
                    dynamic dataRow = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 8]];
                    dataRow.Font.Size = 14;
                    dataRow.HorizontalAlignment = GetXlHAlignCenter();
                    dataRow.VerticalAlignment = GetXlVAlignCenter();

                    // Форматируем ячейку товаров (переносится текст)
                    dynamic productCell = worksheet.Cells[currentRow, 6];
                    productCell.WrapText = true;
                    productCell.HorizontalAlignment = GetXlHAlignCenter();
                    productCell.VerticalAlignment = GetXlVAlignCenter();

                    // Расчитываем высоту строки в зависимости от количества товаров
                    int productCount = 1;
                    if (productName != "")
                    {
                        string[] lines = productName.Split('\n');
                        productCount = lines.Length;
                    }
                    worksheet.Rows[currentRow].RowHeight = 20 + (productCount * 20);

                    // Форматируем статус
                    dynamic statusCell = worksheet.Cells[currentRow, 7];
                    statusCell.HorizontalAlignment = GetXlHAlignCenter();
                    statusCell.VerticalAlignment = GetXlVAlignCenter();

                    // Окрашиваем статус "Отменён" в красный
                    if (statusName == "Отменён")
                    {
                        statusCell.Interior.Color = System.Drawing.Color.FromArgb(255, 200, 200);
                        statusCell.Font.Bold = true;
                    }

                    // Окрашиваем статус "Завершён" в зелёный
                    if (statusName == "Завершён")
                    {
                        statusCell.Interior.Color = System.Drawing.Color.FromArgb(200, 255, 200);
                        statusCell.Font.Bold = true;
                    }

                    currentRow = currentRow + 1;
                }

                currentRow = currentRow + 2;

                // Рассчитываем среднюю сумму
                decimal averageSum = 0;
                if (dataGridView.Rows.Count > 0)
                {
                    averageSum = totalSum / dataGridView.Rows.Count;
                }

                // Заголовок итоговой информации
                worksheet.Cells[currentRow, 1].Value = "ИТОГОВАЯ ИНФОРМАЦИЯ";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 18;
                worksheet.Cells[currentRow, 1].Font.Color = System.Drawing.Color.White;
                worksheet.Cells[currentRow, 1].Interior.Color = System.Drawing.Color.FromArgb(45, 156, 219);
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 8]].Merge();
                currentRow = currentRow + 2;

                // Заголовок левой колонки (статусы)
                worksheet.Cells[currentRow, 1].Value = "СТАТУСЫ";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 16;
                worksheet.Cells[currentRow, 1].Font.Color = System.Drawing.Color.White;
                worksheet.Cells[currentRow, 1].Interior.Color = System.Drawing.Color.FromArgb(45, 156, 219);
                worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 2]].Merge();

                // Заголовок правой колонки (финансовые показатели)
                worksheet.Cells[currentRow, 7].Value = "ФИНАНСОВЫЕ ПОКАЗАТЕЛИ";
                worksheet.Cells[currentRow, 7].Font.Bold = true;
                worksheet.Cells[currentRow, 7].Font.Size = 16;
                worksheet.Cells[currentRow, 7].Font.Color = System.Drawing.Color.White;
                worksheet.Cells[currentRow, 7].Interior.Color = System.Drawing.Color.FromArgb(45, 156, 219);
                worksheet.Range[worksheet.Cells[currentRow, 7], worksheet.Cells[currentRow, 8]].Merge();
                currentRow = currentRow + 1;

                // Выводим все заказы
                worksheet.Cells[currentRow, 1].Value = "Всего заказов:";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                worksheet.Cells[currentRow, 2].Value = dataGridView.Rows.Count;
                worksheet.Cells[currentRow, 2].Font.Size = 14;
                currentRow = currentRow + 1;

                // Статус "Новый"
                worksheet.Cells[currentRow, 1].Value = "Новый:";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                worksheet.Cells[currentRow, 2].Value = newCount;
                worksheet.Cells[currentRow, 2].Font.Size = 14;
                currentRow = currentRow + 1;

                // Статус "В работе"
                worksheet.Cells[currentRow, 1].Value = "В работе:";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                worksheet.Cells[currentRow, 2].Value = inProgressCount;
                worksheet.Cells[currentRow, 2].Font.Size = 14;
                currentRow = currentRow + 1;

                // Статус "Завершён" - зелёный
                worksheet.Cells[currentRow, 1].Value = "Завершён:";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                worksheet.Cells[currentRow, 1].Font.Color = System.Drawing.Color.Green;
                worksheet.Cells[currentRow, 2].Value = completedCount;
                worksheet.Cells[currentRow, 2].Font.Size = 14;
                worksheet.Cells[currentRow, 2].Font.Color = System.Drawing.Color.Green;
                worksheet.Cells[currentRow, 2].Font.Bold = true;
                currentRow = currentRow + 1;

                // Статус "Отменён" - красный
                worksheet.Cells[currentRow, 1].Value = "Отменён:";
                worksheet.Cells[currentRow, 1].Font.Bold = true;
                worksheet.Cells[currentRow, 1].Font.Size = 14;
                worksheet.Cells[currentRow, 1].Font.Color = System.Drawing.Color.Red;
                worksheet.Cells[currentRow, 2].Value = cancelledCount;
                worksheet.Cells[currentRow, 2].Font.Size = 14;
                worksheet.Cells[currentRow, 2].Font.Color = System.Drawing.Color.Red;
                worksheet.Cells[currentRow, 2].Font.Bold = true;
                currentRow = currentRow + 1;

                int summaryRow = currentRow - 4;

                // Финансовые показатели - общая сумма
                worksheet.Cells[summaryRow, 7].Value = "Общая сумма:";
                worksheet.Cells[summaryRow, 7].Font.Bold = true;
                worksheet.Cells[summaryRow, 7].Font.Size = 14;
                worksheet.Cells[summaryRow, 8].Value = totalSum + " руб.";
                worksheet.Cells[summaryRow, 8].Font.Bold = true;
                worksheet.Cells[summaryRow, 8].Font.Size = 14;
                summaryRow = summaryRow + 1;

                // Средняя сумма
                worksheet.Cells[summaryRow, 7].Value = "Средняя сумма:";
                worksheet.Cells[summaryRow, 7].Font.Bold = true;
                worksheet.Cells[summaryRow, 7].Font.Size = 14;
                worksheet.Cells[summaryRow, 8].Value = averageSum.ToString("0.00") + " руб.";
                worksheet.Cells[summaryRow, 8].Font.Bold = true;
                worksheet.Cells[summaryRow, 8].Font.Size = 14;
                summaryRow = summaryRow + 1;

                // Максимальный заказ
                decimal maxCost = 0;
                for (int i = 0; i < orderCosts.Count; i++)
                {
                    if (orderCosts[i] > maxCost)
                    {
                        maxCost = orderCosts[i];
                    }
                }
                worksheet.Cells[summaryRow, 7].Value = "Макс. заказ:";
                worksheet.Cells[summaryRow, 7].Font.Bold = true;
                worksheet.Cells[summaryRow, 7].Font.Size = 14;
                worksheet.Cells[summaryRow, 8].Value = maxCost + " руб.";
                worksheet.Cells[summaryRow, 8].Font.Bold = true;
                worksheet.Cells[summaryRow, 8].Font.Size = 14;
                summaryRow = summaryRow + 1;

                // Минимальный заказ
                decimal minCost = 99999999;
                for (int i = 0; i < orderCosts.Count; i++)
                {
                    if (orderCosts[i] > 0 && orderCosts[i] < minCost)
                    {
                        minCost = orderCosts[i];
                    }
                }
                if (minCost == 99999999)
                {
                    minCost = 0;
                }
                worksheet.Cells[summaryRow, 7].Value = "Мин. заказ:";
                worksheet.Cells[summaryRow, 7].Font.Bold = true;
                worksheet.Cells[summaryRow, 7].Font.Size = 14;
                worksheet.Cells[summaryRow, 8].Value = minCost + " руб.";
                worksheet.Cells[summaryRow, 8].Font.Bold = true;
                worksheet.Cells[summaryRow, 8].Font.Size = 14;

                // Устанавливаем ширину колонок
                worksheet.Columns[1].ColumnWidth = 18;
                worksheet.Columns[2].ColumnWidth = 52;
                worksheet.Columns[3].ColumnWidth = 40;
                worksheet.Columns[4].ColumnWidth = 30;
                worksheet.Columns[5].ColumnWidth = 24;
                worksheet.Columns[6].ColumnWidth = 40;
                worksheet.Columns[6].WrapText = true;
                worksheet.Columns[7].ColumnWidth = 24;
                worksheet.Columns[8].ColumnWidth = 40;

                // Автофит высоты строк
                worksheet.UsedRange.EntireRow.AutoFit();

                // Определяем формат для сохранения
                string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

                if (extension == ".xls")
                {
                    // Сохраняем в старом формате Excel 97-2003
                    workbook.SaveAs(saveFileDialog.FileName, GetXlWorkbookNormal());
                }
                else
                {
                    // Сохраняем в новом формате Excel 2007+
                    workbook.SaveAs(saveFileDialog.FileName, GetXlOpenXMLWorkbook());
                }

                // Показываем сообщение об успехе
                MessageBox.Show("Отчёт успешно сформирован!\n\nПуть сохранения:\n" + saveFileDialog.FileName, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Ошибка при создании отчёта
                MessageBox.Show("Ошибка при создании отчёта:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    // Закрываем книгу
                    if (workbook != null)
                    {
                        workbook.Close(false);
                    }
                }
                catch { }

                try
                {
                    // Закрываем приложение Excel
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                    }
                }
                catch { }

                // Освобождаем ресурсы
                worksheet = null;
                workbook = null;
                excelApp = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // Константа для выравнивания по центру
        private static int GetXlHAlignCenter()
        {
            return -4108;
        }

        // Константа для вертикального выравнивания по центру
        private static int GetXlVAlignCenter()
        {
            return -4108;
        }

        // Константа для формата OpenXML (Excel 2007+)
        private static int GetXlOpenXMLWorkbook()
        {
            return 51;
        }

        // Константа для формата Excel 97-2003
        private static int GetXlWorkbookNormal()
        {
            return 56;
        }
    }
}