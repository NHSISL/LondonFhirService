import { Validation } from "../models/validations/validation";
import { beforeEach, afterEach, it, expect } from 'vitest'
import { ValidationProcessor } from "./validationProcessor";
import { ValidationMessages } from "./validationMessages";

type TestErrors = {
    hasErrors: false,
    test: string
}

type TestApiErrors = {
    test: string[]
}

type TestErrorsWith2Properties = {
    hasErrors: false,
    test: string
    test2: string
}

type TestApiWith2PropertiesErrors = {
    test: string[]
    test2: string[]
}

const valuesDefinition = {
    test: ""
}

const errorObject: TestErrors = {
    hasErrors: false,
    test: ""
}

const errorObjectWith2Properties: TestErrorsWith2Properties = {
    hasErrors: false,
    test: "",
    test2: ""
}

let container: HTMLSpanElement | null;

beforeEach(() => {
    container = document.createElement('span');
});

afterEach(() => {
    if(container){
        container.remove();
        container = null;
    }
});

it("Disabled validation returns no message", () => {
    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        isRequired: true
    }]

    const validationErrors = ValidationProcessor<TestErrors, TestApiErrors>().validate(errorObject, validation, valuesDefinition, false);
    console.log(validationErrors.test);
    expect(validationErrors.test).toBe("");
})

it("No validation requirement returns empty message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        isRequired: false
    }]

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().validate(errorObject, validation, valuesDefinition, true);

    expect(validationErrors.test).toBe("");
})

it("Is Required validation returns correct message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        isRequired: true
    }]

    const validationErrors = ValidationProcessor<TestErrors, TestApiErrors>().validate(errorObject, validation, valuesDefinition, true);

    expect(validationErrors.test).not.toBe(null);
    expect(validationErrors.test).toBe(`${validation[0].friendlyName} ${ValidationMessages.requiredMessage()}.`);
})

it("Min length validation returns correct message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        minLength: 2
    }]

    const values = { ...valuesDefinition, test: "1" };

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().validate(errorObject, validation, values, true);

    expect(validationErrors.test).toBe(`${validation[0].friendlyName} ${ValidationMessages.minimumLengthMessage(2)}.`);
})

it("Max length validation returns correct message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        maxLength: 2
    }]

    const values = { ...valuesDefinition, test: "123" };

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().validate(errorObject, validation, values, true);

    expect(validationErrors.test).toBe(`${validation[0].friendlyName} ${ValidationMessages.maximumLengthMessage(2)}.`);
})

it("Min value validation returns correct message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        minValue: 2
    }]

    const values = { ...valuesDefinition, test: 1 };

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().validate(errorObject, validation, values, true);

    expect(validationErrors.test).toBe(`${validation[0].friendlyName} ${ValidationMessages.minimumValueMessage(2)}.`);
})

it("Min value validation returns correct message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        maxValue: 2
    }]

    const values = { ...valuesDefinition, test: 3 };

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().validate(errorObject, validation, values, true);

    expect(validationErrors.test).toBe(`${validation[0].friendlyName} ${ValidationMessages.maximumValueMessage(2)}.`);
})


it("Multiple validation errors returns correct message", async () => {

    const validation: Validation[] = [{
        friendlyName: Math.random().toString().substr(2, 8),
        property: "test",
        isRequired: true,
        minLength: 3
    }]

    const values = { ...valuesDefinition };

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().validate(errorObject, validation, values, true);

    expect(validationErrors.test).toBe(`${validation[0].friendlyName} ${ValidationMessages.requiredMessage()} and ${ValidationMessages.minimumLengthMessage(3)}.`);
})

it("API error object is processed correctly", async () => {
    const apiErrors: TestApiErrors = {
        test: ["Error Message 1"]
    }

    const validationErrors = ValidationProcessor<TestErrors,TestApiErrors>().processApiErrors(apiErrors, errorObject);
    expect(validationErrors.hasErrors).toBe(true);
    expect(validationErrors.test).toBe(apiErrors.test[0])
})

it("API complex error object is processed correctly", async () => {
    const apiErrors: TestApiWith2PropertiesErrors = {
        "test": ["Error Message 1", "Error Message 2"],
        "test2": ["Error Message 3", "Error Message 4"] 
    }

    const validationErrors = ValidationProcessor<TestErrorsWith2Properties, TestApiWith2PropertiesErrors>().processApiErrors(apiErrors, errorObjectWith2Properties);
    expect(validationErrors.hasErrors).toBe(true);
    expect(validationErrors.test).toBe(`${apiErrors.test[0]}, ${apiErrors.test[1]}`)
    expect(validationErrors.test2).toBe(`${apiErrors.test2[0]}, ${apiErrors.test2[1]}`)
})