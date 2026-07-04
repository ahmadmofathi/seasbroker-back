import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import RequestQuoteForm from '../component/RequestQuote/RequestQuoteForm';


const RequestQuote: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Request Quote" page="Request Quote" />
      <section id="request_quote_form_area">
        <div className="container">
          <div className="row">
            <div className="col-lg-12 col-sm-12 col-md-12 col-12">
              <RequestQuoteForm />
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  );
};

export default RequestQuote;
