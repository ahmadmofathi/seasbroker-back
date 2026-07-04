import { GoogleMap, LoadScript } from '@react-google-maps/api';

const Map: React.FC = () => {
  const mapStyles = {
    height: "50vh",
    width: "100%",
    margin: "0 0 0 0"
  };
  const defaultCenter = {
    lat: 31.21564, lng: 29.95527
  }
  return (
    <>
      <LoadScript googleMapsApiKey="AIzaSyDtygZ5JPTLgwFLA8nU6bb4d_6SSLlTPGw"> {/* TODO: Replace with own google maps api key. */}
        <GoogleMap mapContainerStyle={mapStyles} zoom={10} center={defaultCenter}></GoogleMap>
      </LoadScript>
    </>
  );
};

export default Map;
