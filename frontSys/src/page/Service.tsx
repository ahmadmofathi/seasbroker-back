import CommonBanner from '../component/Common/Banner';
import Counter from '../component/Common/Counter';
import AboutUs from '../component/Common/AboutUs';
import OurPartner from '../component/Common/OurPartner';
import SectionHeading from '../component/Common/SectionHeading';
import ServiceCard from '../component/Common/Service/ServiceCard';
import { ServiceData } from '../component/Common/Service/ServiceData';
import { useServicesContent } from '../content/servicesContent';

const Service: React.FC = () => {
  const content = useServicesContent();
  return (
    <>
      <CommonBanner heading="Services" page="Services" />
      <section id="services_page">
        <div className="container">
          <SectionHeading heading={content.pageHeading} para={content.pagePara} />
          <div className="service_wrapper_top">
            <div className="row">

              {ServiceData.map((data, index) => (
                <div className="col-lg-4" key={index}>
                  <ServiceCard
                    links={data.link}
                    img={data.img}
                    heading={content.services[index]?.heading ?? data.heading}
                    para={content.services[index]?.para ?? data.para}
                    button={content.services[index]?.button ?? data.button}
                  />
                </div>
              ))}

            </div>
          </div>
        </div>
      </section>
      <Counter />
      <AboutUs />
      <OurPartner />
    </>
  );
};

export default Service;
