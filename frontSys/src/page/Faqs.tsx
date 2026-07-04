import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { FaqsData } from '../component/Faqs/FaqsData';
import QuestionForm from '../component/Faqs/QuestionForm';

const Faqs: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Faqs" page="Faqs" />
      <section id="faqs_area">
        <div className="container">
          <div className="row">
            <div className="col-lg-6 col-md-12 col-sm-12 col-12">
              <div className="tab-pane fade show active">
                <div className="faqs-items">
                  <div id="accordion" className="accordion-wrapper">
                    {FaqsData.map((data, index) => {
                    const stringIndex = index.toString();
                    return (
                      <div id={"accordion" + stringIndex} key={index}>
                        <div className="card  box-shadow">
                          <div className="card-header" id={"heading" + stringIndex}>
                            <h5 className="mb-0">
                              <a href="#!" className="collapsed " role="button"
                                data-toggle="collapse"
                                data-target={"#collapse" + stringIndex} aria-expanded="false"
                                aria-controls={"collapse" + stringIndex}>{data.heading}</a>
                            </h5>
                          </div>
                          <div id={"collapse" + stringIndex}
                            className={index === 0 ? "collapse show active" : "collapse"}
                            aria-labelledby={"heading" + stringIndex}
                            data-parent={"#accordion"}>
                            <div className="card-body active">
                              <p>{data.para}</p>
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                    })}
                  </div>
                </div>
              </div>
            </div>
            <div className="col-lg-6 col-md-12 col-sm-12 col-12">
              <QuestionForm />
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  )
};

export default Faqs;
