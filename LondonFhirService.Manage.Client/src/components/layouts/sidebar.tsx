import MenuComponent from '../core/menu';

const SideBarComponent: React.FC = () => {
    return (
        <div className="sidebar vh-100 bg-dark text-white">
            <h4 className="text-center">&nbsp;</h4>
            <MenuComponent />
        </div>
    );
}

export default SideBarComponent;