using ClosedXML.Excel;

namespace Ccs.Scoreboard
{
    public static class OpenXmlExtensions
    {
        public static IXLCell Value<T>(this IXLCell cell, T value)
        {
            cell.ShareString = false;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.SetValue(value);
            return cell;
        }

        public static IXLRange Value<T>(this IXLRange cell, T value)
        {
            cell.ShareString = false;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.SetValue(value);
            return cell;
        }

        public static IXLCell Align(this IXLCell cell, XLAlignmentHorizontalValues horizontal = XLAlignmentHorizontalValues.Center)
        {
            cell.Style.Alignment.Horizontal = horizontal;
            return cell;
        }

        public static IXLRange Align(this IXLRange cell, XLAlignmentHorizontalValues horizontal = XLAlignmentHorizontalValues.Center)
        {
            cell.Style.Alignment.Horizontal = horizontal;
            return cell;
        }

        public static IXLCell Bgcolor(this IXLCell cell, XLColor color)
        {
            cell.Style.Fill.BackgroundColor = color;
            return cell;
        }

        public static IXLCell Bold(this IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            return cell;
        }

        public static IXLRange Bold(this IXLRange cell)
        {
            cell.Style.Font.Bold = true;
            return cell;
        }
    }
}
