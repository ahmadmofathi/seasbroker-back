import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import ClientCard from '../component/Common/Client/ClientCard';
import { ClientsData } from '../component/Common/Client/ClientData';
import { Swiper, SwiperSlide } from 'swiper/react';

const Testimonials: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Testimonials" page="Testimonials" />
      <section id="testimonial_homeTwo">
        <div className="container">
          <div className="row">
            <div className="col-lg-10 offset-lg-1 col-md-12 col-sm-12 col-12">
              <div className="client_review_two ">
                <Swiper
                  spaceBetween={10}
                  loop={true}
                  pagination={{ clickable: true }}
                  breakpoints={{
                    0: { slidesPerView: 1 },
                    600: { slidesPerView: 1 },
                    960: { slidesPerView: 1 },
                    1200: { slidesPerView: 1 },
                  }}
                >
                  {ClientsData.map((data, index) => (
                    <SwiperSlide key={index}>
                      <ClientCard img={data.img} name={data.name} des={data.des} para={data.para} />
                    </SwiperSlide>
                  ))}
                </Swiper>
              </div>
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default Testimonials;
