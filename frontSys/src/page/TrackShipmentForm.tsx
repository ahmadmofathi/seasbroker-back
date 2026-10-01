import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import SectionHeading from '../component/Common/SectionHeading';
import { useNavigate, useSearchParams } from 'react-router';
import { useState } from 'react';

const TrackShipmentForm: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [trackingNumber, setTrackingNumber] = useState<string>(() => searchParams.get('id') ?? '');
  const [email, setEmail] = useState<string>('');

  const handleSubmit: React.FormEventHandler = (e) => {
    e.preventDefault();
    // Hand the lookup over in router state so the email never appears in the URL.
    void navigate('/your_shipment', { state: { number: trackingNumber.trim(), email: email.trim() } });
  };

  return (
    <>
      <section id="track_shipment_area">
        <div className="container">
          <SectionHeading heading="Track Your Service" para="Solving your supply chain needs from end to end, taking the
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
                      placeholder={'Eg: SB-7C4A9F2E'}
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
      <CommonBanner heading="Track Your Service" page="Track Your Service" />
      <TrackShipmentForm />
      <OurPartner />
    </>
  )
};

export default TrackYourShip;
