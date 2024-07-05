using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace networkExtension
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private DateTime _connectionStartTime;

        public MainWindow()
        {
            InitializeComponent();
            UpdateNetworkStatus();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (sender, args) => UpdateNetworkStatus();
            _timer.Start();
        }

        private void UpdateNetworkStatus()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && ni.OperationalStatus == OperationalStatus.Up)
                {
                    if (_connectionStartTime == DateTime.MinValue)
                    {
                        _connectionStartTime = DateTime.Now; // Встановити час початку з'єднання
                    }

                    var ipv4 = ni.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    var speed = ni.Speed / 1000000; // Мбіт/с
                    var duration = DateTime.Now - _connectionStartTime;

                    statusTextBlock.Text = "Статус носія: Увімкнуто";
                    speedTextBlock.Text = $"Швидкість: {speed} Мбіт/с";
                    durationTextBlock.Text = $"Тривалість: {duration:hh\\:mm\\:ss}";
                    return;
                }
            }

            _connectionStartTime = DateTime.MinValue; // Скидання часу початку з'єднання, якщо немає активного з'єднання
            statusTextBlock.Text = "Статус носія: Вимкнуто";
            speedTextBlock.Text = "Швидкість: Н/Д";
            durationTextBlock.Text = "Тривалість: Н/Д";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Прикріпити вікно до робочого столу
            AttachToDesktop();
        }

        private void AttachToDesktop()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr desktop = GetDesktopWindow();
            SetParent(hwnd, desktop);
            SetWindowPos(hwnd, new IntPtr(1), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
    }
}
