using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Infrastructure;

/// <summary>
/// Initial schemas reproducing the 3 public forms exactly as specified (all cargo-type /
/// vessel-type / clearance-type conditional branches included), so the Dynamic Forms feature
/// doesn't regress functionality versus the hand-coded components it replaces. Admins can still
/// edit/extend these further through the builder UI.
///
/// Two fields the old hard-coded forms derived automatically (e.g. "arrival = ready date + 7
/// days") can't be computed here - there's no formula-field support - so each form instead asks
/// for that date explicitly. DepartureTime/ArrivalTime must always resolve to a real, always-
/// visible, required field: the backend rejects a submission with an empty/unparseable date.
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
                    MultiSelect("cargoFeatures", "Cargo Features", 1, options: new[]
                    {
                        ("Dangerous Goods", "Dangerous Goods"),
                        ("Fragile", "Fragile"),
                        ("Oversized / OOG", "Oversized / OOG"),
                        ("Temperature Controlled", "Temperature Controlled"),
                    })),

                Section("shipment-route", "Shipment Route", 1,
                    Port("departurePort", "Departure / Loading Port", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.DeparturePort, width: FormFieldWidth.Half),
                    Port("arrivalPort", "Arrival / Discharge Port", 1, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalPort, width: FormFieldWidth.Half),
                    Date("cargoReadyDate", "Cargo Ready Date", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.DepartureTime, width: FormFieldWidth.Half),
                    Date("estimatedArrivalDate", "Estimated Arrival Date", 3, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalTime, width: FormFieldWidth.Half),
                    Select("dangerousGoods", "Dangerous Goods?", 4, required: true, options: YesNoOptions, width: FormFieldWidth.Half),
                    Textarea("remarks", "Remarks / Special Requirements", 5, systemKey: FormsConstants.SystemFieldKeys.AdditionalInfo),
                    File("documentsFile", "Documents / Photos", 6)),

                Section("dangerous-goods", "Dangerous Goods Details", 2,
                    ConditionalOn("dangerousGoods", "Yes", Text("unNumber", "UN Number", 0, required: true, width: FormFieldWidth.Third, placeholder: "1234",
                        validation: new FormFieldValidationDto { Pattern = "^UN \\d{4}$", MaxLength = 4, DigitsOnly = true, FixedPrefix = "UN" })),
                    ConditionalOn("dangerousGoods", "Yes", Text("properShippingName", "Proper Shipping Name", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("dangerousGoods", "Yes", Text("imoClass", "IMO/IMDG Class", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("dangerousGoods", "Yes", Select("packingGroup", "Packing Group", 3, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("I", "I"), ("II", "II"), ("III", "III"), ("N/A", "N/A") })),
                    ConditionalOn("dangerousGoods", "Yes", Number("flashPointDg", "Flash Point (°C)", 4, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOn("dangerousGoods", "Yes", Select("marinePollutant", "Marine Pollutant", 5, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOn("dangerousGoods", "Yes", File("sdsFile", "SDS/MSDS", 6))),

                Section("dry-bulk", "Dry Bulk", 3,
                    ConditionalOnCargoType("Dry Bulk", Text("dryBulkCommodity", "Commodity", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkQuantity", "Quantity (MT)", 1, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkStowageFactor", "Stowage Factor (m³/MT)", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkLoadingRate", "Loading Rate (MT/day)", 3, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkDischargeRate", "Discharge Rate (MT/day)", 4, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Select("dryBulkTerms", "Loading/Discharge Terms", 5, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("FIO", "FIO"), ("FIOS", "FIOS"), ("FIOST", "FIOST"), ("Liner Terms", "Liner Terms"), ("Other", "Other") })),
                    ConditionalOn("dryBulkTerms", "Other", Text("dryBulkTermsOther", "Specify Terms", 6, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkTolerance", "Quantity Tolerance (%)", 7, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkMaxDraft", "Maximum Draft (m)", 8, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Number("dryBulkMoisture", "Moisture Content (%)", 9, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Dry Bulk", Select("dryBulkVesselGear", "Vessel Gear Required", 10, options: YesNoOptions, width: FormFieldWidth.Third))),

                Section("breakbulk", "General & Breakbulk Cargo", 4,
                    ConditionalOnCargoType("General & Breakbulk Cargo", Text("bbDescription", "Cargo Description", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbUnits", "Number of Units / Packages (pcs)", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Select("bbPackingType", "Packing Type", 2, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("Bags", "Bags"), ("Pallets", "Pallets"), ("Bundles", "Bundles"), ("Crates", "Crates"), ("Loose", "Loose"), ("Other", "Other") })),
                    ConditionalOn("bbPackingType", "Other", Text("bbPackingTypeOther", "Specify Packing Type", 3, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbWeight", "Total Gross Weight (MT)", 4, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbVolume", "Total Volume (m³)", 5, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbLength", "Largest Unit Length (m)", 6, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbWidth", "Largest Unit Width (m)", 7, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbHeight", "Largest Unit Height (m)", 8, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Number("bbHeaviestWeight", "Heaviest Unit Weight (MT)", 9, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Select("bbStackable", "Stackable?", 10, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("General & Breakbulk Cargo", Select("bbGearRequired", "Ship's Gear Required?", 11, options: YesNoOptions, width: FormFieldWidth.Third))),

                Section("project-heavy-lift", "Project & Heavy-Lift Cargo", 5,
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Text("projDescription", "Cargo / Project Description", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projUnits", "Number of Units (pcs)", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projWeight", "Total Weight (MT)", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projHeaviestWeight", "Heaviest Unit Weight (MT)", 3, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projLength", "Largest Unit Length (m)", 4, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projWidth", "Largest Unit Width (m)", 5, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projHeight", "Largest Unit Height (m)", 6, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Select("projCoGAvailable", "Center of Gravity Available?", 7, required: true, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Select("projLiftingPointsAvailable", "Lifting Points Available?", 8, required: true, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", File("projDrawingsFile", "Drawings / Packing List", 9, required: true)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projVolume", "Total Volume (m³)", 10, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projLiftingPointCapacity", "Lifting Point Capacity (MT)", 11, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projCoG", "Center of Gravity (m)", 12, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Number("projRequiredLiftingCapacity", "Required Lifting Capacity (MT)", 13, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Project & Heavy-Lift Cargo", Select("projSelfPropelled", "Self-propelled?", 14, options: YesNoOptions, width: FormFieldWidth.Third))),

                Section("containerized", "Containerized Cargo", 6,
                    ConditionalOnCargoType("Containerized Cargo", Text("containerCommodity", "Commodity", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Containerized Cargo", Select("containerShipmentType", "Shipment Type", 1, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("FCL", "FCL"), ("LCL", "LCL") })),
                    ConditionalOnCargoType("Containerized Cargo", Select("containerType", "Container Type", 2, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("20'GP", "20'GP"), ("40'GP", "40'GP"), ("40'HC", "40'HC"), ("20'OT", "20'OT"), ("40'OT", "40'OT"), ("20'FR", "20'FR"), ("40'FR", "40'FR") })),
                    ConditionalOnCargoType("Containerized Cargo", Number("containerNumber", "Number of Containers", 3, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Containerized Cargo", Select("containerNumberUnit", "Container Count Unit", 4, width: FormFieldWidth.Third,
                        options: new[] { ("TEU", "TEU"), ("FEU", "FEU") })),
                    ConditionalOnCargoType("Containerized Cargo", Number("containerWeight", "Gross Weight per Container (MT)", 5, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnAll(new[] { ("cargoType", "Containerized Cargo"), ("containerShipmentType", "LCL") },
                        Number("lclPackages", "Number of Packages (pcs)", 6, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnAll(new[] { ("cargoType", "Containerized Cargo"), ("containerShipmentType", "LCL") },
                        Number("lclWeight", "Total Weight (kg)", 7, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnAll(new[] { ("cargoType", "Containerized Cargo"), ("containerShipmentType", "LCL") },
                        Number("lclVolume", "Total Volume (m³)", 8, required: true, width: FormFieldWidth.Third))),

                Section("roro", "RoRo Cargo", 7,
                    ConditionalOnCargoType("RoRo", Text("roroVehicleType", "Vehicle / Equipment Type", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Text("roroMakeModel", "Make / Model", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Number("roroQuantity", "Quantity (units)", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Number("roroLength", "Length per Unit (m)", 3, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Number("roroWidth", "Width per Unit (m)", 4, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Number("roroHeight", "Height per Unit (m)", 5, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Number("roroWeight", "Weight per Unit (MT)", 6, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Select("roroOperational", "Operational / Running?", 7, required: true, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("RoRo", Select("roroSelfPropelled", "Self-Propelled?", 8, required: true, options: YesNoOptions, width: FormFieldWidth.Third))),

                Section("liquid-bulk", "Liquid Bulk", 8,
                    ConditionalOnCargoType("Liquid Bulk", Text("liquidProductName", "Product Name", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidQuantity", "Quantity (MT)", 1, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Text("liquidDensity", "Density (kg/m³ @ °C)", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidLoadingTemp", "Loading Temperature (°C)", 3, required: true, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidGrades", "Number of Grades", 4, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Select("liquidHeatingRequired", "Heating Required?", 5, required: true, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidLoadingRate", "Loading Rate (m³/h)", 6, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidDischargeRate", "Discharge Rate (m³/h)", 7, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidFlashPoint", "Flash Point (°C)", 8, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Liquid Bulk", Text("liquidViscosity", "Viscosity (cSt @ °C)", 9, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Liquid Bulk", Number("liquidCarriageTemp", "Required Carriage Temperature (°C)", 10, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Liquid Bulk", File("liquidSdsFile", "SDS", 11))),

                Section("gas-cargo", "Gas Cargo", 9,
                    ConditionalOnCargoType("Gas", Select("gasCargoType", "Gas Cargo Type", 0, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("LNG", "LNG"), ("LPG", "LPG"), ("Ethylene", "Ethylene"), ("Ammonia", "Ammonia"), ("Other Liquefied Gas", "Other Liquefied Gas"), ("Compressed Gas", "Compressed Gas") })),
                    ConditionalOnCargoType("Gas", Text("gasProduct", "Product", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Gas", Number("gasQuantity", "Quantity", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Gas", Select("gasQuantityUnit", "Quantity Unit", 3, width: FormFieldWidth.Third,
                        options: new[] { ("MT", "MT"), ("m³", "m³") })),
                    ConditionalOnCargoType("Gas", Number("gasDensity", "Density (kg/m³)", 4, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Gas", Number("gasLoadingTemp", "Loading Temperature (°C)", 5, required: true, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Gas", Number("gasLoadingPressure", "Loading Pressure (bar)", 6, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Gas", Number("gasLoadingRate", "Loading Rate (m³/h)", 7, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Gas", Number("gasDischargeRate", "Discharge Rate (m³/h)", 8, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Gas", File("gasSpecFile", "Cargo Specification", 9))),

                Section("refrigerated", "Refrigerated & Perishable Cargo", 10,
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Select("reeferTransportMethod", "Transport Method", 0, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("Reefer Container", "Reefer Container"), ("Refrigerated Vessel", "Refrigerated Vessel"), ("Other", "Other") })),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Text("reeferCommodity", "Commodity", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Number("reeferQuantity", "Quantity (MT)", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Select("reeferPackingType", "Packing Type", 3, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("Cartons", "Cartons"), ("Pallets", "Pallets"), ("Bags", "Bags"), ("Bulk", "Bulk"), ("Other", "Other") })),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Number("reeferSetTemp", "Required Set Temperature (°C)", 4, required: true, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Number("reeferMinTemp", "Minimum Temperature (°C)", 5, required: true, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Number("reeferMaxTemp", "Maximum Temperature (°C)", 6, required: true, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Number("reeferVentilation", "Ventilation (m³/h)", 7, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Number("reeferHumidity", "Relative Humidity (% RH)", 8, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Refrigerated & Perishable Cargo", Select("reeferControlledAtmosphere", "Controlled Atmosphere", 9, options: YesNoOptions, width: FormFieldWidth.Third)),
                    ConditionalOnAll(new[] { ("cargoType", "Refrigerated & Perishable Cargo"), ("reeferTransportMethod", "Reefer Container") },
                        Select("reeferContainerType", "Reefer Type", 10, required: true, width: FormFieldWidth.Third,
                            options: new[] { ("20'RF", "20'RF"), ("40'RF", "40'RF"), ("40'HC RF", "40'HC RF") })),
                    ConditionalOnAll(new[] { ("cargoType", "Refrigerated & Perishable Cargo"), ("reeferTransportMethod", "Reefer Container") },
                        Number("reeferContainerCount", "Number of Containers (units)", 11, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnAll(new[] { ("cargoType", "Refrigerated & Perishable Cargo"), ("reeferTransportMethod", "Reefer Container") },
                        Number("reeferContainerWeight", "Gross Weight / Container (MT)", 12, required: true, width: FormFieldWidth.Third))),

                Section("other-cargo", "Other / Not Sure", 11,
                    ConditionalOnCargoType("Other", Text("otherDescription", "Cargo Description", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Other", Number("otherQuantity", "Quantity (pcs)", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Other", Number("otherWeight", "Total Weight (MT)", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Other", Number("otherVolume", "Total Volume (m³)", 3, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Other", Text("otherDimensions", "Largest Dimensions (m)", 4, width: FormFieldWidth.Third)),
                    ConditionalOnCargoType("Other", File("otherDocFile", "Documents", 5)),
                    ConditionalOnCargoType("Other", Textarea("otherSpecialRequirements", "Special Requirements", 6))),

                ContactSection(12),
            },
        });

    public static (string Key, string Name, string? Description, FormSchemaDto Schema) RequestRoute() => (
        FormsConstants.FormKeys.RequestRoute,
        "Register Ship Brokerage",
        "Public ship-brokerage / vessel availability request form.",
        new FormSchemaDto
        {
            Sections =
            {
                Section("vessel-details", "Vessel Details", 0,
                    Text("vesselName", "Vessel Name", 0, required: true, width: FormFieldWidth.Half, placeholder: "e.g. Ocean Pioneer"),
                    Text("imoNumber", "IMO Number", 1, required: true, width: FormFieldWidth.Half, placeholder: "7-digit number",
                        validation: new FormFieldValidationDto { Pattern = "^\\d{7}$", MaxLength = 7, DigitsOnly = true }),
                    Select("vesselType", "Vessel Type", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.CargoType,
                        options: VesselTypeOptions)),

                Section("bulk-carrier-specs", "Bulk Carrier Specifications", 1,
                    ConditionalOnVesselType("Bulk Carrier", Number("bulkGrain", "Grain Capacity (m³)", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnVesselType("Bulk Carrier", Number("bulkHolds", "Number of Holds / Hatches", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnVesselType("Bulk Carrier", Number("bulkCrane", "Crane Capacity (MT)", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnVesselType("Bulk Carrier", Checkbox("bulkGeared", "Geared", 3, width: FormFieldWidth.Half)),
                    ConditionalOnVesselType("Bulk Carrier", Checkbox("bulkGearless", "Gearless", 4, width: FormFieldWidth.Half))),

                Section("tanker-specs", "Tanker Specifications", 2,
                    ConditionalOnIn("vesselType", TankerTypes, Number("tankCargoCapacity", "Cargo Capacity (m³)", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnIn("vesselType", TankerTypes, Number("tankCount", "Number of Tanks", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnIn("vesselType", TankerTypes, Text("tankImoClass", "IMO Type / Class", 2, width: FormFieldWidth.Third)),
                    ConditionalOnIn("vesselType", TankerTypes, Checkbox("tankHeating", "Heating", 3, width: FormFieldWidth.Half)),
                    ConditionalOnIn("vesselType", TankerTypes, Checkbox("tankCoated", "Coated Tanks", 4, width: FormFieldWidth.Half)),
                    ConditionalOnIn("vesselType", TankerTypes, Number("tankPumpingRate", "Pumping Rate (m³/h)", 5, width: FormFieldWidth.Third))),

                Section("container-ship-specs", "Container Ship Specifications", 3,
                    ConditionalOn("vesselType", "Container Ship", Number("contCapacityTeu", "Capacity (TEU)", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "Container Ship", Checkbox("contReeferPlugs", "Reefer Plugs", 1, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "Container Ship", Number("contMaxDraft", "Max Draft (m)", 2, required: true, width: FormFieldWidth.Third))),

                Section("roro-pctc-specs", "RoRo / PCTC Specifications", 4,
                    ConditionalOn("vesselType", "RoRo or PCTC", Number("roroLaneMetres", "Lane Metres (m)", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "RoRo or PCTC", Number("roroVehicleCapacity", "Vehicle Capacity", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "RoRo or PCTC", Number("roroRampSwl", "Ramp SWL (MT)", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "RoRo or PCTC", Text("roroRampDimensions", "Ramp Dimensions (m)", 3, width: FormFieldWidth.Third))),

                Section("heavy-lift-mpp-specs", "Heavy Lift / MPP Specifications", 5,
                    ConditionalOn("vesselType", "General Cargo or Multipurpose", Number("mppDeckArea", "Deck Area (m²)", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "General Cargo or Multipurpose", Number("mppDeckStrength", "Deck Strength (MT/m²)", 1, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "General Cargo or Multipurpose", Number("mppCraneCapacity", "Crane Capacity (MT)", 2, required: true, width: FormFieldWidth.Third)),
                    ConditionalOn("vesselType", "General Cargo or Multipurpose", Number("mppCombinedLiftCapacity", "Combined Lift Capacity (MT)", 3, width: FormFieldWidth.Third))),

                Section("gas-carrier-specs", "LNG / LPG Specifications", 6,
                    ConditionalOnIn("vesselType", GasCarrierTypes, Number("gasCargoCapacity", "Cargo Capacity (m³)", 0, required: true, width: FormFieldWidth.Third)),
                    ConditionalOnIn("vesselType", GasCarrierTypes, Text("gasCargoTypeField", "Cargo Type", 1, width: FormFieldWidth.Third)),
                    ConditionalOnIn("vesselType", GasCarrierTypes, Text("gasTankType", "Tank Type", 2, width: FormFieldWidth.Third)),
                    ConditionalOnIn("vesselType", GasCarrierTypes, Number("gasMinCargoTemp", "Minimum Cargo Temperature (°C)", 3, width: FormFieldWidth.Third, validation: AllowNegative)),
                    ConditionalOnIn("vesselType", GasCarrierTypes, Number("gasMaxWorkingPressure", "Max Working Pressure (bar)", 4, width: FormFieldWidth.Third))),

                Section("general-particulars", "General Particulars", 7,
                    Number("dwt", "DWT (MT)", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.Weight, width: FormFieldWidth.Third),
                    Text("yearBuilt", "Year Built", 1, required: true, width: FormFieldWidth.Third, placeholder: "YYYY",
                        validation: new FormFieldValidationDto { Pattern = "^\\d{4}$", MaxLength = 4, DigitsOnly = true, NoFutureYear = true }),
                    Select("flag", "Flag", 2, required: true, width: FormFieldWidth.Third, options: CountryOptions),
                    Port("currentOpenPort", "Current / Open Port", 3, required: true, systemKey: FormsConstants.SystemFieldKeys.DeparturePort, width: FormFieldWidth.Half),
                    Date("openDateFrom", "Open Date From", 4, required: true, systemKey: FormsConstants.SystemFieldKeys.DepartureTime, width: FormFieldWidth.Third,
                        validation: NoPastDates),
                    Date("openDateTo", "Open Date To", 5, width: FormFieldWidth.Third, validation: NoPastDates),
                    Number("draft", "Draft (m)", 6, width: FormFieldWidth.Third),
                    Number("loa", "LOA (m)", 7, width: FormFieldWidth.Third),
                    Number("beam", "Beam (m)", 8, width: FormFieldWidth.Third),
                    Select("gear", "Gear", 9, width: FormFieldWidth.Half, options: new[] { ("Geared", "Geared"), ("Gearless", "Gearless") }),
                    File("vesselParticularsFile", "Vessel Particulars", 10, width: FormFieldWidth.Half)),

                Section("cargo-employment-preference", "Cargo / Employment Preference", 8,
                    Select("prefType", "Preference Type", 0, width: FormFieldWidth.Half,
                        options: new[] { ("Heavy Lift or Project Cargo Vessel", "Heavy Lift or Project Cargo Vessel"), ("Reefer Vessel", "Reefer Vessel"), ("Other", "Other") }),
                    ConditionalOn("prefType", "Other", Text("prefTypeOther", "Specify Preference", 1, required: true, width: FormFieldWidth.Half)),
                    MultiSelect("cargoTypesAccepted", "Cargo Types Accepted", 2, options: new[]
                    {
                        ("Dry Bulk", "Dry Bulk"),
                        ("General & Breakbulk", "General & Breakbulk"),
                        ("Project & Heavy Lift", "Project & Heavy Lift"),
                        ("Containerized", "Containerized"),
                        ("RoRo", "RoRo"),
                        ("Liquid Bulk", "Liquid Bulk"),
                        ("Gas Cargo", "Gas Cargo"),
                        ("Refrigerated & Perishable", "Refrigerated & Perishable"),
                    }),
                    Number("minCargoQty", "Minimum Cargo Quantity (MT)", 3, width: FormFieldWidth.Third),
                    Number("maxCargoQty", "Maximum Cargo Quantity (MT)", 4, width: FormFieldWidth.Third),
                    Select("dgAccepted", "Dangerous Goods Accepted?", 5, options: YesNoOptions, width: FormFieldWidth.Third),
                    Text("preferredCommodity", "Preferred Cargo / Commodity", 6, width: FormFieldWidth.Half)),

                Section("availability", "Availability Details", 9,
                    Select("availabilityType", "Availability Type", 0, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("Open Vessel", "Open Vessel"), ("Scheduled Route", "Scheduled Route") }),
                    ConditionalOn("availabilityType", "Open Vessel", Port("openPort", "Open Port", 1, required: true, width: FormFieldWidth.Half)),
                    ConditionalOn("availabilityType", "Open Vessel", Date("openDate", "Open Date", 2, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalTime, width: FormFieldWidth.Half, validation: NoPastDates)),
                    ConditionalOn("availabilityType", "Open Vessel", Port("preferredTradingArea", "Preferred Trading Area", 3, width: FormFieldWidth.Half)),
                    ConditionalOn("availabilityType", "Open Vessel", Port("preferredDestinationArea", "Preferred Destination Area", 4, width: FormFieldWidth.Half)),
                    ConditionalOn("availabilityType", "Scheduled Route", Port("schedDeparturePort", "Departure Port", 5, required: true, width: FormFieldWidth.Half)),
                    ConditionalOn("availabilityType", "Scheduled Route", Date("schedDepartureDate", "Departure Date", 6, required: true, width: FormFieldWidth.Half, validation: NoPastDates)),
                    ConditionalOn("availabilityType", "Scheduled Route", Port("schedDestinationPort", "Destination Port", 7, required: true, width: FormFieldWidth.Half)),
                    ConditionalOn("availabilityType", "Scheduled Route", Field("schedEta", "ETA", FormFieldType.DateTime, 8, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalTime, width: FormFieldWidth.Half, validation: NoPastDates)),
                    ConditionalOn("availabilityType", "Scheduled Route", Text("transitPorts", "Transit Ports", 9))),

                Section("additional-information", "Additional Information", 10,
                    Textarea("remarks", "Remarks", 0, systemKey: FormsConstants.SystemFieldKeys.AdditionalInfo)),

                ContactSection(11, firstNameLabel: "Company Name", lastNameLabel: "Contact Person",
                    middleFields: new[] { Text("position", "Position", 0, required: true, width: FormFieldWidth.Half) }),
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
                    Select("clearanceType", "Clearance Type", 0, required: true, systemKey: "ClearanceType", width: FormFieldWidth.Half,
                        options: new[] { ("Import", "Import"), ("Export", "Export"), ("Transit", "Transit"), ("Temporary Import-Export", "Temporary Import-Export") }),
                    Select("countryOfClearance", "Country of Clearance", 1, required: true, width: FormFieldWidth.Half, options: CountryOptions),
                    Port("portOfClearance", "Port / Customs Office", 2, required: true, width: FormFieldWidth.Half),
                    Select("countryOfOrigin", "Country of Origin", 3, required: true, width: FormFieldWidth.Half, options: CountryOptions),
                    Select("countryOfExport", "Country of Export", 4, required: true, width: FormFieldWidth.Half, options: CountryOptions),
                    Date("arrivalDate", "Expected Arrival / Departure Date", 5, required: true, systemKey: FormsConstants.SystemFieldKeys.DepartureTime, width: FormFieldWidth.Half, validation: NoPastDates),
                    Date("clearanceCompletionDate", "Expected Clearance Completion Date", 6, required: true, systemKey: FormsConstants.SystemFieldKeys.ArrivalTime, width: FormFieldWidth.Half, validation: NoPastDates)),

                Section("cargo-details", "Cargo Details", 1,
                    Select("cargoType", "Cargo Type", 0, required: true, systemKey: FormsConstants.SystemFieldKeys.CargoType, width: FormFieldWidth.Half,
                        options: ClearanceCargoTypeOptions),
                    ConditionalOn("cargoType", "Other", Text("cargoTypeOther", "Describe Cargo Type", 1, required: true, width: FormFieldWidth.Half)),
                    Text("goodsDescription", "Commodity / Goods Description", 2, required: true),
                    Text("hsCode", "HS Code (6–10 digits)", 3, width: FormFieldWidth.Third,
                        validation: new FormFieldValidationDto { Pattern = "^\\d{6,10}$", MaxLength = 10, DigitsOnly = true }),
                    Number("quantity", "Quantity (pcs / packages / units)", 4, required: true, width: FormFieldWidth.Third),
                    Select("packagingType", "Packaging Type", 5, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("Cartons", "Cartons"), ("Pallets", "Pallets"), ("Crates", "Crates"), ("Bags", "Bags"), ("Bulk", "Bulk"), ("Drums", "Drums"), ("IBCs", "IBCs"), ("Containers", "Containers"), ("Breakbulk", "Breakbulk"), ("Other", "Other") }),
                    Number("grossWeight", "Total Gross Weight (kg or MT)", 6, required: true, width: FormFieldWidth.Third),
                    Number("netWeight", "Total Net Weight (kg or MT)", 7, width: FormFieldWidth.Third),
                    Number("volume", "Total Volume (m³)", 8, width: FormFieldWidth.Third),
                    Select("dangerousGoods", "Dangerous Goods?", 9, required: true, options: YesNoOptions, width: FormFieldWidth.Half)),

                Section("dangerous-goods", "Dangerous Goods Details", 2,
                    ConditionalOn("dangerousGoods", "Yes", Text("unNumber", "UN Number", 0, width: FormFieldWidth.Third)),
                    ConditionalOn("dangerousGoods", "Yes", Text("imoClass", "IMO / IMDG Class", 1, width: FormFieldWidth.Third)),
                    ConditionalOn("dangerousGoods", "Yes", Text("properShippingName", "Proper Shipping Name", 2, width: FormFieldWidth.Third)),
                    ConditionalOn("dangerousGoods", "Yes", File("sdsFile", "SDS/MSDS", 3))),

                Section("import-details", "Import Details", 3,
                    ConditionalOn("clearanceType", "Import", Text("importerOfRecord", "Importer of Record", 0, width: FormFieldWidth.Half)),
                    ConditionalOn("clearanceType", "Import", Text("importLicenseNumber", "Import License Number", 1, width: FormFieldWidth.Half))),

                Section("export-details", "Export Details", 4,
                    ConditionalOn("clearanceType", "Export", Text("exporterOfRecord", "Exporter of Record", 0, width: FormFieldWidth.Half)),
                    ConditionalOn("clearanceType", "Export", Text("exportLicenseNumber", "Export License Number", 1, width: FormFieldWidth.Half))),

                Section("container-details", "Container Details", 5,
                    ConditionalOn("cargoType", "Containerized Cargo", Select("containerType", "Container Type", 0, width: FormFieldWidth.Third,
                        options: new[] { ("20'GP", "20'GP"), ("40'GP", "40'GP"), ("40'HC", "40'HC"), ("Reefer", "Reefer"), ("OT (Open Top)", "OT (Open Top)"), ("FR (Flat Rack)", "FR (Flat Rack)"), ("Tank Container", "Tank Container") })),
                    ConditionalOn("cargoType", "Containerized Cargo", Number("containerCount", "Number of Containers", 1, width: FormFieldWidth.Third)),
                    ConditionalOn("cargoType", "Containerized Cargo", Text("blNumberContainer", "Bill of Lading Number", 2, width: FormFieldWidth.Third))),

                Section("vessel-shipping-details", "Vessel / Shipping Details", 6,
                    ConditionalOnIn("cargoType", ClearanceBulkOrRoroTypes, Text("vesselName", "Vessel Name", 0, width: FormFieldWidth.Third)),
                    ConditionalOnIn("cargoType", ClearanceBulkOrRoroTypes, Text("voyageNumber", "Voyage Number", 1, width: FormFieldWidth.Third)),
                    ConditionalOnIn("cargoType", ClearanceBulkOrRoroTypes, Text("blNumberBulk", "Bill of Lading Number", 2, width: FormFieldWidth.Third))),

                Section("commercial-value", "Commercial / Customs Value", 7,
                    Number("invoiceValue", "Invoice Value", 0, required: true, width: FormFieldWidth.Third),
                    Select("currency", "Currency", 1, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("USD", "USD"), ("EUR", "EUR"), ("GBP", "GBP"), ("EGP", "EGP"), ("SAR", "SAR"), ("AED", "AED"), ("CNY", "CNY"), ("JPY", "JPY") }),
                    Select("incoterm", "Incoterm", 2, required: true, width: FormFieldWidth.Third,
                        options: new[] { ("EXW", "EXW"), ("FCA", "FCA"), ("CPT", "CPT"), ("CIP", "CIP"), ("DAP", "DAP"), ("DPU", "DPU"), ("DDP", "DDP"), ("FAS", "FAS"), ("FOB", "FOB"), ("CFR", "CFR"), ("CIF", "CIF") })),

                Section("documents", "Transport Documents", 8,
                    File("commercialInvoice", "Commercial Invoice", 0, width: FormFieldWidth.Third),
                    File("packingList", "Packing List", 1, width: FormFieldWidth.Third),
                    File("billOfLading", "Bill of Lading / Air Waybill / CMR", 2, width: FormFieldWidth.Third),
                    File("certificateOfOrigin", "Certificate of Origin", 3, width: FormFieldWidth.Third),
                    File("importExportLicence", "Import / Export Licence", 4, width: FormFieldWidth.Third),
                    File("sdsMsdsDoc", "SDS / MSDS", 5, width: FormFieldWidth.Third),
                    File("otherSupportingDocs", "Other Supporting Documents", 6, width: FormFieldWidth.Third)),

                ContactSection(9, firstNameLabel: "Company Name", lastNameLabel: "Contact Person",
                    trailingFields: new FormFieldDto[]
                    {
                        Text("taxVatNumber", "Tax / VAT Number", 0, width: FormFieldWidth.Half),
                        Textarea("remarks", "Special Instructions / Remarks", 0, systemKey: FormsConstants.SystemFieldKeys.AdditionalInfo),
                    }),
            },
        });

    // ---- shared field option lists ----

    private static readonly (string Value, string Label)[] CargoTypeOptions =
    {
        ("Dry Bulk", "Dry Bulk"),
        ("General & Breakbulk Cargo", "General & Breakbulk Cargo"),
        ("Project & Heavy-Lift Cargo", "Project & Heavy-Lift Cargo"),
        ("Containerized Cargo", "Containerized Cargo"),
        ("RoRo", "RoRo Cargo"),
        ("Liquid Bulk", "Liquid Bulk"),
        ("Gas", "Gas Cargo"),
        ("Refrigerated & Perishable Cargo", "Refrigerated & Perishable Cargo"),
        ("Other", "Other / Not Sure"),
    };

    private static readonly (string Value, string Label)[] ClearanceCargoTypeOptions =
    {
        ("Containerized Cargo", "Containerized Cargo"),
        ("General or Breakbulk Cargo", "General or Breakbulk Cargo"),
        ("Dry Bulk", "Dry Bulk"),
        ("Liquid Bulk", "Liquid Bulk"),
        ("Gas Cargo", "Gas Cargo"),
        ("RoRo or Vehicles & Equipment", "RoRo or Vehicles & Equipment"),
        ("Project & Heavy-Lift Cargo", "Project & Heavy-Lift Cargo"),
        ("Refrigerated or Perishable Cargo", "Refrigerated or Perishable Cargo"),
        ("Other", "Other"),
    };

    private static readonly string[] ClearanceBulkOrRoroTypes =
    {
        "General or Breakbulk Cargo", "Dry Bulk", "Liquid Bulk", "RoRo or Vehicles & Equipment",
    };

    private static readonly (string Value, string Label)[] VesselTypeOptions =
    {
        ("Bulk Carrier", "Bulk Carrier"),
        ("General Cargo or Multipurpose", "General Cargo or Multipurpose"),
        ("Container Ship", "Container Ship"),
        ("RoRo or PCTC", "RoRo or PCTC"),
        ("Oil Tanker", "Oil Tanker"),
        ("Chemical Tanker", "Chemical Tanker"),
        ("Product Tanker", "Product Tanker"),
        ("LNG Carrier", "LNG Carrier"),
        ("LPG Carrier", "LPG Carrier"),
    };

    private static readonly string[] TankerTypes = { "Oil Tanker", "Chemical Tanker", "Product Tanker" };

    private static readonly string[] GasCarrierTypes = { "LNG Carrier", "LPG Carrier" };

    private static readonly (string Value, string Label)[] YesNoOptions =
    {
        ("Yes", "Yes"),
        ("No", "No"),
    };

    private static readonly (string Value, string Label)[] CountryOptions = new[]
    {
        "Egypt", "Saudi Arabia", "United Arab Emirates", "Kuwait", "Qatar", "Oman", "Bahrain",
        "Jordan", "Lebanon", "Panama", "Liberia", "Marshall Islands", "Malta", "Bahamas",
        "Singapore", "Hong Kong", "Cyprus", "Greece", "Turkey", "Italy", "Spain", "France",
        "Germany", "Netherlands", "Belgium", "United Kingdom", "Norway", "Denmark", "Portugal",
        "China", "Japan", "South Korea", "India", "Pakistan", "Bangladesh", "Sri Lanka",
        "Indonesia", "Malaysia", "Thailand", "Vietnam", "Philippines", "Australia",
        "United States", "Canada", "Brazil", "Argentina", "Chile", "Mexico",
        "South Africa", "Nigeria", "Morocco", "Algeria", "Tunisia", "Libya",
        "Russia", "Ukraine", "Other",
    }.Select(c => (c, c)).ToArray();

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

    private static FormSectionDto ContactSection(
        int order,
        string firstNameLabel = "First Name",
        string lastNameLabel = "Last Name",
        FormFieldDto[]? middleFields = null,
        FormFieldDto[]? trailingFields = null)
    {
        var fields = new List<FormFieldDto>();
        var o = 0;
        fields.Add(Field("firstName", firstNameLabel, FormFieldType.Text, o++, required: true, systemKey: FormsConstants.SystemFieldKeys.FirstName, width: FormFieldWidth.Half));
        fields.Add(Field("lastName", lastNameLabel, FormFieldType.Text, o++, required: true, systemKey: FormsConstants.SystemFieldKeys.LastName, width: FormFieldWidth.Half));
        foreach (var f in middleFields ?? Array.Empty<FormFieldDto>())
        {
            f.Order = o++;
            fields.Add(f);
        }

        fields.Add(Field("email", "Email", FormFieldType.Email, o++, required: true, systemKey: FormsConstants.SystemFieldKeys.Email, width: FormFieldWidth.Half));
        fields.Add(Field("phoneNumber", "Phone Number", FormFieldType.Phone, o++, required: true, systemKey: FormsConstants.SystemFieldKeys.PhoneNumber, width: FormFieldWidth.Half));
        foreach (var f in trailingFields ?? Array.Empty<FormFieldDto>())
        {
            f.Order = o++;
            fields.Add(f);
        }

        return Section("contact-information", "Contact Information", order, fields.ToArray());
    }

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

    private static FormFieldDto Text(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", string? placeholder = null,
        FormFieldValidationDto? validation = null) =>
        Field(key, label, FormFieldType.Text, order, required, systemKey, width, placeholder, validation: validation);

    private static FormFieldDto Textarea(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full") =>
        Field(key, label, FormFieldType.Textarea, order, required, systemKey, width);

    private static FormFieldDto Number(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", FormFieldValidationDto? validation = null) =>
        Field(key, label, FormFieldType.Number, order, required, systemKey, width, validation: validation);

    private static FormFieldDto Date(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", FormFieldValidationDto? validation = null) =>
        Field(key, label, FormFieldType.Date, order, required, systemKey, width, validation: validation);

    private static FormFieldValidationDto NoPastDates => new() { NoPastDates = true };

    private static FormFieldValidationDto AllowNegative => new() { AllowNegative = true };

    private static FormFieldDto Select(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full", (string Value, string Label)[]? options = null) =>
        Field(key, label, FormFieldType.Select, order, required, systemKey, width, options: options);

    private static FormFieldDto MultiSelect(string key, string label, int order, bool required = false, string width = "Full", (string Value, string Label)[]? options = null) =>
        Field(key, label, FormFieldType.MultiSelect, order, required, null, width, options: options);

    private static FormFieldDto Checkbox(string key, string label, int order, bool required = false, string width = "Full") =>
        Field(key, label, FormFieldType.Checkbox, order, required, null, width, placeholder: label);

    private static FormFieldDto File(string key, string label, int order, bool required = false, string width = "Full") =>
        Field(key, label, FormFieldType.File, order, required, null, width);

    private static FormFieldDto Port(string key, string label, int order, bool required = false, string? systemKey = null, string width = "Full") =>
        Field(key, label, FormFieldType.Port, order, required, systemKey, width);

    private static FormFieldDto ConditionalOn(string sourceKey, string value, FormFieldDto field) =>
        WithConditions(field, new[] { (sourceKey, value) });

    private static FormFieldDto ConditionalOnCargoType(string cargoTypeValue, FormFieldDto field) =>
        ConditionalOn("cargoType", cargoTypeValue, field);

    private static FormFieldDto ConditionalOnVesselType(string vesselType, FormFieldDto field) =>
        ConditionalOn("vesselType", vesselType, field);

    private static FormFieldDto ConditionalOnAll((string Key, string Value)[] conditions, FormFieldDto field) =>
        WithConditions(field, conditions);

    private static FormFieldDto ConditionalOnIn(string sourceKey, string[] values, FormFieldDto field)
    {
        field.ConditionCombinator = FormConditionCombinator.And;
        field.Conditions.Add(new FormFieldConditionDto
        {
            SourceFieldKey = sourceKey,
            Operator = FormConditionOperator.In,
            Value = string.Join(",", values),
        });
        return field;
    }

    private static FormFieldDto WithConditions(FormFieldDto field, (string Key, string Value)[] conditions)
    {
        field.ConditionCombinator = FormConditionCombinator.And;
        foreach (var (key, value) in conditions)
        {
            field.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = key, Operator = FormConditionOperator.EqualsOp, Value = value });
        }

        return field;
    }
}
