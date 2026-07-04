import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { Link } from 'react-router';
import img1 from '../assets/img/common/error.png';

const Error: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Error" page="Error" />
      <section id="error_area">
        <div className="d-table">
          <div className="d-table-cell">
            <div className="container">
              <div className="error-img">
                <img src={img1} alt="Error Imgs" />
                <h3>Page Not Found</h3>
                <Link to="/" className="btn btn-theme">Back To Home</Link>
              </div>
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  )
};

export default Error;
