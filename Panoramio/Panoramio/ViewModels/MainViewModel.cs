using System;
using System.Linq;
using System.Net;
using System.Threading;
using Caliburn.Micro;
using Panoramio.Models;
using Panoramio.Common;
using Panoramio.Server.Data;
using Panoramio.UserControls;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Devices.Geolocation;

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

        private MapItemModel _selectedItem;
        public MapItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Set(ref _selectedItem, value);
                OnSelectedItem(_selectedItem);
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

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                Set(ref _isLoading, value);
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
            MapZoom = 14;
        }

        protected override void OnDeactivate(bool close)
        {
            MapItems = null;

            base.OnDeactivate(close);
        }

        private CancellationTokenSource _downloadPhotoCancellationToken;
        private int _from;
        private const int DownloadCountByStep = 20;
        private const int MaxDownloadCount = 40;

        private async void LoadingData(GeoBounds geoBounds)
        {
            _downloadPhotoCancellationToken?.Cancel(true);

            IsLoading = true;

            _downloadPhotoCancellationToken = new CancellationTokenSource();

            _from = 0;
            _currentDownloadItems.Clear();

            while (true)
            {
                Photos photos = null;
                try
                {
                    photos = await Server.ServerFacade.GetPhotos(_from, _from + DownloadCountByStep,
                        geoBounds.MinX, geoBounds.MinY, geoBounds.MaxX, geoBounds.MaxY,
                        _downloadPhotoCancellationToken.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        //TODO логирование
                        _from += DownloadCountByStep;

                        //не корректный запрос
                        //{Name = "WebException" FullName = "System.Net.WebException"}
                        //{"The remote server returned an error: (400) Bad Request."}
                    }
                    else if (ex.Status == WebExceptionStatus.NameResolutionFailure ||
                                ex.Status == WebExceptionStatus.ConnectFailure ||
                                ex.Status == WebExceptionStatus.UnknownError || 
                                ex.Status == WebExceptionStatus.Timeout)
                    {
                        ServerIsUnavailable = true;
                        await Task.Delay(5000);
                        continue;
                        //нет инета
                        //{Name = "WebException" FullName = "System.Net.WebException"}
                        //{"An error occurred while sending the request. The text associated with this error code could not be found.\r\n\r\nThe server name or address could not be resolved\r\n"}
                    }
                    else
                    {
                        //TODO логирование
                        _from += DownloadCountByStep;
                    }
                }
                catch (Exception ex)
                {
                    //TODO логирование
                    _from += DownloadCountByStep;
                }

                if (ServerIsUnavailable)
                    ServerIsUnavailable = false;

                if (photos == null)
                {
                    await Task.Delay(5000);
                    continue;
                }
                
                AddPhotos(photos.PhotoItems);

                if (photos.PhotoItems == null || photos.PhotoItems.Length == 0)
                    break;

                if (_from + DownloadCountByStep >= photos.Count)
                    break;

                if(_from + DownloadCountByStep >= MaxDownloadCount)
                    break;

                _from += DownloadCountByStep;
            }

            if (MapItems != null)
            {
                var removeItems = MapItems.Where(i => _currentDownloadItems.All(p => p.PhotoId != i.Id)).ToList();

                foreach (var item in removeItems)
                {
                    MapItems.Remove(item);
                }
            }

            IsLoading = false;
            _downloadPhotoCancellationToken = null;
        }

        private readonly List<PhotoItem> _currentDownloadItems = new List<PhotoItem>();

        private void AddPhotos(PhotoItem[] photoItems)
        {
            if (photoItems == null || photoItems.Length == 0)
                return;

            _currentDownloadItems.AddRange(photoItems);

            var data = MapItems == null
                ? photoItems
                : photoItems.Where(s => MapItems.All(item => item.Id != s.PhotoId));

            var items = data.Select(photoItem => new MapItemModel
            {
                Location = new Geopoint(new BasicGeoposition
                                        {
                                            Latitude = photoItem.Latitude,
                                            Longitude = photoItem.Longitude,
                                        }),
                Id = photoItem.PhotoId,
                PhotoUrl = photoItem.PhotoFileUrl,
                PhotoTitle = photoItem.PhotoTitle
            }).ToList();

            if (MapItems == null)
                MapItems = new ObservableRangeCollection<IMapItem>(items);
            else
                MapItems.AddRange(items);
        }

        public void OnGeoBoundsChanged(GeoBounds geoBounds)
        {
            LoadingData(geoBounds);
        }
        
        public void OnSelectedItem(MapItemModel mapItem)
        {
            if (mapItem == null)
                return;

            SelectedItem = null;

            NavigationService.For<SelectedPhotoViewModel>()
                .WithParam(s => s.PhotoUrl, mapItem.PhotoUrl)
                .WithParam(s => s.PhotoTitle, mapItem.PhotoTitle)
                .Navigate();
        }
    }
}
