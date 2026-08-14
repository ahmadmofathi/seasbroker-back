import { Link } from 'react-router';
import logo from '../../assets/img/Logo_trans.png'

interface FooterProps {
  title: string;
  links: { title: string; route: string }[];
}

const FooterItems: FooterProps[] = [
  {
    title: "Quick Link",
    links: [
      { title: "Home", route: "/" },
      { title: "About Company", route: "/about" },
      { title: "Our Services", route: "/service" },
      { title: "Contact us", route: "/contact" },
    ],
  },
  {
    title: "Company",
    links: [
      { title: "Track Your Service", route: "/track_ship" },
      { title: "Privacy Policy", route: "/privacyPolicy" },
      { title: "Terms & Condition", route: "/terms" },
    ],
  }
]

const FooterWidget: React.FC<FooterProps> = ({ title, links }) => (
  <>
    <h4>{title}</h4>
    <ul>
      {links.map((link, index) => (
        <li key={index}><Link to={link.route}>{link.title}</Link></li>
      ))}
    </ul>
  </>
)

const Footer: React.FC = () => {
  return (
    <>
      <footer id="footer_area">
        <div className="container">
          <div className="row">
            <div className="col-lg-4 col-md-12 col-sm-12 col-12">
              <div className="footer_wedget">
                <img src={logo} alt="logo_img" style={{ width: "80px", boxShadow: "50px" }} />
                <p>Streamlining international trade with efficient and reliable logistics. Let us handle the complexities while you focus on your business.</p>
                <div className="footer_social_icon">
                  <a href="#!"><i className="fab fa-facebook-f"></i></a>
                  <a href="#!"><i className="fab fa-twitter"></i></a>
                  <a href="#!"><i className="fab fa-linkedin-in"></i></a>
                  <a href="#!"><i className="fab fa-instagram"></i></a>
                </div>
              </div>
            </div>
            <div className="col-lg-2 col-md-6 col-sm-12 col-12">
              <div className="footer_wedget">
                {FooterItems.slice(0, 1).map((data, index) => (
                  <FooterWidget {...data} key={index} />
                ))}

              </div>
            </div>
            <div className="col-lg-3 col-md-6 col-sm-12 col-12">

              <div className="footer_wedget">
                {FooterItems.slice(1, 2).map((data, index) => (
                  <FooterWidget {...data} key={index} />
                ))}
              </div>
            </div>
            <div className="col-lg-3 col-md-12 col-sm-12 col-12">
              <div className="footer_wedget">
                <h4>Contact Info</h4>
                <div className="contact-info-footer">
                  <ul>
                    <li><i className="fas fa-map-marker-alt"></i>Alexandria, Egypt</li>

                    <li><i className="far fa-envelope"></i> <a
                      href="mailto:info@seasbroker.com">info@seasbroker.com</a></li>
                    <li><i className="fas fa-phone-volume"></i> <a href="tel:+01023-456-789"> +20 102 3456 789</a></li>
                    <li><i className="far fa-clock"></i>Mon - Sat: 9:00 - 18:00</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </footer>

    </>
  )
}

export default Footer;

