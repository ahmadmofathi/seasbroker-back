namespace Seasbroker.Modules.Forms.Application.Constants;

public static class FormsConstants
{
    public const string SuperuserPolicy = "Superuser";

    /// <summary>The fixed set of forms this feature manages. Admins configure these, they don't create new ones.</summary>
    public static class FormKeys
    {
        public const string RequestQuote = "request-quote";
        public const string RequestRoute = "request-route";
        public const string RequestClearance = "request-clearance";

        public static readonly IReadOnlySet<string> All = new HashSet<string>
        {
            RequestQuote, RequestRoute, RequestClearance,
        };
    }

    /// <summary>System field keys that map onto real RequestedQuote / Customer columns.</summary>
    public static class SystemFieldKeys
    {
        public const string CargoType = "CargoType";
        public const string Weight = "Weight";
        public const string DeparturePort = "DeparturePort";
        public const string DepartureTime = "DepartureTime";
        public const string ArrivalPort = "ArrivalPort";
        public const string ArrivalTime = "ArrivalTime";
        public const string Dimensions = "Dimensions";
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string Email = "Email";
        public const string PhoneNumber = "PhoneNumber";
        public const string AdditionalInfo = "AdditionalInfo";

        /// <summary>Every other SystemFieldKey (e.g. ClearanceType) has business meaning but no dedicated
        /// RequestedQuote column - its value is captured only in FormSubmissionValue, same as a custom field.</summary>
        public static readonly IReadOnlySet<string> MappedToRequestedQuote = new HashSet<string>
        {
            CargoType, Weight, DeparturePort, DepartureTime, ArrivalPort, ArrivalTime,
            Dimensions, FirstName, LastName, Email, PhoneNumber, AdditionalInfo,
        };
    }

    public const int MaxFileSizeBytesHardCap = 25 * 1024 * 1024;
}
