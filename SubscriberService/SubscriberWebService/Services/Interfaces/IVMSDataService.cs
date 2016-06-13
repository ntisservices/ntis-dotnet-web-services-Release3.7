using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubscriberWebService.Services
{
    public interface IVMSDataService
    {
        void Handle(D2LogicalModel deliverTMUTrafficDataRequest);
    }
}
