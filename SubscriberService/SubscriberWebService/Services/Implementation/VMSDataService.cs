using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

namespace SubscriberWebService.Services
{

    public class VMSDataService : AbstractDatexService, IVMSDataService
    {

        public void Handle(D2LogicalModel VMSTrafficData)
        {

            log.Info("New VMSTrafficData received.");

            // Validate the D2Logical Model
            if (!ExampleDataCheckOk(VMSTrafficData))
            {
                throw new SoapException("Incoming request does not appear to be valid!", SoapException.ClientFaultCode);
            }

            try
            {

                VmsPublication vmsPublication = (VmsPublication) VMSTrafficData.payloadPublication;

                if (vmsPublication != null) {

                    VmsUnit[] vmsUnits = vmsPublication.vmsUnit;

                    log.Info("Number of VMS/Matrix Units in payload: " + vmsUnits.Length);

                    // The publication can contain status info for more than one
                    // unit
                    foreach (VmsUnit vmsUnit in vmsUnits)
                    {
                        extractStatusInformationFromUnitData(vmsUnit);
                    }

                    log.Info("VMSTrafficData: Processing Completed Successfuly");

                }

            }
            catch (Exception e)
            {
                log.Error("Error while obtaining MeasuredDataPublication.");
                log.Error(e.Message);
                throw new SoapException("Error while obtaining MeasuredDataPublication.", SoapException.ServerFaultCode, e);
            }

        }

        private void extractStatusInformationFromUnitData(VmsUnit vmsUnit) {

            // Typically, the service should refer to the NTIS reference Model to
            // determine what type (VMS or Matrix Signal) the unit is; and hence how
            // to process the data. For this simple example, however, the ID of the
            // unit table reference is used.
            String vmsUnitType = vmsUnit.vmsUnitTableReference.id;

            if ("NTIS_VMS_Units".CompareTo(vmsUnitType) == 0) {

                log.Info("VMS Unit ID: " + vmsUnit.vmsUnitReference.id);

                // There is only ever 1 VMS/Matrix unit per vmsUnit element - at
                // index 0
                _VmsUnitVmsIndexVms unit = vmsUnit.vms[0];

                // There is only ever 1 message per unit
                VmsMessage message = unit.vms.vmsMessage[0].vmsMessage;

                log.Info("Message type: " + message.vmsMessageInformationType);
                log.Info("Set by: " + message.messageSetBy.values[0].Value);
                log.Info("Last set at: " + message.timeLastSet);
                log.Info("Related event: " + message.situationToWhichMessageIsRelated.id);


                // There is only ever 1 text page specified, with multiple text lines
                _TextPage textPage = message.textPage[0];
                _VmsTextLineIndexVmsTextLine[] textLines = textPage.vmsText.vmsTextLine;
                foreach (_VmsTextLineIndexVmsTextLine textLine in textLines)
                    log.Info("Text Line #" + (textLine.lineIndex + 1) + ": " + textLine.vmsTextLine.vmsTextLine);

                // There is only ever 1 pictogram display area, with 1 pictogram
                log.Info("Pictogram Displayed: " + message.vmsPictogramDisplayArea[0].vmsPictogramDisplayArea.vmsPictogram[0].vmsPictogram.pictogramDescription[0].ToString());

            } else if ("NTIS_Matrix_Units".CompareTo(vmsUnitType) == 0) {

                log.Info("Matrix Unit ID: " + vmsUnit.vmsUnitReference.id);

                // There is only ever 1 VMS/Matrix unit per vmsUnit element - at
                // index 0
                _VmsUnitVmsIndexVms unit = vmsUnit.vms[0];

                // There is only ever 1 message per unit
                VmsMessage message = unit.vms.vmsMessage[0].vmsMessage;

                log.Info("Last set at: " + message.timeLastSet);

                // There is only ever 1 pictogram display area, with 1 pictogram
                VmsPictogram pictogram = message.vmsPictogramDisplayArea[0].vmsPictogramDisplayArea.vmsPictogram[0].vmsPictogram;
            
                // If the pictogram is 'other', then the actual pictogram displayed is defined by the vmsPictogramEUK 
                // extension (applies to Matrix Signals only)
                VmsDatexPictogramEnum pictogramType = pictogram.pictogramDescription[0];

                String pictogramDesc = null;
                if (pictogramType.CompareTo(VmsDatexPictogramEnum.other) == 0) {
                    pictogramDesc = pictogram.vmsPictogramExtension.vmsPictogramUK.pictogramDescriptionUK.ToString();
                } else {
                    pictogramDesc = pictogramType.ToString();
                }
                log.Info("Pictogram Displayed: " + pictogramDesc);

            } else {
                log.Error("Invalid unit type received in publication: " + vmsUnitType);
            }

        }

    }

}