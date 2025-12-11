using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace WebSiteDev
{
    public static class DataUpdate
    {
        /// <summary>
        /// Обновляет название категории с проверкой на дублирование
        /// </summary>
        public static bool UpdateCategory(int categoryID, string newName)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем что такой названия категории нет (кроме текущей)
                    MySqlCommand CheckQuery = new MySqlCommand("SELECT COUNT(*) FROM Category WHERE CategoryName = '" + newName + "' AND CategoryID != " + categoryID, con);
                    if (Convert.ToInt32(CheckQuery.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Категория с таким названием уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Обновляем категорию
                    MySqlCommand UpdateQuery = new MySqlCommand("UPDATE Category SET CategoryName = '" + newName + "' WHERE CategoryID = " + categoryID, con);

                    return UpdateQuery.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Обновляет название статуса с проверкой на дублирование
        /// </summary>
        public static bool UpdateStatus(int statusID, string newName)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем что такого названия статуса нет (кроме текущего)
                    MySqlCommand CheckQuery = new MySqlCommand("SELECT COUNT(*) FROM Status WHERE StatusName = '" + newName + "' AND StatusID != " + statusID, con);
                    if (Convert.ToInt32(CheckQuery.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Статус с таким названием уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Обновляем статус
                    MySqlCommand UpdateQuery = new MySqlCommand("UPDATE Status SET StatusName = '" + newName + "' WHERE StatusID = " + statusID, con);

                    return UpdateQuery.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Обновляет название роли с проверкой на дублирование
        /// </summary>
        public static bool UpdateRole(int roleID, string newName)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем что такой названия роли нет (кроме текущей)
                    MySqlCommand CheckQuery = new MySqlCommand("SELECT COUNT(*) FROM Role WHERE RoleName = '" + newName + "' AND RoleID != " + roleID, con);
                    if (Convert.ToInt32(CheckQuery.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Роль с таким названием уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Обновляем роль
                    MySqlCommand UpdateQuery = new MySqlCommand("UPDATE Role SET RoleName = '" + newName + "' WHERE RoleID = " + roleID, con);

                    return UpdateQuery.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Обновляет данные пользователя с проверкой на дублирование логина и телефона
        /// </summary>
        public static bool UpdateUser(int userID, string surname, string firstName, string middleName, string login, string password, int roleID, string phone)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем логин на дублирование
                    MySqlCommand checkLogin = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE UserLogin = '" + login + "' AND UserID != " + userID, con);
                    if (Convert.ToInt32(checkLogin.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Логин уже используется!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Проверяем телефон на дублирование
                    string checkPhoneQuery = "SELECT COUNT(*) FROM `Users` WHERE PhoneNumber = '" + phone + "' AND UserID != " + userID;
                    using (MySqlCommand checkPhoneCmd = new MySqlCommand(checkPhoneQuery, con))
                    {
                        int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());

                        if (phoneCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким номером телефона уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Собираем запрос для обновления пользователя
                    string query = "UPDATE Users SET Surname = '" + surname + "', FirstName = '" + firstName + "', " +
                        "MiddleName = '" + middleName + "', UserLogin = '" + login + "', " +
                        "RoleID = " + roleID + ", PhoneNumber = '" + phone + "'";

                    // Если пароль передан - добавляем его обновление
                    if (password != null)
                    {
                        query += ", UserPassword = '" + password + "'";
                    }

                    query += " WHERE UserID = " + userID;

                    MySqlCommand update = new MySqlCommand(query, con);
                    return update.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Обновляет данные товара с проверкой на дублирование названия
        /// </summary>
        public static bool UpdateProduct(int productID, string productName, string productDescription, int categoryID, decimal basePrice)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем название товара на дублирование
                    MySqlCommand checkName = new MySqlCommand("SELECT COUNT(*) FROM Product WHERE ProductName = '" + productName + "' AND ProductID != " + productID, con);

                    if (Convert.ToInt32(checkName.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Услуга с таким названием уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Обновляем товар (цену приводим к формату США через точку)
                    MySqlCommand update = new MySqlCommand(
                        "UPDATE Product SET ProductName = '" + productName + "', ProductDescription = '" + productDescription +
                        "', CategoryID = " + categoryID + ", BasePrice = " + basePrice.ToString().Replace(",", ".") +
                        " WHERE ProductID = " + productID, con);

                    return update.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Обновляет данные клиента с проверкой на дублирование email и телефона
        /// </summary>
        public static bool UpdateClient(int clientID, string surname, string firstName, string middleName, string phone, string email)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем email на дублирование
                    string checkEmailQuery = "SELECT COUNT(*) FROM Clients WHERE Email = '" + email + "' AND ClientID != " + clientID;
                    using (MySqlCommand checkEmailCmd = new MySqlCommand(checkEmailQuery, con))
                    {
                        int emailCount = Convert.ToInt32(checkEmailCmd.ExecuteScalar());
                        if (emailCount > 0)
                        {
                            MessageBox.Show("Клиент с таким email уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Проверяем телефон на дублирование
                    string checkPhoneQuery = "SELECT COUNT(*) FROM Clients WHERE PhoneNumber = '" + phone + "' AND ClientID != " + clientID;
                    using (MySqlCommand checkPhoneCmd = new MySqlCommand(checkPhoneQuery, con))
                    {
                        int phoneCount = Convert.ToInt32(checkPhoneCmd.ExecuteScalar());
                        if (phoneCount > 0)
                        {
                            MessageBox.Show("Клиент с таким номером телефона уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Обновляем клиента
                    string query = "UPDATE Clients SET Surname = '" + surname + "', FirstName = '" + firstName + "', " +
                        "MiddleName = '" + middleName + "', PhoneNumber = '" + phone + "', Email = '" + email + "'" +
                        " WHERE ClientID = " + clientID;

                    MySqlCommand update = new MySqlCommand(query, con);
                    return update.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Обновляет статус заказа и срок выполнения
        /// </summary>
        public static bool UpdateOrderStatusAndDate(int orderID, string status, DateTime compDate)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Получаем ID статуса по названию
                    MySqlCommand cmdStatus = new MySqlCommand("SELECT StatusID FROM Status WHERE StatusName = '" + status.Replace("'", "''") + "'", con);
                    int statusID = Convert.ToInt32(cmdStatus.ExecuteScalar());

                    // Приводим дату к формату БД
                    string dateStr = compDate.ToString("yyyy-MM-dd");

                    // Обновляем заказ со статусом и датой выполнения
                    string query = "UPDATE `Order` SET StatusID = " + statusID + ", OrderCompDate = '" + dateStr + "' WHERE OrderID = " + orderID;

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
    }
}