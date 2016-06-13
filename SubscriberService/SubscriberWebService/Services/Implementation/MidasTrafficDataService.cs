using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{
    public class MidasTrafficDataService: AbstractDatexService, IMidasTrafficDataService
    {        
        private static int MaxSensorReadings = 7;

        #region IMidasTrafficDataService Members

        public void Handle(D2LogicalModel deliverMIDASTrafficDataRequest)
        {

            log.Info("New MidasTrafficDataService received.");
            MeasuredDataPublication measuredDataPublication = null;

            // Validate the D2Logical Model
            if (!ExampleDataCheckOk(deliverMIDASTrafficDataRequest))
            {
                throw new SoapException("Incoming request does not appear to be valid!", SoapException.ClientFaultCode);
            }
            
            // MeasuredDataPublication class contains the feed description, feed
            // type, site measurements, publication time and other header information.
            try
            {

                measuredDataPublication = deliverMIDASTrafficDataRequest.payloadPublication as MeasuredDataPublication;

                if (measuredDataPublication != null)
                {

                    List<SiteMeasurements> siteMeasurementsInPayload = measuredDataPublication.siteMeasurements.ToList();
                    log.Info("Number of Site Measurements in payload: " + siteMeasurementsInPayload.Count);

                    // Eash MIDAS site is encapsulated within a SiteMeasurements object.
                    // Cycle through these to get to the sensor readings for a MIDAS site.
                    foreach (SiteMeasurements siteMeasurement in siteMeasurementsInPayload)
                    {
                        log.Debug("measurementDataPublication ID is " + siteMeasurement.measurementSiteReference.id);
                        log.Debug("measurementDataPublication time default is " + siteMeasurement.measurementTimeDefault.ToString());

                        // Cycle through the MeasuredValues to get the individual sensor readings for a MIDAS site.
                        foreach (_SiteMeasurementsIndexMeasuredValue singleMeasuredValue in siteMeasurement.measuredValue)
                        {
                            extractTrafficDataFromSiteMeasurements(siteMeasurement);
                        }
                        log.Info("MidasTrafficDataService: processed successfully.");

                    }

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
            log.Info("MIDAS site ID: " + siteGUID);
            log.Info("Number of measurements for MIDAS site: " + measurementsForSite.measuredValue.Count());

            // There can be a number of measured values reported for the site
            foreach (_SiteMeasurementsIndexMeasuredValue measuredValue in measurementsForSite.measuredValue) {

                MeasuredValue mv = measuredValue.measuredValue;
                BasicData basicData = mv.basicData;
            
                // The index number of the site measurement is important - as this relates the data
                // to the NTIS reference model, which adds context to the value (e.g. lane information, 
                // or vehicle characteristics)
                int index = measuredValue.index;

                // Determine what class (type) of traffic data is contained in the basic data
                if (basicData.GetType() == typeof(TrafficFlow)) {

                    TrafficFlow flow = (TrafficFlow)basicData;
                    log.Info("[Measurement Index : " + index + "] Vehicle Flow Rate: " + flow.vehicleFlow.vehicleFlowRate);
                
                    if(flow.vehicleFlow.dataError) {
                        List<MultilingualStringValue> errorReason = flow.vehicleFlow.reasonForDataError.values.ToList();
                        foreach (MultilingualStringValue value in errorReason)
                        {
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

        #endregion
    }
}
