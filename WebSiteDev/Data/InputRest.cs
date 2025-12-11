using System;
using System.Linq;
using System.Windows.Forms;

namespace WebSiteDev
{
    public static class InputRest
    {
        /// <summary>
        /// Делает первую букву в TextBox заглавной
        /// </summary>
        public static void FirstLetter(TextBox textBox)
        {
            if (textBox.Text.Length > 0)
            {
                string text = textBox.Text;
                textBox.Text = char.ToUpper(text[0]) + text.Substring(1);
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        /// <summary>
        /// Разрешает ввод символов для email - буквы, цифры, точка, дефис, подчёркивание
        /// </summary>
        public static void EmailInput(KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == '-' || e.KeyChar == '_' || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод символов для логина - буквы, цифры, подчёркивание, точка, дефис
        /// </summary>
        public static void LoginInput(KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z') || (e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '_' || e.KeyChar == '.' || e.KeyChar == '-' || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод только русских букв и буквы Ё
        /// </summary>
        public static void OnlyRussian(KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё' || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод русских букв и дефиса (максимум 2 дефиса, не подряд, не в начале)
        /// </summary>
        public static void OnlyRussianAndDash(KeyPressEventArgs e, TextBox textBox)
        {
            // Базовая проверка - русские буквы и дефис
            if (!((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё' || e.KeyChar == '-' || e.KeyChar == '\b'))
            {
                e.Handled = true;
                return;
            }

            // Если вводят дефис - проверяем ограничения
            if (e.KeyChar == '-')
            {
                // Максимум 2 дефиса в строке
                if (textBox.Text.Count(c => c == '-') >= 2)
                {
                    e.Handled = true;
                    return;
                }

                // Дефис не может быть в начале
                if (textBox.Text.Length == 0)
                {
                    e.Handled = true;
                    return;
                }

                // Два дефиса не могут быть подряд
                if (textBox.SelectionStart > 0 && textBox.Text[textBox.SelectionStart - 1] == '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Разрешает ввод английских букв и пунктуации
        /// </summary>
        public static void OnlyEnglishAndSpecial(KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 'A' && e.KeyChar <= 'z') || char.IsPunctuation(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод только английских букв
        /// </summary>
        public static void OnlyEnglish(KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 'A' && e.KeyChar <= 'z') || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод английских букв, цифр и специальных символов
        /// </summary>
        public static void EnglishDigitsAndSpecial(KeyPressEventArgs e)
        {
            const string allowedSpecial = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            if (!((e.KeyChar >= 'A' && e.KeyChar <= 'z') || char.IsDigit(e.KeyChar) || allowedSpecial.Contains(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод всех символов
        /// </summary>
        public static void AllowAll(KeyPressEventArgs e)
        {
            e.Handled = false;
        }

        /// <summary>
        /// Разрешает ввод только цифр
        /// </summary>
        public static void OnlyNumbers(KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Блокирует опасные символы - угловые скобки и обратный апостроф
        /// Позволяет русские, английские буквы и цифры
        /// </summary>
        public static void RussianEnglishAndDigits(KeyPressEventArgs e)
        {
            char[] blockChars = new char[] { '<', '>', '`' };

            if (blockChars.Contains(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Разрешает ввод для названия категории - русские, английские буквы, цифры, спецсимволы
        /// Допускаются: дефис, скобки, пробел, слэш, амперсанд, плюс
        /// </summary>
        public static void CategoryInput(KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 'А' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё' ||
                  (e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z') ||
                  (e.KeyChar >= '0' && e.KeyChar <= '9') ||
                  e.KeyChar == '-' || e.KeyChar == '(' || e.KeyChar == ')' || e.KeyChar == ' ' ||
                  e.KeyChar == '/' || e.KeyChar == '&' || e.KeyChar == '+' || e.KeyChar == '\b'))
            {
                e.Handled = true;
            }
        }
    }
}