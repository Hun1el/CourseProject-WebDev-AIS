using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace WebSiteDev
{
    /// <summary>
    /// Статический класс для удаления записей из БД с проверкой на связанные данные
    /// </summary>
    public static class DataDelete
    {
        /// <summary>
        /// Удаляет категорию если её нет в товарах
        /// </summary>
        public static bool DeleteCategory(int categoryID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем использует ли категорию какой-нибудь товар
                    MySqlCommand Links = new MySqlCommand("SELECT COUNT(*) FROM Product WHERE CategoryID = " + categoryID, con);

                    if (Convert.ToInt32(Links.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Категория используется в услгах! Удаление невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Удаляем категорию
                    MySqlCommand DeleteQuery = new MySqlCommand("DELETE FROM Category WHERE CategoryID = " + categoryID, con);

                    return DeleteQuery.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Удаляет роль если её нет в пользователях
        /// </summary>
        public static bool DeleteRole(int roleID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем использует ли роль какой-нибудь пользователь
                    MySqlCommand Links = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE RoleID = " + roleID, con);

                    if (Convert.ToInt32(Links.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Данная роль используется! Удаление невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Удаляем роль
                    MySqlCommand DeleteQuery = new MySqlCommand("DELETE FROM Role WHERE RoleID = " + roleID, con);

                    return DeleteQuery.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Удаляет статус если его нет в заказах
        /// </summary>
        public static bool DeleteStatus(int statusID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем использует ли статус какой-нибудь заказ
                    MySqlCommand Links = new MySqlCommand("SELECT COUNT(*) FROM `Order` WHERE StatusID = " + statusID, con);

                    if (Convert.ToInt32(Links.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Данный статус используется! Удаление невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Удаляем статус
                    MySqlCommand DeleteQuery = new MySqlCommand("DELETE FROM Status WHERE StatusID = " + statusID, con);

                    return DeleteQuery.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Удаляет товар если его нет в заказах
        /// </summary>
        public static bool DeleteProduct(int productID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем есть ли товар в каких-нибудь заказах
                    MySqlCommand CheckOrderProduct = new MySqlCommand("SELECT COUNT(*) FROM orderproduct WHERE ProductID = " + productID, con);

                    if (Convert.ToInt32(CheckOrderProduct.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Услуга используется в заказах! Удаление невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Удаляем товар
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Product WHERE ProductID = " + productID, con);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Удаляет пользователя если его нет в заказах и это не текущий пользователь
        /// </summary>
        public static bool DeleteUser(int userID, int currentUserID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем что это не текущий пользователь
                    if (userID == currentUserID)
                    {
                        MessageBox.Show("Невозможно удалить пользователя под которым совершён вход!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Проверяем есть ли у пользователя заказы
                    MySqlCommand checkOrders = new MySqlCommand("SELECT COUNT(*) FROM `Order` WHERE UserID = " + userID, con);

                    if (Convert.ToInt32(checkOrders.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Пользователь имеет заказы! Удаление невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Удаляем пользователя
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Users WHERE UserID = " + userID, con);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Удаляет клиента если его нет в заказах
        /// </summary>
        public static bool DeleteClient(int clientID)
        {
            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    // Проверяем есть ли у клиента заказы
                    MySqlCommand CheckOrders = new MySqlCommand("SELECT COUNT(*) FROM `Order` WHERE ClientID = " + clientID, con);

                    if (Convert.ToInt32(CheckOrders.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Клиент имеет заказы! Удаление невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Удаляем клиента
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM Clients WHERE ClientID = " + clientID, con);

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