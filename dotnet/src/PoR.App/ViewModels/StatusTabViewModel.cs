using System.Collections.ObjectModel;
using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class StatusTabViewModel
{
    private static readonly string[] StatusFieldIds =
    [
        "person",
        "class",
        "model",
        "faction",
        "army",
        "ally",
        "isLeader",
        "action",
        "rescuing",
        "rescued",
        "slotIndex",
        "level",
        "exp",
        "hp",
        "transform",
        "paralysis",
        "silence",
        "sleep",
        "berserk",
        "poison",
        "coordX",
        "coordY",
        "mtype",
        "seq1",
        "seq2",
        "seq3",
        "battle",
        "victory",
    ];

    private static readonly IReadOnlyDictionary<string, string> LabelOverrides =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mtype"] = "模式",
            ["seq1"] = "攻击",
            ["seq2"] = "目标",
            ["seq3"] = "回复",
        };

    public StatusTabViewModel(
        GameSnapshot snapshot,
        SlotInfo slot,
        GameSession session,
        LocalizationService localization)
    {
        Fields = [];

        foreach (var id in StatusFieldIds)
        {
            var field = session.Catalog.GetField(id);
            var label = LabelOverrides.TryGetValue(id, out var custom)
                ? custom
                : field.LegacyKey;
            var value = snapshot.Slots.ReadField(field, slot.Index);
            IReadOnlyList<MappingItem> mapping = field.EnumId is null
                ? Array.Empty<MappingItem>()
                : snapshot.Mappings.GetValueOrDefault(field.EnumId)
                  ?? Array.Empty<MappingItem>();

            FieldViewModelBase editor = field.Kind switch
            {
                FieldKind.Enum => new EnumFieldViewModel(
                    session,
                    field,
                    slot.Index,
                    label,
                    mapping,
                    localization,
                    value),
                FieldKind.Bool => new BoolFieldViewModel(
                    session,
                    field,
                    slot.Index,
                    label,
                    value),
                FieldKind.TriState => new ChoiceFieldViewModel(
                    session,
                    field,
                    slot.Index,
                    label,
                    [
                        new ChoiceItem(0, "未行动"),
                        new ChoiceItem(1, "行动完毕"),
                    ],
                    value),
                _ => new NumericFieldViewModel(
                    session,
                    field,
                    slot.Index,
                    label,
                    value),
            };

            Fields.Add(editor);
        }
    }

    public ObservableCollection<FieldViewModelBase> Fields { get; }
}
