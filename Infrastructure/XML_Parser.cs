﻿using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using EGRPparser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace EGRPparser.Infrastructure
{
    class XML_Parser
    {
        string dataPath = "data.xml";
        string resultPath = "result.txt";
        string inventoryPath = "inventory.txt";

        int estateIndex = 0;

        public void Parse()
        {
            try
            {
                // меняем пути исходных данных на директорию выше 
                DirectoryIdentify();

                // открытие исходного файла
                Console.WriteLine("Положите в текущую папку файл, который нужно распознать. Переименуйте его в data.xml");
                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                //Console.ReadLine();


                XPathDocument xpathDoc = new XPathDocument(GetXMLStream(dataPath));
                XPathNavigator navigator = xpathDoc.CreateNavigator();


                //land_record
                //XPathNodeIterator land_records = navigator.Select("/extract_rights_individ_available_real_estate_objects/base_data/land_records/land_record");

                /*while(land_records.MoveNext())
                {
                    var test = land_records.Current.SelectSingleNode("object");
                    
                    Console.ReadLine();
                }*/

                List<Estate> estates = new List<Estate>();

                var land_records = ParseAllEstates(navigator.Select("/extract_rights_individ_available_real_estate_objects/base_data/land_records/land_record"));
                var room_records = ParseAllEstates(navigator.Select("/extract_rights_individ_available_real_estate_objects/base_data/room_records/room_record"));
                var build_records = ParseAllEstates(navigator.Select("/extract_rights_individ_available_real_estate_objects/base_data/build_records/build_record"));
                var construction_records = ParseAllEstates(navigator.Select("/extract_rights_individ_available_real_estate_objects/base_data/construction_records/construction_record"));

                while (land_records.MoveNext()) estates.Add(land_records.Current);
                while (room_records.MoveNext()) estates.Add(room_records.Current);
                while (build_records.MoveNext()) estates.Add(build_records.Current);
                while (construction_records.MoveNext()) estates.Add(construction_records.Current);

                WriteDataToFile(estates);
                WriteInventoryToFile(estates);
                Console.WriteLine("===========OK===========");
                Console.ReadLine();
            }
            catch (Exception)
            {
                Console.WriteLine("===========КРИТИЧЕСКАЯ ОШИБКА===========");
            }
        }



        #region Files

        Stream GetXMLStream(string dataPath)
        {
            if (File.Exists(dataPath)) return new FileStream(dataPath, FileMode.Open);
            else return null;
        }

        bool WriteDataToFile(List<Estate> estates)
        {
            try
            {
                string[] result = new string[estates.Count + 1];
                result[0] = Estate.TitleToLine();
                estates.Select(es => es.DataToLine()).ToArray().CopyTo(result, 1);

                File.WriteAllLines(resultPath, result);
                if (File.Exists(resultPath)) return true;
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        bool WriteInventoryToFile(List<Estate> estates)
        {
            try
            {
                File.WriteAllLines(inventoryPath, estates.Select(es => es.ToInventoryLine()));
                if (File.Exists(inventoryPath)) return true;
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        void DirectoryIdentify()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            dataPath = dir + @"\" + dataPath;
            resultPath = dir + @"\" + resultPath;
            inventoryPath = dir + @"\" + inventoryPath;
        }

        #endregion

        #region Parse

        IEnumerator<Estate> ParseAllEstates(XPathNodeIterator nodes)
        {
            try
            {
                List<Estate> estates = new List<Estate>();
                while(nodes.MoveNext())
                {
                    Estate estate = ParseSingleEstate(nodes.Current);

                    if (estate == null)estates.Add(Estate.ErrorEstate);
                    else estates.Add(estate);
                }

                return estates.GetEnumerator();
            }
            catch (Exception)
            {
                return null;
            }
        }

        Estate ParseSingleEstate(XPathNavigator node)
        {
            try
            {

                var kadastrNum = ReturnNodeValueOrEmpty(node, "object/common_data/cad_number");

                string name = ReturnNodeValueOrEmpty(node, "object/common_data/type/value");

                string purpose = ReturnNodeValueOrEmpty(node, "params/purpose");

                string area = ReturnNodeValueOrEmpty(node, "params/area"); //params/area/value
                //string inaccuracy = ReturnNodeValueOrEmpty(node, "params/area/inaccuracy");
                //if (!string.IsNullOrEmpty(inaccuracy)) area += " +/- " + inaccuracy;

                string address = ReturnNodeValueOrEmpty(node, "address_location/address/readable_address");

                string rightType = ReturnNodeValueOrEmpty(node, "right_record/right_data/right_type/value");

                string gosRegDate = ReturnNodeValueOrEmpty(node, "right_record/record_info/registration_date");

                string gosRegNum = ReturnNodeValueOrEmpty(node, "right_record/right_data/right_type/right_number");

                string gosRegBasis = "";
                XPathNodeIterator underlying_documents = node.Select("right_record/underlying_documents/underlying_document");
                if (underlying_documents.Count > 0)
                {
                    while (underlying_documents.MoveNext())
                    {
                        string underlying_document = "";
                        try
                        {
                            string document_name = ReturnNodeValueOrEmpty(underlying_documents.Current, "document_name");
                            if (!string.IsNullOrEmpty(document_name)) underlying_document += document_name;

                            string document_number = ReturnNodeValueOrEmpty(underlying_documents.Current, "document_number");
                            if (!string.IsNullOrEmpty(document_number)) underlying_document += ", " + document_number;

                            string document_date = ReturnNodeValueOrEmpty(underlying_documents.Current, "document_date");
                            DateTime _document_date = DateTime.Parse(document_date);
                            if (!string.IsNullOrEmpty(document_date)) underlying_document += ", выдан " + _document_date.Day + "." + _document_date.Month + "." + _document_date.Year;

                            string document_issuer = ReturnNodeValueOrEmpty(underlying_documents.Current, "document_issuer");
                            if (!string.IsNullOrEmpty(document_issuer)) underlying_document += ", " + document_issuer;
                        }
                        catch (Exception)
                        {
                        }
                        gosRegBasis += underlying_document + "; ";
                    }
                }

                //ограничение права
                List<Estate.RightsRestriction> rR = new List<Estate.RightsRestriction>();
                XPathNodeIterator restrict_records = node.Select("restrict_records/restrict_record");
                if (restrict_records.Count > 0)
                {
                    while (restrict_records.MoveNext())
                    {
                        try
                        {
                            string type = ReturnNodeValueOrEmpty(restrict_records.Current, "restrictions_encumbrances_data/restriction_encumbrance_type/value");

                            string number = ReturnNodeValueOrEmpty(restrict_records.Current, "restrictions_encumbrances_data/restriction_encumbrance_number");

                            rR.Add(new Estate.RightsRestriction(type, number));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                estateIndex++;
                return new Estate(estateIndex, kadastrNum, name, purpose, area, address, rightType, gosRegDate, gosRegNum, gosRegBasis, rR);
            }
            catch (Exception)
            {
                estateIndex++;
                return null;
            }
        }


        string ReturnNodeValueOrEmpty(XPathNavigator node, string xpath)
        {
            try
            {
                XPathNavigator valueNode = node.SelectSingleNode(xpath);
                if (valueNode == null) return "";

                string result = valueNode.Value;
                if (!string.IsNullOrEmpty(result)) return result;
            }
            catch (Exception)
            {

            }

            return "";
        }


        void WriteEstatesToConsole(List<Estate> estates)
        {

            foreach (var e in estates)
            {
                Console.WriteLine(e.DataToLine());
            }
        }

        #endregion

        #region Log

        public void AddLog(string msg, bool isError = false)
        {
            if (isError) msg = "ERROR: " + msg;

            Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg);
        }

        #endregion
    }
}
