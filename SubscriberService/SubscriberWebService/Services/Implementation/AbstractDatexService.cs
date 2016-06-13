using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace SubscriberWebService.Services
{
    /// <summary>
    /// Class to check that a request contains a valid D2LogicalModel object,
    /// and demonstrates how to extract data from it.
    /// </summary>
    public abstract class AbstractDatexService
    {
        protected static readonly ILog log = log4net.LogManager.GetLogger(typeof(AbstractDatexService));

        /// <summary>
        /// Check that the D2LogicalModel is not null, and contains an Exchange object.
        /// </summary>
        /// <param name="d2LogicalModel"></param>
        /// <returns>true if valid, false otherwise</returns>
        public bool ExampleDataCheckOk(D2LogicalModel d2LogicalModel)
        {
            if (d2LogicalModel == null)
            {
                log.Error("D2LogicalModel is null! Incoming request does not appear to be valid.");
                return false;
            }

            // Exchange must not be null.
            if (d2LogicalModel.exchange == null)
            {
                log.Error("Exchange is null! Incoming request does not appear to be valid.");
                return false;
            }

            if (d2LogicalModel.payloadPublication == null)
            {
                log.Error("PayloadPublication is null! Incoming request does not appear to be valid");
                return false;
            }

            if (d2LogicalModel.payloadPublication.feedType == null)
            {
                log.Error("FeedType is null! Incoming request does not appear to be valid");
                return false;
            }

            log.Debug("Data checked");
            return true;
        }


    }
}
