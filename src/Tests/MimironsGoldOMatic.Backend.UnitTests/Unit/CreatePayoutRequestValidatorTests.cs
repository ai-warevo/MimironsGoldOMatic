using FluentValidation.TestHelper;
using MimironsGoldOMatic.Backend.Abstract;
using MimironsGoldOMatic.Backend.Shared;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>FluentValidation rules for Extension claim body (character name + idempotency key).</summary>
[Trait("Category", "Unit")]
public sealed class CreatePayoutRequestValidatorTests
{
    private readonly CreatePayoutRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenCharacterNameAndEnrollmentIdValid()
    {
        var r = _sut.TestValidate(new CreatePayoutRequest("Ab", "enroll-1"));
        r.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenCharacterNameTooShort()
    {
        var r = _sut.TestValidate(new CreatePayoutRequest("A", "enroll-1"));
        r.ShouldHaveValidationErrorFor(x => x.CharacterName);
    }

    [Fact]
    public void Should_Fail_WhenEnrollmentRequestIdEmpty()
    {
        var r = _sut.TestValidate(new CreatePayoutRequest("Abcd", ""));
        r.ShouldHaveValidationErrorFor(x => x.EnrollmentRequestId);
    }

    [Fact]
    public void Should_Fail_WhenCharacterNameContainsDigits()
    {
        var r = _sut.TestValidate(new CreatePayoutRequest("Ab12", "enroll-1"));
        r.ShouldHaveValidationErrorFor(x => x.CharacterName);
    }
}
