using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EGRPparser.Models
{
    class Estate
    {
        public Estate(int number, string kadastrNum, string name, string purpose, string area, string address,
            string rightType, string gosRegDate, string gosRegNum, string gosRegBasis, List<RightsRestriction> rightsRestrictions)
        {

            Number = number;
            KadastrNum = NormalizeString(kadastrNum);
            Name = NormalizeString(name);
            Purpose = NormalizeString(purpose);
            Area = NormalizeString(area);
            Address = NormalizeString(address);
            RightType = NormalizeString(rightType);
            GosRegDate = NormalizeString(gosRegDate);
            GosRegNum = NormalizeString(gosRegNum);
            GosRegBasis = NormalizeString(gosRegBasis);
            RightsRestrictions = rightsRestrictions;

        }

        #region Fileds

        public int Number { get; private set; }
        public static string NumberTitle = "№ п/п";

        public string KadastrNum { get; private set; }
        public static string KadastrNumTitle = "Кадастровый номер";

        public string Name
        {
            get => name;
            private set
            {
                name = value.ToLower();
            }
        }
        private string name;
        public static string NameTitle = "Наименование";

        public string Purpose { 
            get => purpose; 
            private set {
                purpose = Regex.Replace(value, @"^[0-9|)|.]+", "").Trim();
            }
        }
        private string purpose;
        public static string PurposeTitle = "Назначение";

        public string Area { get; private set; }
        public static string AreaTitle = "Площадь";

        public string Address { get; private set; }
        public static string AddressTitle = "Адрес (местоположение)";

        public string RightType { get; private set; }
        public static string RightTypeTitle = "Вид права, доля в праве";

        public string GosRegDate { get; private set; }
        public static string GosRegDateTitle = "Дата гос. регистрации";

        public string GosRegNum { get; private set; }
        public static string GosRegNumTitle = "Номер гос. регистрации";

        public string GosRegBasis { get; private set; }
        public static string GosRegBasisTitle = "Основание гос. регистрации";

        public List<RightsRestriction> RightsRestrictions { get; private set; }
        public static string RightsRestrictionsTitle = "Ограничения права";

        public struct RightsRestriction
        {

            // вид:
            public string Type { get; private set; }

            // номер государственной регистрации:
            public string GosRegNum { get; private set; }

            public RightsRestriction(string type, string gosRegNum)
            {
                Type = NormalizeString(type);
                GosRegNum = NormalizeString(gosRegNum);
            }

            public string ToConsoleLine { get => Type + ", " + GosRegNum + "; "; }
        }

        #endregion

        #region Static

        public static Estate ErrorEstate = new Estate( 0, "---", "---", "---", "---", "---", "---", "---", "---", "---", new List<Estate.RightsRestriction> { new Estate.RightsRestriction("---", "---") });

        #endregion

        #region Normalize

        public static string NormalizeString(string input)
        {
            string[] replaceables = new[] { "\n", "\t", "\r", " " };
            string rxString = string.Join("|", replaceables.Select(s => Regex.Escape(s)));

            string result = Regex.Replace(input, rxString, " ");
            result = Regex.Replace(result, @"[ ]{2,}", " ");

            if (result == " ") 
                result = "";

            return result;
        }

        public static string NormalizeDate(string input)
        {
            try
            {
                DateTime date = DateTime.Parse(input);
                return NormalizeString(date.Day + "." + date.Month + "." + date.Year);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string NormalizeNumber(string input)
        {
            return NormalizeString(input.Replace(".", ","));
        }

        #endregion

        #region ToLine

        public string DataToLine()
        {
            string text = Number.ToString() + "\t" +
                    KadastrNum + "\t" +
                    Name + "\t" +
                    Purpose + "\t" +
                    Area + "\t" +
                    Address + "\t" +
                    RightType + "\t" +
                    GosRegDate + "\t" +
                    GosRegNum + "\t" +
                    GosRegBasis + "\t";

            foreach (var rR in RightsRestrictions)
            {
                text += rR.ToConsoleLine;
            }

            return text;
        }

        public static string TitleToLine() //базовая таблица
        {
            return NumberTitle + "\t" +
                    KadastrNumTitle + "\t" +
                    NameTitle + "\t" +
                    PurposeTitle + "\t" +
                    AreaTitle + "\t" +
                    AddressTitle + "\t" +
                    RightTypeTitle + "\t" +
                    GosRegDateTitle + "\t" +
                    GosRegNumTitle + "\t" +
                    GosRegBasisTitle + "\t" +
                    RightsRestrictionsTitle;
        }

        public string ToInventoryLine() // инвентарная опись
        {
            string text = Number.ToString() + "\t" +
                    "Нежилое " + Name + ", с кадастровым номером " + KadastrNum + ", по адресу " + Address;

            if (Area == "") text += "\t";
            else text += ", площадью " + Area + "\t";

            text += KadastrNum + "\t";

            return text;
        }

        #endregion
    }
}
