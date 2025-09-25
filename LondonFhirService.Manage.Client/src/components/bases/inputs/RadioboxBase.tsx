import { FunctionComponent, ChangeEvent } from "react";
import { Label, Radios } from 'nhsuk-react-components'
import { InputGroup, Form } from "react-bootstrap"

interface RadioBaseProps {
    id: string;
    name: string;
    label?: string;
    prependLabel?: string;
    appendLabel?: string;
    description?: string,
    onChange: (event: ChangeEvent<HTMLInputElement>) => void;
    checked: boolean
    error?: string;
}

const RadioBase: FunctionComponent<RadioBaseProps> = (props) => {
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
                    <Radios>
                        <Radios.Radio
                            id={props.id}
                            name={props.name}
                            checked={props.checked}
                            onChange={props.onChange}>
                            &nbsp;
                        </Radios.Radio>
                    </Radios>
                    {
                        props.appendLabel !== undefined
                        && props.appendLabel.length > 0
                        && (
                            <InputGroup.Text>{props.appendLabel}</InputGroup.Text>
                        )}
                </InputGroup>
            </div>
            {props.error && <Form.Control.Feedback type="invalid">{props.error}</Form.Control.Feedback>}
        </Form.Group>
    );
};

RadioBase.defaultProps = {
    error: "",
};

export default RadioBase;
