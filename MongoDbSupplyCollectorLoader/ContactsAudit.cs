using System;

namespace MongoDbSupplyCollectorLoader
{
    public class ContactsAudit
    {
        public Guid? AUDIT_ID { get; set; }
        public string AUDIT_ACTION { get; set; }
        public DateTime? AUDIT_DATE { get; set; }
        public string AUDIT_VERSION { get; set; }
        public string AUDIT_COLUMNS { get; set; }
        public string AUDIT_TOKEN { get; set; }
        public Guid? ID { get; set; }
        public bool DELETED { get; set; }
        public Guid? CREATED_BY { get; set; }
        public DateTime? DATE_ENTERED { get; set; }
        public Guid? MODIFIED_USER_ID { get; set; }
        public DateTime? DATE_MODIFIED { get; set; }
        public DateTime? DATE_MODIFIED_UTC { get; set; }
        public Guid? ASSIGNED_USER_ID { get; set; }
        public Guid? TEAM_ID { get; set; }
        public string CONTACT_NUMBER { get; set; }
        public string SALUTATION { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string LEAD_SOURCE { get; set; }
        public string TITLE { get; set; }
        public string DEPARTMENT { get; set; }
        public Guid? REPORTS_TO_ID { get; set; }
        public string BIRTHDATE { get; set; }
        public string DO_NOT_CALL { get; set; }
        public string PHONE_HOME { get; set; }
        public string PHONE_MOBILE { get; set; }
        public string PHONE_WORK { get; set; }
        public string PHONE_OTHER { get; set; }
        public string PHONE_FAX { get; set; }
        public string EMAIL1 { get; set; }
        public string EMAIL2 { get; set; }
        public string ASSISTANT { get; set; }
        public string ASSISTANT_PHONE { get; set; }
        public string EMAIL_OPT_OUT { get; set; }
        public string INVALID_EMAIL { get; set; }
        public string SMS_OPT_IN { get; set; }
        public string TWITTER_SCREEN_NAME { get; set; }
        public string PRIMARY_ADDRESS_STREET { get; set; }
        public string PRIMARY_ADDRESS_CITY { get; set; }
        public string PRIMARY_ADDRESS_STATE { get; set; }
        public string PRIMARY_ADDRESS_POSTALCODE { get; set; }
        public string PRIMARY_ADDRESS_COUNTRY { get; set; }
        public string ALT_ADDRESS_STREET { get; set; }
        public string ALT_ADDRESS_CITY { get; set; }
        public string ALT_ADDRESS_STATE { get; set; }
        public string ALT_ADDRESS_POSTALCODE { get; set; }
        public string ALT_ADDRESS_COUNTRY { get; set; }
        public string DESCRIPTION { get; set; }
        public string PORTAL_NAME { get; set; }
        public string PORTAL_PASSWORD { get; set; }
        public string PORTAL_ACTIVE { get; set; }
        public string PORTAL_APP { get; set; }
        public Guid? CAMPAIGN_ID { get; set; }
        public Guid? TEAM_SET_ID { get; set; }
        public string PICTURE { get; set; }
    }
}
