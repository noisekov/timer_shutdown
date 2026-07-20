using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ShutdownTimer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            int hours = int.TryParse(HoursBox.Text, out hours) ? hours : 0;
            int minutes = int.TryParse(MinutesBox.Text, out minutes) ? minutes : 0;
            int seconds = int.TryParse(SecondsBox.Text, out seconds) ? seconds : 0;

            int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;

            if (totalSeconds <= 0)
            {
                StatusText.Foreground = Brushes.Red;
                StatusText.Text = "Ошибка: Время должно быть больше 0!";
                return;
            }

            string command = $"/c shutdown -s -t {totalSeconds}";

            ExecuteCommand(command);

            StatusText.Foreground = Brushes.Green;
            StatusText.Text = $"Выключение через {hours}ч. {minutes}мин. {seconds}сек.";
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            string command = "/c shutdown /a";
            ExecuteCommand(command);

            StatusText.Foreground = Brushes.Blue;
            StatusText.Text = "Таймер выключения отменен!";
        }

        private void ExecuteCommand(string command)
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

                using (Process process = Process.Start(psi))
                {
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        StatusText.Foreground = Brushes.OrangeRed;
                        StatusText.Text = $"Ошибка CMD: {error.Trim()}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Foreground = Brushes.Red;
                StatusText.Text = $"Критическая ошибка: {ex.Message}";
            }
        }
    }
}