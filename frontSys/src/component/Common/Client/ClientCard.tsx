interface ClientCardProps {
  img: string;
  name: string;
  des: string;
  para: string;
}

const ClientCard: React.FC<ClientCardProps> = ({ img, name, des, para }) => {
  return (
    <>
      <div className="client_two_item">
        <div className="slider_two_img">
          <img src={img} alt="Client_Img" />
        </div>
        <div className="slider_two_name">
          <h3>{name}</h3>
          <p>{des}</p>
        </div>
        <div className="slider_two_rating">
          <i className="fas fa-star"></i>
          <i className="fas fa-star"></i>
          <i className="fas fa-star"></i>
          <i className="fas fa-star"></i>
          <i className="fas fa-star"></i>
        </div>
        <div className="slider_two_text">
          <i className="fas fa-quote-left"></i>
          <p>{para}</p>
        </div>
      </div>
    </>
  )
};

export default ClientCard;
