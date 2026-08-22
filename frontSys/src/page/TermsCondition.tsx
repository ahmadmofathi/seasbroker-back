import { useEffect, useState } from 'react';
import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { listCollection } from '../api/client';

const TermsCondition: React.FC = () => {
  const [termsHtml, setTermsHtml] = useState('');

  useEffect(() => {
    listCollection<{ key: string; value: string }>('settings', { page: 1, perPage: 100 })
      .then((res) => {
        const item = res.items?.find((s) => s.key === 'terms_conditions');
        if (item) {
          setTermsHtml(item.value);
        }
      })
      .catch((err) => console.error('Failed to load terms and conditions:', err));
  }, []);

  return (
    <>
      <CommonBanner heading="Terms & Condition" page="Terms & Condition" />
      <section id="privacy_policy">
        <div className="container">
          <div className="row">
            <div className="col-lg-12">
              <div className="text_heading_para" dangerouslySetInnerHTML={{ __html: termsHtml || 'Loading terms and conditions...' }} />
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  )
};

export default TermsCondition;
