import CommonBanner from '../component/Common/Banner';
import AboutUs from '../component/Common/AboutUs';
import OurPartner from '../component/Common/OurPartner';
import Teams from '../component/Home/Teams';

const About: React.FC = () => {
  return (
    <>
      <CommonBanner heading="About" page="About" />
      <AboutUs />
      <Teams />
      <OurPartner />
    </>
  )
};

export default About;
