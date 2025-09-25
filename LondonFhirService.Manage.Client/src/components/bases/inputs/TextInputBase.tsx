import { FunctionComponent, ChangeEvent } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAsterisk } from "@fortawesome/free-solid-svg-icons";
import { Form, InputGroup } from "react-bootstrap";

interface TextInputBaseProps {
    id: string;
    name: string;
    label?: string;
    placeholder?: string;
    prependLabel?: string;
    appendLabel?: string;
    description?: string;
    required?: boolean;
    onChange: (event: ChangeEvent<HTMLInputElement>) => void;
    value?: string | number;
    error?: string;
    type?: string;
    disabled?: boolean;
    maxLength?: number; // Added maxLength prop
}

const TextInputBase: FunctionComponent<TextInputBaseProps> = (props) => {
    return (
        <Form.Group>
            {props.label && (<b><Form.Label htmlFor={props.id}>{props.label}</Form.Label> </b>)}
            <div>
                <InputGroup>
                    {
                        props.prependLabel !== undefined
                        && props.prependLabel.length > 0
                        && (
                            <InputGroup.Text>{props.prependLabel}</InputGroup.Text>
                        )}
                    <Form.Control
                        id={props.id}
                        name={props.name}
                        value={props.value}
                        onChange={props.onChange}
                        type={props.type || "text"}
                        placeholder={props.placeholder || ""}
                        disabled={props.disabled}
                        maxLength={props.maxLength} // Pass maxLength to Form.Control
                    //error={props.error}
                    />
                    {
                        props.appendLabel !== undefined
                        && props.appendLabel.length > 0
                        && (
                            <InputGroup.Text>{props.appendLabel}</InputGroup.Text>
                        )}
                    {
                        props.required &&
                        <span style={{ marginTop: "8px" }}> &nbsp;
                            <FontAwesomeIcon icon={faAsterisk} className="text-danger" title="required" />
                        </span>
                    }
                </InputGroup>
                {props.description && (<><br /><small>{props.description}</small></>)}
            </div>
        </Form.Group>
    );
}

TextInputBase.defaultProps = {
    error: "",
}

export default TextInputBase;