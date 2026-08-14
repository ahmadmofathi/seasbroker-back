import Banner from '../component/Home/Banner';
import AboutUs from '../component/Common/AboutUs';
import Services from '../component/Home/Services';
import Counter from '../component/Common/Counter';
import MapSection from '../component/Home/MapSection';

const Home: React.FC = () => {
  return (
    <>
      <Banner />
      <AboutUs/>
      <Services />
      <Counter />
      <MapSection/>
    </>
  );
};

export default Home;
