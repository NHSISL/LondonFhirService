import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { TextInput, Button, Select } from "nhsuk-react-components";
import { Container, Row, Col } from "react-bootstrap";

export const SearchByNhsNumber = () => {
    const [nhsNumberInput, setNhsNumberInput] = useState("1234567890");
    const [poaNhsNumberInput, setPoaNhsNumberInput] = useState("");
    const [poaFirstname, setPoaFirstname] = useState("");
    const [poaSurname, setPoaSurname] = useState("");
    const [poaRelationship, setPoaRelationship] = useState("");
    const [error, setError] = useState("");
    const [poaNhsNumberError, setPoaNhsNumberError] = useState("");
    const [poaFirstnameError, setPoaFirstnameError] = useState("");
    const [poaSurnameError, setPoaSurnameError] = useState("");
    const [poaRelationshipError, setPoaRelationshipError] = useState("");
    const [isPowerOfAttorney, setIsPowerOfAttorney] = useState(false);

    const navigate = useNavigate();

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value.replace(/\D/g, "").slice(0, 10);
        setNhsNumberInput(value);
        if (error) setError("");
    };

    const handlePoaNhsNumberChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value.replace(/\D/g, "").slice(0, 10);
        setPoaNhsNumberInput(value);
        setPoaNhsNumberError("");
    };
    const handlePoaFirstnameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setPoaFirstname(e.target.value);
        setPoaFirstnameError("");
    };
    const handlePoaSurnameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setPoaSurname(e.target.value);
        setPoaSurnameError("");
    };
    const handlePoaRelationshipChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        setPoaRelationship(e.target.value);
        setPoaRelationshipError("");
    };

    const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setIsPowerOfAttorney(e.target.checked);
        setError("");
        // Reset PoA fields when unchecked
        if (!e.target.checked) {
            setPoaNhsNumberInput("");
            setPoaFirstname("");
            setPoaSurname("");
            setPoaRelationship("");
            setPoaNhsNumberError("");
            setPoaFirstnameError("");
            setPoaSurnameError("");
            setPoaRelationshipError("");
        }
    };

    const validatePoaFields = () => {
        let valid = true;
        if (poaNhsNumberInput.length !== 10) {
            setPoaNhsNumberError("Enter a 10-digit NHS Number");
            valid = false;
        }
        if (!poaFirstname.trim()) {
            setPoaFirstnameError("Enter a first name");
            valid = false;
        }
        if (!poaSurname.trim()) {
            setPoaSurnameError("Enter a surname");
            valid = false;
        }
        if (!poaRelationship) {
            setPoaRelationshipError("Select a relationship");
            valid = false;
        }
        return valid;
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        setError("");
        setPoaNhsNumberError("");
        setPoaFirstnameError("");
        setPoaSurnameError("");
        setPoaRelationshipError("");

        if (isPowerOfAttorney) {
            if (!validatePoaFields()) return;
            // Use PoA NHS number for navigation or API
            navigate("/confirmDetails");
        } else {
            if (nhsNumberInput.length !== 10) {
                setError("NHS Number must be exactly 10 digits.");
                return;
            }
            navigate("/confirmDetails");
        }
    };

    return (
        <Container fluid>
            <Row className="justify-content-center">
                <Col md={6} lg={7} xl={6}>
                    <form autoComplete="off" onSubmit={handleSubmit}>
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

                        {!isPowerOfAttorney && (
                            <TextInput
                                label="NHS Number"
                                hint="It's on your National Insurance card, benefit letter, payslip or P60."
                                id="nhs-number"
                                name="nhs-number"
                                inputMode="numeric"
                                pattern="\d*"
                                maxLength={10}
                                autoComplete="off"
                                value={nhsNumberInput}
                                onChange={handleInputChange}
                                error={error || undefined}
                                style={{ maxWidth: "200px" }}
                            />
                        )}

                        {isPowerOfAttorney && (
                            <div style={{ marginBottom: "1.5rem" }}>
                                <TextInput
                                    label="NHS Number of the person they are representing"
                                    id="poa-nhs-number"
                                    name="poa-nhs-number"
                                    inputMode="numeric"
                                    pattern="\d*"
                                    maxLength={10}
                                    autoComplete="off"
                                    value={poaNhsNumberInput}
                                    onChange={handlePoaNhsNumberChange}
                                    error={poaNhsNumberError || undefined}
                                    style={{ maxWidth: "300px" }}
                                />
                                <TextInput
                                    label="Firstname"
                                    id="poa-firstname"
                                    name="poa-firstname"
                                    autoComplete="off"
                                    value={poaFirstname}
                                    onChange={handlePoaFirstnameChange}
                                    error={poaFirstnameError || undefined}
                                    style={{ maxWidth: "400px" }}
                                />
                                <TextInput
                                    label="Surname"
                                    id="poa-surname"
                                    name="poa-surname"
                                    autoComplete="off"
                                    value={poaSurname}
                                    onChange={handlePoaSurnameChange}
                                    error={poaSurnameError || undefined}
                                    style={{ maxWidth: "400px" }}
                                />
                                <div style={{ marginBottom: "1rem" }}>
                                    <Select
                                        label="Relationship"
                                        id="poa-relationship"
                                        name="poa-relationship"
                                        aria-label="Relationship to the person you are representing"
                                        aria-required="true"
                                        required
                                        value={poaRelationship}
                                        onChange={handlePoaRelationshipChange}
                                        error={poaRelationshipError || undefined}
                                        style={{ maxWidth: "400px", marginBottom: "1rem" }}
                                    >
                                        <option value="" disabled>
                                            Select relationship
                                        </option>
                                        <option value="Parent">The patient is under 13 and you are their parent</option>
                                        <option value="Guardian">The patient is under 13 and you are their appointed guardian</option>
                                        <option value="poa">The patient is over 13 and you have power of attorney with the right to act on their behalf.</option>
                                    </Select>
                                </div>
                            </div>
                        )}

                        <div style={{ display: "flex", gap: "1rem", marginBottom: "0.2rem", marginTop: "1rem" }}>
                            <Button
                                type="submit"
                                disabled={
                                    isPowerOfAttorney
                                        ? !poaNhsNumberInput ||
                                        !poaFirstname.trim() ||
                                        !poaSurname.trim() ||
                                        !poaRelationship ||
                                        poaNhsNumberInput.length !== 10
                                        : nhsNumberInput.length !== 10
                                }
                            >
                                Search
                            </Button>
                        </div>
                    </form>
                </Col>
                <Col md={6} lg={5} xl={6}>
                    {isPowerOfAttorney && (
                        <aside
                            style={{
                                background: "#f8f8f8",
                                border: "1px solid #e5e5e5",
                                borderRadius: "4px",
                                padding: "1.5rem",
                                minWidth: "250px",
                                marginTop: "3.6rem"
                            }}>
                            <h2 className="mb-3" style={{ color: "#005eb8" }}>Help & Guidance</h2>
                            <h3 className="mb-3" style={{ color: "#005eb8" }}>
                                Requesting an Opt-out on someone else's behalf
                            </h3>
                            <p>
                                You can make a request to opt-out on behalf of someone else to stop their personal data being used for secondary purposes if:
                            </p>
                            <ul>
                                <li>The patient is under 13 and you are their parent</li>
                                <li>The patient is under 13 and you are their appointed guardian</li>
                                <li>The patient is over 13 and you have power of attorney with the right to act on their behalf.</li>
                            </ul>
                            <p>
                                If you are in these circumstances then please enter your details in this blue box and in every other box use the patient's details.
                            </p>
                            <p>
                                If one of these circumstances does not describe you then you cannot opt someone else out. Please click the back button.
                            </p>
                        </aside>
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default SearchByNhsNumber;