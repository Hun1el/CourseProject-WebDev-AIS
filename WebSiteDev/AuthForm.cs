using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using WebSiteDev.ManagerForm;

namespace WebSiteDev
{
    public partial class AuthForm : Form
    {
        private BlockForms blockForms;

        public AuthForm()
        {
            InitializeComponent();
            FolderPermissions.InitializeImagesFolder();
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {
            blockForms = Program.GetBlockForms();
            blockForms.RegisterForm(this);
            blockForms.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text;
            string hashedPassword = GetSha256(password);

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string adminLogin = Properties.Settings.Default.AdminLogin;
            string adminPassword = Properties.Settings.Default.AdminPassword;

            if (login == adminLogin && password == adminPassword)
            {
                blockForms.Stop();

                MainForm adminForm = new MainForm("Администратор", "Администратор", 0);
                blockForms.RegisterForm(adminForm);
                this.Hide();
                adminForm.ShowDialog();
                this.Show();
                blockForms.UnregisterForm(adminForm);

                blockForms.Restart();

                textBox1.Text = "";
                textBox2.Text = "";
                return;
            }

            using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
            {
                try
                {
                    con.Open();

                    string query = @"SELECT u.UserID, u.FirstName, u.Surname, u.MiddleName, r.RoleName 
                             FROM Users u JOIN Role r ON u.RoleID = r.RoleID 
                             WHERE u.UserLogin = @login AND u.UserPassword = @password
                             LIMIT 1;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userID = Convert.ToInt32(reader["UserID"]);
                            string fullName = string.Format("{0} {1} {2}",
                                reader["Surname"],
                                reader["FirstName"],
                                reader["MiddleName"]);
                            string role = reader["RoleName"].ToString();

                            blockForms.Stop();

                            if (role == "Администратор")
                            {
                                MainForm adminForm = new MainForm(fullName, role, userID);
                                blockForms.RegisterForm(adminForm);
                                this.Hide();
                                adminForm.ShowDialog();
                                this.Show();
                                blockForms.UnregisterForm(adminForm);
                            }
                            else if (role == "Менеджер")
                            {
                                ManagerMainForm managerForm = new ManagerMainForm(fullName, role, userID);
                                blockForms.RegisterForm(managerForm);
                                this.Hide();
                                managerForm.ShowDialog();
                                this.Show();
                                blockForms.UnregisterForm(managerForm);
                            }
                            else if (role == "Директор")
                            {
                                DirectorMainForm directorForm = new DirectorMainForm(fullName, role);
                                blockForms.RegisterForm(directorForm);
                                this.Hide();
                                directorForm.ShowDialog();
                                this.Show();
                                blockForms.UnregisterForm(directorForm);
                            }

                            blockForms.Restart();

                            textBox1.Text = "";
                            textBox2.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textBox1.Text = "";
                            textBox2.Text = "";
                        }
                    }
                }
                catch (MySqlException Ex)
                {
                    HandleDatabaseError(Ex);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Неожиданная ошибка:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
        }

        private void HandleDatabaseError(MySqlException ex)
        {
            string errorMessage = "";

            switch (ex.Number)
            {
                case 0:
                    errorMessage = "Не удаётся подключиться к серверу базы данных.\n\nПроверьте:\n• Адрес хоста\n• Доступность сервера";
                    break;

                case 1045:
                    errorMessage = "Ошибка доступа отклонена!\n\nПроверьте:\n• Имя пользователя\n• Пароль";
                    break;

                case 1049:
                    errorMessage = "База данных не найдена!\n\nПроверьте имя базы данных в настройках.";
                    break;

                case 2003:
                    errorMessage = "Не удаётся подключиться к MySQL серверу.\n\nПроверьте:\n• IP адрес хоста\n• Работает ли сервер MySQL";
                    break;

                case 2006:
                    errorMessage = "MySQL сервер отключен.\n\nПожалуйста, проверьте состояние сервера.";
                    break;

                default:
                    errorMessage = string.Format("Ошибка базы данных (код: {0}):\n{1}", ex.Number, ex.Message);
                    break;
            }

            MessageBox.Show(errorMessage, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из приложения?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (textBox2.UseSystemPasswordChar)
            {
                textBox2.UseSystemPasswordChar = false;
                pictureBox2.BackgroundImage = Properties.Resources.EyeHide;
            }
            else
            {
                textBox2.UseSystemPasswordChar = true;
                pictureBox2.BackgroundImage = Properties.Resources.EyeView;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            this.Visible = false;
            settingsForm.ShowDialog();
            this.Visible = true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EnglishDigitsAndSpecial(e);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputRest.EnglishDigitsAndSpecial(e);
        }

        private string GetSha256(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private void AuthForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (blockForms != null)
            {
                blockForms.UnregisterForm(this);
                blockForms.Stop();
            }
        }
    }
}
