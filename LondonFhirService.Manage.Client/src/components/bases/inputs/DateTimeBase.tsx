import { FunctionComponent, ChangeEvent } from "react";
import { DateInput } from 'nhsuk-react-components'

interface DateTimeBaseProps {
    id: string;
    name: string;
    label?: string;
    hint?: string;
    value: string;
    type: string;
    onChange: (event: ChangeEvent<HTMLInputElement>) => void;
    error?: string;
}

const DateTimeBase: FunctionComponent<DateTimeBaseProps> = (props) => {
    let day = "";
    let month = "";
    let year = "";

    if (props.value !== undefined) {
        year = new Date(props.value).getFullYear().toString();
        month = ('0' + (new Date(props.value).getMonth() + 1)).slice(-2)
        day = ('0' + new Date(props.value).getDate()).slice(-2)
    }

    const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
        event.target.name = props.name;
        props.onChange(event);
    }

    return (
        <div>
            <DateInput
                id={props.id}
                name={props.name}
                onChange={handleChange}
                error={props.error}
                hint={props.hint}
                type={props.type}
                label={props.label}>
                <DateInput.Day defaultValue={day} />
                <DateInput.Month defaultValue={month} />
                <DateInput.Year defaultValue={year} />
            </DateInput>
        </div>
    );
};

DateTimeBase.defaultProps = {
    error: "",
};

export default DateTimeBase;
