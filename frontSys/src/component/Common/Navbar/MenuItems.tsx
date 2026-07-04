import { Link } from 'react-router';
import type { MenuItemProps } from './MenuData';


// MenuItems Area
const MenuItems: React.FC<MenuItemProps> = ( item ) => {
  return (
    <>
      <li className="nav-item">
        <Link to={item.href} className="nav-link" >
          {item.name}
          {item.has_children && (
            <i className="fas fa-angle-down"></i>
          )}
        </Link>
        {item.has_children && (
          <ul className="dropdown-menu">
            {item.children.map((item, index) => (
              <MenuItems {...item} key={index} />
            ))}
          </ul>
        )}

      </li>
    </>
  );
};

export default MenuItems;