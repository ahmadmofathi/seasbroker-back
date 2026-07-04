import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import SectionHeading from '../component/Common/SectionHeading';
import { useNavigate } from 'react-router';
import { useState } from 'react';

// TODO: Replace with more secure way of tracking shipments
const TrackShipmentForm: React.FC = () => {
  const navigate = useNavigate();
  // let options1 = [
  //   {
  //     text: "Select a city...",
  //     value: ""
  //   },
  //   {
  //     text: "Singapore",
  //     value: "Singapore"
  //   },
  //   {
  //     text: "Rotterdam",
  //     value: "Rotterdam"
  //   },
  //   {
  //     text: "Shanghai",
  //     value: "Shanghai"
  //   },
  //   {
  //     text: "Los Angeles",
  //     value: "Los Angeles"
  //   },
  //   {
  //     text: "Hamburg",
  //     value: "Hamburg"
  //   },
  //   {
  //     text: "Dubai",
  //     value: "Dubai"
  //   },
  //   {
  //     text: "Busan",
  //     value: "Busan"
  //   },
  //   {
  //     text: "Antwerp",
  //     value: "Antwerp"
  //   },
  //   {
  //     text: "Hong Kong",
  //     value: "Hong Kong"
  //   },
  //   {
  //     text: "Port of New York and New Jersey",
  //     value: "New York"
  //   },
  //   {
  //     text: "Manila",
  //     value: "Manila"
  //   },
  //   {
  //     text: "Mumbai",
  //     value: "Mumbai"
  //   },
  //   {
  //     text: "Santos",
  //     value: "Santos"
  //   },
  //   {
  //     text: "Jebel Ali",
  //     value: "Jebel Ali"
  //   },
  //   {
  //     text: "Colombo",
  //     value: "Colombo"
  //   },
  // ];

  const [trackingNumber, setTrackingNumber] = useState<string>('');
  const [email, setEmail] = useState<string>('');

  const handleSubmit: React.FormEventHandler = (e) => {
    e.preventDefault();
    // Redirect to track shipment page with query parameters
    void navigate(`/your_shipment?id=${(trackingNumber)}&email=${(email)}`);
  };

  return (
    <>
      <section id="track_shipment_area">
        <div className="container">
          <SectionHeading heading="Track Your Shipment" para="Solving your supply chain needs from end to end, taking the
        complexity out of container shipping. We are at the forefront of developing innovation."/>
          <div className="row">
            <div className="col-lg-8 offset-lg-2 col-md-12 col-sm-12 col-12">
              <div className="track_area_form">
                <form onSubmit={handleSubmit} id="track_form_area">

                  <div className="form-group">
                    <label htmlFor='tracking'>Tracking Number</label>
                    <input
                      type='text'
                      name='tracking'
                      value={trackingNumber}
                      onChange={(e) => {setTrackingNumber(e.target.value)}}
                      required
                      className={'form-control'}
                      placeholder={'Eg: AWB Num or CB Num'}
                    />
                  </div>
                  <div className="form-group">
                    <label htmlFor='email'>Email Address</label>
                    <input
                      type='email'
                      name='email'
                      value={email}
                      onChange={(e) => {setEmail(e.target.value)}}
                      required
                      className='form-control'
                      placeholder="example@email.com"
                    />
                  </div>
                  <div className="track_now_btn">
                    <button type='submit' className="btn btn-theme">Track Now</button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  )
};

const TrackYourShip: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Track Your Shipment" page="Track Your Shipment" />
      <TrackShipmentForm />
      <OurPartner />
    </>
  )
};

export default TrackYourShip;
