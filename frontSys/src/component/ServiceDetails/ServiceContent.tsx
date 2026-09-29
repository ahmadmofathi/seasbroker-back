import img1 from '../../assets/img/service/service_details1.png'
import img2 from '../../assets/img/service/service_details2.png'
import { paragraphs, type ServiceDetailsSection } from '../../content/servicesContent';

// The page's two pictures go with its first two sections, as in the original design.
const SECTION_IMAGES = [img1, img2];

interface ServiceContentProps {
  sections: ServiceDetailsSection[];
}

const ServiceContent: React.FC<ServiceContentProps> = ({ sections }) => {
  return (
    <>
      <div className="col-lg-8">
        <div className="service_details_wrapper">
          {sections.map((section, index) => (
            <div className="service_details_items" key={index}>
              {SECTION_IMAGES[index] && <img src={SECTION_IMAGES[index]} alt="img" />}
              <div className="service_details_para">
                {section.heading && <h3>{section.heading}</h3>}
                {paragraphs(section.body).map((para, i) => (
                  <p key={i}>{para}</p>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  )
};

export default ServiceContent;
