import { Link } from "react-router";

const TopHeader: React.FC = () => {
  return (
    <>
      <div className="top-header">
        <div className="container">
          <div className="row align-items-center">
            <div className="col-lg-6 col-md-12 col-sm-12 col-12">
              <ul className="left-info">
                <li>
                  <a href="mailto:info@seasbroker.com">
                    <i className="far fa-envelope"></i>
                    info@seasbroker.com
                  </a>
                </li>
                <li>
                  <a href="tel:+201023456789">
                    <i className="fas fa-phone-volume"></i>
                    +20 1023 456 789
                  </a>
                </li>
              </ul>
            </div>
            <div className="col-lg-6 col-md-12 col-sm-12 col-12">
              <ul className="right-info">
                <li className="mr-20">
                  <Link to="/contact">24/7 Customer Service</Link>
                </li>
                <li className="mr-20">
                  <Link to="/track_ship">Track Your Service</Link>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default TopHeader;
