using Panoramio.UserControls;
using Panoramio.ViewModels;

namespace Panoramio.Views
{
    public sealed partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void OnGeoBoundsChanged(GeoBounds geobounds)
        {
            ((MainViewModel) DataContext).OnGeoBoundsChanged(geobounds);
        }
    }
}
