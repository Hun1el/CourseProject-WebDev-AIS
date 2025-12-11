using System;
using System.Collections.Generic;
using System.Data;

namespace WebSiteDev
{
    /// <summary>
    /// Класс для защиты конфиденциальных данных
    /// </summary>
    public class DataSecurity
    {
        // Словари для хранения оригинальных данных
        private Dictionary<int, string> originalLogins = new Dictionary<int, string>();
        private Dictionary<int, string> originalPhones = new Dictionary<int, string>();
        private Dictionary<int, string> originalFirstNames = new Dictionary<int, string>();
        private Dictionary<int, string> originalMiddleNames = new Dictionary<int, string>();
        private Dictionary<int, string> originalClientNames = new Dictionary<int, string>();
        private Dictionary<int, string> originalUserNames = new Dictionary<int, string>();

        /// <summary>
        /// Загружает оригинальные данные из DataTable в словари для последующей защиты
        /// </summary>
        public void LoadOriginalData(DataTable dt, string loginColumn = "UserLogin", string phoneColumn = "PhoneNumber", string firstNameColumn = "FirstName", string middleNameColumn = "MiddleName")
        {
            originalLogins.Clear();
            originalPhones.Clear();
            originalFirstNames.Clear();
            originalMiddleNames.Clear();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int id = -1;

                // Определяем ID - может быть ClientID или UserID
                if (dt.Columns.Contains("ClientID"))
                {
                    id = Convert.ToInt32(dt.Rows[i]["ClientID"]);
                }
                else if (dt.Columns.Contains("UserID"))
                {
                    id = Convert.ToInt32(dt.Rows[i]["UserID"]);
                }
                else
                {
                    id = i;
                }

                // Загружаем оригинальные данные если колонки существуют
                if (dt.Columns.Contains(loginColumn))
                {
                    originalLogins[id] = dt.Rows[i][loginColumn].ToString();
                }
                if (dt.Columns.Contains(phoneColumn))
                {
                    originalPhones[id] = dt.Rows[i][phoneColumn].ToString();
                }
                if (dt.Columns.Contains(firstNameColumn))
                {
                    originalFirstNames[id] = dt.Rows[i][firstNameColumn].ToString();
                }
                if (dt.Columns.Contains(middleNameColumn))
                {
                    originalMiddleNames[id] = dt.Rows[i][middleNameColumn].ToString();
                }
            }
        }

        /// <summary>
        /// Загружает оригинальные имена клиентов из таблицы заказов
        /// </summary>
        public void LoadOriginalClientNames(DataTable dt, string clientNameColumn = "ClientName")
        {
            originalClientNames.Clear();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int clientID = Convert.ToInt32(dt.Rows[i]["OrderID"]);

                if (dt.Columns.Contains(clientNameColumn))
                {
                    originalClientNames[clientID] = dt.Rows[i][clientNameColumn].ToString();
                }
            }
        }

        /// <summary>
        /// Загружает оригинальные имена сотрудников из таблицы заказов
        /// </summary>
        public void LoadOriginalUserNames(DataTable dt, string userNameColumn = "UserName")
        {
            originalUserNames.Clear();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int orderID = Convert.ToInt32(dt.Rows[i]["OrderID"]);

                if (dt.Columns.Contains(userNameColumn))
                {
                    originalUserNames[orderID] = dt.Rows[i][userNameColumn].ToString();
                }
            }
        }

        /// <summary>
        /// Обновляет оригинальные данные пользователя после изменений
        /// </summary>
        public void UpdateOriginalData(int clientID, DataRow row)
        {
            if (row.Table.Columns.Contains("UserLogin"))
            {
                originalLogins[clientID] = row["UserLogin"].ToString();
            }
            if (row.Table.Columns.Contains("PhoneNumber"))
            {
                originalPhones[clientID] = row["PhoneNumber"].ToString();
            }
            if (row.Table.Columns.Contains("FirstName"))
            {
                originalFirstNames[clientID] = row["FirstName"].ToString();
            }
            if (row.Table.Columns.Contains("MiddleName"))
            {
                originalMiddleNames[clientID] = row["MiddleName"].ToString();
            }
        }

        /// <summary>
        /// Получает оригинальный логин по ID
        /// </summary>
        public string GetOriginalLogin(int id)
        {
            if (originalLogins.ContainsKey(id))
            {
                return originalLogins[id];
            }
            return null;
        }

        /// <summary>
        /// Получает оригинальный номер телефона по ID
        /// </summary>
        public string GetOriginalPhone(int id)
        {
            if (originalPhones.ContainsKey(id))
            {
                return originalPhones[id];
            }
            return null;
        }

        /// <summary>
        /// Получает оригинальное имя по ID
        /// </summary>
        public string GetOriginalFirstName(int id)
        {
            if (originalFirstNames.ContainsKey(id))
            {
                return originalFirstNames[id];
            }
            return null;
        }

        /// <summary>
        /// Получает оригинальное отчество по ID
        /// </summary>
        public string GetOriginalMiddleName(int id)
        {
            if (originalMiddleNames.ContainsKey(id))
            {
                return originalMiddleNames[id];
            }
            return null;
        }

        /// <summary>
        /// Получает оригинальное имя клиента по ID заказа
        /// </summary>
        public string GetOriginalClientName(int id)
        {
            if (originalClientNames.ContainsKey(id))
            {
                return originalClientNames[id];
            }
            return null;
        }

        /// <summary>
        /// Получает оригинальное имя сотрудника по ID заказа
        /// </summary>
        public string GetOriginalUserName(int id)
        {
            if (originalUserNames.ContainsKey(id))
            {
                return originalUserNames[id];
            }
            return null;
        }

        /// <summary>
        /// Маскирует логин полностью - заменяет все символы на •
        /// </summary>
        public static string MaskLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                return login;
            }
            return new string('•', login.Length);
        }

        /// <summary>
        /// Маскирует имя - оставляет первый символ, остальное заменяет на •
        /// </summary>
        public static string MaskName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            if (name.Length == 1)
            {
                return name;
            }
            return name[0] + new string('•', name.Length - 1);
        }

        /// <summary>
        /// Маскирует номер телефона - показывает начало, скрывает последние 4-5 символов
        /// </summary>
        public static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            {
                return phone;
            }

            if (phone.Length >= 5)
            {
                string visible = phone.Substring(0, phone.Length - 5);
                string masked = "••-••";
                return visible + masked;
            }

            string visiblePart = phone.Substring(0, phone.Length - 4);
            string maskedPart = new string('•', 4);
            return visiblePart + maskedPart;
        }

        /// <summary>
        /// Маскирует ФИО клиента - показывает фамилию и инициалы имени/отчества
        /// </summary>
        public static string MaskClientName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            string[] parts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return fullName;
            }

            // Фамилия - видна полностью
            string surname = parts[0];
            string firstInitial = "";
            string middleInitial = "";

            // Имя - только инициал
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                firstInitial = parts[1][0] + ".";
            }

            // Отчество - только инициал
            if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
            {
                middleInitial = parts[2][0] + ".";
            }

            return surname + " " + firstInitial + middleInitial;
        }

        /// <summary>
        /// Маскирует ФИО сотрудника - показывает фамилию и инициалы имени/отчества
        /// </summary>
        public static string MaskUserName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            string[] parts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return fullName;
            }

            // Фамилия - видна полностью
            string surname = parts[0];
            string firstInitial = "";
            string middleInitial = "";

            // Имя - только инициал
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                firstInitial = parts[1][0] + ".";
            }

            // Отчество - только инициал
            if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
            {
                middleInitial = parts[2][0] + ".";
            }

            return surname + " " + firstInitial + middleInitial;
        }

        public void Clear()
        {
            originalLogins.Clear();
            originalPhones.Clear();
            originalFirstNames.Clear();
            originalMiddleNames.Clear();
            originalClientNames.Clear();
            originalUserNames.Clear();
        }
    }
}