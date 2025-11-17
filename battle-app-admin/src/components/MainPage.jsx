import mainImage from '../assets/main.png';
import './MainPage.css';

const MainPage = () => {
  return (
    <div className="main-page">
      <img src={mainImage} alt="Great Void Battle" className="main-image" />
    </div>
  );
};

export default MainPage;
