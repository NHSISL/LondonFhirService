import { cleanup, render, screen } from "@testing-library/react";
import { ReactNode } from "react";
import { afterEach, expect, it, vi } from "vitest";
import Login from "./login";

// The signed in accounts and the active account, as MSAL holds them. Swapped per test.
let accounts: { username: string }[] = [];
let activeAccount: { username: string } | null = null;

vi.mock("@azure/msal-react", () => ({
    useMsal: () => ({
        instance: { getActiveAccount: () => activeAccount },
        accounts
    }),
    AuthenticatedTemplate: ({ children }: { children: ReactNode }) => <>{children}</>,
    UnauthenticatedTemplate: () => null
}));

vi.mock("../../authConfig", () => ({ MsalConfig: { loginRequest: {} } }));

afterEach(() => {
    cleanup();
    accounts = [];
    activeAccount = null;
});

it("should show the signed in account straight after the login redirect, before one is made active", () => {
    accounts = [{ username: "someone@nhs.net" }];

    render(<Login />);

    expect(screen.getByText("someone@nhs.net")).toBeTruthy();
});

it("should show the active account when one has been made active", () => {
    accounts = [{ username: "someone@nhs.net" }, { username: "someone.else@nhs.net" }];
    activeAccount = { username: "someone.else@nhs.net" };

    render(<Login />);

    expect(screen.getByText("someone.else@nhs.net")).toBeTruthy();
});
