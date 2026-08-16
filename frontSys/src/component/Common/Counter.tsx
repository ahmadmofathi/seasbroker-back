import React from "react";
import CountUp from "react-countup";

interface CounterItem {
  countStart: number;
  countEnd: number;
  heading: string;
  icon: string;
  suffix?: string;
}

const CounterData: CounterItem[] = [
  {
    countStart: 24,
    countEnd: 24,
    heading: "CUSTOMER SERVICE",
    icon: "fas fa-headset",
    suffix: "/7",
  },
  {
    countStart: 1,
    countEnd: 100,
    heading: "COUNTRIES SERVED",
    icon: "fas fa-globe",
  },
  {
    countStart: 1,
    countEnd: 2500,
    heading: "PORTS SERVED",
    icon: " fas fa-ship",
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
                    <h2 className="count">
                      <CountUp
                        start={data.countStart}
                        end={data.countEnd}
                        suffix={data.suffix ?? ""}
                      />
                    </h2>
                    <h5>{data.heading}</h5>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>
    </>
  );
};

export default Counter;
