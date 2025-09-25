import { FunctionComponent, ChangeEvent } from "react";
import { InputGroup, Form } from "react-bootstrap"
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAsterisk } from "@fortawesome/free-solid-svg-icons";

interface TextAreaInputBaseProps {
    id: string;
    name: string;
    label?: string;
    placeholder?: string;
    prependLabel?: string;
    appendLabel?: string;
    description?: string,
    onChange: (event: ChangeEvent<HTMLTextAreaElement>) => void;
    value?: string;
    rows: number;
    showCount?: boolean;
    required?: boolean;
    showRemaingCount?: boolean;
    maxCharacters?: number;
    disabled?: boolean;
    error?: string
}

const TextAreaInputBase: FunctionComponent<TextAreaInputBaseProps> = (props) => {
    return (
        <Form.Group>
            {props.label && (<b><Form.Label htmlFor={props.id}>{props.label}</Form.Label></b>)}
            <div>
                <InputGroup className="mb-0">
                    {
                        props.prependLabel !== undefined 
                        && props.prependLabel.length > 0 
                        && <InputGroup.Text>{props.prependLabel}</InputGroup.Text>
                    }
                    <Form.Control as="textarea"
                        id={props.id}
                        name={props.name}
                        value={props.value || ""}
                        onChange={props.onChange}
                        rows={props.rows}
                        placeholder={props.placeholder || ""}
                        disabled={props.disabled}
                        //error={props.error}
                    />
                    {
                        props.appendLabel !== undefined 
                        && props.appendLabel.length > 0 
                        && <InputGroup.Text>{props.appendLabel}</InputGroup.Text>
                    }

                    {
                        props.required &&
                        <span style={{ marginTop: "8px" }}> &nbsp;
                            <FontAwesomeIcon icon={faAsterisk} className="text-danger" title="required" />
                        </span>
                    }
                </InputGroup>
            </div>

            {props.showCount === true && (
                <>
                    <small>{`Characters: ${props.value?.length}`}  </small>
                   
                </>
            )}
            
            {
                props.showRemaingCount === true 
                && props.maxCharacters !== undefined 
                && props.maxCharacters > 0 
                && <small>{`Remaining: ${props.maxCharacters - (props.value?.length || 0)}`}</small>
            }

            {props.description && <><br /><small>{props.description}</small></>}
            {props.error && <Form.Control.Feedback type="invalid">{props.error}</Form.Control.Feedback>}
        </Form.Group>
    );
};

TextAreaInputBase.defaultProps = {
    rows: 5,
    error: "",
};

export default TextAreaInputBase;
