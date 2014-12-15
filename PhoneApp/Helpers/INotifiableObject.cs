using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneApp.Helpers
{
    public interface INotifiableObject
    {
        void NotifyFinished();
        void NotifyFailed();
    }
}
