using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WebSiteDev;

namespace WebSiteDev
{
    /// <summary>
    /// Контрол для управления изображениями товара - загрузка, сохранение и отображение фото
    /// </summary>
    public partial class ImageControl : UserControl
    {
        private string selectedImagePath;
        private Image originalImage;

        public string CurrentImagePath { get; set; }

        public ImageControl()
        {
            InitializeComponent();
        }

        private string GetImagesFolderPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "WebShop", "Images");
        }

        public void ShowChangeButton(bool show)
        {
            button1.Visible = show;
        }

        public void InitializeImage(string currentImagePath)
        {
            CurrentImagePath = currentImagePath;
            LoadImage(currentImagePath);

            // Сохраняем оригинальное изображение для возможности отката изменений
            if (pictureBox1.Image != null)
            {
                originalImage = new Bitmap(pictureBox1.Image);
            }
            else
            {
                originalImage = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                ofd.Title = "Выберите изображение";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string sourcePath = ofd.FileName;

                    try
                    {
                        // Проверяем размер файла - не более 2 МБ
                        FileInfo fileInfo = new FileInfo(sourcePath);
                        if (fileInfo.Length > 2 * 1024 * 1024)
                        {
                            MessageBox.Show("Изображение не должно превышать 2 МБ!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        byte[] newImageBytes = File.ReadAllBytes(sourcePath);

                        // Сравниваем новое изображение со старым чтобы избежать дублей
                        if (!string.IsNullOrEmpty(CurrentImagePath))
                        {
                            string imagesFolder = GetImagesFolderPath();
                            string oldFullPath = Path.Combine(imagesFolder, CurrentImagePath);

                            if (File.Exists(oldFullPath))
                            {
                                byte[] oldImageBytes = File.ReadAllBytes(oldFullPath);

                                // Сравниваем побайтово - если идентичны, отменяем операцию
                                if (oldImageBytes.Length == newImageBytes.Length)
                                {
                                    bool isIdentical = true;
                                    for (int i = 0; i < oldImageBytes.Length; i++)
                                    {
                                        if (oldImageBytes[i] != newImageBytes[i])
                                        {
                                            isIdentical = false;
                                            break;
                                        }
                                    }

                                    if (isIdentical)
                                    {
                                        MessageBox.Show("Вы выбрали изображение с идентичным содержимым. Изменений нет.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }
                                }
                            }
                        }

                        // Загружаем выбранное изображение во временное хранилище
                        selectedImagePath = sourcePath;

                        // Освобождаем старое изображение
                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                        }

                        Image tempImage = Image.FromFile(sourcePath);
                        pictureBox1.Image = tempImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Сохраняет выбранное изображение в папку и обновляет путь в БД
        /// </summary>
        public void SaveImage(int productID)
        {
            // Если изображение не было выбрано - выходим
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                return;
            }

            // Очищаем PictureBox чтобы освободить файл для копирования
            pictureBox1.Image = null;

            string fileName = Path.GetFileNameWithoutExtension(selectedImagePath);
            string extension = Path.GetExtension(selectedImagePath);
            string imagesFolder = GetImagesFolderPath();

            try
            {
                // Создаём папку для изображений если её нет
                if (!FolderPermissions.CreateFolderWithFullAccess(imagesFolder))
                {
                    MessageBox.Show("Ошибка: не удалось создать папку для изображений!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                byte[] newImageBytes = File.ReadAllBytes(selectedImagePath);
                string destPath = Path.Combine(imagesFolder, fileName + extension);
                string finalFileName = fileName + extension;

                // Если файл уже существует - проверяем содержимое или генерируем новое имя
                if (File.Exists(destPath))
                {
                    byte[] existingImageBytes = File.ReadAllBytes(destPath);
                    bool isIdentical = false;

                    // Сравниваем файлы побайтово
                    if (existingImageBytes.Length == newImageBytes.Length)
                    {
                        isIdentical = true;
                        for (int i = 0; i < newImageBytes.Length; i++)
                        {
                            if (newImageBytes[i] != existingImageBytes[i])
                            {
                                isIdentical = false;
                                break;
                            }
                        }
                    }

                    // Если идентичны - используем существующий файл
                    if (isIdentical)
                    {
                        finalFileName = Path.GetFileName(destPath);
                    }
                    else
                    {
                        // Если различаются - добавляем номер к имени файла
                        int n = 1;
                        while (File.Exists(destPath))
                        {
                            destPath = Path.Combine(imagesFolder, fileName + " (" + n.ToString() + ")" + extension);
                            n++;
                        }
                        File.Copy(selectedImagePath, destPath, false);
                        finalFileName = Path.GetFileName(destPath);
                    }
                }
                else
                {
                    // Копируем новый файл в папку изображений
                    File.Copy(selectedImagePath, destPath, false);
                }

                // Обновляем путь к фото в БД
                using (MySqlConnection con = new MySqlConnection(Data.GetConnectionString()))
                {
                    con.Open();
                    string updateQuery = "UPDATE Product SET ProductPhoto = '" + finalFileName + "' WHERE ProductID = " + productID;
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Загружаем сохранённое изображение и обновляем оригинал
                LoadImage(finalFileName);
                CurrentImagePath = finalFileName;
                selectedImagePath = null;

                if (pictureBox1.Image != null)
                {
                    originalImage = new Bitmap(pictureBox1.Image);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Нет прав доступа к папке изображений!\n\n" +
                    "Запустите программу от имени администратора.",
                    "Ошибка доступа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CancelEdit()
        {
            selectedImagePath = null;

            // Восстанавливаем оригинальное изображение если оно было сохранено
            if (originalImage != null)
            {
                // Освобождаем старое изображение перед присвоением нового
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                pictureBox1.Image = new Bitmap(originalImage);
            }
        }

        public void LoadImage(string photoName)
        {
            // Освобождаем старое изображение
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            if (string.IsNullOrEmpty(photoName))
            {
                return;
            }

            string imagesFolder = GetImagesFolderPath();
            string imagePath = Path.Combine(imagesFolder, photoName);

            if (File.Exists(imagePath))
            {
                try
                {
                    // Читаем файл в MemoryStream чтобы избежать блокировки файла
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        pictureBox1.Image = Image.FromStream(ms, true);
                    }
                }
                catch
                {
                    // Если ошибка при загрузке - показываем изображение по умолчанию
                    pictureBox1.Image = Properties.Resources.no_image;
                }
            }
            else
            {
                // Если файл не найден - показываем изображение по умолчанию
                pictureBox1.Image = Properties.Resources.no_image;
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        private void ImageControl_MouseDown(object sender, MouseEventArgs e)
        {

        }
    }
}