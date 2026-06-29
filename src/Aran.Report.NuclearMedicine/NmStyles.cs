using Chuvadi.Pdf.Authoring;

namespace Aran.Report.NuclearMedicine;

/// <summary>Shared colour and style constants for the NM shielding report.</summary>
internal static class NmStyles
{
    internal static readonly Color Navy = Color.FromHex("#1B3A6B");
    internal static readonly Color DarkGrey = Color.FromHex("#444444");
    internal static readonly Color LightGrey = Color.FromHex("#F2F2F2");
    internal static readonly Color HeaderBlue = Color.FromHex("#D6E4F0");
    internal static readonly Color PassGreen = Color.FromHex("#E8F5E9");
    internal static readonly Color FailRed = Color.FromHex("#FFEBEE");

    internal static ParagraphStyle Body => new ParagraphStyle
    {
        Font = ReportFont.Helvetica,
        FontSize = 9,
        Color = DarkGrey,
        LineSpacing = 1.3,
        SpaceAfter = 4,
    };

    internal static ParagraphStyle BodySmall => new ParagraphStyle
    {
        Font = ReportFont.Helvetica,
        FontSize = 8,
        Color = DarkGrey,
        LineSpacing = 1.2,
        SpaceAfter = 2,
    };

    internal static ParagraphStyle H1 => new ParagraphStyle
    {
        Font = ReportFont.HelveticaBold,
        FontSize = 14,
        Color = Navy,
        SpaceBefore = 8,
        SpaceAfter = 6,
    };

    internal static ParagraphStyle H2 => new ParagraphStyle
    {
        Font = ReportFont.HelveticaBold,
        FontSize = 11,
        Color = Navy,
        SpaceBefore = 10,
        SpaceAfter = 4,
    };

    internal static ParagraphStyle H3 => new ParagraphStyle
    {
        Font = ReportFont.HelveticaBold,
        FontSize = 9,
        Color = DarkGrey,
        SpaceBefore = 6,
        SpaceAfter = 2,
    };

    internal static ParagraphStyle Note => new ParagraphStyle
    {
        Font = ReportFont.Helvetica,
        FontSize = 7,
        Color = DarkGrey,
        LineSpacing = 1.2,
        SpaceAfter = 2,
    };

    internal static ReportTable DataTable() => new ReportTable
    {
        Style = new TableStyle
        {
            Font = ReportFont.Helvetica,
            FontSize = 8,
            TextColor = DarkGrey,
            ShowHeader = true,
            RepeatHeaderOnEveryPage = true,
            HeaderFont = ReportFont.HelveticaBold,
            HeaderFontSize = 8,
            HeaderTextColor = Navy,
            HeaderBackground = HeaderBlue,
            BorderMode = TableBorderMode.Grid,
            BorderColor = Color.FromHex("#CCCCCC"),
            BorderWidth = 0.5,
            CellPadding = 3,
            LineSpacing = 1.2,
            SpaceAfter = 8,
        },
    };

    internal static ReportTable InputTable() => new ReportTable
    {
        Style = new TableStyle
        {
            Font = ReportFont.Helvetica,
            FontSize = 9,
            TextColor = DarkGrey,
            ShowHeader = false,
            BorderMode = TableBorderMode.HorizontalOnly,
            BorderColor = Color.FromHex("#CCCCCC"),
            BorderWidth = 0.4,
            CellPadding = 4,
            AlternatingRowBackground = LightGrey,
            SpaceAfter = 10,
        },
    };
}
