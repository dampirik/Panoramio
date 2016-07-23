using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Caliburn.Micro;

namespace Panoramio.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private bool _serverIsUnavailable;
        public bool ServerIsUnavailable
        {
            get { return _serverIsUnavailable; }
            set
            {
                Set(ref _serverIsUnavailable, value);
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Set(ref _selectedItem, value);
            }
        }

        private Geopoint _geoCenter;
        public Geopoint GeoCenter
        {
            get { return _geoCenter; }
            set
            {
                Set(ref _geoCenter, value);
            }
        }

        public MainViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }
        protected override void OnInitialize()
        {
            base.OnInitialize();

            GeoCenter = new Geopoint(new BasicGeoposition
            {
                //0 км Москва
                Latitude = 55.755831,
                Longitude = 37.617673
            });
        }

        protected override async Task LoadData()
        {
            var result = await Server.ServerFacade.GetPhotos(CancellationToken.None);
        }

        public void OnMapTapped(BasicGeoposition tappedGeoPosition)
        {
            string status = "MapTapped at \nLatitude:" + tappedGeoPosition.Latitude + "\nLongitude: " + tappedGeoPosition.Longitude;
            //rootPage.NotifyUser(status, NotifyType.StatusMessage);

        }

        public void Share()
        {

        }

        public void Save()
        {

        }
    }
}
