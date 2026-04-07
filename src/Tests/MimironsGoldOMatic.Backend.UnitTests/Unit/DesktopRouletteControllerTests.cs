using MimironsGoldOMatic.Backend.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Desktop API key route for <c>verify-candidate</c>.</summary>
[Trait("Category", "Unit")]
public sealed class DesktopRouletteControllerTests
{
    [Fact]
    public async Task Should_ReturnOk_WhenVerifyCandidateSucceeds()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<VerifyCandidateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<global::MediatR.Unit>(true, global::MediatR.Unit.Value, 200, null));

        var sut = new DesktopRouletteController(mediator.Object);
        var body = new VerifyCandidateRequest(1, Guid.NewGuid(), "Hero", true, DateTime.UtcNow);

        var result = await sut.VerifyCandidate(body, CancellationToken.None);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Should_ReturnStatusCode_WhenVerifyCandidateFails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<VerifyCandidateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<global::MediatR.Unit>(false, default, 400, new ApiErrorDto("invalid_payload", "x", new { })));

        var sut = new DesktopRouletteController(mediator.Object);
        var body = new VerifyCandidateRequest(2, Guid.NewGuid(), "Hero", true, DateTime.UtcNow);

        var result = await sut.VerifyCandidate(body, CancellationToken.None);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }
}
