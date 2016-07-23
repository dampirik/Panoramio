﻿using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Caliburn.Micro;

namespace Panoramio.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
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
    }
}