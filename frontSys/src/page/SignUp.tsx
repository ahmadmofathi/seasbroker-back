import CommonBanner from '../component/Common/Banner';
import SignUpForm from "../component/SignUp/SignUpForm";

const SignUp: React.FC = () => {
  return (
    <>
      <CommonBanner heading="SignUp" page="SignUp" />
      <SignUpForm heading="Create an Account!" />
    </>
  );
};

export default SignUp;