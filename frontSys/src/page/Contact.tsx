import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import ContactForm from '../component/Contact/ContactForm';
import ContactInfo from '../component/Contact/ContactInfo';
import ContactMap from '../component/Contact/ContactMap';

const Contact: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Contact Us" page="Contact Us" />
      <section id="contact_area_main">
        <div className="container">
          <div className="row">
            <div className="col-lg-12">
              <div className="section_heading_center">
                <h2>Contact Info</h2>
              </div>
            </div>
          </div>
          <div className="contact_form_info_area">
            <div className="row">
              <div className="col-lg-6">
                <div className="contact_form_main">
                  <ContactForm />
                </div>
              </div>
              <div className="col-lg-6">
                <ContactInfo />
              </div>
            </div>
          </div>
        </div>
        <div className="map_area">
          <ContactMap />
        </div>
      </section>
      <OurPartner />
    </>
  )
};

export default Contact;
