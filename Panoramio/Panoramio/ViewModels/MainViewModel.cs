using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Caliburn.Micro;
using Panoramio.Models;
using Panoramio.UserControls;
using System.Collections.ObjectModel;
using Panoramio.Common;
using Panoramio.Server.Data;

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

        private double _mapZoom;
        public double MapZoom
        {
            get { return _mapZoom; }
            set
            {
                Set(ref _mapZoom, value);
            }
        }

        private Geopoint _mapCenter;
        public Geopoint MapCenter
        {
            get { return _mapCenter; }
            set
            {
                Set(ref _mapCenter, value);
            }
        }

        private ObservableRangeCollection<IMapItem> _mapItems;
        public ObservableRangeCollection<IMapItem> MapItems
        {
            get { return _mapItems; }
            set { Set(ref _mapItems, value); }
        }

        public MainViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }
        protected override void OnInitialize()
        {
            base.OnInitialize();

            //MapCenter = new Geopoint(new BasicGeoposition
            //{
            //    майями 
            //    Latitude = 25.766906,
            //    Longitude = -80.196225
            //});

            MapCenter = new Geopoint(new BasicGeoposition
                                     {
                                         //0 км Москва
                                         Latitude = 55.755831,
                                         Longitude = 37.617673
                                     });
            MapZoom = 10;
        }

        protected override async Task LoadData()
        {
            //var result = await Server.ServerFacade.GetPhotos(CancellationToken.None);

            //var items = result.PhotoItems.Select(photoItem => new MapItemModel
            //                                                  {
            //                                                      Location = new Geopoint(new BasicGeoposition
            //                                                                              {
            //                                                                                  Latitude =
            //                                                                                      photoItem.Latitude,
            //                                                                                  Longitude =
            //                                                                                      photoItem.Longitude,
            //                                                                              })
            //                                                  });

            //MapItems = new BindableCollection<IMapItem>(items);
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

        public async void OnGeoBoundsChanged(GeoBounds geoBounds)
        {
            var result =
                await Server.ServerFacade.GetPhotos(0, 20, geoBounds.MinX, geoBounds.MinY, geoBounds.MaxX, geoBounds.MaxY,
                        CancellationToken.None);

            var data = MapItems == null
                ? result.PhotoItems
                : result.PhotoItems.Where(s => MapItems.All(item => item.Id != s.PhotoId));

            var items = data.Select(photoItem => new MapItemModel
            {
                Location = new Geopoint(new BasicGeoposition
                {
                    Latitude = photoItem.Latitude,
                    Longitude = photoItem.Longitude,
                }),
                Id = photoItem.PhotoId
            }).ToList();

            if (MapItems == null)
                MapItems = new ObservableRangeCollection<IMapItem>(items);
            else
                MapItems.AddRange(items);
        }
    }
}
