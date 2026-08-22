import { useEffect, useState } from 'react';
import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { FaqsData } from '../component/Faqs/FaqsData';
import QuestionForm from '../component/Faqs/QuestionForm';
import { listCollection } from '../api/client';

interface FaqRecord {
  id: string;
  heading: string;
  para: string;
  sortOrder: number;
}

const Faqs: React.FC = () => {
  const [faqsList, setFaqsList] = useState<FaqRecord[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    listCollection<FaqRecord>('faqs', { page: 1, perPage: 100 })
      .then((res) => {
        setFaqsList(res.items || []);
      })
      .catch((err) => {
        console.error('Failed to load FAQs:', err);
        // Fallback
        setFaqsList(FaqsData.map((f, i) => ({ id: String(i), heading: f.heading, para: f.para, sortOrder: i + 1 })));
      })
      .finally(() => setLoading(false));
  }, []);

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
                    {loading ? (
                      <p>Loading FAQs...</p>
                    ) : (
                      faqsList.map((data, index) => {
                        const stringIndex = index.toString();
                        return (
                          <div id={"accordion" + stringIndex} key={data.id}>
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
                      })
                    )}
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
