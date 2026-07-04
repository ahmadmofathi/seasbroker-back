import ClientCard from '../Common/Client/ClientCard';
import { ClientsData } from '../Common/Client/ClientData';
import { Swiper, SwiperSlide } from 'swiper/react';

const Clients: React.FC = () => {
  return (
    <>
      <section id="testimonial_homeTwo">
        <div className="container">
          <div className="row">
            <div className="col-lg-12">
              <div className="section_heading_center">
                <h2>Our Clients Around The World</h2>
                <p>Solving your supply chain needs from end to end, taking the complexity out of
                  container shipping.
                  We are at the forefront of developing innovative supply chain solutions.</p>
              </div>
            </div>
          </div>
          <div className="row">
            <div className="col-lg-10 offset-lg-1 col-md-12 col-sm-12 col-12">
              <div className="client_review_two">
                <Swiper
                  spaceBetween={20}
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
    </>
  );
};

export default Clients;
