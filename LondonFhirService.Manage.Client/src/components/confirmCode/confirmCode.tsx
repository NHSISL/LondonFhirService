import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

export const ConfirmCode = () => {
    const [code, setCode] = useState("AGENT");
    const [error, setError] = useState("");
    const navigate = useNavigate();

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const rawValue = e.target.value.toUpperCase();
        if (rawValue === "AGENT") {
            setCode("AGENT");
            if (error) setError("");
            return;
        }
        const value = rawValue.replace(/\D/g, "").slice(0, 5);
        setCode(value);
        if (error) setError("");
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const isAgent = code.trim().toUpperCase() === "AGENT";
        const isFiveDigits = /^\d{5}$/.test(code);
        if (!isAgent && !isFiveDigits) {
            setError("Please enter a 5-digit code.");
            return;
        }
        navigate("/optInOut");
    };
    return (
        <form className="nhsuk-form-group" autoComplete="off" onSubmit={handleSubmit} >
            <label className="nhsuk-label" htmlFor="code">
                Enter Code
            </label>
            <input
                className="nhsuk-input"
                id="code"
                name="code"
                type="text"
                inputMode="numeric"
                maxLength={5}
                autoComplete="one-time-code"
                value={code}
                onChange={handleInputChange}
                style={{ width: "100%", maxWidth: "200px" }}
                aria-describedby={error ? "code-error" : undefined}
                aria-invalid={!!error}
                readOnly={code.trim().toUpperCase() === "AGENT"}
            />
            {error && (
                <div
                    id="code-error"
                    className="nhsuk-error-message"
                    style={{ marginTop: "0.5rem" }}
                    role="alert"
                >
                    <strong>Error:</strong> {error}
                </div>
            )}

            <button className="nhsuk-button" type="submit" style={{ width: "100%", marginTop: "1.5rem" }}>
                Submit
            </button>
        </form>
    );
};

export default ConfirmCode;
