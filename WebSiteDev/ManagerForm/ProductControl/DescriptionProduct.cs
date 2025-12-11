using System;
using System.Windows.Forms;

namespace WebSiteDev
{
    /// <summary>
    /// Форма для просмотра полного описания товара в отдельном окне
    /// </summary>
    public partial class DescriptionProduct : Form
    {
        /// <summary>
        /// Инициализирует форму описания товара
        /// </summary>
        public DescriptionProduct()
        {
            InitializeComponent();
        }

        public void SetDescription(string productName, string description)
        {
            // Устанавливаем заголовок окна с названием товара
            this.Text = "Описание: " + productName;

            // Выводим полное описание в многострочное текстовое поле
            textBox1.Text = description;

            // Убираем выделение текста и устанавливаем курсор в начало
            textBox1.Select(0, 0);
        }

        /// <summary>
        /// Кнопка закрыть - закрывает окно описания товара
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}