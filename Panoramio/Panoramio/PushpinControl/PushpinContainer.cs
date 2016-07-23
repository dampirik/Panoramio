using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Panoramio.PushpinControl
{
    /// <summary>
    /// A container for one or more pushpins at a given screen coordinate.
    /// </summary>
    public class PushpinContainer
    {
        private readonly List<IPushpinModel> _partnerns;

        /// <summary>
        /// Creates a container for the given pushpin
        /// </summary>
        public PushpinContainer(IPushpinModel parentPartner, Point location)
        {
            _partnerns = new List<IPushpinModel> { parentPartner };
            ScreenLocation = location;
        }

        /// <summary>
        /// Adds the pins from the given container
        /// </summary>
        public void Merge(PushpinContainer pinContainer)
        {
            foreach (var pin in pinContainer._partnerns)
            {
                _partnerns.Add(pin);
            }
        }

        /// <summary>
        /// Gets or sets the current screen location of this container
        /// </summary>
        public Point ScreenLocation { get; private set; }

        /// <summary>
        /// Gets the visual representation of the contents of this container. If it is 
        /// a single pushpin, the pushpin itself is returned. If multiple pushpins are present
        /// a pushpin with the given clusterTemplate is returned.
        /// </summary>
        public BasePushpinControl GetMapElement()
        {
            var parent = _partnerns.First();
            
            if (_partnerns.Count > 1)
            {
                return new ClusterPushpinControl(_partnerns)
                {
                    Location = parent.Location,
                    Count = _partnerns.Count
                };
            }

            return new PushpinControl(parent)
            {
                Location = parent.Location
            };
        }
    }
}
