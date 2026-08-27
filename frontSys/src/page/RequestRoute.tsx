import CommonBanner from '../component/Common/Banner';
import RequestRouteForm from '../component/RequestRoute/RequestRouteForm';

const RequestRoute: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Request Route" page="Request Route" />
      <section id="request_quote_form_area">
        <div className="container">
          <div className="row">
            <div className="col-lg-12 col-sm-12 col-md-12 col-12">
              <RequestRouteForm />
            </div>
          </div>
        </div>
      </section>
    </>
  );
};

export default RequestRoute;
