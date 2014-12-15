using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Threading;
using System.IO;
using System.ServiceModel;
using SharedProxy.Streams.Output;

namespace SharedProxy.Services.Host
{
    public partial class ProxyServiceContextEntities
    {
        #region Populate Service Data

        static IList<ServiceItem> _proxyServiceItems;
        static int _size = 512;
        static List<int> _freeIndices = new List<int>();
        static Semaphore _semFreeIndices = new Semaphore(_size, _size);

        static ProxyServiceContextEntities()
        {
            _proxyServiceItems = new ServiceItem[_size];

            for (int i = 0; i < _size; ++i)
            {
                _proxyServiceItems[i] = new ServiceItem() { NoCacheItemId = i };
                _freeIndices.Add(i);
            }
        }

        #endregion

        public IQueryable<ServiceItem> ProxyServiceItems
        {
            get { return _proxyServiceItems.AsQueryable<ServiceItem>(); }
        }

        public ServiceItem GetFreeItem()
        {
            _semFreeIndices.WaitOne();
            ServiceItem item = null;
            lock (_freeIndices) {
                int index = _freeIndices[0];
                _freeIndices.RemoveAt(0);
                item = _proxyServiceItems.ElementAt(index);
                item.Used = true;
            }
            return item;
        }

        public static void DisposeItem(ServiceItem item)
        {
            if (item.Used == true)
            {
                item.Used = false;
                item.DisposeStreamItem();
                lock (_freeIndices)
                {
                    _freeIndices.Add(item.NoCacheItemId);
                    _semFreeIndices.Release();
                }
            }
        }
    }

    [ServiceBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class ProxyService : DataService<ProxyServiceContextEntities>, IServiceProvider
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            config.SetEntitySetAccessRule("ProxyServiceItems", EntitySetRights.All);

            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.UseVerboseErrors = true;
        }
    }

    public partial class ProxyService
    {
        #region IServiceProvider Members

        public object GetService(Type ServiceType)
        {
            if (ServiceType == typeof(IDataServiceStreamProvider))
            {
                return new ProxyServiceStreamProvider(this.CurrentDataSource);
            }
            return null;
        }

        #endregion
    }

    public class ServiceItemTimer : System.Timers.Timer
    {
        public ServiceItem ServiceItem { get; set; }

        public ServiceItemTimer(double interval)
            : base(interval)
        { }
    }
}
