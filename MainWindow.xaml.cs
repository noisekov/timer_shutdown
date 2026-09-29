using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShutdownTimer
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer countdownTimer;
        private DateTime shutdownTime;

        public MainWindow()
        {
            InitializeComponent();

            InitializeTimeSelectors();

            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            countdownTimer.Tick += CountdownTimer_Tick;

            CountdownText.Text = "00:00:00";
        }

        private void InitializeTimeSelectors()
        {
            for (int i = 0; i <= 23; i++)
            {
                HoursBox.Items.Add(i.ToString("00"));
            }

            for (int i = 0; i <= 59; i++)
            {
                MinutesBox.Items.Add(i.ToString("00"));
                SecondsBox.Items.Add(i.ToString("00"));
            }

            HoursBox.SelectedIndex = 0;
            MinutesBox.SelectedIndex = 0;
            SecondsBox.SelectedIndex = 0;
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            int hours = HoursBox.SelectedIndex;
            int minutes = MinutesBox.SelectedIndex;
            int seconds = SecondsBox.SelectedIndex;

            int totalSeconds =
                (hours * 3600) +
                (minutes * 60) +
                seconds;

            if (totalSeconds <= 0)
            {
                StatusText.Foreground = Brushes.OrangeRed;
                StatusText.Text = "Ошибка: время должно быть больше 0!";
                CountdownText.Text = "00:00:00";

                return;
            }

            string command = $"/c shutdown -s -t {totalSeconds}";

            if (!ExecuteCommand(command))
            {
                return;
            }

            shutdownTime = DateTime.Now.AddSeconds(totalSeconds);

            countdownTimer.Start();

            ApplyBtn.IsEnabled = false;

            StatusText.Foreground = Brushes.LimeGreen;
            StatusText.Text = "До выключения осталось:";

            UpdateCountdown();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            string command = "/c shutdown /a";

            ExecuteCommand(command, false);

            countdownTimer.Stop();

            ApplyBtn.IsEnabled = true;

            StatusText.Foreground = Brushes.LightSkyBlue;
            StatusText.Text = "Таймер выключения отменён";

            CountdownText.Text = "00:00:00";
        }

        private bool ExecuteCommand(string command, bool showError = true)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = command,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process? process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        if (showError)
                        {
                            ShowError("Не удалось запустить команду.");
                        }

                        return false;
                    }

                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        if (showError)
                        {
                            ShowError($"Ошибка CMD: {error.Trim()}");
                        }

                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (showError)
                {
                    ShowError($"Критическая ошибка: {ex.Message}");
                }

                return false;
            }
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan remaining = shutdownTime - DateTime.Now;

            if (remaining <= TimeSpan.Zero)
            {
                countdownTimer.Stop();

                CountdownText.Text = "00:00:00";
                StatusText.Text = "Компьютер выключается...";
                StatusText.Foreground = Brushes.Orange;

                return;
            }

            int hours = (int)remaining.TotalHours;

            CountdownText.Text =
                $"{hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
        }

        private void ShowError(string message)
        {
            countdownTimer.Stop();

            ApplyBtn.IsEnabled = true;

            StatusText.Foreground = Brushes.OrangeRed;
            StatusText.Text = message;

            CountdownText.Text = "00:00:00";
        }
    }
}