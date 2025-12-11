using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using WebSiteDev;

/// <summary>
/// Класс для управления фильтрацией, сортировкой и поиском данных в DataTable
/// </summary>
public class DataManipulation
{
    public DataTable table { get; set; }
    public DataView view { get; set; }

    public DataManipulation(DataTable dt)
    {
        table = dt;
        view = dt.DefaultView;
    }

    // ===== МЕТОДЫ ДЛЯ ПРИМЕНЕНИЯ ВСЕХ ФУНКЦИЙ СРАЗУ =====

    /// <summary>
    /// Применяет поиск, фильтр и сортировку для таблицы пользователей
    /// </summary>
    public void ApplyAllUser(ComboBox comboSort, ComboBox comboFilter, TextBox textSearch)
    {
        ApplySearchUser(textSearch);
        ApplyFilterUser(comboFilter);
        ApplySortUser(comboSort);
    }

    /// <summary>
    /// Применяет поиск и сортировку для таблицы категорий
    /// </summary>
    public void ApplyAllCategory(ComboBox comboSort, TextBox textSearch)
    {
        ApplySearchCategory(textSearch);
        ApplySortCategory(comboSort);
    }

    /// <summary>
    /// Применяет поиск, фильтр и сортировку для таблицы товаров
    /// </summary>
    public void ApplyAllProduct(ComboBox comboSort, ComboBox comboFilter, TextBox textSearch)
    {
        ApplySearchProduct(textSearch);
        ApplyFilterProduct(comboFilter);
        ApplySortProduct(comboSort);
    }

    /// <summary>
    /// Применяет поиск, фильтр и сортировку для таблицы заказов менеджера
    /// </summary>
    public void ApplyAllOrder(ComboBox comboSort, ComboBox comboFilter, TextBox textSearch)
    {
        ApplySearchOrder(textSearch);
        ApplyFilterOrder(comboFilter);
        ApplySortOrder(comboSort);
    }

    /// <summary>
    /// Применяет поиск и сортировку для таблицы клиентов
    /// </summary>
    public void ApplyAllClient(ComboBox comboSort, TextBox textSearch)
    {
        ApplySearchClient(textSearch);
        ApplySortClient(comboSort);
    }

    /// <summary>
    /// Применяет поиск, фильтр и сортировку для таблицы заказов директора
    /// </summary>
    public void ApplyAllDirector(ComboBox comboSort, ComboBox comboFilter, TextBox textSearch)
    {
        ApplySearchDirector(textSearch);
        ApplyFilterDirector(comboFilter);
        ApplySortDirector(comboSort);
    }

    // ===== МЕТОДЫ ДЛЯ ЗАПОЛНЕНИЯ COMBOBOX =====

    /// <summary>
    /// Заполняет ComboBox ролями из БД
    /// </summary>
    public void FillComboBoxWithRoles(ComboBox combo, string firstItem)
    {
        using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
        {
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT RoleID, RoleName FROM Role ORDER BY RoleName", con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Добавляем первый пустой элемент
            DataRow dr = dt.NewRow();
            dr["RoleID"] = 0;
            dr["RoleName"] = firstItem;
            dt.Rows.InsertAt(dr, 0);

            combo.DataSource = dt;
            combo.DisplayMember = "RoleName";
            combo.ValueMember = "RoleID";
            combo.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Заполняет ComboBox категориями из БД
    /// </summary>
    public void FillComboBoxWithCategories(ComboBox combo, string firstItem)
    {
        using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
        {
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT CategoryID, CategoryName FROM Category ORDER BY CategoryName", con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Добавляем первый пустой элемент
            DataRow dr = dt.NewRow();
            dr["CategoryID"] = 0;
            dr["CategoryName"] = firstItem;
            dt.Rows.InsertAt(dr, 0);

            combo.DataSource = dt;
            combo.DisplayMember = "CategoryName";
            combo.ValueMember = "CategoryID";
            combo.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Заполняет ComboBox статусами из БД
    /// </summary>
    public void FillComboBoxWithStatuses(ComboBox combo, string firstItem)
    {
        using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
        {
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT StatusID, StatusName FROM Status ORDER BY StatusName", con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Добавляем первый пустой элемент
            DataRow dr = dt.NewRow();
            dr["StatusID"] = 0;
            dr["StatusName"] = firstItem;
            dt.Rows.InsertAt(dr, 0);

            combo.DataSource = dt;
            combo.DisplayMember = "StatusName";
            combo.ValueMember = "StatusID";
            combo.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Заполняет ComboBox товарами из БД
    /// </summary>
    public void FillComboBoxWithProducts(ComboBox combo, string firstItem)
    {
        using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
        {
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT ProductID, ProductName FROM Product ORDER BY ProductName", con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Добавляем первый пустой элемент
            DataRow dr = dt.NewRow();
            dr["ProductID"] = 0;
            dr["ProductName"] = firstItem;
            dt.Rows.InsertAt(dr, 0);

            combo.DataSource = dt;
            combo.DisplayMember = "ProductName";
            combo.ValueMember = "ProductID";
            combo.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Заполняет ComboBox пользователями из БД
    /// </summary>
    public void FillComboBoxWithUsers(ComboBox combo, string firstItem)
    {
        using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
        {
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT UserID, CONCAT(IFNULL(Surname,''), ' ', IFNULL(FirstName,''), ' ', IFNULL(MiddleName,'')) AS FullName FROM Users ORDER BY FullName", con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Добавляем первый пустой элемент
            DataRow dr = dt.NewRow();
            dr["UserID"] = 0;
            dr["FullName"] = firstItem;
            dt.Rows.InsertAt(dr, 0);

            combo.DataSource = dt;
            combo.DisplayMember = "FullName";
            combo.ValueMember = "UserID";
            combo.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Заполняет ComboBox клиентами из БД
    /// </summary>
    public void FillComboBoxWithClients(ComboBox combo, string firstItem)
    {
        using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
        {
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT ClientID, CONCAT(IFNULL(Surname,''), ' ', IFNULL(FirstName,''), ' ', IFNULL(MiddleName,'')) AS FullName FROM Clients ORDER BY FullName", con);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Добавляем первый пустой элемент
            DataRow dr = dt.NewRow();
            dr["ClientID"] = 0;
            dr["FullName"] = firstItem;
            dt.Rows.InsertAt(dr, 0);

            combo.DataSource = dt;
            combo.DisplayMember = "FullName";
            combo.ValueMember = "ClientID";
            combo.SelectedIndex = 0;
        }
    }

    // ===== МЕТОДЫ ПОИСКА =====

    /// <summary>
    /// Применяет поиск по фамилии пользователя
    /// </summary>
    public void ApplySearchUser(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"Surname LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по названию категории
    /// </summary>
    public void ApplySearchCategory(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"CategoryName LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по названию роли
    /// </summary>
    public void ApplySearchRole(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"RoleName LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по названию статуса
    /// </summary>
    public void ApplySearchStatus(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"StatusName LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по названию товара
    /// </summary>
    public void ApplySearchProduct(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"ProductName LIKE '%{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по номеру заказа
    /// </summary>
    public void ApplySearchOrder(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"Convert(OrderID, 'System.String') LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по фамилии клиента
    /// </summary>
    public void ApplySearchClient(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"Surname LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет поиск по номеру заказа для директора
    /// </summary>
    public void ApplySearchDirector(TextBox textSearch)
    {
        string searchText = textSearch.Text.Trim().Replace("'", "''");

        if (!string.IsNullOrEmpty(searchText))
        {
            view.RowFilter = $"Convert(OrderID, 'System.String') LIKE '{searchText}%'";
        }
        else
        {
            view.RowFilter = "";
        }
    }

    // ===== МЕТОДЫ ФИЛЬТРАЦИИ =====

    /// <summary>
    /// Применяет фильтр по роли для пользователей
    /// </summary>
    public void ApplyFilterUser(ComboBox comboFilter)
    {
        List<string> filters = new List<string>();

        if (comboFilter.SelectedIndex > 0)
        {
            var row = comboFilter.SelectedItem as DataRowView;
            if (row != null)
            {
                string selectedRole = row["RoleName"].ToString().Replace("'", "''");
                filters.Add("RoleName = '" + selectedRole + "'");
            }
        }

        if (!string.IsNullOrEmpty(view.RowFilter))
        {
            filters.Add(view.RowFilter);
        }

        if (filters.Count > 0)
        {
            view.RowFilter = string.Join(" AND ", filters);
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет фильтр по категории для товаров
    /// </summary>
    public void ApplyFilterProduct(ComboBox comboFilter)
    {
        List<string> filters = new List<string>();

        if (comboFilter.SelectedIndex > 0)
        {
            var row = comboFilter.SelectedItem as DataRowView;
            if (row != null)
            {
                string selectedCategory = row["CategoryName"].ToString().Replace("'", "''");
                filters.Add("Category = '" + selectedCategory + "'");
            }
        }

        if (!string.IsNullOrEmpty(view.RowFilter))
        {
            filters.Add(view.RowFilter);
        }

        if (filters.Count > 0)
        {
            view.RowFilter = string.Join(" AND ", filters);
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет фильтр по статусу для заказов менеджера
    /// </summary>
    public void ApplyFilterOrder(ComboBox comboFilter)
    {
        List<string> filters = new List<string>();

        if (comboFilter.SelectedIndex > 0)
        {
            var row = comboFilter.SelectedItem as DataRowView;
            if (row != null)
            {
                string selectedStatus = row["StatusName"].ToString().Replace("'", "''");
                filters.Add("StatusName = '" + selectedStatus + "'");
            }
        }

        if (!string.IsNullOrEmpty(view.RowFilter))
        {
            filters.Add(view.RowFilter);
        }

        if (filters.Count > 0)
        {
            view.RowFilter = string.Join(" AND ", filters);
        }
        else
        {
            view.RowFilter = "";
        }
    }

    /// <summary>
    /// Применяет фильтр по статусу для заказов директора
    /// </summary>
    public void ApplyFilterDirector(ComboBox comboFilter)
    {
        List<string> filters = new List<string>();

        if (comboFilter.SelectedIndex > 0)
        {
            var row = comboFilter.SelectedItem as DataRowView;
            if (row != null)
            {
                string selectedStatus = row["StatusName"].ToString().Replace("'", "''");
                filters.Add("StatusName = '" + selectedStatus + "'");
            }
        }

        if (!string.IsNullOrEmpty(view.RowFilter))
        {
            filters.Add(view.RowFilter);
        }

        if (filters.Count > 0)
        {
            view.RowFilter = string.Join(" AND ", filters);
        }
        else
        {
            view.RowFilter = "";
        }
    }

    // ===== МЕТОДЫ СОРТИРОВКИ =====

    /// <summary>
    /// Применяет сортировку по фамилии для пользователей
    /// </summary>
    public void ApplySortUser(ComboBox comboSort)
    {
        if (comboSort.SelectedIndex == 1)
        {
            view.Sort = "Surname ASC";
        }
        else if (comboSort.SelectedIndex == 2)
        {
            view.Sort = "Surname DESC";
        }
        else
        {
            view.Sort = "";
        }
    }

    /// <summary>
    /// Применяет сортировку по названию для категорий
    /// </summary>
    public void ApplySortCategory(ComboBox comboSort)
    {
        if (comboSort.SelectedIndex == 1)
        {
            view.Sort = "CategoryName ASC";
        }
        else if (comboSort.SelectedIndex == 2)
        {
            view.Sort = "CategoryName DESC";
        }
        else
        {
            view.Sort = "";
        }
    }

    /// <summary>
    /// Применяет сортировку по цене для товаров
    /// </summary>
    public void ApplySortProduct(ComboBox comboSort)
    {
        if (comboSort.SelectedIndex == 1)
        {
            view.Sort = "BasePrice ASC";
        }
        else if (comboSort.SelectedIndex == 2)
        {
            view.Sort = "BasePrice DESC";
        }
        else
        {
            view.Sort = "";
        }
    }

    /// <summary>
    /// Применяет сортировку по датам для заказов менеджера
    /// </summary>
    public void ApplySortOrder(ComboBox comboSort)
    {
        if (comboSort.SelectedIndex == 1)
        {
            view.Sort = "OrderDate ASC";
        }
        else if (comboSort.SelectedIndex == 2)
        {
            view.Sort = "OrderDate DESC";
        }
        else if (comboSort.SelectedIndex == 3)
        {
            view.Sort = "OrderCompDate ASC";
        }
        else if (comboSort.SelectedIndex == 4)
        {
            view.Sort = "OrderCompDate DESC";
        }
        else
        {
            view.Sort = "";
        }
    }

    /// <summary>
    /// Применяет сортировку по фамилии для клиентов
    /// </summary>
    public void ApplySortClient(ComboBox comboSort)
    {
        if (comboSort.SelectedIndex == 1)
        {
            view.Sort = "Surname ASC";
        }
        else if (comboSort.SelectedIndex == 2)
        {
            view.Sort = "Surname DESC";
        }
        else
        {
            view.Sort = "";
        }
    }
    /// <summary>
    /// Применяет сортировку по датам для заказов директора
    /// </summary>
    public void ApplySortDirector(ComboBox comboSort)
    {
        if (comboSort.SelectedIndex == 1)
        {
            view.Sort = "OrderDate ASC";
        }
        else if (comboSort.SelectedIndex == 2)
        {
            view.Sort = "OrderDate DESC";
        }
        else if (comboSort.SelectedIndex == 3)
        {
            view.Sort = "OrderCompDate ASC";
        }
        else if (comboSort.SelectedIndex == 4)
        {
            view.Sort = "OrderCompDate DESC";
        }
        else
        {
            view.Sort = "";
        }
    }

    // ===== МЕТОДЫ СБРОСА И ОБНОВЛЕНИЯ =====

    /// <summary>
    /// Сбрасывает все фильтры и сортировку - очищает ComboBox и TextBox
    /// </summary>
    public void ResetFilters(ComboBox comboSort = null, ComboBox comboFilter = null, TextBox textSearch = null)
    {
        if (comboSort != null)
        {
            comboSort.SelectedIndex = 0;
        }
        if (comboFilter != null)
        {
            comboFilter.SelectedIndex = 0;
        }
        if (textSearch != null)
        {
            textSearch.Clear();
        }

        view.RowFilter = "";
        view.Sort = "";
    }

    /// <summary>
    /// Обновляет количество отображаемых записей после фильтрации
    /// </summary>
    public void UpdateRecordCountLabel(Label label)
    {
        int visibleCount = view.Count;
        label.Text = $"Количество записей: {visibleCount}";
    }
}