﻿using System;
using System.Collections.Generic;
using SIL.Tool;

namespace SIL.PublishingSolution
{
    public class InMapProperty
    {
        private Dictionary<string, string> _IDProperty = new Dictionary<string, string>();
        private Dictionary<string, string> _cssProperty = new Dictionary<string, string>();
        private bool _IsKeepLineWrittern = false;
        //TextInfo _titleCase = CultureInfo.CurrentCulture.TextInfo;
        public Dictionary<string, string> IDProperty(Dictionary<string, string> cssProperty)
        {
            _IDProperty.Clear();
            _cssProperty = cssProperty;
            foreach (KeyValuePair<string, string> property in cssProperty)
            {
                switch (property.Key.ToLower())
                {
                    case "font-weight":
                    case "font-style":
                        FontWeight(cssProperty);
                        break;
                    case "text-align":
                        TextAlign(property.Value);
                        break;
                    case "font-size":
                        FontSize(property.Value);
                        break;
                    case "text-decoration":
                        TextDecoration(property.Value);
                        break;
                    case "font-variant":
                        FontVariant(property.Value);
                        break;
                    case "text-indent":
                        TextIndent(property.Value);
                        break;
                    case "margin-left":
                        MarginLeft(property.Value);
                        break;
                    case "margin-right":
                        MarginRight(property.Value);
                        break;
                    case "margin-top":
                        MarginTop(property.Value);
                        break;
                    case "margin-bottom":
                        MarginBottom(property.Value);
                        break;
                    case "font-family":
                        FontFamily(property.Value);
                        break;
                    case "page-width":
                        PageWidth(property.Value);
                        break;
                    case "page-height":
                        PageHeight(property.Value);
                        break;
                    case "mirror":
                        Mirror(property.Value);
                        break;
                    case "padding-left":
                        PaddingLeft(property.Value);
                        break;
                    case "padding-right":
                        PaddingRight(property.Value);
                        break;
                    case "padding-top":
                        PaddingTop(property.Value);
                        break;
                    case "padding-bottom":
                        PaddingBottom(property.Value);
                        break;
                    case "padding":
                    case "margin":
                        //Margin(styleAttributeInfo);
                        break;
                    case "color":
                        Color(property.Value);
                        break;
                    case "background-color":
                        BGColor(property.Value);
                        break;
                    case "size":
                        //Size(styleAttributeInfo);
                        break;
                    case "language":
                        //Language(styleAttributeInfo);
                        break;
                    case "border":
                        //Border(styleAttributeInfo);
                        break;
                    case "column-count":
                        ColumnCount(property.Value);
                        break;
                    case "column-gap":
                        ColumnGap(property.Value);
                        break;
                    case "display":
                         Display(property.Value);
                        break;
                    case "page-break-before":
                        PageBreakBefore(property.Value);
                        break;
                    case "text-transform":
                        TextTransform(property.Value);
                        break;
                    case "vertical-align":
                        VerticalAlign(property.Value);
                        break;
                    case "line-height":
                        LineHeight(property.Value);
                        break;
                    case "hyphens":
                        Hyphens(property.Value);
                        break;
                    case "hyphenate-before":
                        HyphenateBefore(property.Value);
                        break;
                    case "hyphenate-after":
                        HyphenateAfter(property.Value);
                        break;
                    case "hyphenate-lines":
                        HyphenateLines(property.Value);
                        break;
                    case "letter-spacing":
                        LetterSpacing(property.Value);
                        break;
                    case "word-spacing":
                        WordSpacing(property.Value);
                        break;
                    case "orphans":
                        Orphans(property.Value);
                        break;
                    case "widows":
                        Widows(property.Value);
                        break;
                    case "direction":
                        Direction(property.Value);
                        break;
                    case "-ps-vertical-justification":
                        VerticalJustification(property.Value);
                        break;
                    default:
                        SimpleProperty(property);
                        break;
                }
            }
            return _IDProperty;
        }

        public void VerticalJustification(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            string value = propertyValue;
            if (propertyValue == "Top")
            {
                value = "TopAlign";
            }
            else if (propertyValue == "Center")
            {
                value = "CenterAlign";
            }
            else if (propertyValue == "Bottom")
            {
                value = "BottomAlign";
            }
            _IDProperty["VerticalJustification"] = value;
        }

        public void Direction(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            if (propertyValue == "rtl")
            {
                _IDProperty["Composer"] = "HL Composer Optyca";
                _IDProperty["DigitsType"] = "ArabicDigits";
                _IDProperty["CharacterDirection"] = "RightToLeftDirection";
                _IDProperty["ParagraphDirection"] = "RightToLeftDirection";
                _IDProperty["ParagraphJustification"] = "ArabicJustification";
                _IDProperty["Justification"] = "RightAlign";
            }
            //_IDProperty["Composer"] = "HL Composer Optyca";
            //_IDProperty["DigitsType"] = "DefaultDigits";
            //_IDProperty["CharacterDirection"] = "LeftToRightDirection";
            //_IDProperty["ParagraphDirection"] = "LeftToRightDirection";
            //_IDProperty["ParagraphJustification"] = "DefaultJustification";
            //_IDProperty["Justification"] = "LeftAlign";
        }

        private void Widows(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["KeepLastLines"] = propertyValue;
            AddKeepLinesTogetherProperty();
        }

        private void Orphans(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["KeepFirstLines"] = propertyValue;
            AddKeepLinesTogetherProperty();
        }

        private void AddKeepLinesTogetherProperty()
        {
            if (_IsKeepLineWrittern == false)
            {
                _IDProperty["KeepLinesTogether"] = "true";
            }
            _IsKeepLineWrittern = true;
        }

        public void WordSpacing(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["MinimumWordSpacing"] = "0";
            _IDProperty["DesiredWordSpacing"] = propertyValue;
            _IDProperty["MaximumWordSpacing"] = propertyValue;
        }
        public void LetterSpacing(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["MinimumLetterSpacing"] = "0";
            _IDProperty["DesiredLetterSpacing"] = propertyValue;
            _IDProperty["MaximumLetterSpacing"] = propertyValue;
        }


        public void HyphenateLines(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["HyphenateLadderLimit"] = propertyValue;
        }
        public void HyphenateAfter(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["HyphenateAfterFirst"] = propertyValue;
        }
        public void HyphenateBefore(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["HyphenateBeforeLast"] = propertyValue;
        }
        public void Hyphens(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            string value = propertyValue == "none" ? "false" : "true";
            _IDProperty["Hyphenation"] = value;
        }
        public void SimpleProperty(KeyValuePair<string, string> property)
        {
            string value = property.Value;
            switch (property.Key.ToLower())
            {
                case "float":
                case "clear":
                case "white-space":
                case "counter-increment":
                case "counter-reset":
                case "content":
                case "position":
                case "left":
                case "right":
                case "width":
                case "height":
                case "visibility":
                    _IDProperty[property.Key] = value;
                    break;
                default:
                    //throw new Exception("Not a valid CSS Command");
                    break;
            }
        }

        public void LineHeight(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            if (propertyValue == "normal")
                propertyValue = "Auto";

            _IDProperty["Leading"] = propertyValue;
        }
        public void VerticalAlign(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }

            switch (propertyValue.ToLower())
            {
                case "baseline":
                    _IDProperty["Position"] = "Normal";
                    break;
                case "super":
                    _IDProperty["Position"] = "Superscript";
                    break;
                case "sub":
                    _IDProperty["Position"] = "Subscript";
                    break;
                case "text-top":
                case "top":
                    _IDProperty["BaselineShift"] = "50%";
                    break;
                case "middle":
                    _IDProperty["BaselineShift"] = "0%";
                    break;
                case "text-bottom":
                case "bottom":
                    _IDProperty["BaselineShift"] = "-50%";
                    break;
                default:
                    if (propertyValue.IndexOf("%") == (propertyValue.Length - 1))
                    {
                        _IDProperty["BaselineShift"] = propertyValue;
                    }
                    else
                    {
                        string pointValue = Common.UnitConverter(propertyValue);
                        if (pointValue.Length > 0)
                            _IDProperty["BaselineShift"] = Common.UnitConverter(propertyValue);
                    }
                    break;
            }

        }
        public void TextTransform(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["TextTransform"] = propertyValue;
        }
        public void PageBreakBefore(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["PageBreakBefore"] = propertyValue;
        }
        public void PaddingLeft(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["LeftIndent"] = propertyValue;
        }
        public void PaddingRight(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["RightIndent"] = propertyValue;
        }
        public void PaddingTop(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["SpaceBefore"] = propertyValue;
        }
        public void PaddingBottom(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["SpaceAfter"] = propertyValue;
        }
        public void Mirror(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["FacingPages"] = propertyValue;
        }
        public void PageHeight(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["Page-Height"] = propertyValue;
        }
        public void PageWidth(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["Page-Width"] = propertyValue;
        }
        public void MarginLeft(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["Margin-Left"] = propertyValue;
        }
        public void MarginRight(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["Margin-Right"] = propertyValue;
        }
        public void MarginTop(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["Margin-Top"] = propertyValue;
        }
        public void MarginBottom(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["Margin-Bottom"] = propertyValue;
        }
        public void FontFamily(string propertyValue)
        {
            _IDProperty["AppliedFont"] = propertyValue;
        }
        public void TextIndent(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["FirstLineIndent"] = propertyValue;
        }
        public void Color(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["FillColor"] = "Color/" + propertyValue;
        }
        public void BGColor(string propertyValue)
        {
            if (propertyValue == "transparent")
                return;

            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["StrokeColor"] = "Color/" + propertyValue;
        }
        public void ColumnCount(string propertyValue)
        {
            if (propertyValue == string.Empty || Common.ValidateAlphabets(propertyValue)
                || propertyValue.IndexOf('-') > -1)
            {
                _IDProperty["TextColumnCount"] = "1";
            }
            else
            {
                _IDProperty["TextColumnCount"] = propertyValue;    
            }
        }
        public void ColumnGap(string propertyValue)
        {
            if (propertyValue == string.Empty || Common.ValidateAlphabets(propertyValue)
                || propertyValue.IndexOf('-') > -1)
            {
                _IDProperty["TextColumnGutter"] = "12";
            }
            else
            {
                _IDProperty["TextColumnGutter"] = propertyValue;
            }
        }

        public void FontVariant(string propertyValue)
        {
            if (propertyValue == string.Empty || propertyValue == "inherit")
            {
                return;
            }

            if (propertyValue == "normal")
            {
                propertyValue = "Normal";
            }
            else if (propertyValue == "small-caps")
            {
                propertyValue = "SmallCaps";
            }
            _IDProperty["Capitalization"] = propertyValue;
        }
        public void TextDecoration(string propertyValue)
        {
            if (propertyValue == string.Empty || propertyValue == "inherit")
            {
                return;
            }

            if (propertyValue == "none")
            {
                propertyValue = "false";
            }
            else if (propertyValue == "underline")
            {
                propertyValue = "true";
            }
            _IDProperty["Underline"] = propertyValue;
        }
        public void FontWeight(Dictionary<string, string> cssProperty)
        {
            if (_IDProperty.ContainsKey("FontStyle")) return;

            string propertyWeight = "";
            string propertyStyle = "";
            string propertyValue = "";
            if (cssProperty.ContainsKey("font-weight"))
            {
                propertyWeight = cssProperty["font-weight"];
            }

            if (cssProperty.ContainsKey("font-style"))
            {
                propertyStyle = cssProperty["font-style"];
            }
            string strValue = propertyWeight + propertyStyle;

            if (strValue == "normalnormal" || strValue == "normal")
            {
                propertyValue = "Regular";
            }
            else if (strValue == "boldnormal" || strValue == "bold")
            {
                propertyValue = "Bold";
            }
            else if (strValue == "normalitalic" || strValue == "italic")
            {
                propertyValue = "Italic";
            }
            else if (strValue == "bolditalic")
            {
                propertyValue = "Bold Italic";
            }

            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["FontStyle"] = propertyValue;
        }
        public void TextAlign(string propertyValue)
        {
            if (propertyValue == string.Empty || propertyValue == "inherit")
            {
                return;
            }

            if (propertyValue == "justify")
            {
                propertyValue = "FullyJustified";
            }
            else if (propertyValue == "center")
            {
                propertyValue = "CenterAlign";
            }
            else if (propertyValue == "left")
            {
                propertyValue = "LeftAlign";
            }
            else if (propertyValue == "right")
            {
                propertyValue = "RightAlign";
            }
            _IDProperty["Justification"] = propertyValue;
        }
        public void FontSize(string propertyValue)
        {
            if (propertyValue == "larger" || propertyValue == "smaller")
            {
                _IDProperty["PointSize"] = propertyValue;
            }
            else if (propertyValue == string.Empty || Common.ValidateAlphabets(propertyValue)
                || propertyValue.IndexOf('-') > -1)
            {
                return;
            }
            _IDProperty["PointSize"] = propertyValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hexValue">"#ff0000"</param>
        /// <returns>"255 0 0"</returns>
        public string ConvertHexToDec(string hexValue)
        {
            string concatChar = string.Empty;
            string decValue = string.Empty;
            try
            {
                string hexFormat = hexValue.Replace("#", "");
                char[] RGB = hexFormat.ToCharArray();
                if (RGB.Length < 6)
                    return "00 00 00";

                for (int i = 0; i < RGB.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        concatChar = RGB[i].ToString();
                        continue;
                    }
                    concatChar += RGB[i].ToString();
                    decValue += " " + int.Parse(concatChar, System.Globalization.NumberStyles.HexNumber);
                    concatChar = string.Empty;
                }
            }
            catch
            {
                decValue = "00 00 00";
            }
            return decValue.Trim();
        }

        public void Display(string propertyValue)
        {
            if (propertyValue == string.Empty)
            {
                return;
            }
            _IDProperty["display"] = propertyValue;
        }        
    }
}