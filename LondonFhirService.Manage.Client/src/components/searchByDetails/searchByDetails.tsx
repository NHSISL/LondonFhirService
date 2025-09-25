import React, { useState } from "react";
import { Container, Row, Col } from "react-bootstrap";

interface SearchByDetailsProps {
    nextStep: () => void;
}

const SearchByDetails: React.FC<SearchByDetailsProps> = ({ nextStep }) => {
    // Main form state
    const [surname, setSurname] = useState("");
    const [postcode, setPostcode] = useState("");
    const [dobDay, setDobDay] = useState("");
    const [dobMonth, setDobMonth] = useState("");
    const [dobYear, setDobYear] = useState("");

    // Power of Attorney form state
    const [poaSurname, setPoaSurname] = useState("");
    const [poaPostcode, setPoaPostcode] = useState("");
    const [poaDobDay, setPoaDobDay] = useState("");
    const [poaDobMonth, setPoaDobMonth] = useState("");
    const [poaDobYear, setPoaDobYear] = useState("");

    const [error, setError] = useState("");
    const [isPowerOfAttorney, setIsPowerOfAttorney] = useState(false);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!surname || !postcode || !dobDay || !dobMonth || !dobYear) {
            setError("All fields are required.");
            return;
        }
        if (isPowerOfAttorney) {
            if (!poaSurname || !poaPostcode || !poaDobDay || !poaDobMonth || !poaDobYear) {
                setError("All Power of Attorney fields are required.");
                return;
            }
        }
        setError("");
        nextStep();
    };

    const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setIsPowerOfAttorney(e.target.checked);
        setError("");
        if (!e.target.checked) {
            setPoaSurname("");
            setPoaPostcode("");
            setPoaDobDay("");
            setPoaDobMonth("");
            setPoaDobYear("");
        }
    };

    return (
        <Container fluid>
            <Row className="justify-content-center">
                <Col md={5} lg={6} xl={6}>
                    <form className="nhsuk-form-group" autoComplete="off" onSubmit={handleSubmit} >
                        <div style={{ margin: "1rem 0" }}>
                            <label>
                                <input
                                    type="checkbox"
                                    checked={isPowerOfAttorney}
                                    onChange={handleCheckboxChange}
                                    style={{ marginRight: "0.5rem" }}
                                />
                                Requesting an Opt-out on someone else's behalf.
                            </label>
                        </div>

                        <div
                            style={{
                                padding: "1rem",
                                marginBottom: "1rem",
                            }}
                            data-testid="power-of-attorney-section">

                            {isPowerOfAttorney && (
                                <>
                                <h5 style={{ marginBottom: "0.5rem" }}>Provide Details of the person they are representing</h5>
                                   
                                </>
                            )}
                            {!isPowerOfAttorney && (
                                <>
                                    <h5 style={{ marginBottom: "0.5rem" }}>Provide Patients Details</h5>
                                    
                                </>
                            )}

                            <label className="nhsuk-label mt-2" htmlFor="surname">
                                Surname
                            </label>
                            <input
                                className="nhsuk-input"
                                id="surname"
                                name="surname"
                                type="text"
                                autoComplete="family-name"
                                value={surname}
                                onChange={e => setSurname(e.target.value)}
                                style={{ marginBottom: "1rem" }}
                            />

                            <label className="nhsuk-label" htmlFor="postcode">
                                Postcode
                            </label>
                            <input
                                className="nhsuk-input"
                                id="postcode"
                                name="postcode"
                                type="text"
                                autoComplete="postal-code"
                                value={postcode}
                                onChange={e => setPostcode(e.target.value)}
                                style={{ marginBottom: "1rem" }}
                            />

                            <fieldset className="nhsuk-fieldset" style={{ marginBottom: "1rem" }}>
                                <legend className="nhsuk-fieldset__legend nhsuk-label">
                                    Date of birth
                                </legend>
                                <div className="nhsuk-date-input" id="dob">
                                    <div className="nhsuk-date-input__item" style={{ display: "inline-block", marginRight: "0.5rem" }}>
                                        <label className="nhsuk-label nhsuk-date-input__label" htmlFor="dob-day">
                                            Day
                                        </label>
                                        <input
                                            className="nhsuk-input nhsuk-date-input__input"
                                            id="dob-day"
                                            name="dob-day"
                                            type="text"
                                            inputMode="numeric"
                                            pattern="[0-9]*"
                                            maxLength={2}
                                            value={dobDay}
                                            onChange={e => setDobDay(e.target.value.replace(/\D/g, ""))}
                                            style={{ width: "3em" }}
                                            autoComplete="bday-day"
                                        />
                                    </div>
                                    <div className="nhsuk-date-input__item" style={{ display: "inline-block", marginRight: "0.5rem" }}>
                                        <label className="nhsuk-label nhsuk-date-input__label" htmlFor="dob-month">
                                            Month
                                        </label>
                                        <input
                                            className="nhsuk-input nhsuk-date-input__input"
                                            id="dob-month"
                                            name="dob-month"
                                            type="text"
                                            inputMode="numeric"
                                            pattern="[0-9]*"
                                            maxLength={2}
                                            value={dobMonth}
                                            onChange={e => setDobMonth(e.target.value.replace(/\D/g, ""))}
                                            style={{ width: "3em" }}
                                            autoComplete="bday-month"
                                        />
                                    </div>
                                    <div className="nhsuk-date-input__item" style={{ display: "inline-block" }}>
                                        <label className="nhsuk-label nhsuk-date-input__label" htmlFor="dob-year">
                                            Year
                                        </label>
                                        <input
                                            className="nhsuk-input nhsuk-date-input__input"
                                            id="dob-year"
                                            name="dob-year"
                                            type="text"
                                            inputMode="numeric"
                                            pattern="[0-9]*"
                                            maxLength={4}
                                            value={dobYear}
                                            onChange={e => setDobYear(e.target.value.replace(/\D/g, ""))}
                                            style={{ width: "4em" }}
                                            autoComplete="bday-year"
                                        />
                                    </div>
                                </div>
                            </fieldset>
                        </div>


                        {isPowerOfAttorney && (
                            <div
                                style={{
                                    border: "1px solid #005eb8",
                                    borderRadius: "4px",
                                    padding: "1rem",
                                    marginBottom: "1rem",
                                    background: "#f0f4f5"
                                }}
                                data-testid="power-of-attorney-section"
                            >
                                <h5>Provide Details of the person that is representing</h5>
                                <label className="nhsuk-label" htmlFor="poa-surname">
                                    Surname
                                </label>
                                <input
                                    className="nhsuk-input"
                                    id="poa-surname"
                                    name="poa-surname"
                                    type="text"
                                    autoComplete="family-name"
                                    value={poaSurname}
                                    onChange={e => setPoaSurname(e.target.value)}
                                    style={{ marginBottom: "1rem" }}
                                />

                                <label className="nhsuk-label" htmlFor="poa-postcode">
                                    Postcode
                                </label>
                                <input
                                    className="nhsuk-input"
                                    id="poa-postcode"
                                    name="poa-postcode"
                                    type="text"
                                    autoComplete="postal-code"
                                    value={poaPostcode}
                                    onChange={e => setPoaPostcode(e.target.value)}
                                    style={{ marginBottom: "1rem" }}
                                />

                                <fieldset className="nhsuk-fieldset" style={{ marginBottom: "1rem" }}>
                                    <legend className="nhsuk-fieldset__legend nhsuk-label">
                                        Date of birth
                                    </legend>
                                    <div className="nhsuk-date-input" id="poa-dob">
                                        <div className="nhsuk-date-input__item" style={{ display: "inline-block", marginRight: "0.5rem" }}>
                                            <label className="nhsuk-label nhsuk-date-input__label" htmlFor="poa-dob-day">
                                                Day
                                            </label>
                                            <input
                                                className="nhsuk-input nhsuk-date-input__input"
                                                id="poa-dob-day"
                                                name="poa-dob-day"
                                                type="text"
                                                inputMode="numeric"
                                                pattern="[0-9]*"
                                                maxLength={2}
                                                value={poaDobDay}
                                                onChange={e => setPoaDobDay(e.target.value.replace(/\D/g, ""))}
                                                style={{ width: "3em" }}
                                                autoComplete="bday-day"
                                            />
                                        </div>
                                        <div className="nhsuk-date-input__item" style={{ display: "inline-block", marginRight: "0.5rem" }}>
                                            <label className="nhsuk-label nhsuk-date-input__label" htmlFor="poa-dob-month">
                                                Month
                                            </label>
                                            <input
                                                className="nhsuk-input nhsuk-date-input__input"
                                                id="poa-dob-month"
                                                name="poa-dob-month"
                                                type="text"
                                                inputMode="numeric"
                                                pattern="[0-9]*"
                                                maxLength={2}
                                                value={poaDobMonth}
                                                onChange={e => setPoaDobMonth(e.target.value.replace(/\D/g, ""))}
                                                style={{ width: "3em" }}
                                                autoComplete="bday-month"
                                            />
                                        </div>
                                        <div className="nhsuk-date-input__item" style={{ display: "inline-block" }}>
                                            <label className="nhsuk-label nhsuk-date-input__label" htmlFor="poa-dob-year">
                                                Year
                                            </label>
                                            <input
                                                className="nhsuk-input nhsuk-date-input__input"
                                                id="poa-dob-year"
                                                name="poa-dob-year"
                                                type="text"
                                                inputMode="numeric"
                                                pattern="[0-9]*"
                                                maxLength={4}
                                                value={poaDobYear}
                                                onChange={e => setPoaDobYear(e.target.value.replace(/\D/g, ""))}
                                                style={{ width: "4em" }}
                                                autoComplete="bday-year"
                                            />
                                        </div>
                                    </div>
                                </fieldset>
                            </div>
                        )}

                        {error && (
                            <div className="nhsuk-error-message" style={{ marginBottom: "1rem" }} role="alert">
                                <strong>Error:</strong> {error}
                            </div>
                        )}

                        <button className="nhsuk-button" type="submit" style={{ width: "100%" }}>
                            Search
                        </button>
                    </form>
                </Col>
                <Col md={7} lg={6} xl={6}>
                    {isPowerOfAttorney && (
                        <aside
                            style={{
                                background: "#f8f8f8",
                                border: "1px solid #e5e5e5",
                                borderRadius: "4px",
                                padding: "1.5rem",
                                minWidth: "250px",
                                marginTop: "3.5rem"
                            }}
                        >
                            <h3>Help and Guidance</h3>
                            <p>
                                Please enter your surname, postcode, and date of birth as they appear on your NHS records. If you are unsure of your details, check any official NHS correspondence or contact your GP practice for assistance.
                            </p>
                            <ul>
                                <li>Ensure your surname matches your NHS record.</li>
                                <li>Enter your full postcode, including any spaces.</li>
                                <li>Provide your date of birth in the correct format (day, month, year).</li>
                                <li>If you are acting under a Power of Attorney, tick the box and provide the details for the person you represent.</li>
                            </ul>
                        </aside>
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default SearchByDetails;