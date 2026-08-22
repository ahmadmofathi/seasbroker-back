import { useEffect, useState } from 'react';
import CommonBanner from '../component/Common/Banner';
import OurPartner from '../component/Common/OurPartner';
import { listCollection } from '../api/client';

const PrivacyPolicy: React.FC = () => {
  const [policyHtml, setPolicyHtml] = useState('');

  useEffect(() => {
    listCollection<{ key: string; value: string }>('settings', { page: 1, perPage: 100 })
      .then((res) => {
        const item = res.items?.find((s) => s.key === 'privacy_policy');
        if (item) {
          setPolicyHtml(item.value);
        }
      })
      .catch((err) => console.error('Failed to load privacy policy:', err));
  }, []);

  return (
    <>
      <CommonBanner heading="Privacy Policy" page="Privacy Policy" />
      <section id="privacy_policy">
        <div className="container">
          <div className="row">
            <div className="col-lg-12">
              <div className="text_heading_para" dangerouslySetInnerHTML={{ __html: policyHtml || 'Loading privacy policy...' }} />
            </div>
          </div>
        </div>
      </section>
      <OurPartner />
    </>
  )
};

export default PrivacyPolicy;
