import React from 'react'

interface TeamCardProps {
  img: string;
  para: string;
  name: string;
  des: string;
}

// TeamCard Area
const TeamCard: React.FC<TeamCardProps> = ({ img, para, name, des }) => {
  return (
    <>
      <div className="team-member">
        <div className="team_inner">
          <img src={img} className="img-responsive" alt="img_team" />
          <div className="team_text">
            <p>{para}</p>
            <ul>
              <li><a href="#!"><i className="fab fa-facebook-f fa-2x"></i></a></li>
              <li><a href="#!"><i className="fab fa-twitter fa-2x"></i></a></li>
              <li><a href="#!"><i className="fab fa-linkedin fa-2x"></i></a></li>
            </ul>
          </div>
        </div>
        <div className="team_name">
          <h4>{name}</h4>
          <p>{des}</p>
        </div>
      </div>
    </>
  )
}

export default TeamCard
