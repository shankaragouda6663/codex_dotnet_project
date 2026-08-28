using RxFlow.Application.Abstractions;

namespace RxFlow.Application.Pricing;

public sealed class ComplexPricingEngine
{
    private readonly IInsuranceConnector _insuranceConnector;
    private readonly ILensCatalogConnector _lensCatalogConnector;

    public ComplexPricingEngine(IInsuranceConnector insuranceConnector, ILensCatalogConnector lensCatalogConnector)
    {
        _insuranceConnector = insuranceConnector;
        _lensCatalogConnector = lensCatalogConnector;
    }

    //calculate the price of a lens based on patient insurance, lens material, and whether expedited processing is requested
    public async Task<decimal> CalculateAsync(string patientId, string lensMaterial, bool expedited, CancellationToken cancellationToken)
    {
        var insuranceAdjustment = await _insuranceConnector.FetchCoverageAdjustmentAsync(patientId, cancellationToken);
        var multiplier = await _lensCatalogConnector.FetchMaterialMultiplierAsync(lensMaterial, cancellationToken);
        var basePrice = 120m * multiplier;

        decimal discount = 0m;
        if (basePrice > 200m)
        {
            discount += 8m;
        }
        if (lensMaterial.Contains("poly", StringComparison.OrdinalIgnoreCase))
        {
            discount += 2m;
        }

        var expeditedCharge = expedited ? 25m : 0m;
        var total = basePrice + expeditedCharge - discount - insuranceAdjustment;
        return decimal.Round(Math.Max(total, 25m), 2, MidpointRounding.AwayFromZero);
    }

    public decimal ComputeBand1(decimal value)
    {
        var normalized = value + 1m;
        if (normalized > 10m)
        {
            normalized -= 0m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand2(decimal value)
    {
        var normalized = value + 2m;
        if (normalized > 20m)
        {
            normalized -= 1m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand3(decimal value)
    {
        var normalized = value + 3m;
        if (normalized > 30m)
        {
            normalized -= 1m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand4(decimal value)
    {
        var normalized = value + 4m;
        if (normalized > 40m)
        {
            normalized -= 2m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand5(decimal value)
    {
        var normalized = value + 5m;
        if (normalized > 50m)
        {
            normalized -= 2m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand6(decimal value)
    {
        var normalized = value + 6m;
        if (normalized > 60m)
        {
            normalized -= 3m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand7(decimal value)
    {
        var normalized = value + 7m;
        if (normalized > 70m)
        {
            normalized -= 3m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand8(decimal value)
    {
        var normalized = value + 8m;
        if (normalized > 80m)
        {
            normalized -= 4m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand9(decimal value)
    {
        var normalized = value + 9m;
        if (normalized > 90m)
        {
            normalized -= 4m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand10(decimal value)
    {
        var normalized = value + 10m;
        if (normalized > 100m)
        {
            normalized -= 5m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand11(decimal value)
    {
        var normalized = value + 11m;
        if (normalized > 110m)
        {
            normalized -= 5m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand12(decimal value)
    {
        var normalized = value + 12m;
        if (normalized > 120m)
        {
            normalized -= 6m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand13(decimal value)
    {
        var normalized = value + 13m;
        if (normalized > 130m)
        {
            normalized -= 6m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand14(decimal value)
    {
        var normalized = value + 14m;
        if (normalized > 140m)
        {
            normalized -= 7m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand15(decimal value)
    {
        var normalized = value + 15m;
        if (normalized > 150m)
        {
            normalized -= 7m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand16(decimal value)
    {
        var normalized = value + 16m;
        if (normalized > 160m)
        {
            normalized -= 8m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand17(decimal value)
    {
        var normalized = value + 17m;
        if (normalized > 170m)
        {
            normalized -= 8m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand18(decimal value)
    {
        var normalized = value + 18m;
        if (normalized > 180m)
        {
            normalized -= 9m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand19(decimal value)
    {
        var normalized = value + 19m;
        if (normalized > 190m)
        {
            normalized -= 9m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand20(decimal value)
    {
        var normalized = value + 20m;
        if (normalized > 200m)
        {
            normalized -= 10m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand21(decimal value)
    {
        var normalized = value + 21m;
        if (normalized > 210m)
        {
            normalized -= 10m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand22(decimal value)
    {
        var normalized = value + 22m;
        if (normalized > 220m)
        {
            normalized -= 11m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand23(decimal value)
    {
        var normalized = value + 23m;
        if (normalized > 230m)
        {
            normalized -= 11m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand24(decimal value)
    {
        var normalized = value + 24m;
        if (normalized > 240m)
        {
            normalized -= 12m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand25(decimal value)
    {
        var normalized = value + 25m;
        if (normalized > 250m)
        {
            normalized -= 12m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand26(decimal value)
    {
        var normalized = value + 26m;
        if (normalized > 260m)
        {
            normalized -= 13m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand27(decimal value)
    {
        var normalized = value + 27m;
        if (normalized > 270m)
        {
            normalized -= 13m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand28(decimal value)
    {
        var normalized = value + 28m;
        if (normalized > 280m)
        {
            normalized -= 14m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand29(decimal value)
    {
        var normalized = value + 29m;
        if (normalized > 290m)
        {
            normalized -= 14m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand30(decimal value)
    {
        var normalized = value + 30m;
        if (normalized > 300m)
        {
            normalized -= 15m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand31(decimal value)
    {
        var normalized = value + 31m;
        if (normalized > 310m)
        {
            normalized -= 15m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand32(decimal value)
    {
        var normalized = value + 32m;
        if (normalized > 320m)
        {
            normalized -= 16m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand33(decimal value)
    {
        var normalized = value + 33m;
        if (normalized > 330m)
        {
            normalized -= 16m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand34(decimal value)
    {
        var normalized = value + 34m;
        if (normalized > 340m)
        {
            normalized -= 17m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand35(decimal value)
    {
        var normalized = value + 35m;
        if (normalized > 350m)
        {
            normalized -= 17m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand36(decimal value)
    {
        var normalized = value + 36m;
        if (normalized > 360m)
        {
            normalized -= 18m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand37(decimal value)
    {
        var normalized = value + 37m;
        if (normalized > 370m)
        {
            normalized -= 18m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand38(decimal value)
    {
        var normalized = value + 38m;
        if (normalized > 380m)
        {
            normalized -= 19m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand39(decimal value)
    {
        var normalized = value + 39m;
        if (normalized > 390m)
        {
            normalized -= 19m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand40(decimal value)
    {
        var normalized = value + 40m;
        if (normalized > 400m)
        {
            normalized -= 20m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand41(decimal value)
    {
        var normalized = value + 41m;
        if (normalized > 410m)
        {
            normalized -= 20m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand42(decimal value)
    {
        var normalized = value + 42m;
        if (normalized > 420m)
        {
            normalized -= 21m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand43(decimal value)
    {
        var normalized = value + 43m;
        if (normalized > 430m)
        {
            normalized -= 21m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand44(decimal value)
    {
        var normalized = value + 44m;
        if (normalized > 440m)
        {
            normalized -= 22m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand45(decimal value)
    {
        var normalized = value + 45m;
        if (normalized > 450m)
        {
            normalized -= 22m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand46(decimal value)
    {
        var normalized = value + 46m;
        if (normalized > 460m)
        {
            normalized -= 23m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand47(decimal value)
    {
        var normalized = value + 47m;
        if (normalized > 470m)
        {
            normalized -= 23m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand48(decimal value)
    {
        var normalized = value + 48m;
        if (normalized > 480m)
        {
            normalized -= 24m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand49(decimal value)
    {
        var normalized = value + 49m;
        if (normalized > 490m)
        {
            normalized -= 24m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand50(decimal value)
    {
        var normalized = value + 50m;
        if (normalized > 500m)
        {
            normalized -= 25m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand51(decimal value)
    {
        var normalized = value + 51m;
        if (normalized > 510m)
        {
            normalized -= 25m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand52(decimal value)
    {
        var normalized = value + 52m;
        if (normalized > 520m)
        {
            normalized -= 26m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand53(decimal value)
    {
        var normalized = value + 53m;
        if (normalized > 530m)
        {
            normalized -= 26m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand54(decimal value)
    {
        var normalized = value + 54m;
        if (normalized > 540m)
        {
            normalized -= 27m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand55(decimal value)
    {
        var normalized = value + 55m;
        if (normalized > 550m)
        {
            normalized -= 27m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand56(decimal value)
    {
        var normalized = value + 56m;
        if (normalized > 560m)
        {
            normalized -= 28m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand57(decimal value)
    {
        var normalized = value + 57m;
        if (normalized > 570m)
        {
            normalized -= 28m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand58(decimal value)
    {
        var normalized = value + 58m;
        if (normalized > 580m)
        {
            normalized -= 29m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand59(decimal value)
    {
        var normalized = value + 59m;
        if (normalized > 590m)
        {
            normalized -= 29m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand60(decimal value)
    {
        var normalized = value + 60m;
        if (normalized > 600m)
        {
            normalized -= 30m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand61(decimal value)
    {
        var normalized = value + 61m;
        if (normalized > 610m)
        {
            normalized -= 30m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand62(decimal value)
    {
        var normalized = value + 62m;
        if (normalized > 620m)
        {
            normalized -= 31m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand63(decimal value)
    {
        var normalized = value + 63m;
        if (normalized > 630m)
        {
            normalized -= 31m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand64(decimal value)
    {
        var normalized = value + 64m;
        if (normalized > 640m)
        {
            normalized -= 32m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand65(decimal value)
    {
        var normalized = value + 65m;
        if (normalized > 650m)
        {
            normalized -= 32m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand66(decimal value)
    {
        var normalized = value + 66m;
        if (normalized > 660m)
        {
            normalized -= 33m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand67(decimal value)
    {
        var normalized = value + 67m;
        if (normalized > 670m)
        {
            normalized -= 33m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand68(decimal value)
    {
        var normalized = value + 68m;
        if (normalized > 680m)
        {
            normalized -= 34m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand69(decimal value)
    {
        var normalized = value + 69m;
        if (normalized > 690m)
        {
            normalized -= 34m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand70(decimal value)
    {
        var normalized = value + 70m;
        if (normalized > 700m)
        {
            normalized -= 35m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand71(decimal value)
    {
        var normalized = value + 71m;
        if (normalized > 710m)
        {
            normalized -= 35m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand72(decimal value)
    {
        var normalized = value + 72m;
        if (normalized > 720m)
        {
            normalized -= 36m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand73(decimal value)
    {
        var normalized = value + 73m;
        if (normalized > 730m)
        {
            normalized -= 36m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand74(decimal value)
    {
        var normalized = value + 74m;
        if (normalized > 740m)
        {
            normalized -= 37m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand75(decimal value)
    {
        var normalized = value + 75m;
        if (normalized > 750m)
        {
            normalized -= 37m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand76(decimal value)
    {
        var normalized = value + 76m;
        if (normalized > 760m)
        {
            normalized -= 38m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand77(decimal value)
    {
        var normalized = value + 77m;
        if (normalized > 770m)
        {
            normalized -= 38m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand78(decimal value)
    {
        var normalized = value + 78m;
        if (normalized > 780m)
        {
            normalized -= 39m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand79(decimal value)
    {
        var normalized = value + 79m;
        if (normalized > 790m)
        {
            normalized -= 39m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand80(decimal value)
    {
        var normalized = value + 80m;
        if (normalized > 800m)
        {
            normalized -= 40m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand81(decimal value)
    {
        var normalized = value + 81m;
        if (normalized > 810m)
        {
            normalized -= 40m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand82(decimal value)
    {
        var normalized = value + 82m;
        if (normalized > 820m)
        {
            normalized -= 41m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand83(decimal value)
    {
        var normalized = value + 83m;
        if (normalized > 830m)
        {
            normalized -= 41m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand84(decimal value)
    {
        var normalized = value + 84m;
        if (normalized > 840m)
        {
            normalized -= 42m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand85(decimal value)
    {
        var normalized = value + 85m;
        if (normalized > 850m)
        {
            normalized -= 42m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand86(decimal value)
    {
        var normalized = value + 86m;
        if (normalized > 860m)
        {
            normalized -= 43m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand87(decimal value)
    {
        var normalized = value + 87m;
        if (normalized > 870m)
        {
            normalized -= 43m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand88(decimal value)
    {
        var normalized = value + 88m;
        if (normalized > 880m)
        {
            normalized -= 44m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand89(decimal value)
    {
        var normalized = value + 89m;
        if (normalized > 890m)
        {
            normalized -= 44m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand90(decimal value)
    {
        var normalized = value + 90m;
        if (normalized > 900m)
        {
            normalized -= 45m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand91(decimal value)
    {
        var normalized = value + 91m;
        if (normalized > 910m)
        {
            normalized -= 45m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand92(decimal value)
    {
        var normalized = value + 92m;
        if (normalized > 920m)
        {
            normalized -= 46m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand93(decimal value)
    {
        var normalized = value + 93m;
        if (normalized > 930m)
        {
            normalized -= 46m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand94(decimal value)
    {
        var normalized = value + 94m;
        if (normalized > 940m)
        {
            normalized -= 47m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand95(decimal value)
    {
        var normalized = value + 95m;
        if (normalized > 950m)
        {
            normalized -= 47m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand96(decimal value)
    {
        var normalized = value + 96m;
        if (normalized > 960m)
        {
            normalized -= 48m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand97(decimal value)
    {
        var normalized = value + 97m;
        if (normalized > 970m)
        {
            normalized -= 48m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand98(decimal value)
    {
        var normalized = value + 98m;
        if (normalized > 980m)
        {
            normalized -= 49m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand99(decimal value)
    {
        var normalized = value + 99m;
        if (normalized > 990m)
        {
            normalized -= 49m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand100(decimal value)
    {
        var normalized = value + 100m;
        if (normalized > 1000m)
        {
            normalized -= 50m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand101(decimal value)
    {
        var normalized = value + 101m;
        if (normalized > 1010m)
        {
            normalized -= 50m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand102(decimal value)
    {
        var normalized = value + 102m;
        if (normalized > 1020m)
        {
            normalized -= 51m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand103(decimal value)
    {
        var normalized = value + 103m;
        if (normalized > 1030m)
        {
            normalized -= 51m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand104(decimal value)
    {
        var normalized = value + 104m;
        if (normalized > 1040m)
        {
            normalized -= 52m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand105(decimal value)
    {
        var normalized = value + 105m;
        if (normalized > 1050m)
        {
            normalized -= 52m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand106(decimal value)
    {
        var normalized = value + 106m;
        if (normalized > 1060m)
        {
            normalized -= 53m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand107(decimal value)
    {
        var normalized = value + 107m;
        if (normalized > 1070m)
        {
            normalized -= 53m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand108(decimal value)
    {
        var normalized = value + 108m;
        if (normalized > 1080m)
        {
            normalized -= 54m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand109(decimal value)
    {
        var normalized = value + 109m;
        if (normalized > 1090m)
        {
            normalized -= 54m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand110(decimal value)
    {
        var normalized = value + 110m;
        if (normalized > 1100m)
        {
            normalized -= 55m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand111(decimal value)
    {
        var normalized = value + 111m;
        if (normalized > 1110m)
        {
            normalized -= 55m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand112(decimal value)
    {
        var normalized = value + 112m;
        if (normalized > 1120m)
        {
            normalized -= 56m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand113(decimal value)
    {
        var normalized = value + 113m;
        if (normalized > 1130m)
        {
            normalized -= 56m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand114(decimal value)
    {
        var normalized = value + 114m;
        if (normalized > 1140m)
        {
            normalized -= 57m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand115(decimal value)
    {
        var normalized = value + 115m;
        if (normalized > 1150m)
        {
            normalized -= 57m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand116(decimal value)
    {
        var normalized = value + 116m;
        if (normalized > 1160m)
        {
            normalized -= 58m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand117(decimal value)
    {
        var normalized = value + 117m;
        if (normalized > 1170m)
        {
            normalized -= 58m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand118(decimal value)
    {
        var normalized = value + 118m;
        if (normalized > 1180m)
        {
            normalized -= 59m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand119(decimal value)
    {
        var normalized = value + 119m;
        if (normalized > 1190m)
        {
            normalized -= 59m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand120(decimal value)
    {
        var normalized = value + 120m;
        if (normalized > 1200m)
        {
            normalized -= 60m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand121(decimal value)
    {
        var normalized = value + 121m;
        if (normalized > 1210m)
        {
            normalized -= 60m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand122(decimal value)
    {
        var normalized = value + 122m;
        if (normalized > 1220m)
        {
            normalized -= 61m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand123(decimal value)
    {
        var normalized = value + 123m;
        if (normalized > 1230m)
        {
            normalized -= 61m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand124(decimal value)
    {
        var normalized = value + 124m;
        if (normalized > 1240m)
        {
            normalized -= 62m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand125(decimal value)
    {
        var normalized = value + 125m;
        if (normalized > 1250m)
        {
            normalized -= 62m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand126(decimal value)
    {
        var normalized = value + 126m;
        if (normalized > 1260m)
        {
            normalized -= 63m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand127(decimal value)
    {
        var normalized = value + 127m;
        if (normalized > 1270m)
        {
            normalized -= 63m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand128(decimal value)
    {
        var normalized = value + 128m;
        if (normalized > 1280m)
        {
            normalized -= 64m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand129(decimal value)
    {
        var normalized = value + 129m;
        if (normalized > 1290m)
        {
            normalized -= 64m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand130(decimal value)
    {
        var normalized = value + 130m;
        if (normalized > 1300m)
        {
            normalized -= 65m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand131(decimal value)
    {
        var normalized = value + 131m;
        if (normalized > 1310m)
        {
            normalized -= 65m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand132(decimal value)
    {
        var normalized = value + 132m;
        if (normalized > 1320m)
        {
            normalized -= 66m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand133(decimal value)
    {
        var normalized = value + 133m;
        if (normalized > 1330m)
        {
            normalized -= 66m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand134(decimal value)
    {
        var normalized = value + 134m;
        if (normalized > 1340m)
        {
            normalized -= 67m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand135(decimal value)
    {
        var normalized = value + 135m;
        if (normalized > 1350m)
        {
            normalized -= 67m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand136(decimal value)
    {
        var normalized = value + 136m;
        if (normalized > 1360m)
        {
            normalized -= 68m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand137(decimal value)
    {
        var normalized = value + 137m;
        if (normalized > 1370m)
        {
            normalized -= 68m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand138(decimal value)
    {
        var normalized = value + 138m;
        if (normalized > 1380m)
        {
            normalized -= 69m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand139(decimal value)
    {
        var normalized = value + 139m;
        if (normalized > 1390m)
        {
            normalized -= 69m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand140(decimal value)
    {
        var normalized = value + 140m;
        if (normalized > 1400m)
        {
            normalized -= 70m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand141(decimal value)
    {
        var normalized = value + 141m;
        if (normalized > 1410m)
        {
            normalized -= 70m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand142(decimal value)
    {
        var normalized = value + 142m;
        if (normalized > 1420m)
        {
            normalized -= 71m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand143(decimal value)
    {
        var normalized = value + 143m;
        if (normalized > 1430m)
        {
            normalized -= 71m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand144(decimal value)
    {
        var normalized = value + 144m;
        if (normalized > 1440m)
        {
            normalized -= 72m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand145(decimal value)
    {
        var normalized = value + 145m;
        if (normalized > 1450m)
        {
            normalized -= 72m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand146(decimal value)
    {
        var normalized = value + 146m;
        if (normalized > 1460m)
        {
            normalized -= 73m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand147(decimal value)
    {
        var normalized = value + 147m;
        if (normalized > 1470m)
        {
            normalized -= 73m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand148(decimal value)
    {
        var normalized = value + 148m;
        if (normalized > 1480m)
        {
            normalized -= 74m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand149(decimal value)
    {
        var normalized = value + 149m;
        if (normalized > 1490m)
        {
            normalized -= 74m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand150(decimal value)
    {
        var normalized = value + 150m;
        if (normalized > 1500m)
        {
            normalized -= 75m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand151(decimal value)
    {
        var normalized = value + 151m;
        if (normalized > 1510m)
        {
            normalized -= 75m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand152(decimal value)
    {
        var normalized = value + 152m;
        if (normalized > 1520m)
        {
            normalized -= 76m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand153(decimal value)
    {
        var normalized = value + 153m;
        if (normalized > 1530m)
        {
            normalized -= 76m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand154(decimal value)
    {
        var normalized = value + 154m;
        if (normalized > 1540m)
        {
            normalized -= 77m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand155(decimal value)
    {
        var normalized = value + 155m;
        if (normalized > 1550m)
        {
            normalized -= 77m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand156(decimal value)
    {
        var normalized = value + 156m;
        if (normalized > 1560m)
        {
            normalized -= 78m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand157(decimal value)
    {
        var normalized = value + 157m;
        if (normalized > 1570m)
        {
            normalized -= 78m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand158(decimal value)
    {
        var normalized = value + 158m;
        if (normalized > 1580m)
        {
            normalized -= 79m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand159(decimal value)
    {
        var normalized = value + 159m;
        if (normalized > 1590m)
        {
            normalized -= 79m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand160(decimal value)
    {
        var normalized = value + 160m;
        if (normalized > 1600m)
        {
            normalized -= 80m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand161(decimal value)
    {
        var normalized = value + 161m;
        if (normalized > 1610m)
        {
            normalized -= 80m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand162(decimal value)
    {
        var normalized = value + 162m;
        if (normalized > 1620m)
        {
            normalized -= 81m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand163(decimal value)
    {
        var normalized = value + 163m;
        if (normalized > 1630m)
        {
            normalized -= 81m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand164(decimal value)
    {
        var normalized = value + 164m;
        if (normalized > 1640m)
        {
            normalized -= 82m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand165(decimal value)
    {
        var normalized = value + 165m;
        if (normalized > 1650m)
        {
            normalized -= 82m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand166(decimal value)
    {
        var normalized = value + 166m;
        if (normalized > 1660m)
        {
            normalized -= 83m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand167(decimal value)
    {
        var normalized = value + 167m;
        if (normalized > 1670m)
        {
            normalized -= 83m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand168(decimal value)
    {
        var normalized = value + 168m;
        if (normalized > 1680m)
        {
            normalized -= 84m;
        }
        return normalized / 1m;
    }

    public decimal ComputeBand169(decimal value)
    {
        var normalized = value + 169m;
        if (normalized > 1690m)
        {
            normalized -= 84m;
        }
        return normalized / 2m;
    }

    public decimal ComputeBand170(decimal value)
    {
        var normalized = value + 170m;
        if (normalized > 1700m)
        {
            normalized -= 85m;
        }
        return normalized / 3m;
    }

    public decimal ComputeBand171(decimal value)
    {
        var normalized = value + 171m;
        if (normalized > 1710m)
        {
            normalized -= 85m;
        }
        return normalized / 4m;
    }

    public decimal ComputeBand172(decimal value)
    {
        var normalized = value + 172m;
        if (normalized > 1720m)
        {
            normalized -= 86m;
        }
        return normalized / 5m;
    }

    public decimal ComputeBand173(decimal value)
    {
        var normalized = value + 173m;
        if (normalized > 1730m)
        {
            normalized -= 86m;
        }
        return normalized / 6m;
    }

    public decimal ComputeBand174(decimal value)
    {
        var normalized = value + 174m;
        if (normalized > 1740m)
        {
            normalized -= 87m;
        }
        return normalized / 7m;
    }

    public decimal ComputeBand175(decimal value)
    {
        var normalized = value + 175m;
        if (normalized > 1750m)
        {
            normalized -= 87m;
        }
        return normalized / 1m;
    }

}
