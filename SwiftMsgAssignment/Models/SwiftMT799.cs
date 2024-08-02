namespace SwiftMsgAssignment.Models
{
    public class BasicHeaderBlock
    { 
        //basic header
        public char AppID { get; set; }
        public string ServiceID { get; set; }
        public string LTAddress { get; set; }
        public string SessionNumber { get; set; }
        public string SequenceNumber { get; set; }
    }
    public class ApplicationHeaderBlock
    { 
        //application header
        public char IOID { get; set; }
        public int SWIFTMT { get; set; }
        public TimeOnly InputTime { get; set; }
        public string MIR { get; set; }
        public DateOnly OutputDate { get; set; }
        public TimeOnly OutputTime { get; set; }
        public char? Priority { get; set; }
    }
    public class MT799TextBlock
    { 
        //text
        public string TransactionReference { get; set; }
        public string? RelatedReference { get; set; }
        public string Narrative { get; set; }
    }
    public class SwiftMT799
    {
        public BasicHeaderBlock BasicHeader { get; set; }
        public ApplicationHeaderBlock? ApplicationHeader { get; set; }
        //should probably become a dictionary instead
        public string[]? UserHeader { get; set; }
        public MT799TextBlock? Text { get; set; }
        public string[]? Trailers { get; set; }


    }
}
