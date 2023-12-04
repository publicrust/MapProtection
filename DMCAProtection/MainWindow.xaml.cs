using Ookii.Dialogs.Wpf;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DMCAProtection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var path = GetPathOpenFileDialog();

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Path null");
                return;
            }

            if (string.IsNullOrEmpty(TextBox.Text))
            {
                MessageBox.Show("DMCA Protection has null");
                return;
            }

            var worldSerialization = new WorldSerialization();

            worldSerialization.Load(path);

            worldSerialization.AddMap("DMCA", Encoding.UTF8.GetBytes(TextBox.Text));
            worldSerialization.Save(path + ".DMCA.map");
        }

        private string GetPathOpenFileDialog()
        {
            VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
            openFileDialog.Filter = "Карты раст (*.map)|*.map";

            if (openFileDialog.ShowDialog() == false)
            {
                return "";
            }

            return openFileDialog.FileName;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var path = GetPathOpenFileDialog();

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Path null");
                return;
            }

            var worldSerialization = new WorldSerialization();

            worldSerialization.Load(path);

            TextBlock.Text = Encoding.UTF8.GetString(worldSerialization.GetMap("DMCA").data);
        }
    }
}