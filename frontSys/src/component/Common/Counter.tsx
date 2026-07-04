import React from 'react'
import CountUp from 'react-countup';

const CounterData = [
  {
    countStart: 100,
    countEnd: 12345,
    heading: "COMPANY ESTABLISHED",
    icon: " fas fa-building"
  },
  {
    countStart: 1,
    countEnd: 100,
    heading: "COUNTRIES SERVED",
    icon: "fas fa-globe"
  },
  {
    countStart: 1,
    countEnd: 2500,
    heading: "PORTS SERVED",
    icon: " fas fa-ship"
  },
];

const Counter: React.FC = () => {
  return (
    <>
      <section id="counter_area_main">
        <div className="container">
          <div className="row">
            {CounterData.map((data, index) => (
              <div key={index} className="col-lg-4 col-md-6 col-sm-12 col-12">
                <div className="counter_area">
                  <div className="counters_icon">
                    <i className={data.icon}></i>
                  </div>
                  <div className="counter_count">
                    <h2 className="count"><CountUp start={data.countStart} end={data.countEnd} /></h2>
                    <h5>{data.heading}</h5>
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

export default Counter;

