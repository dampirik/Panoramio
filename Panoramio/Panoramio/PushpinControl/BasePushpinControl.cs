using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace Panoramio.PushpinControl
{
    public abstract class BasePushpinControl : Control
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location", typeof(Geopoint), typeof(BasePushpinControl),
            new PropertyMetadata(default(Geopoint), LocationPropertyCallback));

        private static void LocationPropertyCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            MapControl.SetLocation(dependencyObject, (Geopoint)dependencyPropertyChangedEventArgs.NewValue);
        }

        public Geopoint Location
        {
            get { return (Geopoint)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DependencyProperty AnchorProperty = DependencyProperty.Register(
            "Anchor", typeof(Point), typeof(BasePushpinControl),
            new PropertyMetadata(default(Point), AnchorPropertyCallback));

        private static void AnchorPropertyCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            MapControl.SetNormalizedAnchorPoint(dependencyObject, (Point)dependencyPropertyChangedEventArgs.NewValue);
        }

        public Point Anchor
        {
            get { return (Point)GetValue(AnchorProperty); }
            set { SetValue(AnchorProperty, value); }
        }
    }
}
