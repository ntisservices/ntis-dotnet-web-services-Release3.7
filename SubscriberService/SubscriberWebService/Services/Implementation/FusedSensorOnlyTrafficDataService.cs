using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{

    public class FusedSensorOnlyTrafficDataService : AbstractDatexService, IFusedSensorOnlyTrafficDataService
    {

        public void Handle(D2LogicalModel deliverGetFusedSensorOnlyTrafficDataRequest)
        {

            log.Info("New FusedSensorOnlyTrafficData received.");

            // Validate the D2Logical Model
            if (!ExampleDataCheckOk(deliverGetFusedSensorOnlyTrafficDataRequest))
            {
                throw new SoapException("Incoming request does not appear to be valid!", SoapException.ClientFaultCode);
            }

            ElaboratedDataPublication elaboratedDataPublication = null;

            try
            {

                elaboratedDataPublication = (ElaboratedDataPublication) deliverGetFusedSensorOnlyTrafficDataRequest.payloadPublication;

                if (elaboratedDataPublication != null)
                {

                    ElaboratedData[] elaboratedDataList = elaboratedDataPublication.elaboratedData;

                    log.Info("Number of data items in the publication: " + elaboratedDataList.Length);
                
                    foreach (ElaboratedData dataItem in elaboratedDataList)
                    {
                        extractTrafficDataFromElaboratedData(dataItem);
                    }

                    log.Info("FusedSensorOnlyTrafficData: processed successfully.");

                }
                
            } catch (Exception e)
            {
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

            // Determine what class (type) of traffic data is contained in the basic
            // data
            if (basicData.GetType() == typeof(TrafficSpeed))
            {

                TrafficSpeed speed = (TrafficSpeed) basicData;
                log.Info("Average Speed: " + speed.averageVehicleSpeed.speed);

            }
            else if (basicData.GetType() == typeof(TravelTimeData))
            {

                TravelTimeData travelTimeData = (TravelTimeData) basicData;
                log.Info("Travel Time: " + travelTimeData.travelTime.duration);
                log.Info("Free Flow Travel Time: " + travelTimeData.freeFlowTravelTime.duration);
                log.Info("Normally Expected Travel Time: " + travelTimeData.normallyExpectedTravelTime.duration);

            }
            else if (basicData.GetType() == typeof(TrafficFlow))
            {

                TrafficFlow flow = (TrafficFlow) basicData;
                log.Info("Traffic Flow: " + flow.vehicleFlow.vehicleFlowRate);

            }
            else if (basicData.GetType() == typeof(TrafficConcentration))
            {

                TrafficConcentration concentration = (TrafficConcentration) basicData;
                log.Info("Occupancy (%age): " + concentration.occupancy.percentage);

            }
            else if (basicData.GetType() == typeof(TrafficHeadway))
            {

                TrafficHeadway headway = (TrafficHeadway) basicData;
                log.Info("Headway: " + headway.averageTimeHeadway.duration);

            }
            else
            {
                log.Error("Unexpected traffic data type contained in publication: " + dataItem.GetType().ToString());
            }
    
        }

    }

}