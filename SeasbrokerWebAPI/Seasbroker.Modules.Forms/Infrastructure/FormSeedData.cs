using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Infrastructure;

/// <summary>
/// Initial schemas reproducing the 3 hard-coded public forms, so admins never have to
/// manually recreate the existing fields. Where the spec asks for behaviour the old hard-coded
/// forms never had (e.g. Customs Clearance's Import/Export split), that field is added here.
/// Coverage is representative, not an exhaustive port of every cargo-type/vessel-type branch in
/// the old ~1000-line components - the whole point of this feature is an admin can add the rest
/// through the builder UI, without a developer, so remaining blocks aren't hand-ported.
/// </summary>
public static class FormSeedData
{
    public static (string Key, string Name, string? Description, FormSchemaDto Schema) RequestQuote() => (
        FormsConstants.FormKeys.RequestQuote,
        "Request Quote / Register Cargo",
        "Public cargo-brokerage quote request form.",
        new FormSchemaDto
        {
            Sections =
            {
                Section("cargo-details", "Cargo Details", 0,
                    Select("cargoType", "Cargo Type", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.CargoType,
                        options: CargoTypeOptions),
                    Text("commodity", "Commodity", 1, placeholder: "e.g. Steel coils, Machinery parts"),
                    Number("weight", "Weight (MT)", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Half,
                        validation: new FormFieldValidationDto { Min = 0 }),
                    Text("dimensions", "Dimensions", 3, systemKey: FormsConstants.SystemFieldKeys.Dimensions, width: FormFieldWidth.Half,
                        placeholder: "L x W x H"),
                    Select("dangerousGoods", "Dangerous Goods", 4, required: true, options: YesNoOptions, width: FormFieldWidth.Half)),

                Section("shipment-route", "Shipment Route", 1,
                    Text("departurePort", "Departure Port", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.DeparturePort, width: FormFieldWidth.Half),
                    Date("cargoReadyDate", "Cargo Ready Date", 1, systemKey: FormsConstants.SystemFieldKeys.DepartureTime, width: FormFieldWidth.Half),
                    Text("arrivalPort", "Arrival Port", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalPort, width: FormFieldWidth.Half)),

                Section("project-heavy-lift", "Project & Heavy-Lift Details", 2,
                    ConditionalOnCargoType(Number("unitsCount", "Number of Units", 0, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType(Number("totalWeight", "Total Weight (MT)", 1, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType(Number("heaviestUnitWeight", "Heaviest Unit Weight (MT)", 2, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType(Number("largestUnitLength", "Largest Unit Length (m)", 3, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType(Number("largestUnitWidth", "Largest Unit Width (m)", 4, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType(Number("largestUnitHeight", "Largest Unit Height (m)", 5, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType(Text("centerOfGravity", "Center of Gravity", 6, width: FormFieldWidth.Half)),
                    ConditionalOnCargoType(Text("liftingPoints", "Lifting Points", 7, width: FormFieldWidth.Half)),
                    ConditionalOnCargoType(Text("liftingCapacity", "Lifting Capacity", 8, width: FormFieldWidth.Half)),
                    ConditionalOnCargoType(File("drawingsPackingList", "Drawings / Packing List", 9))),

                Section("dangerous-goods", "Dangerous Goods Details", 3,
                    ConditionalOnDangerousGoods(Text("unNumber", "UN Number", 0, width: FormFieldWidth.Third)),
                    ConditionalOnDangerousGoods(Text("properShippingName", "Proper Shipping Name", 1, width: FormFieldWidth.Third)),
                    ConditionalOnDangerousGoods(Text("imoClass", "IMO Class", 2, width: FormFieldWidth.Third)),
                    ConditionalOnDangerousGoods(Text("packingGroup", "Packing Group", 3, width: FormFieldWidth.Third)),
                    ConditionalOnDangerousGoods(Text("flashPoint", "Flash Point", 4, width: FormFieldWidth.Third)),
                    ConditionalOnDangerousGoods(Toggle("marinePollutant", "Marine Pollutant", 5, width: FormFieldWidth.Third)),
                    ConditionalOnDangerousGoods(File("sdsFile", "SDS / MSDS", 6))),

                Section("additional-information", "Additional Information", 4,
                    Textarea("remarks", "Remarks", 0, systemKey: FormsConstants.SystemFieldKeys.AdditionalInfo)),

                ContactSection(5),
            },
        });

    public static (string Key, string Name, string? Description, FormSchemaDto Schema) RequestRoute() => (
        FormsConstants.FormKeys.RequestRoute,
        "Request Route / Ship Brokerage",
        "Public ship-brokerage / vessel availability request form.",
        new FormSchemaDto
        {
            Sections =
            {
                Section("vessel-details", "Vessel Details", 0,
                    Select("vesselType", "Vessel Type", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.CargoType,
                        options: VesselTypeOptions),
                    Number("dwt", "DWT (MT)", 1, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Half,
                        validation: new FormFieldValidationDto { Min = 0 }),
                    Text("vesselDimensions", "Vessel Dimensions (LOA x Beam x Draft)", 2, systemKey: FormsConstants.SystemFieldKeys.Dimensions, width: FormFieldWidth.Half)),

                Section("availability", "Availability", 1,
                    Text("openPort", "Open Port", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.DeparturePort, width: FormFieldWidth.Half),
                    Date("availableFrom", "Available From", 1, systemKey: FormsConstants.SystemFieldKeys.DepartureTime, width: FormFieldWidth.Half),
                    Text("preferredRoute", "Preferred Route / Destination", 2, systemKey: FormsConstants.SystemFieldKeys.ArrivalPort)),

                Section("bulk-carrier-specs", "Bulk Carrier Specifications", 2,
                    ConditionalOnVesselType("Bulk Carrier", Text("holdConfiguration", "Hold / Hatch Configuration", 0, width: FormFieldWidth.Half)),
                    ConditionalOnVesselType("Bulk Carrier", Toggle("selfGeared", "Self-Geared", 1, width: FormFieldWidth.Half)),
                    ConditionalOnVesselType("Bulk Carrier", Text("lastThreeCargoes", "Last 3 Cargoes Carried", 2))),

                Section("tanker-specs", "Tanker Specifications", 3,
                    ConditionalOnVesselType("Tanker", Text("tankCoating", "Tank Coating", 0, width: FormFieldWidth.Half)),
                    ConditionalOnVesselType("Tanker", Text("segregations", "Number of Segregations", 1, width: FormFieldWidth.Half)),
                    ConditionalOnVesselType("Tanker", Text("cargoImoClass", "Cargo IMO Class", 2))),

                Section("container-specs", "Container Ship Specifications", 4,
                    ConditionalOnVesselType("Container Ship", Number("teuCapacity", "TEU Capacity", 0, width: FormFieldWidth.Half)),
                    ConditionalOnVesselType("Container Ship", Number("reeferPlugs", "Reefer Plugs", 1, width: FormFieldWidth.Half))),

                Section("additional-information", "Additional Information", 5,
                    Textarea("remarks", "Remarks", 0, systemKey: FormsConstants.SystemFieldKeys.AdditionalInfo)),

                ContactSection(6, firstNameLabel: "Contact Person", lastNameLabel: "Company Name"),
            },
        });

    public static (string Key, string Name, string? Description, FormSchemaDto Schema) RequestClearance() => (
        FormsConstants.FormKeys.RequestClearance,
        "Request Customs Clearance",
        "Public customs clearance request form.",
        new FormSchemaDto
        {
            Sections =
            {
                Section("clearance-details", "Clearance Details", 0,
                    Select("clearanceType", "Clearance Type", 0, required: true, systemKey: "ClearanceType",
                        options: new[] { ("Import", "Import"), ("Export", "Export"), ("Transit", "Transit") }, width: FormFieldWidth.Half),
                    Select("cargoType", "Cargo Type", 1, required: true, systemKey: FormsConstants.SystemFieldKeys.CargoType,
                        options: CargoTypeOptions, width: FormFieldWidth.Half),
                    Text("commodityDescription", "Commodity Description", 2),
                    Number("weight", "Weight (MT)", 3, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third,
                        validation: new FormFieldValidationDto { Min = 0 }),
                    Text("dimensions", "Dimensions", 4, systemKey: FormsConstants.SystemFieldKeys.Dimensions, width: FormFieldWidth.Third),
                    Number("commercialValue", "Commercial Invoice Value (USD)", 5, width: FormFieldWidth.Third,
                        validation: new FormFieldValidationDto { Min = 0 }),
                    Select("dangerousGoods", "Dangerous Goods", 6, required: true, options: YesNoOptions, width: FormFieldWidth.Half)),

                Section("shipment-route", "Shipment Route", 1,
                    Text("portOfOrigin", "Port of Origin", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.DeparturePort, width: FormFieldWidth.Half),
                    Text("portOfDestination", "Port of Destination", 1, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalPort, width: FormFieldWidth.Half),
                    Date("eta", "ETA", 2, systemKey: FormsConstants.SystemFieldKeys.ArrivalTime, width: FormFieldWidth.Half)),

                Section("import-details", "Import Details", 2,
                    ConditionalOnClearanceType("Import", Text("importerOfRecord", "Importer of Record", 0, width: FormFieldWidth.Half)),
                    ConditionalOnClearanceType("Import", Text("importLicenseNumber", "Import License Number", 1, width: FormFieldWidth.Half))),

                Section("export-details", "Export Details", 3,
                    ConditionalOnClearanceType("Export", Text("exporterOfRecord", "Exporter of Record", 0, width: FormFieldWidth.Half)),
                    ConditionalOnClearanceType("Export", Text("exportLicenseNumber", "Export License Number", 1, width: FormFieldWidth.Half))),

                Section("dangerous-goods", "Dangerous Goods Details", 4,
                    ConditionalOnDangerousGoods(Text("unNumber", "UN Number", 0, width: FormFieldWidth.Half)),
                    ConditionalOnDangerousGoods(Text("imoClass", "IMO Class", 1, width: FormFieldWidth.Half))),

                Section("documents", "Documents", 5,
                    File("commercialInvoice", "Commercial Invoice", 0, width: FormFieldWidth.Third),
                    File("packingList", "Packing List", 1, width: FormFieldWidth.Third),
                    File("billOfLading", "Bill of Lading / AWB", 2, width: FormFieldWidth.Third)),

                Section("additional-information", "Additional Information", 6,
                    Textarea("remarks", "Remarks", 0, systemKey: FormsConstants.SystemFieldKeys.AdditionalInfo)),

                ContactSection(7),
            },
        });

    // ---- shared field option lists ----

    private static readonly (string Value, string Label)[] CargoTypeOptions =
    {
        ("Dry Bulk", "Dry Bulk"),
        ("General & Breakbulk Cargo", "General & Breakbulk Cargo"),
        ("Project & Heavy-Lift Cargo", "Project & Heavy-Lift Cargo"),
        ("Containerized Cargo", "Containerized Cargo"),
        ("RoRo", "RoRo (Roll-on/Roll-off)"),
        ("Liquid Bulk", "Liquid Bulk"),
        ("Gas", "Gas (LNG/LPG)"),
        ("Refrigerated & Perishable Cargo", "Refrigerated & Perishable Cargo"),
        ("Other", "Other"),
    };

    private static readonly (string Value, string Label)[] VesselTypeOptions =
    {
        ("Bulk Carrier", "Bulk Carrier"),
        ("Tanker", "Tanker"),
        ("Container Ship", "Container Ship"),
        ("RoRo / PCTC", "RoRo / PCTC"),
        ("General Cargo / MPP", "General Cargo / MPP"),
        ("Gas Carrier", "Gas Carrier"),
        ("Other", "Other"),
    };

    private static readonly (string Value, string Label)[] YesNoOptions =
    {
        ("Yes", "Yes"),
        ("No", "No"),
    };

    // ---- small builder helpers to keep the schemas above readable ----

    private static FormSectionDto Section(string key, string label, int order, params FormFieldDto[] fields) =>
        new()
        {
            Key = key,
            Label = label,
            Order = order,
            Visible = true,
            Fields = fields.ToList(),
        };

    private static FormSectionDto ContactSection(int order, string firstNameLabel = "First Name", string lastNameLabel = "Last Name") =>
        Section("contact-information", "Contact Information", order,
            Field("firstName", firstNameLabel, FormFieldType.Text, 0, required: true, systemKey: FormsConstants.SystemFieldKeys.FirstName, width: FormFieldWidth.Half),
            Field("lastName", lastNameLabel, FormFieldType.Text, 1, required: true, systemKey: FormsConstants.SystemFieldKeys.LastName, width: FormFieldWidth.Half),
            Field("email", "Email", FormFieldType.Email, 2, required: true, systemKey: FormsConstants.SystemFieldKeys.Email, width: FormFieldWidth.Half),
            Field("phoneNumber", "Phone Number", FormFieldType.Phone, 3, required: true, systemKey: FormsConstants.SystemFieldKeys.PhoneNumber, width: FormFieldWidth.Half));

    private static FormFieldDto Field(
        string key, string label, string type, int order,
        bool required = false, string? systemKey = null, string width = "Full",
        string? placeholder = null, string? help = null,
        (string Value, string Label)[]? options = null,
        FormFieldValidationDto? validation = null) =>
        new()
        {
            Key = key,
            Label = label,
            Type = type,
            Order = order,
            Required = required,
            Visible = true,
            Width = width,
            Placeholder = placeholder,
            HelpText = help,
            IsSystemField = systemKey is not null,
            SystemFieldKey = systemKey,
            Validation = validation,
            Options = (options ?? Array.Empty<(string, string)>())
                .Select((o, i) => new FormFieldOptionDto { Value = o.Value, Label = o.Label, Order = i })
                .ToList(),
        };

    private static FormFieldDto Text(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", string? placeholder = null) =>
        Field(key, label, FormFieldType.Text, order, required, systemKey, width, placeholder);

    private static FormFieldDto Textarea(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full") =>
        Field(key, label, FormFieldType.Textarea, order, required, systemKey, width);

    private static FormFieldDto Number(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", FormFieldValidationDto? validation = null) =>
        Field(key, label, FormFieldType.Number, order, required, systemKey, width, validation: validation);

    private static FormFieldDto Date(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full") =>
        Field(key, label, FormFieldType.Date, order, required, systemKey, width);

    private static FormFieldDto Select(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", (string Value, string Label)[]? options = null) =>
        Field(key, label, FormFieldType.Select, order, required, systemKey, width, options: options);

    private static FormFieldDto Toggle(string key, string label, int order, bool required = false, string width = "Full") =>
        Field(key, label, FormFieldType.Toggle, order, required, null, width);

    private static FormFieldDto File(string key, string label, int order, bool required = false, string width = "Full") =>
        Field(key, label, FormFieldType.File, order, required, null, width);

    private static FormFieldDto ConditionalOnCargoType(FormFieldDto field) =>
        WithCondition(field, "cargoType", FormConditionOperator.EqualsOp, "Project & Heavy-Lift Cargo");

    private static FormFieldDto ConditionalOnDangerousGoods(FormFieldDto field) =>
        WithCondition(field, "dangerousGoods", FormConditionOperator.EqualsOp, "Yes");

    private static FormFieldDto ConditionalOnVesselType(string vesselType, FormFieldDto field) =>
        WithCondition(field, "vesselType", FormConditionOperator.EqualsOp, vesselType);

    private static FormFieldDto ConditionalOnClearanceType(string clearanceType, FormFieldDto field) =>
        WithCondition(field, "clearanceType", FormConditionOperator.EqualsOp, clearanceType);

    private static FormFieldDto WithCondition(FormFieldDto field, string sourceKey, string op, string value)
    {
        field.ConditionCombinator = FormConditionCombinator.And;
        field.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = sourceKey, Operator = op, Value = value });
        return field;
    }
}
