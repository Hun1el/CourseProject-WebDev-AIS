using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WebSiteDev
{
    class BlockForms
    {
        private Timer inactivityTimer;
        private int inactivityTimeoutSeconds;
        private DateTime lastActivityTime;
        private List<Form> monitoredForms;
        private Form loginForm;
        private bool isRunning;

        public event EventHandler OnInactivityDetected;

        public BlockForms(Form login)
        {
            loginForm = login;
            monitoredForms = new List<Form>();
            inactivityTimeoutSeconds = Properties.Settings.Default.InactivityTime;
            lastActivityTime = DateTime.Now;
            isRunning = false;

            inactivityTimer = new Timer();
            inactivityTimer.Interval = 1000;
            inactivityTimer.Tick += InactivityTimer_Tick;
        }

        public void RegisterForm(Form form)
        {
            monitoredForms.Add(form);
            SubscribeToActivityEvents(form);
        }

        public void UnregisterForm(Form form)
        {
            monitoredForms.Remove(form);
        }

        private void SubscribeToActivityEvents(Control parent)
        {
            parent.MouseMove += Activity_Detected;
            parent.MouseClick += Activity_Detected;
            parent.KeyDown += Activity_Detected;

            foreach (Control child in parent.Controls)
            {
                SubscribeToActivityEvents(child);
            }
        }

        private void Activity_Detected(object sender, EventArgs e)
        {
            lastActivityTime = DateTime.Now;
        }

        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan inactivityDuration = DateTime.Now - lastActivityTime;

            if (inactivityDuration.TotalSeconds >= inactivityTimeoutSeconds)
            {
                OnInactivityDetected?.Invoke(this, EventArgs.Empty);
                Stop();
            }
        }

        public void Start()
        {
            lastActivityTime = DateTime.Now;
            isRunning = true;
            inactivityTimer.Start();
        }

        public void Stop()
        {
            isRunning = false;
            inactivityTimer.Stop();
        }

        public void Restart()
        {
            Stop();
            lastActivityTime = DateTime.Now;
            Start();
        }

        public void LockAllForms()
        {
            foreach (Form form in monitoredForms.ToList())
            {
                if (form != null && !form.IsDisposed)
                {
                    if (form.GetType().Name != "AuthForm")
                    {
                        form.Close();
                    }
                }
            }

            if (loginForm != null && !loginForm.IsDisposed)
            {
                loginForm.Show();
                loginForm.BringToFront();
            }
        }

        public void UpdateTimeout(int newTimeoutSeconds)
        {
            inactivityTimeoutSeconds = newTimeoutSeconds;
            Properties.Settings.Default.InactivityTime = newTimeoutSeconds;
            Properties.Settings.Default.Save();
        }

        public int GetTimeout()
        {
            return inactivityTimeoutSeconds;
        }

        public bool IsRunning()
        {
            return isRunning;
        }
    }
}
