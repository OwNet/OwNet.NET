using System;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.ServiceModel;
using WPFServer.DatabaseContext;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;

namespace WPFServer.SharedFiles
{
    [ServiceBehavior(UseSynchronizationContext=false, ConcurrencyMode=ConcurrencyMode.Multiple)]
    public partial class SharedFilesDataService : DataService<MyDBContext>, IServiceProvider
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            // Examples:
            // config.SetEntitySetAccessRule("MyEntityset", EntitySetRights.AllRead);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
            //config.SetEntitySetAccessRule("CacheSet",
            //    EntitySetRights.ReadMultiple |
            //    EntitySetRights.ReadSingle |
            //    EntitySetRights.AllWrite);
            //config.SetEntitySetAccessRule("CacheHeaderSet",
            //    EntitySetRights.ReadMultiple |
            //    EntitySetRights.ReadSingle |
            //    EntitySetRights.AllWrite);
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);

            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.UseVerboseErrors = true;
        }
    }

    public partial class SharedFilesDataService
    {
        #region IServiceProvider Members

        public object GetService(Type ServiceType)
        {
            if (ServiceType == typeof(IDataServiceStreamProvider))
            {
                return new SharedFilesServiceStreamProvider(this.CurrentDataSource);
            }
            return null;
        }

        #endregion
    }
}
