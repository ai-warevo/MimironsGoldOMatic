using System.Security.Claims;
using MimironsGoldOMatic.Shared;
using MimironsGoldOMatic.Backend.Api.Controllers;
using MimironsGoldOMatic.Backend.Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>JWT-scoped roulette API: delegates to MediatR; asserts HTTP mapping.</summary>
[Trait("Category", "Unit")]
public sealed class RouletteControllerTests
{
    [Fact]
    public async Task Should_ReturnOk_WhenGetStateSucceeds()
    {
        var next = DateTime.UtcNow.AddMinutes(5);
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetRouletteStateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<RouletteStateResponse>(true,
                new RouletteStateResponse(next, DateTime.UtcNow, 300, 0, "idle", null), 200, null));

        var sut = new RouletteController(mediator.Object);
        var result = await sut.GetState(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RouletteStateResponse>(ok.Value);
        Assert.Equal("idle", dto.SpinPhase);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenPoolMeMissingTwitchUserId()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var sut = new RouletteController(mediator.Object);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await sut.GetPoolMe(CancellationToken.None);
        Assert.IsType<UnauthorizedResult>(result);
        mediator.Verify(m => m.Send(It.IsAny<GetPoolMeQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnOk_WhenPoolMeWithUserId()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.Is<GetPoolMeQuery>(q => q.TwitchUserId == "u1"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<PoolMeResponse>(true, new PoolMeResponse(true, "Abcd"), 200, null));

        var sut = new RouletteController(mediator.Object);
        var ctx = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", "u1")])) };
        sut.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await sut.GetPoolMe(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<PoolMeResponse>(ok.Value);
        Assert.True(body.IsEnrolled);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenMyLastPayoutMissing()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetMyLastPayoutQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<PayoutDto?>(true, null, 404, null));

        var sut = new RouletteController(mediator.Object);
        var ctx = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", "u1")])) };
        sut.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await sut.GetMyLast(CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Should_ReturnCreated_WhenClaimReturns201()
    {
        var mediator = new Mock<IMediator>();
        var resp = new PoolEnrollmentResponse("Abcd", "e1");
        mediator
            .Setup(m => m.Send(It.IsAny<PostClaimCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<PoolEnrollmentResponse>(true, resp, 201, null));

        var sut = new RouletteController(mediator.Object);
        var ctx = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", "u1"), new Claim(ClaimTypes.Name, "Disp")])),
        };
        sut.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await sut.Claim(new CreatePayoutRequest("Abcd", "e1"), CancellationToken.None);
        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenClaimMediatorReturns409()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<PostClaimCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<PoolEnrollmentResponse>(false, null, 409,
                new ApiErrorDto("active_payout_exists", "x", new { })));

        var sut = new RouletteController(mediator.Object);
        var ctx = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user_id", "u1")])),
        };
        sut.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await sut.Claim(new CreatePayoutRequest("Abcd", "e1"), CancellationToken.None);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, obj.StatusCode);
    }
}
