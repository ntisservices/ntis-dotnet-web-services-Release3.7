using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriberWebService.Services
{
    public interface IFusedFvdAndSensorTrafficDataService
    {
        void Handle(D2LogicalModel deliverFVDTrafficDataRequest); 
    }
}
