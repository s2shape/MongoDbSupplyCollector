using System;

namespace MongoDbSupplyCollectorLoader
{
    public class Email
    {
        public Guid? ID { get; set; }
        public bool DELETED { get; set; }
        public Guid? CREATED_BY { get; set; }
        public DateTime? DATE_ENTERED { get; set; }
        public Guid? MODIFIED_USER_ID { get; set; }
        public DateTime? DATE_MODIFIED { get; set; }
        public DateTime? DATE_MODIFIED_UTC { get; set; }
        public Guid? ASSIGNED_USER_ID { get; set; }
        public Guid? TEAM_ID { get; set; }
        public string NAME { get; set; }
        public DateTime? DATE_START { get; set; }
        public DateTime? TIME_START { get; set; }
        public string PARENT_TYPE { get; set; }
        public Guid? PARENT_ID { get; set; }
        public string DESCRIPTION { get; set; }
        public string DESCRIPTION_HTML { get; set; }
        public string FROM_ADDR { get; set; }
        public string FROM_NAME { get; set; }
        public string TO_ADDRS { get; set; }
        public string CC_ADDRS { get; set; }
        public string BCC_ADDRS { get; set; }
        public Guid? TO_ADDRS_IDS { get; set; }
        public string TO_ADDRS_NAMES { get; set; }
        public string TO_ADDRS_EMAILS { get; set; }
        public Guid? CC_ADDRS_IDS { get; set; }
        public string CC_ADDRS_NAMES { get; set; }
        public string CC_ADDRS_EMAILS { get; set; }
        public string BCC_ADDRS_IDS { get; set; }
        public string BCC_ADDRS_NAMES { get; set; }
        public string BCC_ADDRS_EMAILS { get; set; }
        public string TYPE { get; set; }
        public string STATUS { get; set; }
        public Guid? MESSAGE_ID { get; set; }
        public string REPLY_TO_NAME { get; set; }
        public string REPLY_TO_ADDR { get; set; }
        public string INTENT { get; set; }
        public Guid? MAILBOX_ID { get; set; }
        public string RAW_SOURCE { get; set; }
        public Guid? TEAM_SET_ID { get; set; }

    }
}
