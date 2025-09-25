import { FunctionComponent, ChangeEvent } from "react";
import { Label } from 'nhsuk-react-components'
import { InputGroup, Form } from "react-bootstrap"
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAsterisk } from "@fortawesome/free-solid-svg-icons";

interface SelectInputBaseProps {
    id: string;
    name: string;
    label?: string;
    prependLabel?: string;
    appendLabel?: string;
    description?: string;
    required?: boolean;
    onChange: (event: ChangeEvent<HTMLSelectElement>) => void;
    options: Array<Option>;
    value: string | number | undefined ;
    error?: string;
}

interface Option {
    id: string | number;
    name: string;
}

const SelectInputBase: FunctionComponent<SelectInputBaseProps> = (props) => {
    return (
        <Form.Group>
            {props.label && (<b><Label htmlFor={props.id}>{props.label}</Label></b>)}
            <div>
                <InputGroup>
                    {props.prependLabel !== undefined && props.prependLabel.length > 0 && (
                        <InputGroup.Text>{props.prependLabel}</InputGroup.Text>
                    )}
                    <Form.Select 
                        id={props.id}
                        name={props.name}
                        value={props.value}
                        title={props.name}
                        onChange={props.onChange}
                    >
                        {props.options.length > 0 &&
                            props.options.map((option, i) => {
                                return (
                                    <option key={i} value={option.id.toString()}>
                                        {option.name}
                                    </option>
                                );
                            })}
                    </Form.Select>

                    {props.appendLabel !== undefined && props.appendLabel.length > 0 && (
                        <InputGroup.Text>{props.appendLabel}</InputGroup.Text>
                    )}

                    {props.required &&
                        <span style={{ marginTop: "8px" }}> &nbsp;
                            <FontAwesomeIcon icon={faAsterisk} className="text-danger" title="required" />
                        </span>
                    }
                </InputGroup>
                {props.description && (<><br /><small>{props.description}</small></>)}
            </div>
        </Form.Group>
    );
};

SelectInputBase.defaultProps = {
    error: "",
};

export default SelectInputBase;
