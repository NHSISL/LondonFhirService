import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";
import { Outlet } from "react-router-dom";
import NavbarComponent from "./layouts/navbar";
import { useState, useEffect } from "react";
import SideBarComponent from "./layouts/sidebar";
import FooterComponent from "./layouts/footer";
import LoginUnAuthorisedComponent from "./layouts/loginUnauth";

export default function Root() {
    const [sidebarOpen, setSidebarOpen] = useState(true);

    const toggleSidebar = () => {
        setSidebarOpen(!sidebarOpen);
    };

    useEffect(() => {
        const handleResize = () => {
            if (window.innerWidth < 1024) {
                setSidebarOpen(false);
            } else {
                setSidebarOpen(true);
            }
        };

        window.addEventListener('resize', handleResize);

        handleResize();

        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    return (
        <>
            <AuthenticatedTemplate>
                <div className="layout-container">
                    <div className={`sidebar bg-light ${sidebarOpen ? 'sidebar-open' : 'sidebar-closed'}`}>
                        <SideBarComponent />
                        <div className="footerContent">
                            <FooterComponent />
                        </div>
                    </div>

                    <div className={`content ${sidebarOpen ? 'content-shift-right' : 'content-shift-left'}`}>
                        <NavbarComponent toggleSidebar={toggleSidebar} showMenuButton={true} />

                        <div className="content-inner">
                            <Outlet />
                        </div>

                    </div>
                </div>
            </AuthenticatedTemplate>

            <UnauthenticatedTemplate>
                <LoginUnAuthorisedComponent />
            </UnauthenticatedTemplate>
        </>
    );
}