import { FunctionComponent } from "react";
import { DateInput, type DateInputChangeEvent } from 'nhsuk-react-components'

interface DateTimeBaseProps {
    id: string;
    name: string;
    label?: string;
    hint?: string;
    value: string;
    onChange: (event: DateInputChangeEvent) => void;
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

    return (
        <div>
            <DateInput
                id={props.id}
                name={props.name}
                onChange={props.onChange}
                error={props.error}
                hint={props.hint}
                legend={props.label}>
                <DateInput.Day defaultValue={day} />
                <DateInput.Month defaultValue={month} />
                <DateInput.Year defaultValue={year} />
            </DateInput>
        </div>
    );
};

export default DateTimeBase;
