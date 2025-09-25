import { FunctionComponent, ChangeEvent } from "react";
import { Label, Checkboxes } from 'nhsuk-react-components'
import { InputGroup, Form } from "react-bootstrap"
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAsterisk } from "@fortawesome/free-solid-svg-icons";

interface CheckboxBaseProps {
    id: string;
    name: string;
    label?: string;
    prependLabel?: string;
    appendLabel?: string;
    description?: string,
    required?: boolean,
    onChange: (event: ChangeEvent<HTMLInputElement>) => void;
    checked: boolean;
    error?: string;
}

const CheckboxBase: FunctionComponent<CheckboxBaseProps> = (props) => {
    return (
        <Form.Group>
            {props.label && (<b><Label htmlFor={props.id}>{props.label}</Label></b>)}
            <div>
                <InputGroup hasValidation className="mb-3">
                    {
                        props.prependLabel !== undefined
                        && props.prependLabel.length > 0
                        && (
                            <InputGroup.Text>{props.prependLabel}</InputGroup.Text>
                        )}
                    <Checkboxes>
                        <Checkboxes.Box
                            id={props.id}
                            name={props.name}
                            type="checkbox"
                            checked={props.checked}
                            onChange={props.onChange}>
                            &nbsp;
                        </Checkboxes.Box>
                    </Checkboxes>
                    {
                        props.appendLabel !== undefined
                        && props.appendLabel.length > 0
                        && (<InputGroup.Text>{props.appendLabel}</InputGroup.Text>)
                    }

                    {props.required &&
                        <span style={{ marginTop: "8px" }}> &nbsp;
                            <FontAwesomeIcon icon={faAsterisk} className="text-danger" title="required" />
                        </span>
                    }
                </InputGroup>
            </div>
            {props.error && <Form.Control.Feedback type="invalid">{props.error}</Form.Control.Feedback>}
        </Form.Group>
    );
};

CheckboxBase.defaultProps = {
    error: "",
};

export default CheckboxBase;
