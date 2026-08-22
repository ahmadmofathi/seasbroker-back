import { Link } from 'react-router';
import { useEffect, useState } from 'react';
import { listCollection } from '../../api/client';
import logo from '../../assets/img/Logo_trans.png'

interface FooterProps {
  title: string;
  links: { title: string; route: string }[];
}

interface SettingRecord {
  id: string;
  key: string;
  value: string;
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
  const [address, setAddress] = useState('Alexandria, Egypt');
  const [phone, setPhone] = useState('+20 102 3456 789');
  const [email, setEmail] = useState('info@seasbroker.com');
  const [facebook, setFacebook] = useState('#!');
  const [twitter, setTwitter] = useState('#!');
  const [linkedin, setLinkedin] = useState('#!');
  const [instagram, setInstagram] = useState('#!');

  useEffect(() => {
    listCollection<SettingRecord>('settings', { page: 1, perPage: 100 })
      .then((res) => {
        const items = res.items || [];
        items.forEach((item) => {
          switch (item.key) {
            case 'address': setAddress(item.value); break;
            case 'phone': setPhone(item.value); break;
            case 'email': setEmail(item.value); break;
            case 'facebook': setFacebook(item.value); break;
            case 'twitter': setTwitter(item.value); break;
            case 'linkedin': setLinkedin(item.value); break;
            case 'instagram': setInstagram(item.value); break;
          }
        });
      })
      .catch((err) => console.error('Failed to load settings in footer:', err));
  }, []);

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
                  <a href={facebook} target="_blank" rel="noopener noreferrer"><i className="fab fa-facebook-f"></i></a>
                  <a href={twitter} target="_blank" rel="noopener noreferrer"><i className="fab fa-twitter"></i></a>
                  <a href={linkedin} target="_blank" rel="noopener noreferrer"><i className="fab fa-linkedin-in"></i></a>
                  <a href={instagram} target="_blank" rel="noopener noreferrer"><i className="fab fa-instagram"></i></a>
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
                    <li><i className="fas fa-map-marker-alt"></i>{address}</li>

                    <li><i className="far fa-envelope"></i> <a
                      href={`mailto:${email}`}>{email}</a></li>
                    <li><i className="fas fa-phone-volume"></i> <a href={`tel:${phone.replace(/\s+/g, '')}`}>{phone}</a></li>
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

