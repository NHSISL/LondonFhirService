// ---
// skill: the-standard-exceptions
// type: example
// source-section: "2.1.3.1 Exception Handling / 2.0.4 Exception Models"
// demonstrates: "ts-exceptions-001, ts-exceptions-002, ts-exceptions-003, ts-exceptions-004, ts-exceptions-005, ts-exceptions-006, ts-exceptions-007"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

// ✅ Inner exception types (local to this layer)
public class NullStudentException : Xeption
{
    public NullStudentException()
        : base(message: "Student is null.") { }
}

public class InvalidStudentException : Xeption
{
    public InvalidStudentException()
        : base(message: "Invalid student. Please correct the errors and try again.") { }
}

public class FailedStudentStorageException : Xeption
{
    public FailedStudentStorageException(Exception innerException)
        : base(message: "Failed student storage error occurred, contact support.", innerException) { }
}

public class AlreadyExistsStudentException : Xeption
{
    public AlreadyExistsStudentException(Exception innerException)
        : base(message: "Student with the same id already exists.", innerException) { }
}

// ✅ Outer exception categories — exactly three (+ DependencyValidation)
public class StudentValidationException : Xeption
{
    public StudentValidationException(Xeption innerException)
        : base(message: "Student validation error occurred, fix the errors and try again.", innerException) { }
}

public class StudentDependencyValidationException : Xeption
{
    public StudentDependencyValidationException(Xeption innerException)
        : base(message: "Student dependency validation error occurred, fix the errors and try again.", innerException) { }
}

public class StudentDependencyException : Xeption
{
    public StudentDependencyException(Xeption innerException)
        : base(message: "Student dependency error occurred, contact support.", innerException) { }
}

public class StudentServiceException : Xeption
{
    public StudentServiceException(Exception innerException)
        : base(message: "Student service error occurred, contact support.", innerException) { }
}
