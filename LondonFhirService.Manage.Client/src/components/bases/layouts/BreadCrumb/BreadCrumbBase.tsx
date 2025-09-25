import React, { FunctionComponent } from "react";
import "./BreadCrumbStyle.css"
import { Link, LinkProps } from "react-router-dom";

interface BreadCrumbBaseProps {
    children?: React.ReactNode;
    link: LinkProps['to'];
    backLink: string;
    currentLink: string;
}

const BreadCrumbBase: FunctionComponent<BreadCrumbBaseProps> = (props) => {
    return (

        <nav className="mt-2 rounded" aria-label="breadcrumb">
            <ol className="breadcrumb px-3 py-2 rounded mb-0">
                <li className="breadcrumb-item">
                    <Link to={props.link}>
                        {props.backLink}
                    </Link>
                </li>
                <li className="breadcrumb-item active"> {props.currentLink}</li>
            </ol>
        </nav>
    )
}

export default BreadCrumbBase
