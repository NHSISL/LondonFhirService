import { faHome, faUser } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React from 'react';
import { ListGroup } from 'react-bootstrap';
import { useLocation, useNavigate } from 'react-router-dom'; 

const MenuComponent: React.FC = () => {
    const location = useLocation();
    const navigate = useNavigate(); 

    const handleItemClick = (path: string) => {
        navigate(path); 
    };

    return (
        <ListGroup variant="flush" className="text-start border-0">
            <ListGroup.Item
                className={`bg-dark text-white ${location.pathname === '/home' ? 'active' : ''}`}
                onClick={() => handleItemClick('/home')}>
                <FontAwesomeIcon icon={faHome} className="me-2 fa-icon" />
                Home
            </ListGroup.Item>

            <ListGroup.Item
                className={`bg-dark text-white ${location.pathname === '/testPage' ? 'active' : ''}`}
                onClick={() => handleItemClick('/testPage')}>
                <FontAwesomeIcon icon={faUser} className="me-2 fa-icon" />
               Test Nav Page
            </ListGroup.Item>
        </ListGroup>
    );
};

export default MenuComponent;
