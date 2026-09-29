import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import ServiceSideBar from '../component/ServiceDetails/ServiceSideBar';
import ServiceContent from '../component/ServiceDetails/ServiceContent';
import { usePublicSettings, useServicesContentFrom } from '../content/servicesContent';

const ServiceDetails: React.FC = () => {
  const settings = usePublicSettings();
  const content = useServicesContentFrom(settings);
  return (
    <>
      <CommonBanner heading={content.details.title} page={content.details.title} />
      <section id="service_details_area">
        <div className="container">
          <div className="row">
            <ServiceContent sections={content.details.sections} />
            <ServiceSideBar
              services={content.services}
              contact={{ address: settings.address, phone: settings.phone, email: settings.email }}
            />
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default ServiceDetails;
