using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using WebSiteDev.ManagerForm;

namespace WebSiteDev
{
    public partial class AuthForm : Form
    {
        private BlockForms blockForms;
        private string captchaText;
        private bool captchaRequired = false;
        private int failedAttempts = 0;
        private Timer lockoutTimer;
        private int lockoutSeconds = 0;

        public AuthForm()
        {
            InitializeComponent();
            FolderPermissions.InitializeImagesFolder();
            lockoutTimer = new Timer();
            lockoutTimer.Interval = 1000;
            lockoutTimer.Tick += LockoutTimer_Tick;
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {
            blockForms = Program.GetBlockForms();
            blockForms.RegisterForm(this);
            blockForms.Start();
            HideCaptcha();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lockoutTimer.Enabled)
            {
                MessageBox.Show("Вход заблокирован. Попробуйте через " + lockoutSeconds + " сек.", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string login = textBox1.Text.Trim();
            string password = textBox2.Text;
            string hashedPassword = GetSha256(password);

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (captchaRequired && textBox3.Text != captchaText)
            {
                MessageBox.Show("Неверно введена CAPTCHA!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                failedAttempts++;
                if (failedAttempts >= 2)
                {
                    MessageBox.Show("Вход заблокирован на 10 секунд!", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    StartLockout();
                }
                else
                {
                    CaptchaToImage();
                }
                textBox3.Clear();
                return;
            }

            string adminLogin = Properties.Settings.Default.AdminLogin;
            string adminPassword = Properties.Settings.Default.AdminPassword;

            if (login == adminLogin && password == adminPassword)
            {
                failedAttempts = 0;
                captchaRequired = false;
                HideCaptcha();
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
                     WHERE u.UserLogin = @login AND u.UserPassword = @password LIMIT 1;";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            failedAttempts = 0;
                            captchaRequired = false;
                            HideCaptcha();

                            int userID = Convert.ToInt32(reader["UserID"]);
                            string fullName = reader["Surname"].ToString() + " " + reader["FirstName"].ToString() + " " + reader["MiddleName"].ToString();
                            string role = reader["RoleName"].ToString();

                            blockForms.Stop();

                            if (role == "Администратор")
                            {
                                MainForm mainForm = new MainForm(fullName, role, userID);
                                blockForms.RegisterForm(mainForm);
                                this.Hide();
                                mainForm.ShowDialog();
                                this.Show();
                                blockForms.UnregisterForm(mainForm);
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
                            failedAttempts++;
                            if (failedAttempts == 1)
                            {
                                MessageBox.Show("Неверный логин или пароль!\nДля дальнейших попыток требуется ввод CAPTCHA.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                captchaRequired = true;
                                ShowCaptcha();
                            }
                            else if (failedAttempts >= 2)
                            {
                                MessageBox.Show("Неверный логин или пароль!\nВход заблокирован на 10 секунд!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                StartLockout();
                            }
                            textBox1.Text = "";
                            textBox2.Text = "";
                            textBox3.Clear();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    HandleDatabaseError(ex);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
        }

        private void CaptchaToImage()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            captchaText = "";

            for (int i = 0; i < 5; i++)
            {
                captchaText += chars[random.Next(chars.Length)];
            }

            Bitmap bmp = new Bitmap(pictureBox4.Width, pictureBox4.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            Font font = new Font("Arial", 20, FontStyle.Bold);

            for (int i = 0; i < 5; i++)
            {
                var state = g.Save();
                g.TranslateTransform(75, 60);
                g.RotateTransform(random.Next(-10, 10));
                g.DrawString(captchaText[i].ToString(), font, Brushes.Black, i * 20, 0);
                g.Restore(state);
            }

            for (int i = 0; i < 10; i++)
            {
                Pen pen = new Pen(Color.Black, random.Next(2, 5));
                g.DrawLine(pen, random.Next(pictureBox4.Width), random.Next(pictureBox4.Height),
                random.Next(pictureBox4.Width), random.Next(pictureBox4.Height));
            }

            font.Dispose();
            g.Dispose();
            pictureBox4.Image = bmp;
        }

        private void ShowCaptcha()
        {
            pictureBox4.Visible = true;
            textBox3.Visible = true;
            pictureBox5.Visible = true;
            label5.Visible = true;
            label6.Visible = true;

            if (this.Width != 895)
            {
                this.Width = 895;
            }

            CaptchaToImage();
        }

        private void HideCaptcha()
        {
            pictureBox4.Visible = false;
            textBox3.Visible = false;
            pictureBox5.Visible = false;
            label5.Visible = false;
            label6.Visible = false;

            if (this.Width != 660)
            {
                this.Width = 660;
            }
            textBox3.Clear();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            CaptchaToImage();
            textBox3.Clear();
            textBox3.Focus();
        }

        private void StartLockout()
        {
            lockoutSeconds = 10;
            button1.Enabled = false;
            button2.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            pictureBox5.Enabled = false;
            pictureBox2.Enabled = false;
            pictureBox3.Enabled = false;
            lockoutTimer.Start();
        }

        private void LockoutTimer_Tick(object sender, EventArgs e)
        {
            lockoutSeconds--;
            button1.Text = "Вход (" + lockoutSeconds + " сек)";
            if (lockoutSeconds <= 0)
            {
                lockoutTimer.Stop();
                button1.Enabled = true;
                button2.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                pictureBox2.Enabled = true;
                pictureBox3.Enabled = true;
                pictureBox5.Enabled = true;
                button1.Text = "Вход";
                CaptchaToImage();
                textBox3.Clear();
                textBox3.Focus();
            }
        }

        private void HandleDatabaseError(MySqlException ex)
        {
            string msg = "";
            if (ex.Number == 0)
            {
                msg = "Не удаётся подключиться к серверу базы данных.";
            }
            else if (ex.Number == 1045)
            {
                msg = "Ошибка доступа отклонена!";
            }
            else if (ex.Number == 1049)
            {
                msg = "База данных не найдена!";
            }
            else if (ex.Number == 2003)
            {
                msg = "Не удаётся подключиться к MySQL серверу.";
            }
            else if (ex.Number == 2006)
            {
                msg = "MySQL сервер отключен.";
            }
            else
            {
                msg = "Ошибка БД (код: " + ex.Number + "): " + ex.Message;
            }

            MessageBox.Show(msg, "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            if (allowedChars.IndexOf(e.KeyChar) == -1 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox3.SelectionStart = textBox3.Text.Length;
        }

        private string GetSha256(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
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
            if (lockoutTimer != null)
            {
                lockoutTimer.Stop();
            }
        }
    }
}