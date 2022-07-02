using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using InteropPdfOptions = PuppeteerSharp.PdfOptions;
using PaperFormat = PuppeteerSharp.Media.PaperFormat;

namespace Xylab.BricksService.Puppeteer
{
    public class PdfOptions
    {
        [JsonProperty("fullPage")]
        public bool FullPage { get; set; }

        [JsonProperty("scale")]
        public decimal Scale { get; set; } = 1;

        [JsonProperty("displayHeaderFooter")]
        public bool DisplayHeaderFooter { get; set; }

        [JsonProperty("footerTemplate")]
        public string FooterTemplate { get; set; }

        [JsonProperty("headerTemplate")]
        public string HeaderTemplate { get; set; }

        [JsonProperty("landscape")]
        public bool Landscape { get; set; }

        [JsonProperty("pageRanges")]
        public string PageRanges { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; } = "A4";

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("margin")]
        public MarginOptions Margin { get; set; }

        [JsonProperty("printBackground")]
        public bool PrintBackground { get; set; } = true;

        [JsonProperty("omitBackground")]
        public bool OmitBackground { get; set; } = true;

        public static implicit operator InteropPdfOptions(PdfOptions pdfOptions)
        {
            PaperFormat format;
            if (pdfOptions.Format == null)
            {
                format = null;
            }
            else if (KnownPaperFormat.ContainsKey(pdfOptions.Format))
            {
                format = KnownPaperFormat[pdfOptions.Format];
            }
            else if (pdfOptions.Format.Split('x') is { Length: 2 } splitted
                && decimal.TryParse(splitted[0], out decimal width)
                && decimal.TryParse(splitted[1], out decimal height))
            {
                format = new PaperFormat(width, height);
            }
            else
            {
                throw new InvalidOperationException("Unexpected format");
            }

            return new InteropPdfOptions
            {
                DisplayHeaderFooter = pdfOptions.DisplayHeaderFooter,
                FooterTemplate = pdfOptions.FooterTemplate,
                HeaderTemplate = pdfOptions.HeaderTemplate,
                Landscape = pdfOptions.Landscape,
                PageRanges = pdfOptions.PageRanges,
                PrintBackground = pdfOptions.PrintBackground,
                Width = pdfOptions.Width,
                Height = pdfOptions.Height,
                Scale = pdfOptions.Scale,
                OmitBackground = pdfOptions.OmitBackground,
                Format = format,
                MarginOptions = pdfOptions.Margin,
            };
        }

        private static readonly IReadOnlyDictionary<string, PaperFormat> KnownPaperFormat
            = new Dictionary<string, PaperFormat>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(PaperFormat.A0)] = PaperFormat.A0,
                [nameof(PaperFormat.A1)] = PaperFormat.A1,
                [nameof(PaperFormat.A2)] = PaperFormat.A2,
                [nameof(PaperFormat.A3)] = PaperFormat.A3,
                [nameof(PaperFormat.A4)] = PaperFormat.A4,
                [nameof(PaperFormat.A5)] = PaperFormat.A5,
                [nameof(PaperFormat.A6)] = PaperFormat.A6,
                [nameof(PaperFormat.Ledger)] = PaperFormat.Ledger,
                [nameof(PaperFormat.Legal)] = PaperFormat.Legal,
                [nameof(PaperFormat.Letter)] = PaperFormat.Letter,
                [nameof(PaperFormat.Tabloid)] = PaperFormat.Tabloid,
            };
    }
}
