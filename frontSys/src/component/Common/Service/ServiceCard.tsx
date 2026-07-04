import { Link } from 'react-router';

interface ServiceCardProps {
  img: string;
  heading: string;
  para: string;
  button: string;
  links: string;
}

const ServiceCard: React.FC<ServiceCardProps> = ({ img, heading, para, button, links }) => {
  return (
    <>
      <div className="service-card">
        <img src={img} alt="image_service" />
        <div className="service-caption">
          <h3>{heading}</h3>
          <p>{para}</p>
          <Link to={links} className="btn btn-theme">{button}</Link>
        </div>
      </div>
    </>
  );
};

export default ServiceCard;
