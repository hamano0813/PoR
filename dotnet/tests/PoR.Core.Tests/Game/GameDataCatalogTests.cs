using PoR.Core.Values;
using PoR.Game;

namespace PoR.Core.Tests.Game;

public sealed class GameDataCatalogTests
{
    [Fact]
    public void LoadsEmbeddedCatalog()
    {
        var catalog = GameDataCatalog.Default;

        Assert.Equal(201, catalog.Fields.Count);
        Assert.Equal(159, catalog.Profile.SlotCount);
        Assert.Equal(0x280, catalog.Profile.SlotStep);
        Assert.Equal(0x802AF5E4u, catalog.Profile.SlotBase);

        Assert.Equal(340, catalog.Enums["pid"].Entries.Count);
        Assert.Equal(189, catalog.Enums["iid"].Entries.Count);
        Assert.Equal(97, catalog.Enums["sid"].Entries.Count);

        Assert.Equal(41, catalog.Supports.Rows.Count);
        Assert.Equal(46, catalog.Affinities.Entries.Count);
        Assert.Equal(8, catalog.Groups.Count);
        Assert.Equal(30, catalog.Armies.Count);

        Assert.Equal(32, catalog.Skills.Skills.Count);
        Assert.Equal(14, catalog.Skills.Masteries.Count);
        Assert.Equal(51, catalog.Skills.Features.Count);
    }

    [Fact]
    public void CharacterFieldsFitInsideOneSlot()
    {
        var catalog = GameDataCatalog.Default;

        foreach (var field in catalog.Fields.Values)
        {
            if (field.Scope != FieldScope.Character)
            {
                continue;
            }

            var offset = field.Address - catalog.Profile.SlotBase;
            Assert.InRange(offset, 0u, (uint)(catalog.Profile.SlotStep - field.ByteLength));
        }
    }

    [Fact]
    public void EnumFieldsReferenceKnownEnums()
    {
        var catalog = GameDataCatalog.Default;
        var allowed = catalog.Enums.Keys
            .Concat(["group", "army", "ally"])
            .ToHashSet(StringComparer.Ordinal);

        foreach (var field in catalog.Fields.Values.Where(field => field.Kind == FieldKind.Enum))
        {
            Assert.NotNull(field.EnumId);
            Assert.Contains(field.EnumId, allowed);
        }
    }

    [Fact]
    public void KeyFieldsHaveExpectedAddresses()
    {
        var catalog = GameDataCatalog.Default;

        Assert.Equal(0x802AF76Cu, catalog.GetField("person").Address);
        Assert.Equal(0x802AF770u, catalog.GetField("class").Address);
        Assert.Equal(0x802AF7B0u, catalog.GetField("item1").Address);
        Assert.Equal(0x8032E7A1u, catalog.GetField("money").Address);
        Assert.Equal(0x802AF787u, catalog.GetField("action").Address);
        Assert.Equal(FieldKind.TriState, catalog.GetField("action").Kind);
    }
}
