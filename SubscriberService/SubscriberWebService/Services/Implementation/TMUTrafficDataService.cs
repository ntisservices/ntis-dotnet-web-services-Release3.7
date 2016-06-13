using System;
using System.Collections.Generic;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{

    public class TMUTrafficDataService : AbstractDatexService, ITMUTrafficDataService
    {

        public void Handle(D2LogicalModel request)
        {

            log.Info("New TMUTrafficDataResponse Received.");

            if (!ExampleDataCheckOk(request))
            {
                throw new SoapException("Incoming request does not appear to be valid!", SoapException.ClientFaultCode);
            }

            try
            {
                MeasuredDataPublication measuredDataPublication = request.payloadPublication as MeasuredDataPublication;

                if (measuredDataPublication != null)
                {
                    SiteMeasurements[] siteMeasurementsArray = measuredDataPublication.siteMeasurements;
                    foreach (SiteMeasurements siteMeasurements in siteMeasurementsArray)
                    {
                        extractTrafficDataFromSiteMeasurements(siteMeasurements);
                    }
                    log.Info("TMUTrafficDataResponse : processed successfully.");
                }

            }
            catch (Exception e)
            {
                log.Error("Error while obtaining MeasuredDataPublication.");
                log.Error(e.Message);
                throw new SoapException("Error while obtaining MeasuredDataPublication.", SoapException.ServerFaultCode, e);
            }

        }

        private void extractTrafficDataFromSiteMeasurements(SiteMeasurements measurementsForSite)
        {

            String siteGUID = measurementsForSite.measurementSiteReference.id;
            log.Info("TMU site ID: " + siteGUID);
            log.Info("Number of measurements for TMU site: " + measurementsForSite.measuredValue.Length);

            // There can be a number of measured values reported for the site
            foreach (_SiteMeasurementsIndexMeasuredValue measuredValue in measurementsForSite.measuredValue) {

                MeasuredValue mv = measuredValue.measuredValue;
                BasicData basicData = mv.basicData;

                // The index number of the site measurement is important - as this
                // relates the data
                // to the NTIS reference model, which adds context to the value
                // (e.g. lane information,
                // or vehicle characteristics)
                int index = measuredValue.index;

                // Determine what class (type) of traffic data is contained in the
                // basic data
                if (basicData.GetType() == typeof(TrafficFlow))
                {
                    TrafficFlow flow = (TrafficFlow) basicData;
                    log.Info("[Measurement Index : " + index + "] Vehicle Flow Rate: " + flow.vehicleFlow.vehicleFlowRate);

                    if (flow.vehicleFlow.dataError) {
                        MultilingualStringValue[] errorReason = flow.vehicleFlow.reasonForDataError.values;
                        foreach (MultilingualStringValue value in errorReason) {
                            log.Info("    Data in error. Reason: \"" + value.Value + "\"");
                        }
                    }

                } else if (basicData.GetType() == typeof(TrafficSpeed)) {
                    TrafficSpeed speed = (TrafficSpeed) basicData;
                    log.Info("[Measurement Index : " + index + "] Average Speed: " + speed.averageVehicleSpeed.speed);

                } else if (basicData.GetType() == typeof(TrafficHeadway)) {
                    TrafficHeadway headway = (TrafficHeadway) basicData;
                    log.Info("[Measurement Index : " + index + "] Average Headway: " + headway.averageTimeHeadway.duration);

                } else if (basicData.GetType() == typeof(TrafficConcentration)) {
                    TrafficConcentration concentration = (TrafficConcentration) basicData;
                    log.Info("[Measurement Index : " + index + "] Traffic Occupancy (%): " + concentration.occupancy.percentage);

                } else {
                    log.Error("Unexpected traffic data type contained in publication: " + basicData.GetType().Name.ToString());
                }

            }

        }

    }

}