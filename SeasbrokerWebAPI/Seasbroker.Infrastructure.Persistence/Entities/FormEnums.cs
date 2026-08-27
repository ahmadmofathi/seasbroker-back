namespace Seasbroker.Infrastructure.Persistence.Entities;

public static class FormVersionStatus
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Archived = "Archived";
}

public static class FormFieldType
{
    public const string Text = "Text";
    public const string Textarea = "Textarea";
    public const string Number = "Number";
    public const string Decimal = "Decimal";
    public const string Date = "Date";
    public const string DateTime = "DateTime";
    public const string Time = "Time";
    public const string Email = "Email";
    public const string Phone = "Phone";
    public const string Select = "Select";
    public const string MultiSelect = "MultiSelect";
    public const string Radio = "Radio";
    public const string Checkbox = "Checkbox";
    public const string Toggle = "Toggle";
    public const string File = "File";
    public const string MultiFile = "MultiFile";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Text, Textarea, Number, Decimal, Date, DateTime, Time, Email, Phone,
        Select, MultiSelect, Radio, Checkbox, Toggle, File, MultiFile,
    };

    public static readonly IReadOnlySet<string> OptionBased = new HashSet<string>
    {
        Select, MultiSelect, Radio,
    };

    public static readonly IReadOnlySet<string> FileBased = new HashSet<string>
    {
        File, MultiFile,
    };
}

public static class FormFieldWidth
{
    public const string Full = "Full";
    public const string Half = "Half";
    public const string Third = "Third";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Full, Half, Third };
}

public static class FormConditionOperator
{
    public const string EqualsOp = "Equals";
    public const string NotEquals = "NotEquals";
    public const string Contains = "Contains";
    public const string GreaterThan = "GreaterThan";
    public const string GreaterThanOrEqual = "GreaterThanOrEqual";
    public const string LessThan = "LessThan";
    public const string LessThanOrEqual = "LessThanOrEqual";
    public const string IsEmpty = "IsEmpty";
    public const string IsNotEmpty = "IsNotEmpty";
    public const string In = "In";
    public const string NotIn = "NotIn";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        EqualsOp, NotEquals, Contains, GreaterThan, GreaterThanOrEqual,
        LessThan, LessThanOrEqual, IsEmpty, IsNotEmpty, In, NotIn,
    };
}

public static class FormConditionCombinator
{
    public const string And = "AND";
    public const string Or = "OR";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { And, Or };
}
