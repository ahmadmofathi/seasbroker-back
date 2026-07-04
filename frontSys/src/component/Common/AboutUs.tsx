// import { Link } from 'react-router';
import img1 from '../../assets/img/common/about-service.jpg';

const AboutUs: React.FC = () => {
  return (
    <>
      <section id="service_about">
        <div className="container">
          <div className="row">
            <div className="col-lg-6 col-md-12 col-sm-12 col-12">
              <div className="about_service_img">
                <img src={img1} alt="img-about" />
              </div>
            </div>
            <div className="col-lg-6  col-md-12 col-sm-12 col-12">
              <div className="about_service_text">
                <div className="heading-left-border">
                  <h2>Your Trusted Partner in Global Shipping Solutions</h2>
                </div>
                <p>Our warehousing services are recognized nationwide for their reliability, safety, and affordability, reflecting our commitment to delivering the best at reasonable prices.</p>
                <p>We take pride in providing comprehensive solutions, ensuring satisfaction while navigating any challenges.</p>
                {/* <Link to="/clearance_offices" className="btn btn-theme">Show Clearance Offices</Link> */}
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
};

export default AboutUs;
