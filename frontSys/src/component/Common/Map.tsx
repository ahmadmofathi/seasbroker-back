import { GoogleMap, LoadScript, MarkerF } from "@react-google-maps/api";
import ports from "../../utils/ports.json";

interface PortMarker {
  id: string;
  name: string;
  country: string;
  position: { lat: number; lng: number };
}

const Map: React.FC = () => {
  const mapStyles = {
    height: "50vh",
    width: "100%",
    margin: "0 0 0 0",
  };

  const portMarkers: PortMarker[] = Object.entries(ports)
    .map(([id, port]) => {
      if (!("coordinates" in port)) return null;
      const coordinates = port.coordinates;
      if (!coordinates || coordinates.length !== 2) return null;

      // Source data stores coordinates as [lng, lat].
      const [lng, lat] = coordinates;
      if (typeof lat !== "number" || typeof lng !== "number") return null;

      return {
        id,
        name: port.name,
        country: port.country ?? "",
        position: { lat, lng },
      };
    })
    .filter((marker): marker is PortMarker => marker !== null)
    .slice(0, 500);

  const defaultCenter = {
    lat: 31.21564,
    lng: 29.95527,
  };
  return (
    <>
      <LoadScript googleMapsApiKey="AIzaSyDtygZ5JPTLgwFLA8nU6bb4d_6SSLlTPGw">
        {" "}
        {/* TODO: Replace with own google maps api key. */}
        <GoogleMap
          mapContainerStyle={mapStyles}
          zoom={3}
          center={defaultCenter}
        >
          {portMarkers.map((port) => (
            <MarkerF
              key={port.id}
              position={port.position}
              title={`${port.name}${port.country ? ` - ${port.country}` : ""}`}
            />
          ))}
        </GoogleMap>
      </LoadScript>
    </>
  );
};

export default Map;
