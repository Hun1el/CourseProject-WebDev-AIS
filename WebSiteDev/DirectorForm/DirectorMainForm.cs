using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebSiteDev.ManagerForm
{
    public partial class DirectorMainForm : Form
    {
        private string fullName;
        private string roleName;
        private Button currentSelectedButton = null;
        private UserControl currentControl = null;

        public DirectorMainForm(string fullName, string roleName)
        {
            InitializeComponent();
            this.fullName = fullName;
            this.roleName = roleName;
        }

        private void DirectorMainForm_Load(object sender, EventArgs e)
        {
            Inactivity.OnFormLoad(this);

            label2.Text = $"Сотрудник: {fullName}";
            label3.Text = $"Доступ: {roleName}";
        }

        /// <summary>
        /// Загружает контрол в основную панель и скрывает информацию приветствия
        /// </summary>
        private void LoadControl(UserControl control)
        {
            // Скрываем элементы приветствия
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
        /// Кнопка "Учёт заказов" - загружает контрол для просмотра и экспорта заказов
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            // Если уже открыт этот контрол - ничего не делаем
            if (currentSelectedButton == button3)
            {
                return;
            }

            SelectButton(button3);
            LoadControl(new DirectorOrderControl());
            this.Text = "Учет заказов";
        }

        /// <summary>
        /// Кнопка "Смена учётной записи" - закрывает форму директора и возвращает на форму входа
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            SelectButton(button5);

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите сменить учетную запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Очищаем открытый контрол
                if (currentControl != null)
                {
                    FormControl.ClearPanelControls(panel2);
                    currentControl = null;
                }

                // Закрываем форму директора
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // Если отказали - выбираем предыдущую кнопку
                SelectButton(currentSelectedButton);
            }
        }

        /// <summary>
        /// Кнопка "Выход" - закрывает приложение полностью
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            SelectButton(button6);

            // Запрашиваем подтверждение
            var result = MessageBox.Show("Вы действительно хотите выйти из приложения?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                Application.Exit();
            }
            else
            {
                // Если отказали - выбираем предыдущую кнопку
                SelectButton(currentSelectedButton);
            }
        }

        /// <summary>
        /// Выбирает кнопку и изменяет её оформление - отмечает активную кнопку
        /// </summary>
        private void SelectButton(Button selectedButton)
        {
            Button[] buttons = { button3, button5, button6 };

            foreach (Button btn in buttons)
            {
                if (btn == selectedButton)
                {
                    // Окрашиваем выбранную кнопку в голубой цвет
                    btn.BackColor = Color.FromArgb(45, 156, 219);
                    btn.FlatStyle = FlatStyle.Flat;

                    // Кнопка выхода - красная
                    if (btn == button6)
                    {
                        btn.ForeColor = Color.White;
                        btn.BackColor = Color.Crimson;
                    }
                    else
                    {
                        // Остальные кнопки - белый текст на голубом фоне
                        btn.ForeColor = Color.White;
                    }
                }
                else
                {
                    // Невыбранные кнопки - стандартное оформление
                    btn.BackColor = SystemColors.Control;
                    btn.FlatStyle = FlatStyle.Standard;

                    // Кнопка выхода - красный текст
                    if (btn == button6)
                    {
                        btn.ForeColor = Color.Red;
                    }
                    else
                    {
                        // Остальные кнопки - чёрный текст
                        btn.ForeColor = Color.Black;
                    }
                }
            }

            // Сохраняем текущую выбранную кнопку (кроме кнопок смены учётной записи и выхода)
            if (selectedButton != button5 && selectedButton != button6)
            {
                currentSelectedButton = selectedButton;
            }
        }

        private void DirectorMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Inactivity.OnFormClosing(this);
        }
    }
}