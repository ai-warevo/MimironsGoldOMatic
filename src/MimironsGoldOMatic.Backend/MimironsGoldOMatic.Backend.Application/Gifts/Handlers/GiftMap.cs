using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Gifts.Handlers;

internal static class GiftMap
{
    public static GiftRequestDto ToDto(GiftRequestReadDocument d, int queuePosition)
    {
        var item = d.SelectedItem == null
            ? null
            : new GiftSelectedItemDto(d.SelectedItem.Name, d.SelectedItem.Id, d.SelectedItem.Count, d.SelectedItem.Link, d.SelectedItem.Texture,
                d.SelectedItem.BagId, d.SelectedItem.SlotId);
        var wait = Math.Max(0, (queuePosition - 1) * 65);
        return new GiftRequestDto(d.Id, d.StreamerId, d.ViewerId, d.ViewerDisplayName, d.CharacterName, d.State, item, queuePosition, wait, d.CreatedAt,
            d.UpdatedAt, d.TimeoutAt, d.FailureReason);
    }
}

