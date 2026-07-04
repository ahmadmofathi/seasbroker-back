import { useState, useEffect } from 'react';
import { Link } from 'react-router';
import logo from '../../../assets/img/Logo_trans.png';
import TopHeader from '../TopHeader';
import { MenuData } from './MenuData';
import MenuItems from './MenuItems';
import SearchForm from '../SearchForm';
import { HiMenuAlt3 } from "react-icons/hi";
import { AiOutlineClose } from "react-icons/ai";

const Navbar: React.FC = () => {
  //Responsive Menu Area
  const [click, setClick] = useState(false);

  const isSticky = () => {
    const header = document.querySelector('.navbar-area');
    const scrollTop = window.scrollY;
    if (scrollTop >= 250) {
      header?.classList.add('is-sticky');
    } else {
      header?.classList.remove('is-sticky');
    }
  };

  // Sticky Menu Area
  useEffect(() => {
    window.addEventListener('scroll', isSticky);
    return () => {
      window.removeEventListener('scroll', isSticky);
    };
  }, []);


  const handleClick = () => {
    if (click) {
      document.querySelector("#navbarSupportedContent")?.classList.remove("navber-colpes")
    } else {
      document.querySelector("#navbarSupportedContent")?.classList.add("navber-colpes")
    }
    setClick(!click);
  }

  // const handleSearchOpen = () => {
  //   document.querySelector('.search-overlay')?.classList.toggle('search-overlay-active')
  // }

  return (
    <>
      <header className="header-area">
        <TopHeader />
        <div className="navbar-area" >
          <div className="transtics-nav">
            <div className="container">
              <nav className="navbar navbar-expand-md navbar-light">
                <Link className="navbar-brand" to="/">
                  <img src={logo} alt="logo" style={{ width: "80px" }} />
                </Link>
                {/* Adjusted Text beside the logo to be in one line */}
                <div className="logo-text" style={{ display: "inline-block", verticalAlign: "middle" }}>
                  <h4 style={{ margin: 0, whiteSpace: "nowrap" }}>SEAS BROKER</h4>
                </div>
                <div className="mean-menu" id="navbarSupportedContent">
                  <ul className="navbar-nav">
                    {MenuData.map((item, index) => (
                      <MenuItems {...item} key={index} />
                    ))}
                    {/* 
                      <li className="menu-item">
                        <div className="register-button">
                          <Link to="/signup" className="btn btn-theme">Register</Link>
                        </div>
                      </li> 
                    */}
                  </ul>
                </div>
              </nav>
            </div>
          </div>
          <div className="transtics-responsive-nav">
            <div className="container">
              <div className="responsive-button" onClick={handleClick}>
                {click ? <AiOutlineClose /> : <HiMenuAlt3 />}
              </div>
            </div>
          </div>
        </div>
      </header>
      <SearchForm />
    </>
  )
}

export default Navbar;
