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
namespace PizzaGospel
{
    /// <summary>
    /// Interaction logic for LoginsWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        public static ConsoleWindow Instance;
        public ConsoleWindow()
        {
            InitializeComponent();

            Launcher.Instance.Windows.Add(this);
            Closed += delegate { Launcher.Instance.Windows.Remove(this); };

            Instance = this;
            Height = 660;
        }

        public void ChangeLogText(string NewText) =>
            Logs.Text = NewText;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}