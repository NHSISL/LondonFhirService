export class ApiValidationError extends Error {
    errors: unknown
    
    constructor(validationErrors : Array<unknown>){
        super("ApiValidationError");
        this.errors = validationErrors
    }
}