using PoR.App.Services;
using PoR.App.ViewModels.Fields;
using PoR.Core.Game;
using PoR.Game;

namespace PoR.App.ViewModels;

public sealed class OtherTabViewModel
{
    public OtherTabViewModel(
        GameSnapshot snapshot,
        GameSession session,
        LocalizationService localization)
    {
        var money = session.Catalog.GetField("money");
        var bonusExp = session.Catalog.GetField("bonusExp");

        Money = new NumericFieldViewModel(
            session,
            money,
            null,
            localization.Ui("Ui_Other_Money"),
            snapshot.GlobalValues.GetValueOrDefault("money"));

        BonusExp = new NumericFieldViewModel(
            session,
            bonusExp,
            null,
            localization.Ui("Ui_Other_BonusExp"),
            snapshot.GlobalValues.GetValueOrDefault("bonusExp"));

        WarningText = localization.Ui("Ui_Other_Warning");
        InfoText = localization.Ui("Ui_Other_Info");
    }

    public NumericFieldViewModel Money { get; }

    public NumericFieldViewModel BonusExp { get; }

    public string WarningText { get; }

    public string InfoText { get; }
}
