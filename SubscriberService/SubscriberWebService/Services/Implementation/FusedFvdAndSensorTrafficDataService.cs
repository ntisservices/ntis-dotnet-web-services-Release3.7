using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{

    public class FusedFvdAndSensorTrafficDataService : AbstractDatexService, IFusedFvdAndSensorTrafficDataService
    {

        public void Handle(D2LogicalModel deliverFVDTrafficDataRequest)
        {

            log.Info("New FusedFvdAndSensorTrafficData received");
            ElaboratedDataPublication elaboratedDataPublication = null;

            try
            {

                elaboratedDataPublication = (ElaboratedDataPublication) deliverFVDTrafficDataRequest.payloadPublication;

                if (elaboratedDataPublication != null) {

                    ElaboratedData[] elaboratedDataList = elaboratedDataPublication.elaboratedData;
                    log.Info("Number of data items in the publication: " + elaboratedDataList.Length);

                    foreach (ElaboratedData dataItem in elaboratedDataList)
                    {
                        extractTrafficDataFromElaboratedData(dataItem);
                    }

                }
                log.Info("FusedFvdAndSensorTrafficData: processed successfully.");

            }
            catch (Exception e) {
                log.Error(e.Message);
            }

        }

        private void extractTrafficDataFromElaboratedData(ElaboratedData dataItem)
        {

            // Location is always specified as LocationByReference (referenced to a
            // single Network Link)
            LocationByReference location = (LocationByReference) dataItem.basicData.pertinentLocation;
            log.Info("Data for Network Link: " + location.predefinedLocationReference.id);
        
            BasicData basicData = dataItem.basicData;

            // Determine what class (type) of traffic data is contained in the basic data
            if (basicData.GetType() == typeof (TrafficSpeed))
            {

                TrafficSpeed speed = (TrafficSpeed)basicData;
            
                // There are 2 types of speed data - current and forecast, determined by 
                // whether a measurementOrCalculationTime element exists in the data item.
                if (speed.measurementOrCalculationTime == null)
                {
                    log.Info("Fused Average Speed: " + speed.averageVehicleSpeed.speed);
                    log.Info("FVD-Only Average Speed: " + speed.trafficSpeedExtension.speedFvdOnly.speed);
                } else
                {
                    log.Info("Fused Average Speed (forecast for " + speed.measurementOrCalculationTime.ToString() + "): " + speed.averageVehicleSpeed.speed);
                }
            
            }
            else if (basicData.GetType() == typeof (TravelTimeData))
            {

                TravelTimeData travelTimeData = (TravelTimeData)basicData;

                // There are 2 types of travel time data - current and forecast,
                // determined by whether a measurementOrCalculationTime element
                // exists in the data item.
                if (travelTimeData.measurementOrCalculationTime == null)
                {
                    log.Info("Travel Time: " + travelTimeData.travelTime.duration);
                    log.Info("Free Flow Travel Time: " + travelTimeData.freeFlowTravelTime.duration);
                    log.Info("Normally Expected Travel Time: " + travelTimeData.normallyExpectedTravelTime.duration);
                } else
                {
                    log.Info("Travel Time (forecast for " + travelTimeData.measurementOrCalculationTime.ToString() + "): " + travelTimeData.travelTime.duration);
                }

            }
            else
            {
                log.Error("Unexpected traffic data type contained in publication: " + dataItem.GetType().ToString());
            }

        }

    }

}