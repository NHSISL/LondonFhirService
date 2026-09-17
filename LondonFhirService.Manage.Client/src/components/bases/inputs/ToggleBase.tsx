import { FunctionComponent, ChangeEvent } from "react";
import { Form } from "react-bootstrap";

// A switch rather than a checkbox. CheckboxBase already covers the tick-box case; this is for a
// setting that reads as on or off, where the control should say which state it is in without the
// reader having to interpret an empty box.
interface ToggleBaseProps {
    id: string;
    name: string;
    label?: string;
    description?: string;
    onChange: (event: ChangeEvent<HTMLInputElement>) => void;
    checked: boolean;
    disabled?: boolean;
    error?: string;
}

const ToggleBase: FunctionComponent<ToggleBaseProps> = (props) => {
    return (
        <Form.Group>
            {props.label && (<b><Form.Label htmlFor={props.id}>{props.label}</Form.Label></b>)}
            <div className="d-flex align-items-center">
                <Form.Check
                    type="switch"
                    id={props.id}
                    name={props.name}
                    checked={props.checked}
                    disabled={props.disabled}
                    onChange={props.onChange} />

                {/*
                    Plain text, and hidden from assistive technology on purpose. Passing this to
                    Form.Check's own label prop rendered a second <label for> against the same
                    control, so the accessible name came out as "Demographics only On" and changed
                    to "...Off" when the operator flipped it - a control whose name moves as its
                    state moves. The switch already exposes its state through the checkbox role, so
                    a reader is told it twice and the visible word is decoration.
                */}
                <span className="ms-2" aria-hidden="true">
                    {props.checked ? "On" : "Off"}
                </span>
            </div>
            {props.description && (<small>{props.description}</small>)}
            {props.error && <small className="text-danger">{props.error}</small>}
        </Form.Group>
    );
};

export default ToggleBase;
