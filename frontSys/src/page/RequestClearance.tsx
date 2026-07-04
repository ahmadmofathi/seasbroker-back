import CommonBanner from '../component/Common/Banner';
import RequestClearanceForm from '../component/RequestClearance/RequestClearanceForm';
import OurPartner from '../component/Common/OurPartner';

const RequestQuote: React.FC = () => {
 return (
   <>
     <CommonBanner heading="Request Clearance" page="Request Clearance" />
     <RequestClearanceForm/>
     <OurPartner />
   </>
 );
};

export default RequestQuote;
