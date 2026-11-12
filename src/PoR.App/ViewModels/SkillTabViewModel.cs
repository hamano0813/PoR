using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class SkillTabViewModel
{
    private readonly GameSnapshot _snapshot;
    private readonly SlotInfo _slot;
    private readonly GameSession _session;
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;

    public SkillTabViewModel(
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

        var skills = _session.Catalog.Skills;
        ActiveSkills = skills.Skills
            .Concat(skills.Masteries)
            .Select(Create)
            .ToList();
        Features = skills.Features.Select(Create).ToList();
    }

    public IReadOnlyList<BoolFieldViewModel> ActiveSkills { get; }

    public IReadOnlyList<BoolFieldViewModel> Features { get; }

    private BoolFieldViewModel Create(string id)
    {
        var field = _session.Catalog.GetField(id);
        var label = _localization.Term(field.Id);
        if (string.Equals(label, field.Id, StringComparison.Ordinal) &&
            id.StartsWith("SID_", StringComparison.Ordinal))
        {
            label = id[4..];
        }

        return new BoolFieldViewModel(
            _session,
            field,
            _slot.Index,
            label,
            _snapshot.Slots.ReadField(field, _slot.Index),
            _assets.GetSkillIcon(id),
            32);
    }
}
