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
namespace OvenRedemption
{
    /// <summary>
    /// Interaction logic for LoginsWindow.xaml
    /// </summary>
    public partial class LoginsWindow : Window
    {
        public LoginsWindow()
        {
            InitializeComponent();
            Height = 530;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Launcher.Instance.Username = UsernameTextbox.Text;
            Launcher.Instance.Password = PasswordTextbox.Text;
            Launcher.Instance.LaunchProcess();
        }
    }
}