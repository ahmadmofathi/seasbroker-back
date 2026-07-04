import CommonBanner from '../component/Common/Banner';
import Counter from '../component/Common/Counter';
import AboutUs from '../component/Common/AboutUs';
import OurPartner from '../component/Common/OurPartner';
import SectionHeading from '../component/Common/SectionHeading';
import ServiceCard from '../component/Common/Service/ServiceCard';
import { ServiceData } from '../component/Common/Service/ServiceData';

const Service: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Services" page="Services" />
      <section id="services_page">
        <div className="container">
          <SectionHeading heading="We Serve Various Ways" para="Solving your supply chain needs from end to end, taking the
        complexity out of container shipping. We are at the forefront of developing innovation."/>
          <div className="service_wrapper_top">
            <div className="row">

              {ServiceData.map((data, index) => (
                <div className="col-lg-4" key={index}>
                  <ServiceCard links={data.link} img={data.img} heading={data.heading} para={data.para}
                    button={data.button} />
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
