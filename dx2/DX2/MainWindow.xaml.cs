using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;




namespace DX2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        void NewPoly(object sender, RoutedEventArgs e)
        {
            NativeWindow.self.NewPoly();
        }
        void SelectPoly(object sender, RoutedEventArgs e)
        {
            NativeWindow.self.SelectPoly();
            //System.Windows.MessageBox.Show("туцзщдн");
        }
        void SelectPoint(object sender, RoutedEventArgs e)
        {
            NativeWindow.self.SelectPoint();
         }
        void ColorPoly(object sender, RoutedEventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color c = cd.Color;
                long x = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
                NativeWindow.self.SetColor((uint)x);
            }
        }
        void FillPoly(object sender, RoutedEventArgs e)
        {
            BrushDialog bd = new BrushDialog();
            bd.ShowDialog();
            //if (bd.ShowDialog()==true)
            {
                System.Drawing.Color a = bd.Color1, b = bd.Color2;

                long x = (a.A << 24) | (a.R << 16) | (a.G << 8) | a.B,
                    y = (b.A << 24) | (b.R << 16) | (b.G << 8) | b.B,
                    t = (uint)bd.lg;
                NativeWindow.self.SetGradient((uint)x, (uint)y, (uint)t);
            }
        }
        void DeletePoly(object sender, RoutedEventArgs e)
        {
            NativeWindow.self.DeletePoly();
        }
        void DeleteLine(object sender, RoutedEventArgs e)
        {
            NativeWindow.self.DeletePoint();
        }
        void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:NewPoly(sender, e);
                    break;
                case Key.P:SelectPoint(sender, e);
                    break;
                case Key.L:
                    SelectPoly(sender, e);
                    break;
                case Key.C:
                    ColorPoly(sender, e);
                    break;
                case Key.X:
                    DeletePoly(sender, e);
                    break;
                case Key.Z:
                    DeleteLine(sender, e);
                    break;
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();
            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.grid1.Children.Add(host);
        }
    }
}
