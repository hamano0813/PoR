using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Core.Values;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class StatusTabViewModel
{
    private readonly GameSnapshot _snapshot;
    private readonly SlotInfo _slot;
    private readonly GameSession _session;
    private readonly LocalizationService _localization;
    private readonly AssetService _assets;

    public StatusTabViewModel(
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

        Person = Enum("person");
        Class = Enum("class");
        Model = Enum("model");
        Faction = Enum("faction");
        Army = Enum("army");
        Ally = Enum("ally");
        IsLeader = Bool("isLeader");
        Action = new TriStateLockFieldViewModel(
            session,
            Field("action"),
            slot.Index,
            localization,
            Value("action"));
        Rescuing = Bool("rescuing");
        Rescued = Bool("rescued");
        SlotIndex = Num("slotIndex");
        Level = Num("level");
        Exp = Num("exp");
        Hp = Num("hp");
        Transform = Num("transform");
        Paralysis = Num("paralysis");
        Silence = Num("silence");
        Sleep = Num("sleep");
        Berserk = Num("berserk");
        Poison = Num("poison");
        CoordX = Num("coordX");
        CoordY = Num("coordY");
        MType = Enum("mtype");
        Seq1 = Enum("seq1");
        Seq2 = Enum("seq2");
        Seq3 = Enum("seq3");
        Battle = Num("battle");
        Victory = Num("victory");

        var skill = new SkillTabViewModel(snapshot, slot, session, localization, assets);
        Skill = skill;
        Ability = new AbilityTabViewModel(
            snapshot,
            slot,
            session,
            localization,
            assets,
            Class,
            skill);
        Items = new ItemTabViewModel(
            snapshot,
            slot,
            session,
            localization,
            assets,
            Ability,
            skill);
        Support = new SupportTabViewModel(snapshot, slot, session, localization, assets);
    }

    public EnumFieldViewModel Person { get; }

    public EnumFieldViewModel Class { get; }

    public EnumFieldViewModel Model { get; }

    public EnumFieldViewModel Faction { get; }

    public EnumFieldViewModel Army { get; }

    public EnumFieldViewModel Ally { get; }

    public BoolFieldViewModel IsLeader { get; }

    public TriStateLockFieldViewModel Action { get; }

    public BoolFieldViewModel Rescuing { get; }

    public BoolFieldViewModel Rescued { get; }

    public NumericFieldViewModel SlotIndex { get; }

    public NumericFieldViewModel Level { get; }

    public NumericFieldViewModel Exp { get; }

    public NumericFieldViewModel Hp { get; }

    public NumericFieldViewModel Transform { get; }

    public NumericFieldViewModel Paralysis { get; }

    public NumericFieldViewModel Silence { get; }

    public NumericFieldViewModel Sleep { get; }

    public NumericFieldViewModel Berserk { get; }

    public NumericFieldViewModel Poison { get; }

    public NumericFieldViewModel CoordX { get; }

    public NumericFieldViewModel CoordY { get; }

    public EnumFieldViewModel MType { get; }

    public EnumFieldViewModel Seq1 { get; }

    public EnumFieldViewModel Seq2 { get; }

    public EnumFieldViewModel Seq3 { get; }

    public NumericFieldViewModel Battle { get; }

    public NumericFieldViewModel Victory { get; }

    public AbilityTabViewModel Ability { get; }

    public ItemTabViewModel Items { get; }

    public SkillTabViewModel Skill { get; }

    public SupportTabViewModel Support { get; }

    private FieldDefinition Field(string id) => _session.Catalog.GetField(id);

    private long Value(string id)
        => _snapshot.Slots.ReadField(Field(id), _slot.Index);

    private IReadOnlyList<MappingItem> Mapping(FieldDefinition field)
        => field.EnumId is null
            ? Array.Empty<MappingItem>()
            : _snapshot.Mappings.GetValueOrDefault(field.EnumId)
              ?? Array.Empty<MappingItem>();

    private EnumFieldViewModel Enum(string id)
    {
        var field = Field(id);
        return new EnumFieldViewModel(
            _session,
            field,
            _slot.Index,
            _localization.Ui($"Field_{id}"),
            Mapping(field),
            _localization,
            _assets,
            Value(id));
    }

    private BoolFieldViewModel Bool(string id)
        => new(
            _session,
            Field(id),
            _slot.Index,
            _localization.Ui($"Field_{id}"),
            Value(id));

    private NumericFieldViewModel Num(string id)
        => new(
            _session,
            Field(id),
            _slot.Index,
            _localization.Ui($"Field_{id}"),
            Value(id));
}
