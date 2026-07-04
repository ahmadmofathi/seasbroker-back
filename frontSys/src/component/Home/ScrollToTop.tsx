import { useEffect, type PropsWithChildren } from "react";
import { useLocation } from "react-router";

const ScrollToTop: React.FC<PropsWithChildren> = (props) => {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
  }, [pathname]); // Only run when the route changes

  return <>{props.children}</>;
};

export default ScrollToTop;