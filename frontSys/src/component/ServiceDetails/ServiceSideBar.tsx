import { Link } from 'react-router';
import { ServiceData } from '../Common/Service/ServiceData';
import type { ServiceCardText } from '../../content/servicesContent';

interface ServiceSideBarProps {
  /** Our three services (titles as edited in Admin → Services Content). */
  services: ServiceCardText[];
  /** Contact details from Admin → System Settings; rows without a value are hidden. */
  contact: { address?: string; phone?: string; email?: string };
}

const ServiceSideBar: React.FC<ServiceSideBarProps> = ({ services, contact }) => {
  const hasContact = Boolean(contact.address || contact.phone || contact.email);
  return (
    <>
      <div className="col-lg-4">
        <div className="service_details_sidebar">
          <div className="sidebar_service_wrappers">
            <h3>Our Service</h3>
            <ul>
              {services.map((service, index) => (
                <li key={ServiceData[index]?.link ?? index}>
                  <Link to={ServiceData[index]?.link ?? '/service'}>{service.heading}</Link>
                </li>
              ))}
            </ul>
          </div>
          {hasContact && (
            <div className="sidebar_service_wrappers">
              <h3>Contact Us</h3>
              {contact.address && (
                <div className="contact_sidebar">
                  <h6>Visit our office</h6>
                  <p>{contact.address}</p>
                </div>
              )}
              {contact.phone && (
                <div className="contact_sidebar">
                  <h6>Call us on</h6>
                  <p>{contact.phone}</p>
                </div>
              )}
              {contact.email && (
                <div className="contact_sidebar">
                  <h6>Mail Us at</h6>
                  <p>{contact.email}</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  )
};

export default ServiceSideBar;
