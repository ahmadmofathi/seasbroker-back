import { useState, useMemo } from "react";
import FormSelect from "../Common/FormSelect";
import ports from "../../utils/ports.json";
import { quoteApi } from "../../api";
import { withServiceTag } from "../../api/types";
import { formatApiError } from "../../utils/formatApiError";
import { useAlert } from "../../context/AlertContext";
import { useNavigate } from "react-router";

// FileUpload Component for cleaner file presentation
const FileUpload: React.FC<{
  label: string;
  required?: boolean;
  onChange: (file: File | null) => void;
  selectedFile: File | null;
}> = ({ label, required, onChange, selectedFile }) => {
  return (
    <div className="form-group mb-3">
      <label className="form-label">{label}{required && <span className="text-danger"> *</span>}</label>
      <div className="custom-file-upload border p-2 rounded d-flex align-items-center justify-content-between bg-light">
        <input
          type="file"
          id={label.replace(/\s+/g, '-')}
          style={{ display: 'none' }}
          onChange={(e) => {
            const file = e.target.files?.[0] || null;
            onChange(file);
          }}
        />
        <label
          htmlFor={label.replace(/\s+/g, '-')}
          className="btn btn-sm btn-outline-secondary mb-0 cursor-pointer"
          style={{ cursor: 'pointer' }}
        >
          Choose File
        </label>
        <span className="text-muted small text-truncate ms-2 flex-grow-1" style={{ maxWidth: '60%' }}>
          {selectedFile ? `${selectedFile.name} (${(selectedFile.size / 1024).toFixed(1)} KB)` : 'No file chosen'}
        </span>
        {selectedFile && (
          <button
            type="button"
            className="btn btn-sm text-danger ms-2 p-0 border-0 bg-transparent"
            onClick={() => onChange(null)}
          >
            <i className="ri-close-circle-fill fs-5" />
          </button>
        )}
      </div>
    </div>
  );
};

const RequestQuoteForm: React.FC = () => {
  const navigate = useNavigate();
  const { success, error: showError, warning } = useAlert();
  const [submitting, setSubmitting] = useState(false);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  // 1. Base cargo & feature states
  const [cargoType, setCargoType] = useState("Dry Bulk");
  const [dangerousGoodsFeature, setDangerousGoodsFeature] = useState(false);
  const [fragileFeature, setFragileFeature] = useState(false);
  const [oversizedFeature, setOversizedFeature] = useState(false);
  const [temperatureControlledFeature, setTemperatureControlledFeature] = useState(false);
  const [commodity, setCommodity] = useState("");
  const [weight, setWeight] = useState(""); // MT
  const [departurePort, setDeparturePort] = useState("");
  const [arrivalPort, setArrivalPort] = useState("");
  const [cargoReadyDate, setCargoReadyDate] = useState("");
  const [dangerousGoods, setDangerousGoods] = useState("No");
  const [remarks, setRemarks] = useState("");
  const [documentsFile, setDocumentsFile] = useState<File | null>(null);

  // 2. Dangerous Goods Details
  const [unNumber, setUnNumber] = useState("");
  const [properShippingName, setProperShippingName] = useState("");
  const [imoClass, setImoClass] = useState("");
  const [packingGroup, setPackingGroup] = useState("I");
  const [flashPoint, setFlashPoint] = useState("");
  const [marinePollutant, setMarinePollutant] = useState("No");
  const [sdsFile, setSdsFile] = useState<File | null>(null);

  // 3. Cargo Type Specific States
  // Dry Bulk
  const [dryBulkCommodity, setDryBulkCommodity] = useState("");
  const [dryBulkQuantity, setDryBulkQuantity] = useState("");
  const [dryBulkStowageFactor, setDryBulkStowageFactor] = useState("");
  const [dryBulkLoadingRate, setDryBulkLoadingRate] = useState("");
  const [dryBulkDischargeRate, setDryBulkDischargeRate] = useState("");
  const [dryBulkTerms, setDryBulkTerms] = useState("FIO");
  const [dryBulkTermsOther, setDryBulkTermsOther] = useState("");
  const [dryBulkTolerance, setDryBulkTolerance] = useState("");
  const [dryBulkMaxDraft, setDryBulkMaxDraft] = useState("");
  const [dryBulkMoisture, setDryBulkMoisture] = useState("");
  const [dryBulkVesselGear, setDryBulkVesselGear] = useState("No");

  // General & Breakbulk Cargo
  const [bbDescription, setBbDescription] = useState("");
  const [bbUnits, setBbUnits] = useState("");
  const [bbPackingType, setBbPackingType] = useState("Bags");
  const [bbPackingTypeOther, setBbPackingTypeOther] = useState("");
  const [bbWeight, setBbWeight] = useState("");
  const [bbVolume, setBbVolume] = useState("");
  const [bbLength, setBbLength] = useState("");
  const [bbWidth, setBbWidth] = useState("");
  const [bbHeight, setBbHeight] = useState("");
  const [bbHeaviestWeight, setBbHeaviestWeight] = useState("");
  const [bbStackable, setBbStackable] = useState("No");
  const [bbGearRequired, setBbGearRequired] = useState("No");

  // Project & Heavy-Lift Cargo
  const [projDescription, setProjDescription] = useState("");
  const [projUnits, setProjUnits] = useState("");
  const [projWeight, setProjWeight] = useState("");
  const [projHeaviestWeight, setProjHeaviestWeight] = useState("");
  const [projLength, setProjLength] = useState("");
  const [projWidth, setProjWidth] = useState("");
  const [projHeight, setProjHeight] = useState("");
  const [projCoGAvailable, setProjCoGAvailable] = useState("No");
  const [projLiftingPointsAvailable, setProjLiftingPointsAvailable] = useState("No");
  const [projDrawingsFile, setProjDrawingsFile] = useState<File | null>(null);
  const [projVolume, setProjVolume] = useState("");
  const [projLiftingPointCapacity, setProjLiftingPointCapacity] = useState("");
  const [projCoG, setProjCoG] = useState("");
  const [projRequiredLiftingCapacity, setProjRequiredLiftingCapacity] = useState("");
  const [projSelfPropelled, setProjSelfPropelled] = useState("No");

  // Containerized Cargo
  const [containerCommodity, setContainerCommodity] = useState("");
  const [containerShipmentType, setContainerShipmentType] = useState("FCL");
  const [containerType, setContainerType] = useState("20'GP");
  const [containerNumber, setContainerNumber] = useState("");
  const [containerNumberUnit, setContainerNumberUnit] = useState("TEU");
  const [containerWeight, setContainerWeight] = useState("");
  const [lclPackages, setLclPackages] = useState("");
  const [lclWeight, setLclWeight] = useState("");
  const [lclVolume, setLclVolume] = useState("");

  // RoRo Cargo
  const [roroVehicleType, setRoroVehicleType] = useState("");
  const [roroMakeModel, setRoroMakeModel] = useState("");
  const [roroQuantity, setRoroQuantity] = useState("");
  const [roroLength, setRoroLength] = useState("");
  const [roroWidth, setRoroWidth] = useState("");
  const [roroHeight, setRoroHeight] = useState("");
  const [roroWeight, setRoroWeight] = useState("");
  const [roroOperational, setRoroOperational] = useState("No");
  const [roroSelfPropelled, setRoroSelfPropelled] = useState("No");

  // Liquid Bulk
  const [liquidProductName, setLiquidProductName] = useState("");
  const [liquidQuantity, setLiquidQuantity] = useState("");
  const [liquidDensity, setLiquidDensity] = useState("");
  const [liquidLoadingTemp, setLiquidLoadingTemp] = useState("");
  const [liquidGrades, setLiquidGrades] = useState("");
  const [liquidHeatingRequired, setLiquidHeatingRequired] = useState("No");
  const [liquidLoadingRate, setLiquidLoadingRate] = useState("");
  const [liquidDischargeRate, setLiquidDischargeRate] = useState("");
  const [liquidFlashPoint, setLiquidFlashPoint] = useState("");
  const [liquidViscosity, setLiquidViscosity] = useState("");
  const [liquidCarriageTemp, setLiquidCarriageTemp] = useState("");
  const [liquidSdsFile, setLiquidSdsFile] = useState<File | null>(null);

  // Gas Cargo
  const [gasCargoType, setGasCargoType] = useState("LNG");
  const [gasProduct, setGasProduct] = useState("");
  const [gasQuantity, setGasQuantity] = useState("");
  const [gasQuantityUnit, setGasQuantityUnit] = useState("MT");
  const [gasDensity, setGasDensity] = useState("");
  const [gasLoadingTemp, setGasLoadingTemp] = useState("");
  const [gasLoadingPressure, setGasLoadingPressure] = useState("");
  const [gasLoadingRate, setGasLoadingRate] = useState("");
  const [gasDischargeRate, setGasDischargeRate] = useState("");
  const [gasSpecFile, setGasSpecFile] = useState<File | null>(null);

  // Refrigerated & Perishable Cargo
  const [reeferTransportMethod, setReeferTransportMethod] = useState("Reefer Container");
  const [reeferCommodity, setReeferCommodity] = useState("");
  const [reeferQuantity, setReeferQuantity] = useState("");
  const [reeferPackingType, setReeferPackingType] = useState("Cartons");
  const [reeferSetTemp, setReeferSetTemp] = useState("");
  const [reeferMinTemp, setReeferMinTemp] = useState("");
  const [reeferMaxTemp, setReeferMaxTemp] = useState("");
  const [reeferVentilation, setReeferVentilation] = useState("");
  const [reeferHumidity, setReeferHumidity] = useState("");
  const [reeferControlledAtmosphere, setReeferControlledAtmosphere] = useState("No");
  const [reeferContainerType, setReeferContainerType] = useState("20'RF");
  const [reeferContainerCount, setReeferContainerCount] = useState("");
  const [reeferContainerWeight, setReeferContainerWeight] = useState("");

  // Other / Not Sure
  const [otherDescription, setOtherDescription] = useState("");
  const [otherQuantity, setOtherQuantity] = useState("");
  const [otherWeight, setOtherWeight] = useState("");
  const [otherVolume, setOtherVolume] = useState("");
  const [otherDimensions, setOtherDimensions] = useState("");
  const [otherDocFile, setOtherDocFile] = useState<File | null>(null);
  const [otherSpecialRequirements, setOtherSpecialRequirements] = useState("");

  // 4. Contact Details States
  const [fname, setFname] = useState("");
  const [lname, setLname] = useState("");
  const [email, setEmail] = useState("");
  const [countryCode, setCountryCode] = useState("+20");
  const [phoneRaw, setPhoneRaw] = useState("");

  // Ports list formatting
  const cities = useMemo(() => {
    return Object.values(ports).map((port) => ({
      text: `${port.name} - ${port.country}`,
      value: `${port.name} - ${port.country}`,
    }));
  }, []);

  // FormSelect compatibility states
  const departurePortState = { departurePort };
  const arrivalPortState = { arrivalPort };

  const handleDeparturePortSelect = (val: typeof departurePortState | ((prev: typeof departurePortState) => typeof departurePortState)) => {
    if (typeof val === 'function') {
      const next = val(departurePortState);
      setDeparturePort(next.departurePort);
    } else {
      setDeparturePort(val.departurePort);
    }
  };

  const handleArrivalPortSelect = (val: typeof arrivalPortState | ((prev: typeof arrivalPortState) => typeof arrivalPortState)) => {
    if (typeof val === 'function') {
      const next = val(arrivalPortState);
      setArrivalPort(next.arrivalPort);
    } else {
      setArrivalPort(val.arrivalPort);
    }
  };

  const buildCargoAdditionalInfo = (): string => {
    let notes = `Commodity Description: ${commodity}\n`;
    notes += `Total Quantity: ${weight} MT\n`;
    notes += `Ready Date: ${cargoReadyDate}\n`;
    notes += `Features: ${[
      dangerousGoodsFeature && 'Dangerous Goods',
      fragileFeature && 'Fragile',
      oversizedFeature && 'Oversized / OOG',
      temperatureControlledFeature && 'Temperature Controlled'
    ].filter(Boolean).join(', ') || 'None'}\n`;
    notes += `Remarks: ${remarks || 'None'}\n`;
    notes += `Base Documents: ${documentsFile ? documentsFile.name : 'None'}\n`;
    notes += `Dangerous Goods: ${dangerousGoods}\n`;

    if (dangerousGoods === 'Yes') {
      notes += `\n--- Dangerous Goods Specifications ---\n`;
      notes += `UN Number: ${unNumber}\n`;
      notes += `Proper Shipping Name: ${properShippingName}\n`;
      notes += `IMO/IMDG Class: ${imoClass}\n`;
      notes += `Packing Group: ${packingGroup}\n`;
      notes += `Flash Point: ${flashPoint ? flashPoint + ' °C' : 'N/A'}\n`;
      notes += `Marine Pollutant: ${marinePollutant}\n`;
      notes += `SDS/MSDS File: ${sdsFile ? sdsFile.name : 'None'}\n`;
    }

    notes += `\n--- Cargo Details [${cargoType}] ---\n`;
    if (cargoType === 'Dry Bulk') {
      notes += `Stowage Factor: ${dryBulkStowageFactor} m³/MT\n`;
      notes += `Loading Rate: ${dryBulkLoadingRate} MT/day\n`;
      notes += `Discharge Rate: ${dryBulkDischargeRate} MT/day\n`;
      notes += `Loading/Discharge Terms: ${dryBulkTerms === 'Other' ? `Other (${dryBulkTermsOther})` : dryBulkTerms}\n`;
      notes += `Quantity Tolerance: ${dryBulkTolerance || 'N/A'} %\n`;
      notes += `Maximum Draft: ${dryBulkMaxDraft || 'N/A'} m\n`;
      notes += `Moisture Content: ${dryBulkMoisture || 'N/A'} %\n`;
      notes += `Vessel Gear Required: ${dryBulkVesselGear}\n`;
    } else if (cargoType === 'General & Breakbulk Cargo') {
      notes += `Number of Units: ${bbUnits} pcs\n`;
      notes += `Packing Type: ${bbPackingType === 'Other' ? `Other (${bbPackingTypeOther})` : bbPackingType}\n`;
      notes += `Total Volume: ${bbVolume} m³\n`;
      notes += `Largest Unit Dimensions: ${bbLength}m x ${bbWidth}m x ${bbHeight}m\n`;
      notes += `Heaviest Unit Weight: ${bbHeaviestWeight} MT\n`;
      notes += `Stackable: ${bbStackable}\n`;
      notes += `Ship's Gear Required: ${bbGearRequired}\n`;
    } else if (cargoType === 'Project & Heavy-Lift Cargo') {
      notes += `Number of Units: ${projUnits} pcs\n`;
      notes += `Heaviest Unit Weight: ${projHeaviestWeight} MT\n`;
      notes += `Largest Unit Dimensions: ${projLength}m x ${projWidth}m x ${projHeight}m\n`;
      notes += `Center of Gravity Available: ${projCoGAvailable}\n`;
      notes += `Lifting Points Available: ${projLiftingPointsAvailable}\n`;
      notes += `Drawings/Packing List: ${projDrawingsFile ? projDrawingsFile.name : 'None'}\n`;
      notes += `Total Volume: ${projVolume || 'N/A'} m³\n`;
      notes += `Lifting Point Capacity: ${projLiftingPointCapacity || 'N/A'} MT\n`;
      notes += `Center of Gravity: ${projCoG || 'N/A'} m\n`;
      notes += `Required Lifting Capacity: ${projRequiredLiftingCapacity || 'N/A'} MT\n`;
      notes += `Self-propelled: ${projSelfPropelled}\n`;
    } else if (cargoType === 'Containerized Cargo') {
      notes += `Shipment Type: ${containerShipmentType}\n`;
      notes += `Container Type: ${containerType}\n`;
      notes += `Number of Containers: ${containerNumber} ${containerNumberUnit}\n`;
      notes += `Gross Weight per Container: ${containerWeight} MT\n`;
      if (containerShipmentType === 'LCL') {
        notes += `LCL Packages: ${lclPackages} pcs\n`;
        notes += `LCL Total Weight: ${lclWeight} kg\n`;
        notes += `LCL Total Volume: ${lclVolume} m³\n`;
      }
    } else if (cargoType === 'RoRo Cargo') {
      notes += `Vehicle/Equipment Type: ${roroVehicleType}\n`;
      notes += `Make/Model: ${roroMakeModel}\n`;
      notes += `Quantity: ${roroQuantity} units\n`;
      notes += `Dimensions per Unit: ${roroLength}m x ${roroWidth}m x ${roroHeight}m\n`;
      notes += `Weight per Unit: ${roroWeight} MT\n`;
      notes += `Operational/Running: ${roroOperational}\n`;
      notes += `Self-Propelled: ${roroSelfPropelled}\n`;
    } else if (cargoType === 'Liquid Bulk') {
      notes += `Product Name: ${liquidProductName}\n`;
      notes += `Density: ${liquidDensity} kg/m³\n`;
      notes += `Loading Temp: ${liquidLoadingTemp} °C\n`;
      notes += `Number of Grades: ${liquidGrades}\n`;
      notes += `Heating Required: ${liquidHeatingRequired}\n`;
      notes += `Loading Rate: ${liquidLoadingRate} m³/h\n`;
      notes += `Discharge Rate: ${liquidDischargeRate} m³/h\n`;
      notes += `Flash Point: ${liquidFlashPoint || 'N/A'} °C\n`;
      notes += `Viscosity: ${liquidViscosity || 'N/A'} cSt\n`;
      notes += `Carriage Temp: ${liquidCarriageTemp || 'N/A'} °C\n`;
      notes += `SDS File: ${liquidSdsFile ? liquidSdsFile.name : 'None'}\n`;
    } else if (cargoType === 'Gas Cargo') {
      notes += `Gas Cargo Type: ${gasCargoType}\n`;
      notes += `Product: ${gasProduct}\n`;
      notes += `Quantity: ${gasQuantity} ${gasQuantityUnit}\n`;
      notes += `Density: ${gasDensity} kg/m³\n`;
      notes += `Loading Temp: ${gasLoadingTemp} °C\n`;
      notes += `Loading Pressure: ${gasLoadingPressure} bar\n`;
      notes += `Loading Rate: ${gasLoadingRate} m³/h\n`;
      notes += `Discharge Rate: ${gasDischargeRate} m³/h\n`;
      notes += `Gas Spec File: ${gasSpecFile ? gasSpecFile.name : 'None'}\n`;
    } else if (cargoType === 'Refrigerated & Perishable Cargo') {
      notes += `Transport Method: ${reeferTransportMethod}\n`;
      notes += `Commodity: ${reeferCommodity}\n`;
      notes += `Packing Type: ${reeferPackingType}\n`;
      notes += `Set Temp: ${reeferSetTemp} °C\n`;
      notes += `Min Temp: ${reeferMinTemp} °C\n`;
      notes += `Max Temp: ${reeferMaxTemp} °C\n`;
      notes += `Ventilation: ${reeferVentilation || 'N/A'} m³/h\n`;
      notes += `Humidity: ${reeferHumidity || 'N/A'} % RH\n`;
      notes += `Controlled Atmosphere: ${reeferControlledAtmosphere}\n`;
      if (reeferTransportMethod === 'Reefer Container') {
        notes += `Reefer Type: ${reeferContainerType}\n`;
        notes += `Number of Containers: ${reeferContainerCount} units\n`;
        notes += `Gross Weight/Container: ${reeferContainerWeight} MT\n`;
      }
    } else if (cargoType === 'Other') {
      notes += `Cargo Description: ${otherDescription}\n`;
      notes += `Quantity: ${otherQuantity} pcs\n`;
      notes += `Total Weight: ${otherWeight} MT\n`;
      notes += `Total Volume: ${otherVolume} m³\n`;
      notes += `Largest Dimensions: ${otherDimensions || 'N/A'} m\n`;
      notes += `Documents File: ${otherDocFile ? otherDocFile.name : 'None'}\n`;
      notes += `Special Requirements: ${otherSpecialRequirements || 'None'}\n`;
    }

    return notes;
  };

  const validateForm = (): boolean => {
    const errs: Record<string, string> = {};

    // Base validation
    if (!commodity.trim()) errs.commodity = "Commodity description is required.";
    if (!weight.trim() || Number(weight) <= 0) errs.weight = "Quantity must be a positive number.";
    if (!departurePort) errs.departurePort = "Departure port is required.";
    if (!arrivalPort) errs.arrivalPort = "Arrival port is required.";
    if (!cargoReadyDate) errs.cargoReadyDate = "Cargo ready date is required.";

    // Contact details validation
    if (!fname.trim()) errs.fname = "First name is required.";
    if (!lname.trim()) errs.lname = "Last name is required.";
    if (!email.trim() || !/\S+@\S+\.\S+/.test(email)) errs.email = "Invalid email address.";
    if (!phoneRaw.trim()) errs.phoneRaw = "Phone number is required.";

    // Dangerous goods details validation
    if (dangerousGoods === 'Yes') {
      if (!unNumber.trim()) errs.unNumber = "UN Number is required.";
      if (!properShippingName.trim()) errs.properShippingName = "Proper shipping name is required.";
      if (!imoClass.trim()) errs.imoClass = "IMO Class is required.";
    }

    // Dynamic type validations
    if (cargoType === 'Dry Bulk') {
      if (!dryBulkCommodity.trim()) errs.dryBulkCommodity = "Commodity is required.";
      if (!dryBulkQuantity.trim()) errs.dryBulkQuantity = "Quantity is required.";
      if (!dryBulkStowageFactor.trim()) errs.dryBulkStowageFactor = "Stowage factor is required.";
      if (!dryBulkLoadingRate.trim()) errs.dryBulkLoadingRate = "Loading rate is required.";
      if (!dryBulkDischargeRate.trim()) errs.dryBulkDischargeRate = "Discharge rate is required.";
      if (dryBulkTerms === 'Other' && !dryBulkTermsOther.trim()) errs.dryBulkTermsOther = "Terms are required.";
    } else if (cargoType === 'General & Breakbulk Cargo') {
      if (!bbDescription.trim()) errs.bbDescription = "Cargo description is required.";
      if (!bbUnits.trim()) errs.bbUnits = "Number of packages is required.";
      if (!bbWeight.trim()) errs.bbWeight = "Total weight is required.";
      if (!bbVolume.trim()) errs.bbVolume = "Total volume is required.";
      if (!bbLength.trim()) errs.bbLength = "Largest unit length is required.";
      if (!bbWidth.trim()) errs.bbWidth = "Largest unit width is required.";
      if (!bbHeight.trim()) errs.bbHeight = "Largest unit height is required.";
      if (!bbHeaviestWeight.trim()) errs.bbHeaviestWeight = "Heaviest unit weight is required.";
      if (bbPackingType === 'Other' && !bbPackingTypeOther.trim()) errs.bbPackingTypeOther = "Packing type is required.";
    } else if (cargoType === 'Project & Heavy-Lift Cargo') {
      if (!projDescription.trim()) errs.projDescription = "Cargo description is required.";
      if (!projUnits.trim()) errs.projUnits = "Number of units is required.";
      if (!projWeight.trim()) errs.projWeight = "Total weight is required.";
      if (!projHeaviestWeight.trim()) errs.projHeaviestWeight = "Heaviest unit weight is required.";
      if (!projLength.trim()) errs.projLength = "Largest unit length is required.";
      if (!projWidth.trim()) errs.projWidth = "Largest unit width is required.";
      if (!projHeight.trim()) errs.projHeight = "Largest unit height is required.";
      if (!projDrawingsFile) errs.projDrawingsFile = "Drawings / Packing List is required.";
    } else if (cargoType === 'Containerized Cargo') {
      if (!containerCommodity.trim()) errs.containerCommodity = "Commodity is required.";
      if (!containerNumber.trim()) errs.containerNumber = "Number of containers is required.";
      if (!containerWeight.trim()) errs.containerWeight = "Weight per container is required.";
      if (containerShipmentType === 'LCL') {
        if (!lclPackages.trim()) errs.lclPackages = "Number of packages is required.";
        if (!lclWeight.trim()) errs.lclWeight = "Total weight is required.";
        if (!lclVolume.trim()) errs.lclVolume = "Total volume is required.";
      }
    } else if (cargoType === 'RoRo Cargo') {
      if (!roroVehicleType.trim()) errs.roroVehicleType = "Vehicle type is required.";
      if (!roroMakeModel.trim()) errs.roroMakeModel = "Make / model is required.";
      if (!roroQuantity.trim()) errs.roroQuantity = "Quantity is required.";
      if (!roroLength.trim()) errs.roroLength = "Length is required.";
      if (!roroWidth.trim()) errs.roroWidth = "Width is required.";
      if (!roroHeight.trim()) errs.roroHeight = "Height is required.";
      if (!roroWeight.trim()) errs.roroWeight = "Weight is required.";
    } else if (cargoType === 'Liquid Bulk') {
      if (!liquidProductName.trim()) errs.liquidProductName = "Product name is required.";
      if (!liquidQuantity.trim()) errs.liquidQuantity = "Quantity is required.";
      if (!liquidDensity.trim()) errs.liquidDensity = "Density is required.";
      if (!liquidLoadingTemp.trim()) errs.liquidLoadingTemp = "Loading temperature is required.";
      if (!liquidGrades.trim()) errs.liquidGrades = "Number of grades is required.";
      if (!liquidLoadingRate.trim()) errs.liquidLoadingRate = "Loading rate is required.";
      if (!liquidDischargeRate.trim()) errs.liquidDischargeRate = "Discharge rate is required.";
    } else if (cargoType === 'Gas Cargo') {
      if (!gasProduct.trim()) errs.gasProduct = "Product name is required.";
      if (!gasQuantity.trim()) errs.gasQuantity = "Quantity is required.";
      if (!gasDensity.trim()) errs.gasDensity = "Density is required.";
      if (!gasLoadingTemp.trim()) errs.gasLoadingTemp = "Loading temperature is required.";
      if (!gasLoadingPressure.trim()) errs.gasLoadingPressure = "Loading pressure is required.";
      if (!gasLoadingRate.trim()) errs.gasLoadingRate = "Loading rate is required.";
      if (!gasDischargeRate.trim()) errs.gasDischargeRate = "Discharge rate is required.";
    } else if (cargoType === 'Refrigerated & Perishable Cargo') {
      if (!reeferCommodity.trim()) errs.reeferCommodity = "Commodity name is required.";
      if (!reeferQuantity.trim()) errs.reeferQuantity = "Quantity is required.";
      if (!reeferSetTemp.trim()) errs.reeferSetTemp = "Set temperature is required.";
      if (!reeferMinTemp.trim()) errs.reeferMinTemp = "Minimum temperature is required.";
      if (!reeferMaxTemp.trim()) errs.reeferMaxTemp = "Maximum temperature is required.";
      if (reeferTransportMethod === 'Reefer Container') {
        if (!reeferContainerCount.trim()) errs.reeferContainerCount = "Number of containers is required.";
        if (!reeferContainerWeight.trim()) errs.reeferContainerWeight = "Weight per container is required.";
      }
    } else if (cargoType === 'Other') {
      if (!otherDescription.trim()) errs.otherDescription = "Description is required.";
      if (!otherQuantity.trim()) errs.otherQuantity = "Quantity is required.";
      if (!otherWeight.trim()) errs.otherWeight = "Total weight is required.";
      if (!otherVolume.trim()) errs.otherVolume = "Total volume is required.";
    }

    setValidationErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit: React.FormEventHandler = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      warning('Please correct the validation errors before submitting.');
      return;
    }

    setSubmitting(true);

    const finalPhone = `${countryCode} ${phoneRaw.trim()}`;
    const readyDateObj = new Date(cargoReadyDate);
    const departureTimeISO = readyDateObj.toISOString();
    // Default arrival to cargo ready date + 7 days
    const arrivalTimeISO = new Date(readyDateObj.getTime() + 7 * 24 * 60 * 60 * 1000).toISOString();

    const info = buildCargoAdditionalInfo();

    // Determine dimensions representation
    let dimsRepr = "N/A";
    if (cargoType === 'Dry Bulk') {
      dimsRepr = `SF: ${dryBulkStowageFactor} m³/MT`;
    } else if (cargoType === 'General & Breakbulk Cargo') {
      dimsRepr = `${bbLength}x${bbWidth}x${bbHeight} m`;
    } else if (cargoType === 'Project & Heavy-Lift Cargo') {
      dimsRepr = `${projLength}x${projWidth}x${projHeight} m`;
    } else if (cargoType === 'Containerized Cargo') {
      dimsRepr = `${containerNumber}x ${containerType}`;
    } else if (cargoType === 'RoRo Cargo') {
      dimsRepr = `${roroLength}x${roroWidth}x${roroHeight} m`;
    } else if (cargoType === 'Other') {
      dimsRepr = otherDimensions || "N/A";
    }

    try {
      const response = await quoteApi.submitQuote({
        cargoType,
        weight: Number(weight) || 0,
        departurePort,
        departureTime: departureTimeISO,
        arrivalPort,
        arrivalTime: arrivalTimeISO,
        dimensions: dimsRepr,
        additionalInfo: withServiceTag('Cargo Brokerage', info),
        fname,
        lname,
        email,
        phoneNumber: finalPhone,
      });

      success(
        `${response.message || 'Cargo request registered successfully.'}\n\nIt is now available in Admin → Public Requests.`,
      );
      void navigate('/');
    } catch (err) {
      showError(formatApiError(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form id="request_form" onSubmit={handleSubmit}>
      <div className="row">
        <div className="col-lg-12">
          <div className="heading_quote">
            <h3>Register Cargo</h3>
          </div>
        </div>

        {/* Base Fields */}
        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="cargoType" className="form-label">Cargo Type<span className="text-danger"> *</span></label>
            <select
              id="cargoType"
              name="cargoType"
              value={cargoType}
              className="form-control"
              onChange={(e) => setCargoType(e.target.value)}
            >
              <option value="Dry Bulk">Dry Bulk</option>
              <option value="General & Breakbulk Cargo">General & Breakbulk Cargo</option>
              <option value="Project & Heavy-Lift Cargo">Project & Heavy-Lift Cargo</option>
              <option value="Containerized Cargo">Containerized Cargo</option>
              <option value="RoRo Cargo">RoRo Cargo</option>
              <option value="Liquid Bulk">Liquid Bulk</option>
              <option value="Gas Cargo">Gas Cargo</option>
              <option value="Refrigerated & Perishable Cargo">Refrigerated & Perishable Cargo</option>
              <option value="Other">Other / Not Sure</option>
            </select>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label className="form-label d-block">Cargo Features</label>
            <div className="d-flex flex-wrap gap-3 pt-2">
              <div className="form-check">
                <input
                  type="checkbox"
                  id="feat-dg"
                  className="form-check-input"
                  checked={dangerousGoodsFeature}
                  onChange={(e) => setDangerousGoodsFeature(e.target.checked)}
                />
                <label htmlFor="feat-dg" className="form-check-label">Dangerous Goods</label>
              </div>
              <div className="form-check">
                <input
                  type="checkbox"
                  id="feat-fragile"
                  className="form-check-input"
                  checked={fragileFeature}
                  onChange={(e) => setFragileFeature(e.target.checked)}
                />
                <label htmlFor="feat-fragile" className="form-check-label">Fragile</label>
              </div>
              <div className="form-check">
                <input
                  type="checkbox"
                  id="feat-oog"
                  className="form-check-input"
                  checked={oversizedFeature}
                  onChange={(e) => setOversizedFeature(e.target.checked)}
                />
                <label htmlFor="feat-oog" className="form-check-label">Oversized / OOG</label>
              </div>
              <div className="form-check">
                <input
                  type="checkbox"
                  id="feat-temp"
                  className="form-check-input"
                  checked={temperatureControlledFeature}
                  onChange={(e) => setTemperatureControlledFeature(e.target.checked)}
                />
                <label htmlFor="feat-temp" className="form-check-label">Temperature Controlled</label>
              </div>
            </div>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="commodity" className="form-label">Commodity / Cargo Description<span className="text-danger"> *</span></label>
            <input
              type="text"
              id="commodity"
              className={`form-control${validationErrors.commodity ? ' is-invalid' : ''}`}
              placeholder="e.g. Wheat, Steel Coils, Machinery"
              value={commodity}
              onChange={(e) => setCommodity(e.target.value)}
            />
            {validationErrors.commodity && <div className="invalid-feedback">{validationErrors.commodity}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="weight" className="form-label">Total Quantity (MT)<span className="text-danger"> *</span></label>
            <input
              type="number"
              id="weight"
              step="any"
              className={`form-control${validationErrors.weight ? ' is-invalid' : ''}`}
              placeholder="Total quantity in Metric Tons"
              value={weight}
              onChange={(e) => setWeight(e.target.value)}
            />
            {validationErrors.weight && <div className="invalid-feedback">{validationErrors.weight}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <FormSelect<{ departurePort: string }>
            label="Departure / Loading Port"
            placeholder="Select Loading Port"
            options={cities}
            formField="departurePort"
            error={validationErrors.departurePort || ""}
            formData={departurePortState}
            setFormData={handleDeparturePortSelect}
          />
        </div>

        <div className="col-lg-6">
          <FormSelect<{ arrivalPort: string }>
            label="Arrival / Discharge Port"
            placeholder="Select Discharge Port"
            options={cities}
            formField="arrivalPort"
            error={validationErrors.arrivalPort || ""}
            formData={arrivalPortState}
            setFormData={handleArrivalPortSelect}
          />
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="cargoReadyDate" className="form-label">Cargo Ready Date<span className="text-danger"> *</span></label>
            <input
              type="date"
              id="cargoReadyDate"
              className={`form-control${validationErrors.cargoReadyDate ? ' is-invalid' : ''}`}
              value={cargoReadyDate}
              onChange={(e) => setCargoReadyDate(e.target.value)}
            />
            {validationErrors.cargoReadyDate && <div className="invalid-feedback">{validationErrors.cargoReadyDate}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="dangerousGoods" className="form-label">Dangerous Goods?<span className="text-danger"> *</span></label>
            <select
              id="dangerousGoods"
              className="form-control"
              value={dangerousGoods}
              onChange={(e) => setDangerousGoods(e.target.value)}
            >
              <option value="No">No</option>
              <option value="Yes">Yes</option>
            </select>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="remarks" className="form-label">Remarks / Special Requirements</label>
            <textarea
              id="remarks"
              className="form-control"
              placeholder="e.g. Stowage requirements, crane availability"
              value={remarks}
              rows={3}
              onChange={(e) => setRemarks(e.target.value)}
            />
          </div>
        </div>

        <div className="col-lg-6">
          <FileUpload
            label="Documents / Photos"
            onChange={setDocumentsFile}
            selectedFile={documentsFile}
          />
        </div>

        {/* Dangerous Goods Section */}
        {dangerousGoods === 'Yes' && (
          <div className="col-lg-12 border-top border-bottom py-3 my-3 bg-light rounded">
            <h5 className="mb-3 text-danger"><i className="ri-error-warning-line"></i> Dangerous Goods Details</h5>
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="unNumber" className="form-label">UN Number<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="unNumber"
                    className={`form-control${validationErrors.unNumber ? ' is-invalid' : ''}`}
                    placeholder="UN ####"
                    value={unNumber}
                    onChange={(e) => setUnNumber(e.target.value)}
                  />
                  {validationErrors.unNumber && <div className="invalid-feedback">{validationErrors.unNumber}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="properShippingName" className="form-label">Proper Shipping Name<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="properShippingName"
                    className={`form-control${validationErrors.properShippingName ? ' is-invalid' : ''}`}
                    placeholder="e.g. Ethanol Solution"
                    value={properShippingName}
                    onChange={(e) => setProperShippingName(e.target.value)}
                  />
                  {validationErrors.properShippingName && <div className="invalid-feedback">{validationErrors.properShippingName}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="imoClass" className="form-label">IMO/IMDG Class<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="imoClass"
                    className={`form-control${validationErrors.imoClass ? ' is-invalid' : ''}`}
                    placeholder="e.g. 3.2"
                    value={imoClass}
                    onChange={(e) => setImoClass(e.target.value)}
                  />
                  {validationErrors.imoClass && <div className="invalid-feedback">{validationErrors.imoClass}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="packingGroup" className="form-label">Packing Group<span className="text-danger"> *</span></label>
                  <select
                    id="packingGroup"
                    className="form-control"
                    value={packingGroup}
                    onChange={(e) => setPackingGroup(e.target.value)}
                  >
                    <option value="I">I</option>
                    <option value="II">II</option>
                    <option value="III">III</option>
                    <option value="N/A">N/A</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="flashPoint" className="form-label">Flash Point (°C)</label>
                  <input
                    type="number"
                    id="flashPoint"
                    className="form-control"
                    placeholder="e.g. 23"
                    value={flashPoint}
                    onChange={(e) => setFlashPoint(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="marinePollutant" className="form-label">Marine Pollutant</label>
                  <select
                    id="marinePollutant"
                    className="form-control"
                    value={marinePollutant}
                    onChange={(e) => setMarinePollutant(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <FileUpload
                  label="SDS/MSDS"
                  onChange={setSdsFile}
                  selectedFile={sdsFile}
                />
              </div>
            </div>
          </div>
        )}

        {/* Dynamic Cargo Specific Sections */}
        <div className="col-lg-12 border-top pt-3 my-3">
          <h5 className="mb-3 text-secondary"><i className="ri-ship-line"></i> Specific Cargo Details ({cargoType})</h5>
          
          {cargoType === 'Dry Bulk' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-commodity" className="form-label">Commodity<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="db-commodity"
                    className={`form-control${validationErrors.dryBulkCommodity ? ' is-invalid' : ''}`}
                    placeholder="e.g. Soybeans"
                    value={dryBulkCommodity}
                    onChange={(e) => setDryBulkCommodity(e.target.value)}
                  />
                  {validationErrors.dryBulkCommodity && <div className="invalid-feedback">{validationErrors.dryBulkCommodity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-qty" className="form-label">Quantity (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="db-qty"
                    className={`form-control${validationErrors.dryBulkQuantity ? ' is-invalid' : ''}`}
                    placeholder="Quantity in MT"
                    value={dryBulkQuantity}
                    onChange={(e) => setDryBulkQuantity(e.target.value)}
                  />
                  {validationErrors.dryBulkQuantity && <div className="invalid-feedback">{validationErrors.dryBulkQuantity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-sf" className="form-label">Stowage Factor (m³/MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="db-sf"
                    className={`form-control${validationErrors.dryBulkStowageFactor ? ' is-invalid' : ''}`}
                    placeholder="e.g. 1.2"
                    value={dryBulkStowageFactor}
                    onChange={(e) => setDryBulkStowageFactor(e.target.value)}
                  />
                  {validationErrors.dryBulkStowageFactor && <div className="invalid-feedback">{validationErrors.dryBulkStowageFactor}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-load" className="form-label">Loading Rate (MT/day)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="db-load"
                    className={`form-control${validationErrors.dryBulkLoadingRate ? ' is-invalid' : ''}`}
                    placeholder="e.g. 5000"
                    value={dryBulkLoadingRate}
                    onChange={(e) => setDryBulkLoadingRate(e.target.value)}
                  />
                  {validationErrors.dryBulkLoadingRate && <div className="invalid-feedback">{validationErrors.dryBulkLoadingRate}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-disch" className="form-label">Discharge Rate (MT/day)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="db-disch"
                    className={`form-control${validationErrors.dryBulkDischargeRate ? ' is-invalid' : ''}`}
                    placeholder="e.g. 3500"
                    value={dryBulkDischargeRate}
                    onChange={(e) => setDryBulkDischargeRate(e.target.value)}
                  />
                  {validationErrors.dryBulkDischargeRate && <div className="invalid-feedback">{validationErrors.dryBulkDischargeRate}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-terms" className="form-label">Loading/Discharge Terms<span className="text-danger"> *</span></label>
                  <select
                    id="db-terms"
                    className="form-control"
                    value={dryBulkTerms}
                    onChange={(e) => setDryBulkTerms(e.target.value)}
                  >
                    <option value="FIO">FIO</option>
                    <option value="FIOS">FIOS</option>
                    <option value="FIOST">FIOST</option>
                    <option value="Liner Terms">Liner Terms</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              {dryBulkTerms === 'Other' && (
                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="db-terms-other" className="form-label">Specify Terms<span className="text-danger"> *</span></label>
                    <input
                      type="text"
                      id="db-terms-other"
                      className={`form-control${validationErrors.dryBulkTermsOther ? ' is-invalid' : ''}`}
                      placeholder="Custom terms description"
                      value={dryBulkTermsOther}
                      onChange={(e) => setDryBulkTermsOther(e.target.value)}
                    />
                    {validationErrors.dryBulkTermsOther && <div className="invalid-feedback">{validationErrors.dryBulkTermsOther}</div>}
                  </div>
                </div>
              )}
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-tolerance" className="form-label">Quantity Tolerance (%)</label>
                  <input
                    type="number"
                    id="db-tolerance"
                    className="form-control"
                    placeholder="e.g. 5"
                    value={dryBulkTolerance}
                    onChange={(e) => setDryBulkTolerance(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-draft" className="form-label">Maximum Draft (m)</label>
                  <input
                    type="number"
                    step="any"
                    id="db-draft"
                    className="form-control"
                    placeholder="e.g. 11.5"
                    value={dryBulkMaxDraft}
                    onChange={(e) => setDryBulkMaxDraft(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-moisture" className="form-label">Moisture Content (%)</label>
                  <input
                    type="number"
                    step="any"
                    id="db-moisture"
                    className="form-control"
                    placeholder="e.g. 12"
                    value={dryBulkMoisture}
                    onChange={(e) => setDryBulkMoisture(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="db-gear" className="form-label">Vessel Gear Required</label>
                  <select
                    id="db-gear"
                    className="form-control"
                    value={dryBulkVesselGear}
                    onChange={(e) => setDryBulkVesselGear(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {cargoType === 'General & Breakbulk Cargo' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-desc" className="form-label">Cargo Description<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="bb-desc"
                    className={`form-control${validationErrors.bbDescription ? ' is-invalid' : ''}`}
                    placeholder="e.g. Steel Plates, Wooden Bundles"
                    value={bbDescription}
                    onChange={(e) => setBbDescription(e.target.value)}
                  />
                  {validationErrors.bbDescription && <div className="invalid-feedback">{validationErrors.bbDescription}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-units" className="form-label">Number of Units / Packages (pcs)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="bb-units"
                    className={`form-control${validationErrors.bbUnits ? ' is-invalid' : ''}`}
                    placeholder="e.g. 24"
                    value={bbUnits}
                    onChange={(e) => setBbUnits(e.target.value)}
                  />
                  {validationErrors.bbUnits && <div className="invalid-feedback">{validationErrors.bbUnits}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-packing" className="form-label">Packing Type<span className="text-danger"> *</span></label>
                  <select
                    id="bb-packing"
                    className="form-control"
                    value={bbPackingType}
                    onChange={(e) => setBbPackingType(e.target.value)}
                  >
                    <option value="Bags">Bags</option>
                    <option value="Pallets">Pallets</option>
                    <option value="Bundles">Bundles</option>
                    <option value="Crates">Crates</option>
                    <option value="Loose">Loose</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              {bbPackingType === 'Other' && (
                <div className="col-lg-4">
                  <div className="form-group mb-3">
                    <label htmlFor="bb-packing-other" className="form-label">Specify Packing Type<span className="text-danger"> *</span></label>
                    <input
                      type="text"
                      id="bb-packing-other"
                      className={`form-control${validationErrors.bbPackingTypeOther ? ' is-invalid' : ''}`}
                      placeholder="Custom packaging details"
                      value={bbPackingTypeOther}
                      onChange={(e) => setBbPackingTypeOther(e.target.value)}
                    />
                    {validationErrors.bbPackingTypeOther && <div className="invalid-feedback">{validationErrors.bbPackingTypeOther}</div>}
                  </div>
                </div>
              )}
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-weight" className="form-label">Total Gross Weight (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="bb-weight"
                    className={`form-control${validationErrors.bbWeight ? ' is-invalid' : ''}`}
                    placeholder="Gross weight in MT"
                    value={bbWeight}
                    onChange={(e) => setBbWeight(e.target.value)}
                  />
                  {validationErrors.bbWeight && <div className="invalid-feedback">{validationErrors.bbWeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-vol" className="form-label">Total Volume (m³)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="bb-vol"
                    className={`form-control${validationErrors.bbVolume ? ' is-invalid' : ''}`}
                    placeholder="Volume in cubic meters"
                    value={bbVolume}
                    onChange={(e) => setBbVolume(e.target.value)}
                  />
                  {validationErrors.bbVolume && <div className="invalid-feedback">{validationErrors.bbVolume}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-len" className="form-label">Largest Unit Length (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="bb-len"
                    className={`form-control${validationErrors.bbLength ? ' is-invalid' : ''}`}
                    placeholder="Length in meters"
                    value={bbLength}
                    onChange={(e) => setBbLength(e.target.value)}
                  />
                  {validationErrors.bbLength && <div className="invalid-feedback">{validationErrors.bbLength}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-width" className="form-label">Largest Unit Width (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="bb-width"
                    className={`form-control${validationErrors.bbWidth ? ' is-invalid' : ''}`}
                    placeholder="Width in meters"
                    value={bbWidth}
                    onChange={(e) => setBbWidth(e.target.value)}
                  />
                  {validationErrors.bbWidth && <div className="invalid-feedback">{validationErrors.bbWidth}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-height" className="form-label">Largest Unit Height (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="bb-height"
                    className={`form-control${validationErrors.bbHeight ? ' is-invalid' : ''}`}
                    placeholder="Height in meters"
                    value={bbHeight}
                    onChange={(e) => setBbHeight(e.target.value)}
                  />
                  {validationErrors.bbHeight && <div className="invalid-feedback">{validationErrors.bbHeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-h-weight" className="form-label">Heaviest Unit Weight (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="bb-h-weight"
                    className={`form-control${validationErrors.bbHeaviestWeight ? ' is-invalid' : ''}`}
                    placeholder="Unit weight in MT"
                    value={bbHeaviestWeight}
                    onChange={(e) => setBbHeaviestWeight(e.target.value)}
                  />
                  {validationErrors.bbHeaviestWeight && <div className="invalid-feedback">{validationErrors.bbHeaviestWeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-stackable" className="form-label">Stackable?</label>
                  <select
                    id="bb-stackable"
                    className="form-control"
                    value={bbStackable}
                    onChange={(e) => setBbStackable(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="bb-gear" className="form-label">Ship's Gear Required?</label>
                  <select
                    id="bb-gear"
                    className="form-control"
                    value={bbGearRequired}
                    onChange={(e) => setBbGearRequired(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {cargoType === 'Project & Heavy-Lift Cargo' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-desc" className="form-label">Cargo / Project Description<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="proj-desc"
                    className={`form-control${validationErrors.projDescription ? ' is-invalid' : ''}`}
                    placeholder="Project name / details"
                    value={projDescription}
                    onChange={(e) => setProjDescription(e.target.value)}
                  />
                  {validationErrors.projDescription && <div className="invalid-feedback">{validationErrors.projDescription}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-units" className="form-label">Number of Units (pcs)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="proj-units"
                    className={`form-control${validationErrors.projUnits ? ' is-invalid' : ''}`}
                    placeholder="e.g. 5"
                    value={projUnits}
                    onChange={(e) => setProjUnits(e.target.value)}
                  />
                  {validationErrors.projUnits && <div className="invalid-feedback">{validationErrors.projUnits}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-weight" className="form-label">Total Weight (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="proj-weight"
                    className={`form-control${validationErrors.projWeight ? ' is-invalid' : ''}`}
                    placeholder="Weight in MT"
                    value={projWeight}
                    onChange={(e) => setProjWeight(e.target.value)}
                  />
                  {validationErrors.projWeight && <div className="invalid-feedback">{validationErrors.projWeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-h-weight" className="form-label">Heaviest Unit Weight (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="proj-h-weight"
                    className={`form-control${validationErrors.projHeaviestWeight ? ' is-invalid' : ''}`}
                    placeholder="Heaviest piece weight"
                    value={projHeaviestWeight}
                    onChange={(e) => setProjHeaviestWeight(e.target.value)}
                  />
                  {validationErrors.projHeaviestWeight && <div className="invalid-feedback">{validationErrors.projHeaviestWeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-len" className="form-label">Largest Unit Length (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="proj-len"
                    className={`form-control${validationErrors.projLength ? ' is-invalid' : ''}`}
                    placeholder="Length in meters"
                    value={projLength}
                    onChange={(e) => setProjLength(e.target.value)}
                  />
                  {validationErrors.projLength && <div className="invalid-feedback">{validationErrors.projLength}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-width" className="form-label">Largest Unit Width (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="proj-width"
                    className={`form-control${validationErrors.projWidth ? ' is-invalid' : ''}`}
                    placeholder="Width in meters"
                    value={projWidth}
                    onChange={(e) => setProjWidth(e.target.value)}
                  />
                  {validationErrors.projWidth && <div className="invalid-feedback">{validationErrors.projWidth}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-height" className="form-label">Largest Unit Height (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="proj-height"
                    className={`form-control${validationErrors.projHeight ? ' is-invalid' : ''}`}
                    placeholder="Height in meters"
                    value={projHeight}
                    onChange={(e) => setProjHeight(e.target.value)}
                  />
                  {validationErrors.projHeight && <div className="invalid-feedback">{validationErrors.projHeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-cog-avail" className="form-label">Center of Gravity Available?<span className="text-danger"> *</span></label>
                  <select
                    id="proj-cog-avail"
                    className="form-control"
                    value={projCoGAvailable}
                    onChange={(e) => setProjCoGAvailable(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-lift-avail" className="form-label">Lifting Points Available?<span className="text-danger"> *</span></label>
                  <select
                    id="proj-lift-avail"
                    className="form-control"
                    value={projLiftingPointsAvailable}
                    onChange={(e) => setProjLiftingPointsAvailable(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <FileUpload
                  label="Drawings / Packing List"
                  required
                  onChange={setProjDrawingsFile}
                  selectedFile={projDrawingsFile}
                />
                {validationErrors.projDrawingsFile && <div className="text-danger small mt-1">{validationErrors.projDrawingsFile}</div>}
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-vol" className="form-label">Total Volume (m³)</label>
                  <input
                    type="number"
                    id="proj-vol"
                    className="form-control"
                    placeholder="Volume in m³"
                    value={projVolume}
                    onChange={(e) => setProjVolume(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-lift-cap" className="form-label">Lifting Point Capacity (MT)</label>
                  <input
                    type="number"
                    id="proj-lift-cap"
                    className="form-control"
                    placeholder="Capacity in MT"
                    value={projLiftingPointCapacity}
                    onChange={(e) => setProjLiftingPointCapacity(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-cog" className="form-label">Center of Gravity (m)</label>
                  <input
                    type="number"
                    step="any"
                    id="proj-cog"
                    className="form-control"
                    placeholder="Gravity measurement"
                    value={projCoG}
                    onChange={(e) => setProjCoG(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-lift-req" className="form-label">Required Lifting Capacity (MT)</label>
                  <input
                    type="number"
                    id="proj-lift-req"
                    className="form-control"
                    placeholder="Lifting capacity in MT"
                    value={projRequiredLiftingCapacity}
                    onChange={(e) => setProjRequiredLiftingCapacity(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="proj-self-prop" className="form-label">Self-propelled?</label>
                  <select
                    id="proj-self-prop"
                    className="form-control"
                    value={projSelfPropelled}
                    onChange={(e) => setProjSelfPropelled(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {cargoType === 'Containerized Cargo' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="cont-commodity" className="form-label">Commodity<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="cont-commodity"
                    className={`form-control${validationErrors.containerCommodity ? ' is-invalid' : ''}`}
                    placeholder="e.g. Electronics, Clothing"
                    value={containerCommodity}
                    onChange={(e) => setContainerCommodity(e.target.value)}
                  />
                  {validationErrors.containerCommodity && <div className="invalid-feedback">{validationErrors.containerCommodity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="cont-shipment" className="form-label">Shipment Type<span className="text-danger"> *</span></label>
                  <select
                    id="cont-shipment"
                    className="form-control"
                    value={containerShipmentType}
                    onChange={(e) => setContainerShipmentType(e.target.value)}
                  >
                    <option value="FCL">FCL</option>
                    <option value="LCL">LCL</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="cont-type" className="form-label">Container Type<span className="text-danger"> *</span></label>
                  <select
                    id="cont-type"
                    className="form-control"
                    value={containerType}
                    onChange={(e) => setContainerType(e.target.value)}
                  >
                    <option value="20'GP">20'GP</option>
                    <option value="40'GP">40'GP</option>
                    <option value="40'HC">40'HC</option>
                    <option value="20'OT">20'OT</option>
                    <option value="40'OT">40'OT</option>
                    <option value="20'FR">20'FR</option>
                    <option value="40'FR">40'FR</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="cont-num" className="form-label">Number of Containers<span className="text-danger"> *</span></label>
                  <div className="input-group">
                    <input
                      type="number"
                      id="cont-num"
                      className={`form-control${validationErrors.containerNumber ? ' is-invalid' : ''}`}
                      placeholder="e.g. 5"
                      value={containerNumber}
                      onChange={(e) => setContainerNumber(e.target.value)}
                    />
                    <select
                      className="form-control"
                      style={{ maxWidth: '100px' }}
                      value={containerNumberUnit}
                      onChange={(e) => setContainerNumberUnit(e.target.value)}
                    >
                      <option value="TEU">TEU</option>
                      <option value="FEU">FEU</option>
                    </select>
                    {validationErrors.containerNumber && <div className="invalid-feedback">{validationErrors.containerNumber}</div>}
                  </div>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="cont-weight" className="form-label">Gross Weight per Container (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="cont-weight"
                    className={`form-control${validationErrors.containerWeight ? ' is-invalid' : ''}`}
                    placeholder="Weight in MT"
                    value={containerWeight}
                    onChange={(e) => setContainerWeight(e.target.value)}
                  />
                  {validationErrors.containerWeight && <div className="invalid-feedback">{validationErrors.containerWeight}</div>}
                </div>
              </div>

              {containerShipmentType === 'LCL' && (
                <div className="col-lg-12 border p-3 my-2 bg-light rounded">
                  <h6>LCL Shipment Details</h6>
                  <div className="row">
                    <div className="col-lg-4">
                      <div className="form-group mb-3">
                        <label htmlFor="lcl-packages" className="form-label">Number of Packages (pcs)<span className="text-danger"> *</span></label>
                        <input
                          type="number"
                          id="lcl-packages"
                          className={`form-control${validationErrors.lclPackages ? ' is-invalid' : ''}`}
                          placeholder="e.g. 150"
                          value={lclPackages}
                          onChange={(e) => setLclPackages(e.target.value)}
                        />
                        {validationErrors.lclPackages && <div className="invalid-feedback">{validationErrors.lclPackages}</div>}
                      </div>
                    </div>
                    <div className="col-lg-4">
                      <div className="form-group mb-3">
                        <label htmlFor="lcl-weight" className="form-label">Total Weight (kg)<span className="text-danger"> *</span></label>
                        <input
                          type="number"
                          id="lcl-weight"
                          className={`form-control${validationErrors.lclWeight ? ' is-invalid' : ''}`}
                          placeholder="Total weight in KG"
                          value={lclWeight}
                          onChange={(e) => setLclWeight(e.target.value)}
                        />
                        {validationErrors.lclWeight && <div className="invalid-feedback">{validationErrors.lclWeight}</div>}
                      </div>
                    </div>
                    <div className="col-lg-4">
                      <div className="form-group mb-3">
                        <label htmlFor="lcl-vol" className="form-label">Total Volume (m³)<span className="text-danger"> *</span></label>
                        <input
                          type="number"
                          step="any"
                          id="lcl-vol"
                          className={`form-control${validationErrors.lclVolume ? ' is-invalid' : ''}`}
                          placeholder="Total volume in m³"
                          value={lclVolume}
                          onChange={(e) => setLclVolume(e.target.value)}
                        />
                        {validationErrors.lclVolume && <div className="invalid-feedback">{validationErrors.lclVolume}</div>}
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {cargoType === 'RoRo Cargo' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="roro-type" className="form-label">Vehicle / Equipment Type<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="roro-type"
                    className={`form-control${validationErrors.roroVehicleType ? ' is-invalid' : ''}`}
                    placeholder="e.g. Excavator, SUV"
                    value={roroVehicleType}
                    onChange={(e) => setRoroVehicleType(e.target.value)}
                  />
                  {validationErrors.roroVehicleType && <div className="invalid-feedback">{validationErrors.roroVehicleType}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="roro-model" className="form-label">Make / Model<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="roro-model"
                    className={`form-control${validationErrors.roroMakeModel ? ' is-invalid' : ''}`}
                    placeholder="e.g. Caterpillar 320"
                    value={roroMakeModel}
                    onChange={(e) => setRoroMakeModel(e.target.value)}
                  />
                  {validationErrors.roroMakeModel && <div className="invalid-feedback">{validationErrors.roroMakeModel}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="roro-qty" className="form-label">Quantity (units)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="roro-qty"
                    className={`form-control${validationErrors.roroQuantity ? ' is-invalid' : ''}`}
                    placeholder="Number of units"
                    value={roroQuantity}
                    onChange={(e) => setRoroQuantity(e.target.value)}
                  />
                  {validationErrors.roroQuantity && <div className="invalid-feedback">{validationErrors.roroQuantity}</div>}
                </div>
              </div>
              <div className="col-lg-3">
                <div className="form-group mb-3">
                  <label htmlFor="roro-len" className="form-label">Length per Unit (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="roro-len"
                    className={`form-control${validationErrors.roroLength ? ' is-invalid' : ''}`}
                    placeholder="Length"
                    value={roroLength}
                    onChange={(e) => setRoroLength(e.target.value)}
                  />
                  {validationErrors.roroLength && <div className="invalid-feedback">{validationErrors.roroLength}</div>}
                </div>
              </div>
              <div className="col-lg-3">
                <div className="form-group mb-3">
                  <label htmlFor="roro-width" className="form-label">Width per Unit (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="roro-width"
                    className={`form-control${validationErrors.roroWidth ? ' is-invalid' : ''}`}
                    placeholder="Width"
                    value={roroWidth}
                    onChange={(e) => setRoroWidth(e.target.value)}
                  />
                  {validationErrors.roroWidth && <div className="invalid-feedback">{validationErrors.roroWidth}</div>}
                </div>
              </div>
              <div className="col-lg-3">
                <div className="form-group mb-3">
                  <label htmlFor="roro-height" className="form-label">Height per Unit (m)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="roro-height"
                    className={`form-control${validationErrors.roroHeight ? ' is-invalid' : ''}`}
                    placeholder="Height"
                    value={roroHeight}
                    onChange={(e) => setRoroHeight(e.target.value)}
                  />
                  {validationErrors.roroHeight && <div className="invalid-feedback">{validationErrors.roroHeight}</div>}
                </div>
              </div>
              <div className="col-lg-3">
                <div className="form-group mb-3">
                  <label htmlFor="roro-weight" className="form-label">Weight per Unit (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="roro-weight"
                    className={`form-control${validationErrors.roroWeight ? ' is-invalid' : ''}`}
                    placeholder="Weight in MT"
                    value={roroWeight}
                    onChange={(e) => setRoroWeight(e.target.value)}
                  />
                  {validationErrors.roroWeight && <div className="invalid-feedback">{validationErrors.roroWeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="roro-run" className="form-label">Operational / Running?<span className="text-danger"> *</span></label>
                  <select
                    id="roro-run"
                    className="form-control"
                    value={roroOperational}
                    onChange={(e) => setRoroOperational(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="roro-self-prop" className="form-label">Self-propelled?<span className="text-danger"> *</span></label>
                  <select
                    id="roro-self-prop"
                    className="form-control"
                    value={roroSelfPropelled}
                    onChange={(e) => setRoroSelfPropelled(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {cargoType === 'Liquid Bulk' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-product" className="form-label">Product Name<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="liq-product"
                    className={`form-control${validationErrors.liquidProductName ? ' is-invalid' : ''}`}
                    placeholder="e.g. Crude Oil, Ethanol"
                    value={liquidProductName}
                    onChange={(e) => setLiquidProductName(e.target.value)}
                  />
                  {validationErrors.liquidProductName && <div className="invalid-feedback">{validationErrors.liquidProductName}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-qty" className="form-label">Quantity (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="liq-qty"
                    className={`form-control${validationErrors.liquidQuantity ? ' is-invalid' : ''}`}
                    placeholder="Quantity in MT"
                    value={liquidQuantity}
                    onChange={(e) => setLiquidQuantity(e.target.value)}
                  />
                  {validationErrors.liquidQuantity && <div className="invalid-feedback">{validationErrors.liquidQuantity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-density" className="form-label">Density (kg/m³ @ °C)<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="liq-density"
                    className={`form-control${validationErrors.liquidDensity ? ' is-invalid' : ''}`}
                    placeholder="e.g. 850 @ 15°C"
                    value={liquidDensity}
                    onChange={(e) => setLiquidDensity(e.target.value)}
                  />
                  {validationErrors.liquidDensity && <div className="invalid-feedback">{validationErrors.liquidDensity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-temp" className="form-label">Loading Temperature (°C)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="liq-temp"
                    className={`form-control${validationErrors.liquidLoadingTemp ? ' is-invalid' : ''}`}
                    placeholder="Loading temp in °C"
                    value={liquidLoadingTemp}
                    onChange={(e) => setLiquidLoadingTemp(e.target.value)}
                  />
                  {validationErrors.liquidLoadingTemp && <div className="invalid-feedback">{validationErrors.liquidLoadingTemp}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-grades" className="form-label">Number of Grades<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="liq-grades"
                    className={`form-control${validationErrors.liquidGrades ? ' is-invalid' : ''}`}
                    placeholder="e.g. 2"
                    value={liquidGrades}
                    onChange={(e) => setLiquidGrades(e.target.value)}
                  />
                  {validationErrors.liquidGrades && <div className="invalid-feedback">{validationErrors.liquidGrades}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-heat" className="form-label">Heating Required?<span className="text-danger"> *</span></label>
                  <select
                    id="liq-heat"
                    className="form-control"
                    value={liquidHeatingRequired}
                    onChange={(e) => setLiquidHeatingRequired(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-l-rate" className="form-label">Loading Rate (m³/h)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="liq-l-rate"
                    className={`form-control${validationErrors.liquidLoadingRate ? ' is-invalid' : ''}`}
                    placeholder="e.g. 1500"
                    value={liquidLoadingRate}
                    onChange={(e) => setLiquidLoadingRate(e.target.value)}
                  />
                  {validationErrors.liquidLoadingRate && <div className="invalid-feedback">{validationErrors.liquidLoadingRate}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-d-rate" className="form-label">Discharge Rate (m³/h)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="liq-d-rate"
                    className={`form-control${validationErrors.liquidDischargeRate ? ' is-invalid' : ''}`}
                    placeholder="e.g. 1200"
                    value={liquidDischargeRate}
                    onChange={(e) => setLiquidDischargeRate(e.target.value)}
                  />
                  {validationErrors.liquidDischargeRate && <div className="invalid-feedback">{validationErrors.liquidDischargeRate}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-flash" className="form-label">Flash Point (°C)</label>
                  <input
                    type="number"
                    id="liq-flash"
                    className="form-control"
                    placeholder="e.g. 62"
                    value={liquidFlashPoint}
                    onChange={(e) => setLiquidFlashPoint(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-visc" className="form-label">Viscosity (cSt @ °C)</label>
                  <input
                    type="text"
                    id="liq-visc"
                    className="form-control"
                    placeholder="e.g. 5.4 @ 40°C"
                    value={liquidViscosity}
                    onChange={(e) => setLiquidViscosity(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="liq-carriage" className="form-label">Required Carriage Temperature (°C)</label>
                  <input
                    type="number"
                    id="liq-carriage"
                    className="form-control"
                    placeholder="Carriage temp in °C"
                    value={liquidCarriageTemp}
                    onChange={(e) => setLiquidCarriageTemp(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <FileUpload
                  label="SDS"
                  onChange={setLiquidSdsFile}
                  selectedFile={liquidSdsFile}
                />
              </div>
            </div>
          )}

          {cargoType === 'Gas Cargo' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-type" className="form-label">Gas Cargo Type<span className="text-danger"> *</span></label>
                  <select
                    id="gas-type"
                    className="form-control"
                    value={gasCargoType}
                    onChange={(e) => setGasCargoType(e.target.value)}
                  >
                    <option value="LNG">LNG</option>
                    <option value="LPG">LPG</option>
                    <option value="Ethylene">Ethylene</option>
                    <option value="Ammonia">Ammonia</option>
                    <option value="Other Liquefied Gas">Other Liquefied Gas</option>
                    <option value="Compressed Gas">Compressed Gas</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-prod" className="form-label">Product<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="gas-prod"
                    className={`form-control${validationErrors.gasProduct ? ' is-invalid' : ''}`}
                    placeholder="e.g. Propane"
                    value={gasProduct}
                    onChange={(e) => setGasProduct(e.target.value)}
                  />
                  {validationErrors.gasProduct && <div className="invalid-feedback">{validationErrors.gasProduct}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-qty" className="form-label">Quantity<span className="text-danger"> *</span></label>
                  <div className="input-group">
                    <input
                      type="number"
                      id="gas-qty"
                      className={`form-control${validationErrors.gasQuantity ? ' is-invalid' : ''}`}
                      placeholder="Quantity"
                      value={gasQuantity}
                      onChange={(e) => setGasQuantity(e.target.value)}
                    />
                    <select
                      className="form-control"
                      style={{ maxWidth: '90px' }}
                      value={gasQuantityUnit}
                      onChange={(e) => setGasQuantityUnit(e.target.value)}
                    >
                      <option value="MT">MT</option>
                      <option value="m³">m³</option>
                    </select>
                    {validationErrors.gasQuantity && <div className="invalid-feedback">{validationErrors.gasQuantity}</div>}
                  </div>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-density" className="form-label">Density (kg/m³)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="gas-density"
                    className={`form-control${validationErrors.gasDensity ? ' is-invalid' : ''}`}
                    placeholder="Density in kg/m³"
                    value={gasDensity}
                    onChange={(e) => setGasDensity(e.target.value)}
                  />
                  {validationErrors.gasDensity && <div className="invalid-feedback">{validationErrors.gasDensity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-temp" className="form-label">Loading Temperature (°C)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="gas-temp"
                    className={`form-control${validationErrors.gasLoadingTemp ? ' is-invalid' : ''}`}
                    placeholder="Loading temp in °C"
                    value={gasLoadingTemp}
                    onChange={(e) => setGasLoadingTemp(e.target.value)}
                  />
                  {validationErrors.gasLoadingTemp && <div className="invalid-feedback">{validationErrors.gasLoadingTemp}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-press" className="form-label">Loading Pressure (bar)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="gas-press"
                    className={`form-control${validationErrors.gasLoadingPressure ? ' is-invalid' : ''}`}
                    placeholder="Pressure in bar"
                    value={gasLoadingPressure}
                    onChange={(e) => setGasLoadingPressure(e.target.value)}
                  />
                  {validationErrors.gasLoadingPressure && <div className="invalid-feedback">{validationErrors.gasLoadingPressure}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-l-rate" className="form-label">Loading Rate (m³/h)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="gas-l-rate"
                    className={`form-control${validationErrors.gasLoadingRate ? ' is-invalid' : ''}`}
                    placeholder="Loading rate"
                    value={gasLoadingRate}
                    onChange={(e) => setGasLoadingRate(e.target.value)}
                  />
                  {validationErrors.gasLoadingRate && <div className="invalid-feedback">{validationErrors.gasLoadingRate}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="gas-d-rate" className="form-label">Discharge Rate (m³/h)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="gas-d-rate"
                    className={`form-control${validationErrors.gasDischargeRate ? ' is-invalid' : ''}`}
                    placeholder="Discharge rate"
                    value={gasDischargeRate}
                    onChange={(e) => setGasDischargeRate(e.target.value)}
                  />
                  {validationErrors.gasDischargeRate && <div className="invalid-feedback">{validationErrors.gasDischargeRate}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <FileUpload
                  label="Cargo Specification"
                  onChange={setGasSpecFile}
                  selectedFile={gasSpecFile}
                />
              </div>
            </div>
          )}

          {cargoType === 'Refrigerated & Perishable Cargo' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-transport" className="form-label">Transport Method<span className="text-danger"> *</span></label>
                  <select
                    id="ref-transport"
                    className="form-control"
                    value={reeferTransportMethod}
                    onChange={(e) => setReeferTransportMethod(e.target.value)}
                  >
                    <option value="Reefer Container">Reefer Container</option>
                    <option value="Refrigerated Vessel">Refrigerated Vessel</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-commodity" className="form-label">Commodity<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="ref-commodity"
                    className={`form-control${validationErrors.reeferCommodity ? ' is-invalid' : ''}`}
                    placeholder="e.g. Frozen Fish, Citrus"
                    value={reeferCommodity}
                    onChange={(e) => setReeferCommodity(e.target.value)}
                  />
                  {validationErrors.reeferCommodity && <div className="invalid-feedback">{validationErrors.reeferCommodity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-qty" className="form-label">Quantity (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="ref-qty"
                    className={`form-control${validationErrors.reeferQuantity ? ' is-invalid' : ''}`}
                    placeholder="Quantity in MT"
                    value={reeferQuantity}
                    onChange={(e) => setReeferQuantity(e.target.value)}
                  />
                  {validationErrors.reeferQuantity && <div className="invalid-feedback">{validationErrors.reeferQuantity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-packing" className="form-label">Packing Type<span className="text-danger"> *</span></label>
                  <select
                    id="ref-packing"
                    className="form-control"
                    value={reeferPackingType}
                    onChange={(e) => setReeferPackingType(e.target.value)}
                  >
                    <option value="Cartons">Cartons</option>
                    <option value="Pallets">Pallets</option>
                    <option value="Bags">Bags</option>
                    <option value="Bulk">Bulk</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-set-temp" className="form-label">Required Set Temperature (°C)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="ref-set-temp"
                    className={`form-control${validationErrors.reeferSetTemp ? ' is-invalid' : ''}`}
                    placeholder="Set temperature"
                    value={reeferSetTemp}
                    onChange={(e) => setReeferSetTemp(e.target.value)}
                  />
                  {validationErrors.reeferSetTemp && <div className="invalid-feedback">{validationErrors.reeferSetTemp}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-min-temp" className="form-label">Minimum Temperature (°C)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="ref-min-temp"
                    className={`form-control${validationErrors.reeferMinTemp ? ' is-invalid' : ''}`}
                    placeholder="Minimum temperature"
                    value={reeferMinTemp}
                    onChange={(e) => setReeferMinTemp(e.target.value)}
                  />
                  {validationErrors.reeferMinTemp && <div className="invalid-feedback">{validationErrors.reeferMinTemp}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-max-temp" className="form-label">Maximum Temperature (°C)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="ref-max-temp"
                    className={`form-control${validationErrors.reeferMaxTemp ? ' is-invalid' : ''}`}
                    placeholder="Maximum temperature"
                    value={reeferMaxTemp}
                    onChange={(e) => setReeferMaxTemp(e.target.value)}
                  />
                  {validationErrors.reeferMaxTemp && <div className="invalid-feedback">{validationErrors.reeferMaxTemp}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-vent" className="form-label">Ventilation (m³/h)</label>
                  <input
                    type="number"
                    id="ref-vent"
                    className="form-control"
                    placeholder="Ventilation rate"
                    value={reeferVentilation}
                    onChange={(e) => setReeferVentilation(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-humidity" className="form-label">Relative Humidity (% RH)</label>
                  <input
                    type="number"
                    id="ref-humidity"
                    className="form-control"
                    placeholder="Humidity percentage"
                    value={reeferHumidity}
                    onChange={(e) => setReeferHumidity(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="ref-ca" className="form-label">Controlled Atmosphere</label>
                  <select
                    id="ref-ca"
                    className="form-control"
                    value={reeferControlledAtmosphere}
                    onChange={(e) => setReeferControlledAtmosphere(e.target.value)}
                  >
                    <option value="No">No</option>
                    <option value="Yes">Yes</option>
                  </select>
                </div>
              </div>

              {reeferTransportMethod === 'Reefer Container' && (
                <div className="col-lg-12 border p-3 my-2 bg-light rounded">
                  <h6>Reefer Container Specifics</h6>
                  <div className="row">
                    <div className="col-lg-4">
                      <div className="form-group mb-3">
                        <label htmlFor="rc-type" className="form-label">Reefer Type<span className="text-danger"> *</span></label>
                        <select
                          id="rc-type"
                          className="form-control"
                          value={reeferContainerType}
                          onChange={(e) => setReeferContainerType(e.target.value)}
                        >
                          <option value="20'RF">20'RF</option>
                          <option value="40'RF">40'RF</option>
                          <option value="40'HC RF">40'HC RF</option>
                        </select>
                      </div>
                    </div>
                    <div className="col-lg-4">
                      <div className="form-group mb-3">
                        <label htmlFor="rc-count" className="form-label">Number of Containers (units)<span className="text-danger"> *</span></label>
                        <input
                          type="number"
                          id="rc-count"
                          className={`form-control${validationErrors.reeferContainerCount ? ' is-invalid' : ''}`}
                          placeholder="e.g. 3"
                          value={reeferContainerCount}
                          onChange={(e) => setReeferContainerCount(e.target.value)}
                        />
                        {validationErrors.reeferContainerCount && <div className="invalid-feedback">{validationErrors.reeferContainerCount}</div>}
                      </div>
                    </div>
                    <div className="col-lg-4">
                      <div className="form-group mb-3">
                        <label htmlFor="rc-weight" className="form-label">Gross Weight / Container (MT)<span className="text-danger"> *</span></label>
                        <input
                          type="number"
                          step="any"
                          id="rc-weight"
                          className={`form-control${validationErrors.reeferContainerWeight ? ' is-invalid' : ''}`}
                          placeholder="Weight in MT"
                          value={reeferContainerWeight}
                          onChange={(e) => setReeferContainerWeight(e.target.value)}
                        />
                        {validationErrors.reeferContainerWeight && <div className="invalid-feedback">{validationErrors.reeferContainerWeight}</div>}
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {cargoType === 'Other' && (
            <div className="row">
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="o-desc" className="form-label">Cargo Description<span className="text-danger"> *</span></label>
                  <input
                    type="text"
                    id="o-desc"
                    className={`form-control${validationErrors.otherDescription ? ' is-invalid' : ''}`}
                    placeholder="Details about the cargo"
                    value={otherDescription}
                    onChange={(e) => setOtherDescription(e.target.value)}
                  />
                  {validationErrors.otherDescription && <div className="invalid-feedback">{validationErrors.otherDescription}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="o-qty" className="form-label">Quantity (pcs)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="o-qty"
                    className={`form-control${validationErrors.otherQuantity ? ' is-invalid' : ''}`}
                    placeholder="Pieces count"
                    value={otherQuantity}
                    onChange={(e) => setOtherQuantity(e.target.value)}
                  />
                  {validationErrors.otherQuantity && <div className="invalid-feedback">{validationErrors.otherQuantity}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="o-weight" className="form-label">Total Weight (MT)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    id="o-weight"
                    className={`form-control${validationErrors.otherWeight ? ' is-invalid' : ''}`}
                    placeholder="Total weight in MT"
                    value={otherWeight}
                    onChange={(e) => setOtherWeight(e.target.value)}
                  />
                  {validationErrors.otherWeight && <div className="invalid-feedback">{validationErrors.otherWeight}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="o-vol" className="form-label">Total Volume (m³)<span className="text-danger"> *</span></label>
                  <input
                    type="number"
                    step="any"
                    id="o-vol"
                    className={`form-control${validationErrors.otherVolume ? ' is-invalid' : ''}`}
                    placeholder="Total volume in m³"
                    value={otherVolume}
                    onChange={(e) => setOtherVolume(e.target.value)}
                  />
                  {validationErrors.otherVolume && <div className="invalid-feedback">{validationErrors.otherVolume}</div>}
                </div>
              </div>
              <div className="col-lg-4">
                <div className="form-group mb-3">
                  <label htmlFor="o-dims" className="form-label">Largest Dimensions (m)</label>
                  <input
                    type="text"
                    id="o-dims"
                    className="form-control"
                    placeholder="Length x Width x Height (m)"
                    value={otherDimensions}
                    onChange={(e) => setOtherDimensions(e.target.value)}
                  />
                </div>
              </div>
              <div className="col-lg-4">
                <FileUpload
                  label="Documents"
                  onChange={setOtherDocFile}
                  selectedFile={otherDocFile}
                />
              </div>
              <div className="col-lg-12">
                <div className="form-group mb-3">
                  <label htmlFor="o-spec" className="form-label">Special Requirements</label>
                  <textarea
                    id="o-spec"
                    className="form-control"
                    placeholder="Enter any other instructions or restrictions"
                    value={otherSpecialRequirements}
                    rows={3}
                    onChange={(e) => setOtherSpecialRequirements(e.target.value)}
                  />
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Contact Details */}
        <div className="col-lg-12 border-top pt-3 mt-3">
          <div className="heading_quote">
            <h3>Your Contact Details</h3>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="fname" className="form-label">First Name<span className="text-danger"> *</span></label>
            <input
              type="text"
              id="fname"
              className={`form-control${validationErrors.fname ? ' is-invalid' : ''}`}
              placeholder="First Name"
              value={fname}
              onChange={(e) => setFname(e.target.value)}
            />
            {validationErrors.fname && <div className="invalid-feedback">{validationErrors.fname}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="lname" className="form-label">Last Name<span className="text-danger"> *</span></label>
            <input
              type="text"
              id="lname"
              className={`form-control${validationErrors.lname ? ' is-invalid' : ''}`}
              placeholder="Last Name"
              value={lname}
              onChange={(e) => setLname(e.target.value)}
            />
            {validationErrors.lname && <div className="invalid-feedback">{validationErrors.lname}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="email" className="form-label">Your Email<span className="text-danger"> *</span></label>
            <input
              type="email"
              id="email"
              className={`form-control${validationErrors.email ? ' is-invalid' : ''}`}
              placeholder="e.g. name@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
            {validationErrors.email && <div className="invalid-feedback">{validationErrors.email}</div>}
          </div>
        </div>

        <div className="col-lg-6">
          <div className="form-group mb-3">
            <label htmlFor="phoneNumber" className="form-label">Your Phone Number<span className="text-danger"> *</span></label>
            <div className="input-group">
              <select
                className="form-control"
                style={{ maxWidth: '120px' }}
                value={countryCode}
                onChange={(e) => setCountryCode(e.target.value)}
              >
                <option value="+20">Egypt (+20)</option>
                <option value="+966">Saudi (+966)</option>
                <option value="+971">UAE (+971)</option>
                <option value="+965">Kuwait (+965)</option>
                <option value="+974">Qatar (+974)</option>
                <option value="+968">Oman (+968)</option>
                <option value="+973">Bahrain (+973)</option>
                <option value="+962">Jordan (+962)</option>
                <option value="+961">Lebanon (+961)</option>
                <option value="+1">USA (+1)</option>
                <option value="+44">UK (+44)</option>
              </select>
              <input
                type="tel"
                id="phoneNumber"
                className={`form-control${validationErrors.phoneRaw ? ' is-invalid' : ''}`}
                placeholder="Phone Number"
                value={phoneRaw}
                onChange={(e) => setPhoneRaw(e.target.value)}
              />
            </div>
            {validationErrors.phoneRaw && (
              <div className="invalid-feedback" style={{ display: 'block' }}>
                {validationErrors.phoneRaw}
              </div>
            )}
          </div>
        </div>

        <div className="col-lg-12 mt-4">
          <div className="quote_submit_button text-end">
            <button type="submit" className="btn btn-theme" disabled={submitting}>
              {submitting ? 'Registering Cargo…' : 'Register Cargo'}
            </button>
          </div>
        </div>
      </div>
    </form>
  );
};

export default RequestQuoteForm;