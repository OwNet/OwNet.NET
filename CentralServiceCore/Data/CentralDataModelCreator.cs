using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceCore.Data
{
    class CentralDataModelCreator : IDataModelCreator
    {
        public ICentralDataModelContainer CreateDataModelContainer()
        {
            return new CentralDataModelContainer();
        }
    }
}
