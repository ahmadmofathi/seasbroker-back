import CommonBanner from '../component/Common/Banner';
import RequestClearanceForm from '../component/RequestClearance/RequestClearanceForm';
import OurPartner from '../component/Common/OurPartner';

const RequestClearance: React.FC = () => {
 return (
   <>
     <CommonBanner heading="Request Clearance" page="Request Clearance" />
     <section id="request_quote_form_area">
       <div className="container">
         <div className="row">
           <div className="col-lg-12 col-sm-12 col-md-12 col-12">
             <RequestClearanceForm/>
           </div>
         </div>
       </div>
     </section>
     <OurPartner />
   </>
 );
};

export default RequestClearance;
