using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    public class DataController
    {
        static IDataModelCreator dataModelCreator = null;

        public static IDataModelCreator DataModelCreator
        {
            get
            {
                return dataModelCreator;
            }
            set
            {
                dataModelCreator = value;
            }
        }

        public static ICentralDataModelContainer Container
        {
            get
            {
                if (dataModelCreator == null)
                    dataModelCreator = new CentralDataModelCreator();

                return dataModelCreator.CreateDataModelContainer();
            }
        }
    }
}
