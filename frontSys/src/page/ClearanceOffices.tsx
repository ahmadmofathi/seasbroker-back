import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import SectionHeading from '../component/Common/SectionHeading';
import pb from '../utils/pocketbase';
import type { Office } from '../types/office';
import { useAlert } from '../context/AlertContext';

const TrackYourShip: React.FC = () => {
  const [offices, setOffices] = useState<Office[]>([]);
  const [filteredOffices, setFilteredOffices] = useState<Office[]>([]);
  const { error: showError, warning } = useAlert();
  console.log(offices);

  // Function to parse query parameters
  const useQuery = () => {
    return new URLSearchParams(useLocation().search);
  };

  const query = useQuery();
  const delivery_port = query.get("delivery-port");
  console.log(delivery_port);

  useEffect(() => {
    const getShipmentDetails = async () => {
      try {
        // Call API to get clearance offices and await the result
        const officesData = (await pb.collection('customer_clearance_offices').getFullList())
          .map(offices => offices.expand) as Office[]; // Ensure the data is in the correct format
        if (officesData.length > 0) {
          setOffices(officesData); // Set offices data
          setFilteredOffices(officesData); // Initially, show all offices
        } else {
          warning("No offices found in this region.");
        } 
      }
      catch (err) {
        showError("Failed to fetch offices details");
        console.error(err);
      }
    };

    void getShipmentDetails(); // Call the function
  }, []);

  useEffect(() => {
    if (delivery_port) {
      console.log('Query Parameter:', delivery_port); // Log the query parameter
      console.log('Offices:', offices); // Log the offices data for inspection

      const normalizedDeliveryPort = delivery_port.trim().toLowerCase(); // Trim and normalize the query string

      const filtered = offices.filter((office) => {
        const normalizedCountry = office.port.trim().toLowerCase(); // Trim and normalize the office country

        // Log each comparison to debug
        console.log('Normalized Query:', normalizedDeliveryPort);
        console.log('Normalized Country:', normalizedCountry);

        // Check if delivery-port (query) matches the country
        return normalizedCountry.includes(normalizedDeliveryPort);
      });

      setFilteredOffices(filtered); // Set the filtered offices
    }
  }, [delivery_port, offices]);

  return (
    <>
      <CommonBanner heading="Clearance Offices" page="Clearance Offices" />
      <section id="track_shipment_area">
        <div className="container">
          <SectionHeading heading="Clearance Offices Near You" />
          <div className="row">
            <div className="col-12 col-md-12 col-sm-12">
              <div className="track_area_form">
                {/* Display table of offices if data is available */}
                {filteredOffices.length > 0 ? (
                  <table className="table">
                    <thead>
                      <tr>
                        <th>Office Name</th>
                        <th>Address</th>
                        <th>Email</th>
                        <th>Phone</th>
                        <th>Location</th>
                        <th>Enquire by Email</th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredOffices.map((office, index) => (
                        <tr key={index}>
                          <td>{office.name}</td>
                          <td>{office.address}</td>
                          <td>{office.email}</td>
                          <td>{office.phone_number}</td>
                          <td><a href={office.location} style={{ color: "blue" }} target="_blank" rel="noopener noreferrer">View Location</a></td>
                          <td><a href={`mailto:${office.email}`} style={{ color: "blue" }}>Enquire</a></td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ) : (
                  <p>No offices found matching.</p>
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

export default TrackYourShip;
