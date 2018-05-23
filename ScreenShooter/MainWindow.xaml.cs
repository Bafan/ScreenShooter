using FMUtils.KeyboardHook;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShooter
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private Members
        private Visibility _visibilityOfSplash;
        private System.Windows.Point startPoint;
        private System.Windows.Shapes.Rectangle rect;
        private Hook KeyboardHook = new Hook("Global Action Hook");
        #endregion

        #region Public Property
        public Visibility VisibilityOfSplash
        {
            get
            {
                return _visibilityOfSplash;
            }
            set
            {
                _visibilityOfSplash = value;
                OnPropertyChanged("VisibilityOfSplash");
            }
        }
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Top = 0;
            Left = 0;
            Height = System.Windows.SystemParameters.FullPrimaryScreenHeight;
            Width = System.Windows.SystemParameters.FullPrimaryScreenWidth;
            screenRect.Rect = new Rect(0, 0, Width, Height);
            selectRect.Rect = new Rect(5, 5, Width - 10, Height - 10);
            App.icon.Icon = new System.Drawing.Icon("icon.ico");
            App.icon.Visible = true;
            App.icon.MouseClick += Icon_MouseClick;
            App.icon.Click += Icon_Click;
            KeyboardHook.KeyDownEvent += KeyDown;

        } 
        #endregion

        #region Private Methods
        private BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
        #endregion

        #region Event Handlers
        private void Icon_Click(object sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
            {
                //this.WindowState = WindowState.Normal;
                this.Visibility = Visibility.Visible;
                ShowActivated = true;
                Activate();
                screenRect.Rect = new Rect(0, 0, Width, Height);
                selectRect.Rect = new Rect(5, 5, Width - 10, Height - 10);
                UpdateLayout();
            }
            else
            {
                //this.WindowState = WindowState.Minimized;
                this.Hide();
            }
        }

        private new void KeyDown(KeyboardHookEventArgs e)
        {
            if (e.Key == Keys.F12 && e.isCtrlPressed)
            {
                this.Visibility = Visibility.Visible;
                ShowActivated = true;
                Activate();
                screenRect.Rect = new Rect(0, 0, Width, Height);
                selectRect.Rect = new Rect(5, 5, Width - 10, Height - 10);
                UpdateLayout();
            }
        }

        private void Icon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.Close();
                Environment.Exit(0);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VisibilityOfSplash = Visibility.Hidden;
            startPoint = e.GetPosition(canvas);
            rect = new System.Windows.Shapes.Rectangle
            {
                Stroke = System.Windows.Media.Brushes.Blue,
                StrokeThickness = 2,
            };

            Canvas.SetLeft(rect, startPoint.X);
            Canvas.SetTop(rect, startPoint.X);
            canvas.Children.Add(rect);
            txtMouseLocation.Visibility = Visibility.Visible;
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rect == null)
                return;
            var pos = e.GetPosition(canvas);
            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);
            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;
            txtMouseLocation.Text = $"{w},{h}";
            double mouseLocationX = x + w;
            double mouseLocationY = y + h;
            if (w > 0 && h > 0)
                mouseLocationY -= 16;
            Canvas.SetLeft(txtMouseLocation, mouseLocationX);
            Canvas.SetTop(txtMouseLocation, mouseLocationY);
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rect.Width.ToString() == "NaN" || rect.Height.ToString() == "NaN" || rect.Width == 0 || rect.Height == 0)
                return;
            e.Handled = true;
            ConfirmDialog congfirm = new ConfirmDialog();
            var position = e.GetPosition(this);
            congfirm.Top = position.Y;
            congfirm.Left = position.X;
            // if ((bool)congfirm.ShowDialog())
            {
                Bitmap image = new Bitmap((int)rect.Width, (int)rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Graphics g = Graphics.FromImage(image);
                g.CopyFromScreen((int)startPoint.X, (int)startPoint.Y, 0, 0, image.Size);
                //image.Save(@"c:\image.bmp");
                System.Windows.Clipboard.SetImage(Convert(image));
                //WindowState = WindowState.Minimized;
                VisibilityOfSplash = Visibility.Hidden;
                //Canvas.SetTop(splashRect, startPoint.X);
                //Canvas.SetLeft(splashRect, startPoint.Y);
                //Canvas.SetRight(splashRect, position.X);
                //Canvas.SetBottom(splashRect, position.Y);


                this.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    txtMouseLocation.Visibility = Visibility.Hidden;
                    canvas.Children.Remove(rect);
                    canvas.UpdateLayout();
                    startPoint.X = 0;
                    startPoint.Y = 0;
                    Canvas.SetBottom(rect, 0);
                    Canvas.SetRight(rect, 0);
                    Canvas.SetLeft(rect, 0);
                    Canvas.SetTop(rect, 0);
                    rect.UpdateLayout();
                    canvas.UpdateLayout();
                    path.UpdateLayout();
                    UpdateLayout();

                }, System.Windows.Threading.DispatcherPriority.Send, null);
                //rect = null;
                this.Hide();
            }
            //canvas.Children.OfType<System.Windows.Shapes.Rectangle>().ToList().ForEach(x=>canvas.Children.Remove(x));
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //this.WindowState = WindowState.Minimized;
                this.Hide();
            }
        } 
        #endregion

        #region IPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        } 
        #endregion
    }
}
