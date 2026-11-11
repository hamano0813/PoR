using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class ItemTabViewModel
{
    private readonly GameSnapshot _snapshot;
    private readonly SlotInfo _slot;
    private readonly GameSession _session;
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;

    public ItemTabViewModel(
        GameSnapshot snapshot,
        SlotInfo slot,
        GameSession session,
        LocalizationService localization,
        AssetService assets)
    {
        _snapshot = snapshot;
        _slot = slot;
        _session = session;
        _localization = localization;
        _assets = assets;

        var itemMapping = BuildItemMapping();
        var itemLabel = _localization.Ui("Ui_Item_Item");
        var equippedLabel = _localization.Ui("Ui_Item_Equipped");
        var droppedLabel = _localization.Ui("Ui_Item_Dropped");
        var rows = new List<ItemRowViewModel>(8);

        for (var index = 1; index <= 8; index++)
        {
            var item = new EnumFieldViewModel(
                session,
                Field($"item{index}"),
                slot.Index,
                itemLabel,
                itemMapping,
                _localization,
                _assets,
                Value($"item{index}"));

            var durability = new NumericFieldViewModel(
                session,
                Field($"durability{index}"),
                slot.Index,
                _localization.Ui("Ui_Item_Durability"),
                Value($"durability{index}"));

            var equipped = new BoolFieldViewModel(
                session,
                Field($"equipped{index}"),
                slot.Index,
                equippedLabel,
                Value($"equipped{index}"));

            var dropped = new BoolFieldViewModel(
                session,
                Field($"dropped{index}"),
                slot.Index,
                droppedLabel,
                Value($"dropped{index}"));

            rows.Add(new ItemRowViewModel(index, item, durability, equipped, dropped));
        }

        Rows = rows;
    }

    public IReadOnlyList<ItemRowViewModel> Rows { get; }

    private FieldDefinition Field(string id) => _session.Catalog.GetField(id);

    private long Value(string id)
        => _snapshot.Slots.ReadField(Field(id), _slot.Index);

    private IReadOnlyList<MappingItem> BuildItemMapping()
    {
        var mapping = new List<MappingItem>
        {
            new(0, "IID_NONE", "——"),
        };

        if (_snapshot.Mappings.TryGetValue("iid", out var items))
        {
            mapping.AddRange(items.Where(item => item.Value != 0));
        }

        return mapping;
    }
}

public sealed class ItemRowViewModel(
    int index,
    EnumFieldViewModel item,
    NumericFieldViewModel durability,
    BoolFieldViewModel equipped,
    BoolFieldViewModel dropped)
{
    public int Index { get; } = index;

    public EnumFieldViewModel Item { get; } = item;

    public NumericFieldViewModel Durability { get; } = durability;

    public BoolFieldViewModel Equipped { get; } = equipped;

    public BoolFieldViewModel Dropped { get; } = dropped;
}
