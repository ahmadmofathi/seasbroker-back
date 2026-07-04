import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import SectionHeading from '../component/Common/SectionHeading';
import pb from '../utils/pocketbase';
import type { Shipment } from '../types/shipment';

const TrackShipmentView: React.FC = () => {
  const [shipment, setShipment] = useState<Shipment | null>(null); // Set initial state to null
  const [error, setError] = useState<string | null>(null); // Optional: to handle errors

  // Function to parse query parameters
  const useQuery = () => {
    return new URLSearchParams(useLocation().search);
  }

  const query = useQuery();
  const id = query.get("id");
  const email = query.get("email");

  console.log(id, email);

  useEffect(() => {
    const getShipmentDetails = async (id: string, email: string) => {
      try {
        const shipmentData = await pb.collection('shipments').getFirstListItem(
          `tracking_id='${id}' && email='${email}'`
        );
        setShipment(shipmentData.expand as Shipment); // Set the shipment data
      } catch (error) {
        console.error("Error fetching shipment data:", error);
        setError("Failed to fetch shipment data");
      }
    }

    if (id && email) {
      void getShipmentDetails(id, email);
    }
  }, [id, email]);

  return (
    <>
      <CommonBanner heading="Your Shipment" page="Your Shipment" />
      <section id="track_shipment_area">
        <div className="container">
          <SectionHeading heading="Your Shipment Details" />
          <div className="row">
            <div className="col-lg-8 offset-lg-2 col-md-12 col-sm-12 col-12">
              <div className="track_area_form">
                {/* Display shipment details if available */}
                {shipment ? (
                  <div>
                    <p><strong>Tracking ID:</strong> {shipment.tracking_id}</p>
                    <p><strong>Email:</strong> {shipment.email}</p>

                    <p><strong>Current Port:</strong> {shipment.current_port}</p>
                    <p><strong>Delivery City:</strong> {shipment.delivery_city}</p>
                    <p><strong>Departure City:</strong> {shipment.departure_city}</p>
                  </div>
                ) : (
                  <p style={{ color: 'red' }}>{error}</p>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default TrackShipmentView;
