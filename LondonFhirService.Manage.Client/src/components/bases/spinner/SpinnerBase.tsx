import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons'

export const SpinnerBase = () => {
    return (
        <FontAwesomeIcon icon={faCircleNotch} spin size="2x" className="loadingSpinner p-2" style={{ color: "#0ABF7D"} } />
    );
}