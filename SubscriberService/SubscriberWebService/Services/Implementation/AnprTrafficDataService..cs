using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{

    public class AnprTrafficDataService : AbstractDatexService, IAnprTrafficDataService
    {

        #region IAnprTrafficDataService Members

        public void Handle(D2LogicalModel deliverANPRTrafficDataRequest)
        {

            log.Info("New AnprTrafficDataService received.");

            // Validate the D2Logical Model
            if (!ExampleDataCheckOk(deliverANPRTrafficDataRequest))
            {
                throw new SoapException("Incoming request does not appear to be valid!", SoapException.ClientFaultCode);
            }

            try
            {

                MeasuredDataPublication measuredDataPublication = deliverANPRTrafficDataRequest.payloadPublication as MeasuredDataPublication;

                if (measuredDataPublication != null)
                {

                    List<SiteMeasurements> siteMeasurementsInPayload = measuredDataPublication.siteMeasurements.ToList();

                    log.Info("Got MeasuredDataPublication from request");
                    log.Info("Number of Site Measurements in payload: " + siteMeasurementsInPayload.Count());
                    foreach (SiteMeasurements measurementsForSite in siteMeasurementsInPayload)
                    {
                        extractTravelTimesFromSiteMeasurements(measurementsForSite);
                    }

                }

                log.Info("AnprTrafficDataService: Processed successfuly");

            }
            catch (Exception e)
            {
                log.Error("Error while obtaining MeasuredDataPublication.");
                log.Error(e.Message);
                throw new SoapException("Error while obtaining MeasuredDataPublication.", SoapException.ServerFaultCode, e);
            }

        }

        private void extractTravelTimesFromSiteMeasurements(SiteMeasurements siteMeasurements)
        {
            String anprRouteId = siteMeasurements.measurementSiteReference.id;
            // Should only be one travel time value per SiteMeasurements
            // element (index=0)
            MeasuredValue value = siteMeasurements.measuredValue[0].measuredValue;
            if (value.basicData.GetType() == typeof(TravelTimeData))
            {
                TravelTimeData ttData = (TravelTimeData)value.basicData;
                log.Info("Travel Time for ANPR Route " + anprRouteId + " : " + ttData.travelTime.duration + "s");
            }
        }

        #endregion

    }

}
