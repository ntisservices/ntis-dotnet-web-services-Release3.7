using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubscriberWebService.Services
{
    public interface IAnprTrafficDataService
    {
        void Handle(D2LogicalModel deliverANPRTrafficDataRequest);
    }
}
