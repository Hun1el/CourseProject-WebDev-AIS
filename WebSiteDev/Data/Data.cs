using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteDev
{
    public static class Data
    {
        /// <summary>
        /// Получает строку подключения к MySQL БД с использованием параметров из настроек приложения
        /// </summary>
        public static string GetConnectionString()
        {
            return $"host={Properties.Settings.Default.DbHost};" +
                   $"uid={Properties.Settings.Default.DbUser};" +
                   $"pwd={Properties.Settings.Default.DbPassword};" +
                   $"database={Properties.Settings.Default.DbName};";
        }

        /// <summary>
        /// Получает строку подключения к MySQL серверу БЕЗ указания конкретной БД
        /// </summary>
        public static string GetConnectionStringNoDB()
        {
            return $"host={Properties.Settings.Default.DbHost};" +
                   $"uid={Properties.Settings.Default.DbUser};" +
                   $"pwd={Properties.Settings.Default.DbPassword};";
        }
    }
}