import SectionHeading from '../Common/SectionHeading';
import TeamCard from '../Common/Team/TeamCard';
import { TeamData } from '../Common/Team/TeamData';
import { Swiper, SwiperSlide } from 'swiper/react';

const Teams: React.FC = () => {
  return (
    <>
      <section id="team_area">
        <div className="container">
          <SectionHeading heading="Our Team" para="Solving your supply chain needs from end to end, taking the
complexity out of container shipping. We are at the forefront of developing innovation."/>
          <div className="row">
            <div className="col-lg-12 col-md-12 col-sm-12 col-12">
              <div className="team-slider">
                <Swiper
                  spaceBetween={30}
                  loop={true}
                  pagination={{ clickable: true }}
                  breakpoints={{
                    0: { slidesPerView: 1 },
                    600: { slidesPerView: 1 },
                    960: { slidesPerView: 2 },
                    1200: { slidesPerView: 3 },
                  }}
                >
                  {TeamData.map((data, index) => (
                    <SwiperSlide key={index}>
                      <TeamCard img={data.img} para={data.para} name={data.name} des={data.des} />
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

export default Teams;
