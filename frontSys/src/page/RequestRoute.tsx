import CommonBanner from '../component/Common/Banner';
import RequestRouteForm from '../component/RequestRoute/RequestRouteForm';

const RequestQuote: React.FC = () => {
  return (
    <>
      <CommonBanner heading="Request Route" page="Request Route" />
      <RequestRouteForm />
    </>
  );
};

export default RequestQuote;
