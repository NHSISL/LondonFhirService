import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

export const OptInOptOut = () => {
    const [selected, setSelected] = useState<"optout" | "optin" | "">("");
    const [error, setError] = useState("");
    const navigate = useNavigate();

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!selected) {
            setError("Please select an option to continue.");
            return;
        }
        setError("");
        // Navigate to thankyou route
        navigate("/thankyou");
    };

    return (
        <form onSubmit={handleSubmit}>
            {/* Opt-Out Card */}
            <div
                className="nhsuk-card"
                style={{
                    border: selected === "optout" ? "2px solid #005eb8" : "1px solid #d8dde0",
                    marginBottom: "2rem",
                    padding: "1.5rem",
                    borderRadius: "6px",
                    background: "#fff"
                }}
            >
                <label style={{ display: "flex", alignItems: "flex-start", cursor: "pointer" }}>
                    <input
                        type="radio"
                        name="opt"
                        value="optout"
                        checked={selected === "optout"}
                        onChange={() => setSelected("optout")}
                        style={{ marginRight: "1rem", marginTop: "0.2rem" }}
                        aria-describedby="optout-desc"
                    />
                    <div>
                        <strong>Opt-Out</strong>
                        <div id="optout-desc" style={{ marginTop: "0.5rem" }}>
                            <div>
                                I do not want my personal data to be used in the London Data Service for Research and Commissioning purposes.
                            </div>
                            <div style={{ marginTop: "0.5rem", color: "#505a5f" }}>
                                I acknowledge that my data will still reside in the London Data Service for direct care purposes.
                            </div>
                        </div>
                    </div>
                </label>
            </div>

            {/* Opt-In Card */}
            <div
                className="nhsuk-card"
                style={{
                    border: selected === "optin" ? "2px solid #005eb8" : "1px solid #d8dde0",
                    padding: "1.5rem",
                    borderRadius: "6px",
                    background: "#fff"
                }}
            >
                <label style={{ display: "flex", alignItems: "flex-start", cursor: "pointer" }}>
                    <input
                        type="radio"
                        name="opt"
                        value="optin"
                        checked={selected === "optin"}
                        onChange={() => setSelected("optin")}
                        style={{ marginRight: "1rem", marginTop: "0.2rem" }}
                        aria-describedby="optin-desc"
                    />
                    <div>
                        <strong>Opt-In</strong>
                        <div id="optin-desc" style={{ marginTop: "0.5rem" }}>
                            I do want my personal data to be used in the London Data Service for Direct Care, Research and Commissioning purposes.
                        </div>
                    </div>
                </label>
            </div>

            {error && (
                <div className="nhsuk-error-message" style={{ marginBottom: "1rem" }} role="alert">
                    <strong>Error:</strong> {error}
                </div>
            )}

            <button
                type="submit"
                className="nhsuk-button"
                style={{ width: "100%", marginTop: "1.5rem" }}
            >
                Submit
            </button>
        </form>
    );
};

export default OptInOptOut;
