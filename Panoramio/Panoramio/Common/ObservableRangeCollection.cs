using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Panoramio.Common
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public ObservableRangeCollection()
        {
        }

        public ObservableRangeCollection(IEnumerable<T> collection)
            : base(collection)
        {

        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (var i in collection)
                Items.Add(i);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList)collection, new List<T>()));
        }
    }
}
