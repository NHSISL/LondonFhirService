// ---
// skill: the-standard-testing
// type: example
// source-section: "2.1 Foundation Services — Testing"
// demonstrates: "ts-testing-001, ts-testing-002, ts-testing-003, ts-testing-004, ts-testing-007, ts-testing-008"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

public partial class StudentServiceTests
{
    // ✅ Happy path test — named correctly, single assertion, mocked broker
    [Fact]
    public async Task ShouldAddStudentAsync()
    {
        // given
        Student randomStudent = CreateRandomStudent();
        Student inputStudent = randomStudent;
        Student storageStudent = inputStudent;
        Student expectedStudent = storageStudent.DeepClone();

        this.storageBrokerMock.Setup(broker =>
            broker.InsertStudentAsync(inputStudent))
                .ReturnsAsync(storageStudent);

        // when
        Student actualStudent =
            await this.studentService.AddStudentAsync(inputStudent);

        // then
        actualStudent.Should().BeEquivalentTo(expectedStudent);

        this.storageBrokerMock.Verify(broker =>
            broker.InsertStudentAsync(inputStudent),
                Times.Once);

        this.storageBrokerMock.VerifyNoOtherCalls();
        this.loggingBrokerMock.VerifyNoOtherCalls();
    }

    // ✅ Validation failure test — null input
    [Fact]
    public async Task ShouldThrowValidationExceptionOnAddIfStudentIsNullAsync()
    {
        // given
        Student nullStudent = null;
        var nullStudentException = new NullStudentException();

        var expectedStudentValidationException =
            new StudentValidationException(nullStudentException);

        // when
        ValueTask<Student> addStudentTask =
            this.studentService.AddStudentAsync(nullStudent);

        StudentValidationException actualStudentValidationException =
            await Assert.ThrowsAsync<StudentValidationException>(
                addStudentTask.AsTask);

        // then
        actualStudentValidationException.Should()
            .BeEquivalentTo(expectedStudentValidationException);

        this.loggingBrokerMock.Verify(broker =>
            broker.LogError(It.Is(SameExceptionAs(expectedStudentValidationException))),
                Times.Once);

        this.storageBrokerMock.Verify(broker =>
            broker.InsertStudentAsync(It.IsAny<Student>()),
                Times.Never);

        this.storageBrokerMock.VerifyNoOtherCalls();
        this.loggingBrokerMock.VerifyNoOtherCalls();
    }
}
