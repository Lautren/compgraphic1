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
using System.Windows.Shapes;
using System.Windows.Forms;


namespace DX2
{
    public enum GType {__Linear=1,__Radial };
    /// <summary>
    /// Логика взаимодействия для BrushDialog.xaml
    /// </summary>
    public partial class BrushDialog : Window
    {
        public System.Drawing.Color Color1= System.Drawing.Color.Red, 
            Color2 = System.Drawing.Color.Blue;
        public GType lg=GType.__Linear;
        Brush b1, b2;
        
        public BrushDialog()
        {
            InitializeComponent();
          
        }
        void ColorPoly1(object sender, RoutedEventArgs e)
        {
            Color1 = SelectColor();
            G1.Fill = b1 = new SolidColorBrush(Color.FromArgb(Color1.A,Color1.R,Color1.G,Color1.B));
            fillGradient();
        }
        void ColorPoly2(object sender, RoutedEventArgs e)
        {
            Color2 = SelectColor();
            G2.Fill = b2 = new SolidColorBrush(Color.FromArgb(Color2.A, Color2.R, Color2.G, Color2.B));
            fillGradient();
        }

        void fillGradient()
        {
            switch (lg)
            {
                case GType.__Linear:
                    LinearGradientBrush fill = new LinearGradientBrush();
                    fill.GradientStops.Add(new GradientStop(Color.FromArgb(Color1.A, Color1.R, Color1.G, Color1.B), 0.0));
                    fill.GradientStops.Add(new GradientStop(Color.FromArgb(Color2.A, Color2.R, Color2.G, Color2.B), 1.0));
                    grad.Fill = fill;
                    break;
                case GType.__Radial:
                    RadialGradientBrush fil = new RadialGradientBrush();
                    fil.GradientStops.Add(new GradientStop(Color.FromArgb(Color1.A, Color1.R, Color1.G, Color1.B), 0.0));
                    fil.GradientStops.Add(new GradientStop(Color.FromArgb(Color2.A, Color2.R, Color2.G, Color2.B), 1.0));
                    grad.Fill = fil;
                    break;
            }            
        }
        System.Drawing.Color SelectColor()
        {
            ColorDialog cd = new ColorDialog();
            System.Drawing.Color c = System.Drawing.Color.Black;
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                c = cd.Color;
            }
            return c;
        }
        void SelectStyle(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem mi = sender as System.Windows.Controls.MenuItem;
            if (mi.Name == "L")
                lg = GType.__Linear;
            else
                lg = GType.__Radial;
        }

    }
}
