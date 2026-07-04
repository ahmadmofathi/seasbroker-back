import CommonBanner from '../component/Common/Banner';
import SignInForm from '../component/SignIn/SignInForm';

const SignIn: React.FC = () => {
  return (
    <>
      <CommonBanner heading="SignIn" page="SignIn" />
      <SignInForm heading="Sign in to Your Account!" />
    </>
  );
};

export default SignIn;