using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Protocols;


namespace SubscriberWebService.Services
{

    public class EventDataService : AbstractDatexService, IEventDataService
    {

        #region IEventDataService Members

        private const string PublicationType = "Event Data Publication";
        private const String NumberOfEventsInCache = "Number of Events in cache: ";
        private const String NumberOfEventsInPayload = "Number of Events in payload: ";
        private const string FullRefresh = "FULL REFRESH";
        private static readonly Dictionary<string, SituationRecord> EventCache = new Dictionary<string, SituationRecord>();
        private static readonly object LockEventUpdates = new Object();  

        public void Handle(D2LogicalModel d2LogicalModel)
        {
            log.Info(PublicationType + " : received...");
            log.Info(NumberOfEventsInCache + EventCache.Count());
            if (IsEventFullRefresh(d2LogicalModel))
            {
                HandleFullEventRefresh(d2LogicalModel);
            }
            else
            {
                HandleEventUpdate(d2LogicalModel);
            }

            log.Info(PublicationType + " : processed successfully.");
            log.Info(NumberOfEventsInCache + EventCache.Count());
        }

        private bool IsEventFullRefresh(D2LogicalModel d2LogicalModel)
        {
            return d2LogicalModel.payloadPublication.feedType.ToLower().Contains(FullRefresh.ToLower());
        }

        private void HandleFullEventRefresh(D2LogicalModel request)
        {
            lock (LockEventUpdates)
            {
                try
                {
                    log.Info("Processing Event Data Full Refresh.");
                    Dictionary<String, SituationRecord> tempEventCache = new Dictionary<string, SituationRecord>(EventCache);
                    EventCache.Clear();
                    ProcessEventsInFullRefresh(request, tempEventCache);
                    ProcessEventsUpdatedAfterFullRefreshPublicationTime(request, tempEventCache);
                    ProcessRefreshedEventData();
                    tempEventCache.Clear();
                    log.Info("Finished processing Event Data Full Refresh.");
                }
                catch (Exception e)
                {
                    
                    log.Error(e.Message);
                }
            }
        }

        private void ProcessRefreshedEventData()
        {
            foreach (SituationRecord situationRecord in EventCache.Values)
            {
                ProcessCommonEventData(situationRecord);
            }
        }

        private void ProcessEventsUpdatedAfterFullRefreshPublicationTime(D2LogicalModel request, Dictionary<String, SituationRecord> tempEventCache)
        {
            DateTime fullRefreshPublicationTime = request.payloadPublication.publicationTime;
            log.Info("Full Refresh Publication Time: " + fullRefreshPublicationTime);
            foreach (SituationRecord record in tempEventCache.Values)
            {
                if (IsEventUpdatedAfterFullRefreshPublicationTime(record, fullRefreshPublicationTime))
                {
                    EventCache.Add(record.id, record);
                    log.Info("Keeping cached version of event: " + record.id +" as it has been updated after the Full Refresh Publication Time.");
                }
                else
                {
                    log.Info("Discarding cached version of event: " + record.id +" as it is older than the Full Refresh Publication Time.");
                }
            }
        }

        private bool IsEventUpdatedAfterFullRefreshPublicationTime(SituationRecord situationRecord, DateTime dateTime)
        {
            DateTime eventVersionTime = situationRecord.situationRecordVersionTime;
            return eventVersionTime > dateTime;
        }

        private void ProcessEventsInFullRefresh(D2LogicalModel request, Dictionary<String, SituationRecord> tempEventCache)
        {
            SituationPublication situationPublication = (SituationPublication)request.payloadPublication;
            if (situationPublication != null)
            {
                Situation[] situations = situationPublication.situation;
                log.Info(NumberOfEventsInPayload + situations.Length);
                foreach (Situation situation in situations)
                {
                    RefreshEvent(situation.situationRecord[0], tempEventCache);
                }
            }
        }

        private void RefreshEvent(SituationRecord situationRecord, Dictionary<String, SituationRecord> tempEventCache)
        {
            SituationRecord cachedSituationRecord = null;
            string eventId = situationRecord.id;
            if (tempEventCache.ContainsKey(eventId))
            {
                cachedSituationRecord = tempEventCache[eventId];
                tempEventCache.Remove(eventId);
            }

            if (cachedSituationRecord != null)
            {
                if (IsCachedEventNewerThanRefreshedEvent(cachedSituationRecord, situationRecord))
                {
                    EventCache.Add(eventId, cachedSituationRecord);
                    log.Info("Full Refresh version of Event: " + eventId + " is older than cached version, keeping cached version and ignoring the Full Refresh version.");
                }
                else
                {
                    EventCache.Add(eventId, situationRecord);
                    log.Info("Full Refresh version of Event: " + eventId + " is newer than cached version, keeping the Full Refresh version and ignoring the cached version.");
                }
            }

            else
            {
                EventCache.Add(eventId, situationRecord);
                log.Info("New event: " + eventId + " received from the Full Refresh and stored in the cache.");
            }
        }

        private bool IsCachedEventNewerThanRefreshedEvent(SituationRecord cachedEvent, SituationRecord refreshEvent)
        {
            DateTime cachedVersionTime = cachedEvent.situationRecordVersionTime;
            DateTime refreshVersionTime = refreshEvent.situationRecordVersionTime;
            return cachedVersionTime > refreshVersionTime;
        }


        private void HandleEventUpdate(D2LogicalModel d2LogicalModel)
        {
            lock (LockEventUpdates)
            {
                try
                {
                    SituationPublication situationPublication = (SituationPublication)d2LogicalModel.payloadPublication;
                    if (situationPublication != null)
                    {
                        Situation[] situations = situationPublication.situation;
                        log.Info(NumberOfEventsInPayload + situations.Length);

                        foreach(Situation situation in situations)
                        {
                            // Only have 1 situationRecord per situation (index=0)
                            processEventData(situation);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                }
            }
        }

        private void processEventData(Situation situation)
        {
            SituationRecord situationRecord = situation.situationRecord[0];
            EventCache.Remove(situationRecord.id);
            EventCache.Add(situationRecord.id, situationRecord);
            ProcessEventData(situationRecord);

            _AssociatedEvent associatedEvent = situation.situationExtension.associatedEvent[0];
            if (null != associatedEvent)
            {
                log.Info("Associated event ID: " + associatedEvent.relatedSituation.id);
                log.Info("Associated event reference: " + associatedEvent.relatedSituationReference);
                log.Info("Association type: {}" + associatedEvent.associationType);
            }
        }

        /* Different types of event/situation record contain some common information
          and some type-specific data items and should be handled accordingly
          @param situationRecord */
        private void ProcessEventData(SituationRecord situationRecord)
        {
            ProcessCommonEventData(situationRecord);
            if (typeof(MaintenanceWorks) == (situationRecord.GetType()))
            {
                ProcessMaintenanceWorksEvent((MaintenanceWorks)situationRecord);
            }
        }

        private void ProcessCommonEventData(SituationRecord situationRecord)
        {
            log.Info("Event ID: " + situationRecord.id);
            log.Info("Version Time " + situationRecord.situationRecordVersionTime);
            log.Info("Severity: " + situationRecord.severity);
            log.Info("Current status: " + situationRecord.validity.validityStatus);
            log.Info("Overall start time: " + situationRecord.validity.validityTimeSpecification.overallStartTime);
            log.Info("Overall end time: " + situationRecord.validity.validityTimeSpecification.overallEndTime);         
        }

        private void ProcessMaintenanceWorksEvent(MaintenanceWorks maintenanceWorksEvent)
        {
            if (maintenanceWorksEvent.urgentRoadworks)
                log.Info("Urgent Roadworks!");

            RoadMaintenanceTypeEnum[] maintenanceTypes = maintenanceWorksEvent.roadMaintenanceType;
            foreach (RoadMaintenanceTypeEnum maintenanceType in maintenanceTypes)
            {
                log.Info("Type of maintenance involved: " + maintenanceType);
            }
            log.Info("Mobility: " + maintenanceWorksEvent.mobility.mobilityType);
            log.Info("Scale: " + maintenanceWorksEvent.roadworksScale);
            log.Info("Roadworks Scheme Name: " + maintenanceWorksEvent.maintenanceWorksExtension.roadworksEventDetails.roadworksSchemeName);

        }

        #endregion
    }
}
