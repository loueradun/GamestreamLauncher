using System;
using System.ComponentModel;
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

namespace GamestreamLauncher.UserControls
{
    /// <summary>
    /// Interaction logic for SlideControl.xaml
    /// </summary>
    public partial class SlideControl : UserControl, INotifyPropertyChanged
    {
        Point brushEndpoint = new Point(0,0);

        public event PropertyChangedEventHandler PropertyChanged;

        public SlideControl()
        {
            InitializeComponent();
        }

        public Point BrushEndpoint {
            get { return brushEndpoint; }
            set {
                brushEndpoint = value;
                OnPropertyChanged("BrushEndpoint");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            BrushEndpoint = new Point(sizeInfo.NewSize.Width, 0);
        }
    }
}
