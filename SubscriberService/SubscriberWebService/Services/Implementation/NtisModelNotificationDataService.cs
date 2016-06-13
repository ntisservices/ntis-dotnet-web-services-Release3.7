using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{
    public class NtisModelNotificationDataService : AbstractDatexService, INtisModelNotificationDataService
    {

        public void Handle(D2LogicalModel deliverNtisModelNotificationDataRequest)
        {

            log.Info("New NtisModelNotificationService received.");

            // Validate the D2Logical Model
            if (!ExampleDataCheckOk(deliverNtisModelNotificationDataRequest))
            {
                throw new SoapException("Incoming request does not appear to be valid!", SoapException.ClientFaultCode);
            }

            if (deliverNtisModelNotificationDataRequest.payloadPublication != null)
            {

                GenericPublication genericPublication = deliverNtisModelNotificationDataRequest.payloadPublication as GenericPublication;

                log.Info("Ntis model publication time: " + genericPublication.publicationTime);
                log.Info("Generic publication name: " + genericPublication.genericPublicationName);

                _GenericPublicationExtensionType genericPublicationExtension = genericPublication.genericPublicationExtension;
                NtisModelVersionInformation ntisModelVersionInformation = genericPublicationExtension.ntisModelVersionInformation;

                log.Info("Network model - file name " + ntisModelVersionInformation.modelFilename);
                log.Info("Network model - version " + ntisModelVersionInformation.modelVersion);
                log.Info("Network model - publication time " + ntisModelVersionInformation.modelPublicationTime);                

            }


            log.Info("NtisModelNotificationService: Processing Completed Successfuly");

        }

    }

}