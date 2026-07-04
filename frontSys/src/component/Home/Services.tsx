import SectionHeading from '../Common/SectionHeading';
import ServiceCard from '../Common/Service/ServiceCard';
import { ServiceData } from '../Common/Service/ServiceData';
import { Swiper, SwiperSlide } from 'swiper/react';

const Services: React.FC = () => {
  return (
    <>
      <section id="home_two_service">
        <div className="container">
          <SectionHeading heading="Taking care of you and your business all the way" para="Solving your supply chain needs from end to end, taking the
        complexity out of container shipping. We are at the forefront of developing innovation."/>
          <div className="row">
            <div className="col-lg-12 col-md-12 col-sm-12 col-12">
              <div className="service_slider_home_two">
                <Swiper
                  spaceBetween={30}
                  loop={false}
                  autoplay={{
                    delay: 2500,
                    disableOnInteraction: false,
                  }}
                  breakpoints={{
                    0: { slidesPerView: 1 },
                    600: { slidesPerView: 1 },
                    960: { slidesPerView: 2 },
                    1200: { slidesPerView: 3 },
                  }}
                  pagination={{ clickable: true }}
                >
                  {ServiceData.map((data, index) => (
                    <SwiperSlide key={index}>
                      <ServiceCard
                        links={data.link}
                        img={data.img}
                        heading={data.heading}
                        para={data.para}
                        button={data.button}
                      />
                    </SwiperSlide>
                  ))}
                </Swiper>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  );
};

export default Services;
