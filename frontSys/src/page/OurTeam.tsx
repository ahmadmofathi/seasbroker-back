import CommonBanner from '../component/Common/Banner'
import OurPartner from '../component/Common/OurPartner'
import TeamCard from '../component/Common/Team/TeamCard'
import { TeamData } from '../component/Common/Team/TeamData'
import Pagination from '../component/Common/Pagination'

const OurTeam: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Our Team" page="Our Team" />
      <section id="team_area">
        <div className="container">
          <div className="row">
            {TeamData.map((data, index) => (
              <div className="col-lg-4 col-md-6 col-sm-12 col-12" key={index}>
                <TeamCard img={data.img} para={data.para} name={data.name}
                  des={data.des} />
              </div>
            ))}

            <div className="col-lg-12 col-md-12">
              <Pagination />
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  )
};

export default OurTeam;
