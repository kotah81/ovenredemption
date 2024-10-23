using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using static OvenRedemption.Launcher;
#pragma warning disable CS8622
namespace OvenRedemption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string LogsSTR = "Welcome!";
        public static MainWindow? Instance;
        public bool Dropped = false;
        public Launcher Launcher => Launcher.Instance;
        public MainWindow()
        {
            new Launcher();

            InitializeComponent();

            Instance = this;
            Width = 1032;
            Height = 526;
            Logs.Text = LogsSTR;
            ClearButton.IsEnabled = false;
            ClearButton.Visibility = Visibility.Hidden;

            OnDropSuceeded(false);
            CompositionTarget.Rendering += OnUpdate;
            SetupClickButton.AllowDrop = true;
            if (Launcher.PreviousInstall)
                OnPreviousInstallExists();
            else Tools.ClearFolder(GamePath);
        }


        public void OnPreviousInstallExists()
        {
            ClearButton.IsEnabled = true;
            ClearButton.Visibility = Visibility.Visible;
            OnDropSuceeded(true);
            DropDescription.Text = "got prev. install!";
            Launcher.ChangeState(States.Patching);
        }

        public void OnUpdate(object sender, EventArgs e)
        {
            ProgressBar.Value = Tools.Approach((float)ProgressBar.Value, (float)Launcher.State, 1);
        }

        public async void TriggerClose()
        {
            await Task.Delay(1000);
            Log("OR will close in 5s");
            for (int i = 5; i > 0; i--)
            {
                await Task.Delay(1000);
                LogsSTR = LogsSTR.Replace($"OR will close in {i}s", $"OR will close in {i - 1}s");
                Logs.Text = LogsSTR;
            }
            Application.Current.Shutdown();
        }

        public void OnDropSuceeded(bool Suceeded)
        {
            var MainColor = Tools.HexBrush(Suceeded ? "#FF03B152" : "#FFEB4635");
            var DarkerColor = Tools.HexBrush(Suceeded ? "#FF02652F" : "#9e2f23");
            var ProgramColor = Tools.HexBrush("#FFFBBC1C");

            if (Launcher.PreviousInstall)
            {
                MainColor = Tools.HexBrush("#2773e7");
                DarkerColor = Tools.HexBrush("#1f4ea5");
                ProgramColor = MainColor;
            }

            ProgramTitle.Foreground = ProgramColor;
            ProgressBar.Foreground = ProgramColor;

            SetupBorder.BorderBrush = MainColor;
            DropText.Foreground = MainColor;
            DropText.Text = Suceeded ? "done" : "drop";
            SetupTitle.Foreground = MainColor;

            DropDescription.Text = Suceeded ? "got modpack!" : "drop a redemption zip\r\n<- here";
            DropDescription.Margin = new(122, Suceeded ? 85 : 75, 0, 0);

            LaunchButton.Foreground = DarkerColor;
            LaunchButton.Background = MainColor;
            LaunchButton.BorderBrush = DarkerColor;

            Dropped = Suceeded;
        }

        public void Log(string L)
        {
            if (LogsSTR.Split('\n').Length >= 9)
                LogsSTR = L;
            else LogsSTR += "\n" + L;
            Logs.Text = LogsSTR;
        }
        public void TriggerClearEverything()
        {
            Launcher.ChangeState(States.Idle);
            ClearButton.IsEnabled = false;
            ClearButton.Visibility = Visibility.Hidden;
            Launcher.PreviousInstall = false;
            OnDropSuceeded(false);
            Launcher.ClearEverything();
        }


        #region Object Events
        private void SetupBorder_Click(object sender, RoutedEventArgs e)
        {
            if ((float)Launcher.State > 50) return;
            OpenFileDialog OFD = new();
            OFD.Filter = "Redemption ZIP (*.zip)|*.zip";

            bool? Response = OFD.ShowDialog();
            TriggerClearEverything(); // whatever the fuck happens

            if (Response.HasValue && Response.Value)
                Launcher.CheckDroppedFile(OFD.FileName, OnDropSuceeded);
            else OnDropSuceeded(false);
        }

        private void OnZIPDropped(object sender, DragEventArgs e)
        {
            if ((float)Launcher.State > 50) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] Files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Launcher.CheckDroppedFile(Files[0], OnDropSuceeded);
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Launcher.CanLaunch) return;
            if ((float)Launcher.State > 50) return; 

            if (Launcher.PreviousInstall)
            {
                ClearButton.IsEnabled = false;
                ClearButton.Visibility = Visibility.Hidden;
                Launcher.LaunchGame();
                return;
            }

            var LoginWindow = new LoginsWindow();
            LoginWindow.Show();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => TriggerClearEverything();
        #endregion
    }
}