// CopyRight Area
const CopyRight = () => {
  const currentYear = new Date().getFullYear(); // Get the current year

  return (
    <div id="copy_right">
      <div className="container">
        <div className="row">
          <div className="col-lg-12">
            <div className="copy-right-area">
              <p>Copyright &copy; {currentYear} <span>Seas Broker.</span> All Rights Reserved</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default CopyRight;
