using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SubscriberWebService.Services
{
    public interface ITMUTrafficDataService
    {
        void Handle(D2LogicalModel deliverTMUTrafficDataRequest);
    }
}