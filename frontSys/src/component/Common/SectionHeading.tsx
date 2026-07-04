interface SectionHeadingProps {
  heading: string;
  para?: string;
}

const SectionHeading: React.FC<SectionHeadingProps> = ({heading, para}) => {
  return (
    <>
      <div className="row">
        <div className="col-lg-12">
          <div className="section_heading_center">
            <h2>{heading}</h2>
            {para && <p> {para}</p>}  
          </div>
        </div>
      </div>
    </>
  )
}

export default SectionHeading
