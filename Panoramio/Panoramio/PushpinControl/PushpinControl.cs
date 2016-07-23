using Windows.Foundation;
using Windows.UI.Xaml;

namespace Panoramio.PushpinControl
{
    public class PushpinControl : BasePushpinControl
    {
        public IPushpinModel ParentModel { get; private set; }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(int), typeof(PushpinControl), new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public PushpinControl(IPushpinModel parent)
        {
            DefaultStyleKey = typeof(PushpinControl);
            Anchor = new Point(0.5, 1);
            DataContext = this;

            ParentModel = parent;
        }
    }
}
