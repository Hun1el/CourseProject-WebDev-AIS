using System;
using System.Drawing;
using System.Windows.Forms;
using WebSiteDev.ManagerForm;

namespace WebSiteDev
{
    /// <summary>
    /// Главная форма приложения для администраторов
    /// Содержит меню навигации для управления пользователями, категориями, ролями, статусами, услугами и заказами
    /// </summary>
    public partial class MainForm : Form
    {
        private string fullName;
        private string roleName;
        private int userID;
        private Button currentSelectedButton = null;
        private UserControl currentControl = null;

        /// <summary>
        /// Конструктор главной формы, инициализирует данные пользователя
        /// </summary>
        public MainForm(string fullName, string roleName, int userID = 0)
        {
            InitializeComponent();
            this.fullName = fullName;
            this.roleName = roleName;
            this.userID = userID;
        }

        /// <summary>
        /// При загрузке формы - выводит данные текущего пользователя
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            label2.Text = $"Сотрудник: {fullName}";
            label3.Text = $"Доступ: {roleName}";
        }

        /// <summary>
        /// Загружает контрол в основную панель и скрывает информацию приветствия
        /// </summary>
        private void LoadControl(UserControl control)
        {
            FormControl.ResetFormSize(this);
            pictureBox2.Visible = false;
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;

            // Удаляем старый контрол если он есть
            if (currentControl != null)
            {
                panel2.Controls.Remove(currentControl);
                currentControl.Dispose();
                currentControl = null;
            }

            // Добавляем новый контрол
            control.Dock = DockStyle.Fill;
            panel2.Controls.Add(control);

            currentControl = control;
        }

        /// <summary>
        /// Кнопка "Пользователи" - загружает контрол управления пользователями
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (currentSelectedButton == button1)
            {
                return;
            }

            LoadControl(new AdminForm.UsersControl(userID));
            this.Text = "Пользователи";
            SelectButton(button1);
        }

        /// <summary>
        /// Кнопка "Категории" - загружает контрол управления категориями
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentSelectedButton == button2)
            {
                return;
            }

            LoadControl(new AdminForm.CategoryControl());
            this.Text = "Список категорий";
            SelectButton(button2);
        }

        /// <summary>
        /// Кнопка "Роли" - загружает контрол управления ролями
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentSelectedButton == button3)
            {
                return;
            }

            LoadControl(new AdminForm.RoleControl());
            this.Text = "Список ролей";
            SelectButton(button3);
        }

        /// <summary>
        /// Кнопка "Статусы" - загружает контрол управления статусами
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            if (currentSelectedButton == button4)
            {
                return;
            }

            LoadControl(new AdminForm.StatusControl());
            this.Text = "Список статусов";
            SelectButton(button4);
        }

        /// <summary>
        /// Кнопка "Смена учётной записи" - выход и возврат на форму авторизации
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            SelectButton(button5);
            var result = MessageBox.Show("Вы действительно хотите сменить учетную запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (currentControl != null)
                {
                    FormControl.ClearPanelControls(panel2);
                    currentControl = null;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                SelectButton(currentSelectedButton);
            }
        }

        /// <summary>
        /// Кнопка "Выход" - закрывает приложение
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            SelectButton(button6);
            var result = MessageBox.Show("Вы действительно хотите выйти из приложения?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                Application.Exit();
            }
            else
            {
                SelectButton(currentSelectedButton);
            }
        }

        /// <summary>
        /// Кнопка "Услуги" - загружает контрол управления услугами
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            if (currentSelectedButton == button7)
            {
                return;
            }

            LoadControl(new ProductControl(roleName));
            this.Text = "Список услуг";
            SelectButton(button7);
        }

        /// <summary>
        /// Кнопка "Заказы" - загружает контрол управления заказами
        /// </summary>
        private void button8_Click(object sender, EventArgs e)
        {
            if (currentSelectedButton == button8)
            {
                return;
            }

            LoadControl(new OrderControl(roleName));
            this.Text = "Список заказов";
            SelectButton(button8);
        }

        /// <summary>
        /// Выделяет нажатую кнопку и убирает выделение с других
        /// Кнопка "Выход" всегда красного цвета
        /// </summary>
        private void SelectButton(Button selectedButton)
        {
            Button[] buttons = { button1, button2, button3, button4, button5, button6, button7, button8 };

            // Перебираем все кнопки
            foreach (Button btn in buttons)
            {
                if (btn == selectedButton)
                {
                    // Выделяем нажатую кнопку
                    btn.BackColor = Color.FromArgb(45, 156, 219);
                    btn.FlatStyle = FlatStyle.Flat;

                    // Кнопка выхода всегда красная
                    if (btn == button6)
                    {
                        btn.ForeColor = Color.White;
                        btn.BackColor = Color.Crimson;
                    }
                    else
                    {
                        btn.ForeColor = Color.White;
                    }
                }
                else
                {
                    // Убираем выделение с остальных кнопок
                    btn.BackColor = SystemColors.Control;
                    btn.FlatStyle = FlatStyle.Standard;

                    // Кнопка выхода красного цвета даже без выделения
                    if (btn == button6)
                    {
                        btn.ForeColor = Color.Red;
                    }
                    else
                    {
                        btn.ForeColor = Color.Black;
                    }
                }
            }

            // Не запоминаем выделение для кнопок смены учётной записи и выхода
            if (selectedButton != button5 && selectedButton != button6)
            {
                currentSelectedButton = selectedButton;
            }
        }
    }
}