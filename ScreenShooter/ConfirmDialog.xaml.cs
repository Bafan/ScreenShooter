using System.Windows;
using System.Windows.Input;

namespace ScreenShooter
{
    public partial class ConfirmDialog : Window
    {        
        public ConfirmDialog()
        {
            InitializeComponent();           
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
