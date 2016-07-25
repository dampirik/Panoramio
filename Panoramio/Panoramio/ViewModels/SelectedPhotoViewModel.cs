using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Caliburn.Micro;
using Panoramio.Models;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace Panoramio.ViewModels
{
    public class SelectedPhotoViewModel : BaseViewModel
    {
        public string PhotoUrl { get; set; }

        public string PhotoTitle { get; set; }

        public MapItemModel MapItem { get; set; }

        private WriteableBitmap _photo;
        public WriteableBitmap Photo
        {
            get { return _photo; }
            set { Set(ref _photo, value); }
        }

        private bool _isPhoto;
        public bool IsPhoto
        {
            get { return _isPhoto; }
            set { Set(ref _isPhoto, value); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public SelectedPhotoViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;

            IsPhoto = true;
        }

        protected override void OnDeactivate(bool close)
        {
            Photo = null;

            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= DataTransferManager_DataRequested;

            base.OnDeactivate(close);
        }

        protected override async Task LoadData()
        {
            byte[] buffer;

            IsPhoto = true;
            IsLoading = true;

            try
            {
                buffer = await Server.ConnectHelper.LoadData(PhotoUrl, CancellationToken.None);
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

                    var pixelProvider = await frame.GetPixelDataAsync();

                    var srcPixels = pixelProvider.DetachPixelData();

                    // create an in memory WriteableBitmap of the scaled size
                    var bitmap = new WriteableBitmap((int)frame.PixelWidth, (int)frame.PixelHeight);
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
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Share()
        {
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            
            request.Data.Properties.Title = PhotoTitle;
            request.Data.Properties.Description = PhotoUrl;
            request.Data.Properties.ContentSourceApplicationLink = new Uri(PhotoUrl);
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(Photo.PixelBuffer.AsStream().AsRandomAccessStream()));
        }

        private bool _isSave;

        public async void Save()
        {
            if(_isSave)
                return;

            _isSave = true;

            try
            {
                var picturesLibrary = KnownFolders.PicturesLibrary;
                var savedPicturesFolder = await picturesLibrary.CreateFolderAsync("Saved Pictures", CreationCollisionOption.OpenIfExists);

                var file = await savedPicturesFolder.CreateFileAsync(PhotoTitle + ".jpg", CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream());
                    var pixelStream = Photo.PixelBuffer.AsStream();
                    var pixels = new byte[Photo.PixelBuffer.Length];

                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)Photo.PixelWidth, (uint)Photo.PixelHeight, 96, 96, pixels);

                    await encoder.FlushAsync();
                }
                
                var msg = new MessageDialog("Фотография сохранена.");
                msg.Commands.Add(new UICommand("Продолжить"));
                await msg.ShowAsync();
            }
            catch (Exception)
            {
                var msg = new MessageDialog("Не удалось сохранить фотографию");
                msg.Commands.Add(new UICommand("Продолжить"));
                await msg.ShowAsync();
            }
            
            _isSave = false;
        }
    }
}
