import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import ServiceSideBar from '../component/ServiceDetails/ServiceSideBar';
import ServiceContent from '../component/ServiceDetails/ServiceContent';

const ServiceDetails: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Service Details" page="Service Details" />
      <section id="service_details_area">
        <div className="container">
          <div className="row">
            <ServiceContent />
            <ServiceSideBar />
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default ServiceDetails;
