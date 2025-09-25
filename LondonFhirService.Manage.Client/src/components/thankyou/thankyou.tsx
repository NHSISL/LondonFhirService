import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

export const Thankyou = () => {
    const [selected, setSelected] = useState<string | null>(null);
    const navigate = useNavigate();

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setSelected(e.target.name === selected ? null : e.target.name);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        // Optionally save preferences here
        navigate("/");
    };

    return (
        <form className="nhsuk-form-group" onSubmit={handleSubmit}>
            <p style={{ fontWeight: 500, marginBottom: "1.5rem" }}>
                We will notify you when this has been enacted.
            </p>
            <label className="nhsuk-label" style={{ marginBottom: "1rem" }}>
                How would you like to receive it:
            </label>
            <div className="nhsuk-checkboxes nhsuk-checkboxes--vertical" style={{ marginBottom: "1.5rem" }}>
                <div className="nhsuk-checkboxes__item">
                    <input
                        className="nhsuk-checkboxes__input"
                        id="sms"
                        name="sms"
                        type="checkbox"
                        checked={selected === "sms"}
                        onChange={handleChange}
                    />
                    <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor="sms">
                        SMS
                    </label>
                </div>
                <div className="nhsuk-checkboxes__item">
                    <input
                        className="nhsuk-checkboxes__input"
                        id="email"
                        name="email"
                        type="checkbox"
                        checked={selected === "email"}
                        onChange={handleChange}
                    />
                    <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor="email">
                        EMAIL
                    </label>
                </div>
                <div className="nhsuk-checkboxes__item">
                    <input
                        className="nhsuk-checkboxes__input"
                        id="post"
                        name="post"
                        type="checkbox"
                        checked={selected === "post"}
                        onChange={handleChange}
                    />
                    <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor="post">
                        POST
                    </label>
                </div>
            </div>

            <button className="nhsuk-button" type="submit" style={{ width: "100%" }}>
                Save Preferences
            </button>

            <div style={{ color: "#505a5f" }}>
                <strong>You can come back to this site and change your preference at any time.</strong>
            </div>
        </form>
    );
};

export default Thankyou;