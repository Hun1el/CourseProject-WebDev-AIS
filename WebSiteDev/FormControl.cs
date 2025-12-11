using System;
using System.Windows.Forms;

namespace WebSiteDev
{
    /// <summary>
    /// Статический класс для управления формами - изменение размеров, очистка контролов
    /// </summary>
    public static class FormControl
    {
        private static Form lastForm = null;
        private static int lastWidth = 0;

        /// <summary>
        /// Изменяет ширину формы и расширяет правую панель на соответствующую величину
        /// </summary>
        /// <param name="parentForm">Форма для изменения размера</param>
        /// <param name="newWidth">Новая ширина формы в пикселях</param>
        /// <param name="panelRightName">Имя правой панели для расширения (по умолчанию "panel2")</param>
        public static void Resize(Form parentForm, int newWidth, string panelRightName = "panel2")
        {
            if (parentForm == null)
            {
                return;
            }

            // Сохраняем оригинальный размер формы при первом вызове для конкретной формы
            if (lastForm != parentForm)
            {
                lastForm = parentForm;
                lastWidth = parentForm.Width;
            }

            // Заморозим обновление интерфейса для плавного изменения размера
            parentForm.SuspendLayout();

            // Рассчитываем разницу между новой и текущей шириной
            int delta = newWidth - parentForm.Width;
            parentForm.Width = newWidth;

            // Получаем правую панель и расширяем её
            var panelRight = parentForm.Controls[panelRightName];

            if (panelRight != null)
            {
                panelRight.Width += delta;
            }

            // Восстанавливаем обновление интерфейса
            parentForm.ResumeLayout();
            parentForm.Invalidate();
        }

        /// <summary>
        /// Возвращает форму к оригинальному размеру (при первом расширении)
        /// </summary>
        public static void ResetFormSize(Form parentForm, string panelRightName = "panel2")
        {
            // Если это та же форма и мы сохранили оригинальный размер - восстанавливаем его
            if (parentForm != null && lastForm == parentForm && lastWidth > 0)
            {
                Resize(parentForm, lastWidth, panelRightName);
                lastForm = null;
                lastWidth = 0;
            }
        }

        /// <summary>
        /// Рекурсивно удаляет контрол и все его дочерние контролы, освобождая ресурсы
        /// </summary>
        private static void DispControl(Control ctrl)
        {
            // Рекурсивно удаляем все дочерние контролы
            foreach (Control c in ctrl.Controls)
            {
                DispControl(c);
            }

            // Если это PictureBox - удаляем изображение
            if (ctrl is PictureBox pb && pb.Image != null)
            {
                pb.Image.Dispose();
                pb.Image = null;
            }

            // Удаляем сам контрол
            ctrl.Dispose();
        }

        /// <summary>
        /// Удаляет все контролы из панели и освобождает память
        /// </summary>
        /// <param name="panel">Панель для очистки</param>
        public static void ClearPanelControls(Panel panel)
        {
            if (panel == null)
            {
                return;
            }

            // Удаляем все контролы из панели
            foreach (Control c in panel.Controls)
            {
                DispControl(c);
            }

            // Очищаем коллекцию контролов
            panel.Controls.Clear();

            // Принудительно вызываем сборщик мусора для освобождения памяти
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}