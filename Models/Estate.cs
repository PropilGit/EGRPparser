using System;
using System.Collections.Generic;
using System.Text;

namespace EGRPparser.Models
{
    class Estate
    {
        public int Number { get; private set; }
        public static string NumberTitle = "№ п/п";

        public string KadastrNum { get; private set; }
        public static string KadastrNumTitle = "Кадастровый (или условный) номер объекта";

        public string Name { get; private set; }
        public static string NameTitle = "наименование объекта";

        public string Purpose { get; private set; }
        public static string PurposeTitle = "назначение объекта";

        public string Area { get; private set; }
        public static string AreaTitle = "площадь объекта";

        public string Address { get; private set; }
        public static string AddressTitle = "адрес (местоположение) объекта";

        public string RightType { get; private set; }
        public static string RightTypeTitle = "Вид права, доля в праве";

        public string GosRegDate { get; private set; }
        public static string GosRegDateTitle = "дата государственной регистрации:";

        public string GosRegNum { get; private set; }
        public static string GosRegNumTitle = "номер государственной регистрации:";

        public string GosRegBasis { get; private set; }
        public static string GosRegBasisTitle = "основание государственной регистрации:";

        public List<RightsRestriction> RightsRestrictions { get; private set; }
        public static string RightsRestrictionsTitle = "Ограничение права. Вид\tОграничение права. Номер гос. регистрации";

        public Estate(int number, string kadastrNum, string name, string purpose, string area, string address, 
            string rightType, string gosRegDate, string gosRegNum, string gosRegBasis, List<RightsRestriction> rightsRestrictions)
        {
            Number = number;
            KadastrNum = kadastrNum;
            Name = name;
            Purpose = purpose;
            Area = area;
            Address = address;
            RightType = rightType;
            GosRegDate = gosRegDate;
            GosRegNum = gosRegNum;
            GosRegBasis = gosRegBasis;
            RightsRestrictions = rightsRestrictions;
        }

        public struct RightsRestriction
        {
            //public int Number { get; private set; }

            // вид:
            public string Type { get; private set; }

            // номер государственной регистрации:
            public string GosRegNum { get; private set; }

            public RightsRestriction(string type, string gosRegNum)
            {
                Type = type;
                GosRegNum = gosRegNum;
            }

            public string ToConsoleLine { get => Type + "\t" + GosRegNum + "\t"; }
        }

        public static Estate ErrorEstate = new Estate(
            0, "---", "---", "---", "---", "---", "---", "---", "---", "---",
            new List<Estate.RightsRestriction> { new Estate.RightsRestriction("---", "---") });

        public string ToConsoleLine()
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

        public static string ToConsoleLineTitle()
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
    }
}
