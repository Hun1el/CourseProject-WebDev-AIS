using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WebSiteDev
{
    public static class FolderPermissions
    {
        /// <summary>
        /// Инициализирует папку для изображений в AppData, создаёт её если не существует и выдаёт права
        /// </summary>
        public static void InitializeImagesFolder()
        {
            try
            {
                // Получаем путь к папке ApplicationData пользователя
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string imagesFolder = Path.Combine(appData, "WebShop", "Images");

                System.Diagnostics.Debug.WriteLine($"Инициализирую папку: {imagesFolder}");

                // Создаём папку если её нет
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                    System.Diagnostics.Debug.WriteLine($"Папка создана: {imagesFolder}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Папка уже существует: {imagesFolder}");
                }

                // Проверяем имеет ли текущий пользователь полный доступ
                if (!HasFullAccess(imagesFolder))
                {
                    // Выдаём полный доступ если его нет
                    GiveFullAccessToFolder(imagesFolder);
                    System.Diagnostics.Debug.WriteLine($"Права выданы для: {imagesFolder}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Права уже выданы для: {imagesFolder}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Нет прав администратора для изменения прав доступа
                System.Diagnostics.Debug.WriteLine("Нет прав администратора для выдачи прав доступа!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации папки: {ex.Message}");
            }
        }

        /// <summary>
        /// Создаёт папку по указанному пути и выдаёт полный доступ всем пользователям
        /// </summary>
        /// <param name="folderPath">Путь к папке для создания</param>
        /// <returns>True если папка успешно создана, False при ошибке</returns>
        public static bool CreateFolderWithFullAccess(string folderPath)
        {
            try
            {
                // Создаём папку если её нет
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    System.Diagnostics.Debug.WriteLine($"Папка создана: {folderPath}");
                }

                // Проверяем права доступа
                if (!HasFullAccess(folderPath))
                {
                    // Выдаём полный доступ если его нет
                    GiveFullAccessToFolder(folderPath);
                    System.Diagnostics.Debug.WriteLine($"Права выданы для: {folderPath}");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания папки: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Проверяет имеет ли текущий пользователь полный доступ к папке
        /// </summary>
        /// <param name="folderPath">Путь к папке для проверки</param>
        /// <returns>True если есть полный доступ, False если нет</returns>
        private static bool HasFullAccess(string folderPath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                // Получаем объект безопасности папки
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

                // Проходим по всем правилам доступа
                foreach (FileSystemAccessRule rule in directorySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                {
                    // Ищем правило для всех пользователей (S-1-1-0 = Everyone)
                    if (rule.IdentityReference.Value == "S-1-1-0" &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.FullControl) &&
                        rule.AccessControlType == AccessControlType.Allow)
                    {
                        System.Diagnostics.Debug.WriteLine($"Найдены полные права для: {folderPath}");
                        return true;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Полные права не найдены для: {folderPath}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки прав: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Выдаёт полный доступ всем пользователям к указанной папке
        /// </summary>
        /// <param name="folderPath">Путь к папке для выдачи прав</param>
        private static void GiveFullAccessToFolder(string folderPath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                // Получаем объект безопасности папки
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

                // Создаём правило доступа для всех пользователей
                FileSystemAccessRule accessRule = new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),  // Everyone (все пользователи)
                    FileSystemRights.FullControl,                              // Полный контроль
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,  // Наследуется для папок и файлов
                    PropagationFlags.None,
                    AccessControlType.Allow
                );

                // Добавляем правило
                directorySecurity.AddAccessRule(accessRule);

                // Применяем правило к папке
                directoryInfo.SetAccessControl(directorySecurity);

                System.Diagnostics.Debug.WriteLine($"Права успешно выданы: {folderPath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                // Недостаточно прав для изменения доступа
                System.Diagnostics.Debug.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при выдаче прав: {ex.Message}");
                throw;
            }
        }
    }
}
