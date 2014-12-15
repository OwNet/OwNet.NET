using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace CentralServiceCore.WebTrackerCommunication
{
    public class ClientRegistration
    {
        internal static bool RegisterClient()
        {
            if (Properties.Settings.Default.WebTrackerClientId != -1)
                return true;
            try
            {
                var serviceClient = new WebTrackerClientsService.ClientsServiceClient();
                var clientType = serviceClient.RegisterClient();
                Properties.Settings.Default.WebTrackerClientId = clientType.ClientId;
                Properties.Settings.Default.WebTrackerClientPassword = clientType.Code;
                Properties.Settings.Default.Save();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
