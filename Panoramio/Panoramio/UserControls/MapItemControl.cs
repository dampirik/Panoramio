using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace Panoramio.UserControls
{
    public class MapItemControl : Control
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location", typeof(Geopoint), typeof(MapItemControl),
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
            "Anchor", typeof(Point), typeof(MapItemControl),
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
        
        public IMapItem ParentModel { get; private set; }

        public Point ScreenLocation { get; private set; }

        public static readonly DependencyProperty PhotoProperty = DependencyProperty.Register(
    "Photo", typeof(WriteableBitmap), typeof(MapItemControl), new PropertyMetadata(null));

        public WriteableBitmap Photo
        {
            get { return (WriteableBitmap)GetValue(PhotoProperty); }
            set { SetValue(PhotoProperty, value); }
        }

        public static readonly DependencyProperty IsPhotoProperty = DependencyProperty.Register(
"IsPhoto", typeof(bool), typeof(MapItemControl), new PropertyMetadata(false));

        public bool IsPhoto
        {
            get { return (bool) GetValue(IsPhotoProperty); }
            set { SetValue(IsPhotoProperty, value); }
        }

        public MapItemControl(IMapItem parent, Point location)
        {
            DefaultStyleKey = typeof(MapItemControl);
            Anchor = new Point(0, 1);
            DataContext = this;

            Location = parent.Location;
            ScreenLocation = location;
            ParentModel = parent;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LoadingPhoto();
        }

        private async void LoadingPhoto()
        {
            IsPhoto = true;

            byte[] buffer;

            try
            {
                buffer = await Server.ConnectHelper.LoadData(ParentModel.PhotoUrl, CancellationToken.None);
            }
            catch (Exception)
            {
                IsPhoto = false;
                return;
            }

            if (buffer == null)
            {
                IsPhoto = false;
                return;
            }

            try
            {
                using (var stream = new MemoryStream(buffer).AsRandomAccessStream())
                {
                    stream.Seek(0);

                    // decode a frame (as you do now)
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var frame = await decoder.GetFrameAsync(0);

                    // calculate required scaled size
                    uint newWidth = 38;
                    uint newHeight = 38;

                    // convert (and resize) the frame into pixels
                    var pixelProvider =
                        await frame.GetPixelDataAsync(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Ignore,
                            new BitmapTransform { ScaledWidth = newWidth, ScaledHeight = newHeight },
                            ExifOrientationMode.RespectExifOrientation,
                            ColorManagementMode.DoNotColorManage);

                    var srcPixels = pixelProvider.DetachPixelData();

                    // create an in memory WriteableBitmap of the scaled size
                    var bitmap = new WriteableBitmap((int)newWidth, (int)newHeight);
                    using (var pixelStream = bitmap.PixelBuffer.AsStream())
                    {
                        pixelStream.Seek(0, SeekOrigin.Begin);

                        // push the pixels from the original file into the in-memory bitmap
                        pixelStream.Write(srcPixels, 0, srcPixels.Length);
                    }

                    bitmap.Invalidate();

                    Photo = bitmap;
                }
            }
            catch (Exception)
            {
                IsPhoto = false;
                throw;
            }
        }
    }
}
