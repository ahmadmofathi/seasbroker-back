import { Swiper, SwiperSlide } from 'swiper/react';
//  Client Slider Img Import
import img1 from '../../assets/img/partner/client-1.png'
import img2 from '../../assets/img/partner/client-2.png'
import img3 from '../../assets/img/partner/client-3.png'
import img4 from '../../assets/img/partner/client-4.png'
import img5 from '../../assets/img/partner/client-5.png'

const OurPartnerData = [
  { img: img1 },
  { img: img2 },
  { img: img3 },
  { img: img4 },
  { img: img5 },
  { img: img1 }
];

const OurPartner: React.FC = () => {
  return (
    <>
      <section id="partner_area_slider">
        <div className="container">
          <div className="row align-items-center">
            <div className="col-lg-3 col-md-12 col-sm-12 col-12">
              <div className="partner_heading">
                <h2>Our Partners:</h2>
              </div>
            </div>
            <div className="col-lg-9 col-md-12 col-sm-12 col-12">
              <div className="partner_slider_wrapper">
                <Swiper
                  spaceBetween={10}
                  loop={true}
                  autoplay={{ delay: 2500, disableOnInteraction: false }}
                  slidesPerView={2}
                  breakpoints={{
                    600: { slidesPerView: 2 },
                    960: { slidesPerView: 2 },
                    1200: { slidesPerView: 3 }
                  }}
                >
                  {OurPartnerData.map((data, index) => (
                    <SwiperSlide key={index}>
                      <div className="partner_logo">
                        <img src={data.img} alt="logo-img" />
                      </div>
                    </SwiperSlide>
                  ))}
                </Swiper>
              </div>
            </div>
          </div>
        </div>
      </section>
    </>
  )
};

export default OurPartner;
