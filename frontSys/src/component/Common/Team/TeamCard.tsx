import React from 'react'

interface TeamCardProps {
  para: string;
  name: string;
  des: string;
}

// TeamCard Area
const TeamCard: React.FC<TeamCardProps> = ({ para, name, des }) => {
  return (
    <>
      <div className="team-member">
        <div className="team_inner">
          <div className="team_avatar" style={{
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            width: '100%', aspectRatio: '1', background: 'linear-gradient(135deg, #0e3a6e 0%, #1a6fa8 100%)',
          }}>
            <i className="ri-user-3-fill" style={{ fontSize: '5rem', color: 'rgba(255,255,255,0.85)' }} />
          </div>
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
