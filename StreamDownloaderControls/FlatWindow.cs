using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace StreamDownloaderControls
{
    public class FlatWindow: Window
    {
        private double[] NormalPositon = new double[4];
        protected HwndSource hwndSource;
        protected const int WM_SYSCOMMAND = 0x112;

        /* Import user32.dll. */
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParm, IntPtr lParam);

        static FlatWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatWindow), new FrameworkPropertyMetadata(typeof(FlatWindow)));
        }

        public FlatWindow()
        {
            SourceInitialized += Initialize;
        }

        /// <summary>
        /// Bind events to different WPF components.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Registering methods for the click events.
            ((Button) GetTemplateChild("Minimize")).Click += WindowMinimizeEvent;
            ((Button) GetTemplateChild("Restore")).Click += WindowRestoreEvent;
            ((Button) GetTemplateChild("Close")).Click += WindowCloseEvent;

            ((FrameworkElement) GetTemplateChild("title_bar")).MouseLeftButtonDown += new MouseButtonEventHandler(WindowDragEvent);
            
            Rectangle x = null;
            foreach (string direction in BorderDirectionNames)
            {
                x = ((Rectangle) GetTemplateChild(direction));
                x.MouseEnter += new MouseEventHandler(WindowBorderMouseEnterEvent);
                x.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(WindowBorderMouseLeftDownEvent);
                x.MouseLeave += new MouseEventHandler(WindowBorderMouseLeaveEvent);
            }

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Initializing the hwndSource variable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Initialize(object sender, EventArgs e)
        {
            this.hwndSource = PresentationSource.FromVisual((Visual) sender) as HwndSource;
        }
   
        // Window minimize handler
        protected void WindowMinimizeEvent(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Window restore handler
        protected void WindowRestoreEvent(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                this.NormalPositon[0] = this.Height;
                this.WindowState = WindowState.Maximized;
                this.Height = System.Windows.SystemParameters.WorkArea.Height;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.Height = NormalPositon[0];
            }
        }

        // Window close handler
        protected void WindowCloseEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Mouse enter border event.
        protected void WindowBorderMouseEnterEvent(object sender, MouseEventArgs e)
        {
            DisplayResizeCursor(sender, e);
        }

        // Left mouse button down event.
        protected void WindowBorderMouseLeftDownEvent(object sender, MouseButtonEventArgs e)
        {
            Resize(sender, e);
        }

        // Mouse leave border event.
        protected void WindowBorderMouseLeaveEvent(object sender, MouseEventArgs e)
        {
            ResetCursor(sender, e);
        }

        // Window drag event.
        protected void WindowDragEvent(object sender, MouseEventArgs e)
        {
            DragMove();
        }

        #region Resize

        /// <summary>
        /// Send a resize message to windows.
        /// </summary>
        /// <param name="direction"></param>
        private void ResizeWindow (ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr) direction, IntPtr.Zero);
        }

        /// <summary>
        /// Reset the cursor to the default windows cursor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCursor (object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
                this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Select the correct cursor for the window side.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Resize(object sender, MouseButtonEventArgs e)
        {            
            switch (((sender as Shape).Name))
            {
                case "border_left":
                    this.Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "border_top":
                    this.Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "border_right":
                    this.Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "border_bottom":
                    this.Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "border_top_left":
                    this.Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "border_top_right":
                    this.Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "border_bottom_right":
                    this.Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                case "border_bottom_left":
                    this.Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayResizeCursor(object sender, MouseEventArgs e)
        {
            switch (((sender as Shape).Name))
            {
                case "border_top":
                case "border_bottom":
                    this.Cursor = Cursors.SizeNS;
                    break;
                case "border_left":
                case "border_right":
                    this.Cursor = Cursors.SizeWE;
                    break;
                case "border_top_left":
                case "border_bottom_right":
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case "border_bottom_left":
                case "border_top_right":
                    this.Cursor = Cursors.SizeNESW;
                    break;
                default:
                    break;
            }
        }

        private enum ResizeDirection
        {
            Left = 61441,
            Right = 61442,
            Top = 61443, 
            TopLeft = 61444, 
            TopRight = 61445, 
            Bottom = 61446, 
            BottomLeft = 61447, 
            BottomRight = 61448,
        }

        /// <summary>
        /// Name of all borders (WPF <c>x:Name=""</c>).
        /// </summary>
        private string[] BorderDirectionNames = new string[8]
        {
            "border_left",
            "border_top",
            "border_top_left",
            "border_top_right",
            "border_right",
            "border_bottom",
            "border_bottom_left",
            "border_bottom_right"
        };
        #endregion
    }
}
