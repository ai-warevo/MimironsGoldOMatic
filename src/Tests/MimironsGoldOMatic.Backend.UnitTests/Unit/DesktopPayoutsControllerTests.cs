using MimironsGoldOMatic.Shared;
using MimironsGoldOMatic.Backend.Api.Controllers;
using MimironsGoldOMatic.Backend.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Desktop payouts API (pending list, patch status, confirm acceptance).</summary>
[Trait("Category", "Unit")]
public sealed class DesktopPayoutsControllerTests
{
    [Fact]
    public async Task Should_ReturnOk_WhenPendingSucceeds()
    {
        var mediator = new Mock<IMediator>();
        IReadOnlyList<PayoutDto> list = Array.Empty<PayoutDto>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetPendingPayoutsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<IReadOnlyList<PayoutDto>>(true, list, 200, null));

        var sut = new DesktopPayoutsController(mediator.Object);
        var result = await sut.Pending(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Should_ReturnOk_WhenPatchStatusSucceeds()
    {
        var id = Guid.NewGuid();
        var dto = new PayoutDto(id, "u", "d", "Abcd", 1000, "e", PayoutStatus.Sent, DateTime.UtcNow, false);
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.Is<PatchPayoutStatusCommand>(c => c.Id == id && c.NewStatus == PayoutStatus.Sent),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<PayoutDto>(true, dto, 200, null));

        var sut = new DesktopPayoutsController(mediator.Object);
        var result = await sut.PatchStatus(id, new PatchPayoutStatusRequest(PayoutStatus.Sent), CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(dto, ok.Value);
    }

    [Fact]
    public async Task Should_ReturnStatusCode_WhenConfirmAcceptanceFails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ConfirmAcceptanceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlerResult<global::MediatR.Unit>(false, default, 400, new ApiErrorDto("invalid_character_name", "x", new { })));

        var sut = new DesktopPayoutsController(mediator.Object);
        var result = await sut.ConfirmAcceptance(Guid.NewGuid(), new ConfirmAcceptanceRequest("Wrong"), CancellationToken.None);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
    }
}
