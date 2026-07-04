import { Link } from 'react-router';
import SectionHeading from './SectionHeading'

// Pricing data
const pricingData = [
  {
    title: "Basic Plan",
    amount: "8.9",
    items: [
      { icons: "fa fa-check", text: "Standard Feature" },
      { icons: "fa fa-check", text: "Lifetime free support" },
      { icons: "fa fa-times", text: "Extra features" },
      { icons: "fa fa-check", text: "Upgrate options" },
      { icons: "fa fa-check", text: "Full access" }
    ]
  },
  {
    title: "Popular Plan",
    amount: "29.9",
    items: [
      { icons: "fa fa-check", text: "Standard Feature" },
      { icons: "fa fa-check", text: "Lifetime free support" },
      { icons: "fa fa-times", text: "Extra features" },
      { icons: "fa fa-check", text: "Upgrate options" },
      { icons: "fa fa-check", text: "Full access" }
    ]
  },
  {
    title: "Premium Plan",
    amount: "88.9",
    items: [
      { icons: "fa fa-check", text: "Standard Feature" },
      { icons: "fa fa-check", text: "Lifetime free support" },
      { icons: "fa fa-times", text: "Extra features" },
      { icons: "fa fa-check", text: "Upgrate options" },
      { icons: "fa fa-check", text: "Full access" }
    ]
  }
];

const PricingTable = () => {
  return (
    <>
      <section id="home_pricing_area">
        <div className="container">
          <SectionHeading heading="Our Pricing Plan" para="Solving your supply chain needs from end to end, taking the
        complexity out of container shipping. We are at the forefront of developing innovation."/>
          <div className="row">
            {pricingData.map((data, index) => (
              <div className="col-lg-4 col-md-6 col-sm-12" key={index}>
                <div className="price-table mrb-30 text-center">
                  <div className="table-header">
                    <h4 className="pricing-plan-name">{data.title}</h4>
                    <h3 className="price"><span className="price-currency">$</span>{data.amount}<span className="price-duration text-primary-color">/ Per Month</span></h3>
                  </div>
                  <div className="table-content">
                    <ul className="list-items">
                      {data.items.map((datas, index) => (
                        <li key={index}><i className={datas.icons}></i>{datas.text}</li>
                      ))}
                    </ul>
                  </div>
                  <div className="table-footer">
                    <Link to="/pricing" className="btn btn-theme">Choose Plan</Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>
    </>
  )
}

export default PricingTable;

