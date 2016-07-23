using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Panoramio.PushpinControl
{
    public class ClusterPushpinControl : BasePushpinControl
    {
        public List<IPushpinModel> Parents { get; private set; }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", typeof(int), typeof(ClusterPushpinControl), new PropertyMetadata(0));

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public ClusterPushpinControl(List<IPushpinModel> parents)
        {
            DefaultStyleKey = typeof(ClusterPushpinControl);
            Anchor = new Point(0.5, 0.5);
            DataContext = this;
            Parents = parents;
        }
    }
}
