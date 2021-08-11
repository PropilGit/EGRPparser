using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using EGRPparser.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EGRPparser.Infrastructure
{
    class Parser
    {
        string dataPath = "data.html";
        string resultPath = "result.txt";

        public void Parse()
        {
            // открытие исходного файла
            string html = ReadSourceHTML(dataPath);
            if (String.IsNullOrEmpty(html))
            {
                AddLog("Ошибка при открытии исходного файла: " + dataPath, true);
                return;
            }

            // считывание строк
            var rows = GetElements(html, "table.t tbody tr");
            if(rows == null)
            {
                AddLog("Ошибка при считывании строк таблицы.", true);
                return;
            }

            // парсинг списка имушества
            List<Estate> estates = ParseAllEstates(rows);
            if(estates == null)
            {
                AddLog("Непредвиденная ошибка в ходе парсинга.", true);
                return;
            }
            if (estates.Contains(Estate.ErrorEstate))
            {
                Console.WriteLine("В ходе парсинга возникли ошибки! Полученная таблица содержит некорректные значения!");
                Console.WriteLine("Чтобы сохранить таблицу нажмите любую клавишу...");
                Console.ReadLine();
            }
            //WriteEstatesToConsole(estates);
            if (WriteResultToFile(estates)) Console.WriteLine("Таблица сохранена в файле result.txt");
            else AddLog("Не удалось сохранить таблицу!", true);
        }

        #region Files

        string ReadSourceHTML(string dataPath)
        {
            if (File.Exists(dataPath)) return File.ReadAllText(dataPath);
            else return null;
        }

        bool WriteResultToFile(List<Estate> estates)
        {
            try
            {
                
                File.WriteAllLines(resultPath, estates.Select(es=>es.ToConsoleLine()));
                if (File.Exists(resultPath)) return true;
                else return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Parse

        List<Estate> ParseAllEstates(IHtmlCollection<IElement> rows)
        {
            try
            {
                int estateCounter = 1;
                List<Estate> estates = new List<Estate>();
                for (int r = 0; r < rows.Length;)
                {
                    int rowspan = Int32.Parse(rows[r].QuerySelector("td:nth-child(1)").GetAttribute("rowspan"));

                    Estate estate = ParseSingleEstate(
                        estateCounter,
                        rows.Skip(r).Take(rowspan));
                        //rows.Where<IElement>(row => row.Index() < (r + rowspan)));

                    if (estate == null)
                    {
                        estates.Add(Estate.ErrorEstate);

                        AddLog("Ошибка парсинга в объекте " + estateCounter, true);
                    }
                    else
                    {
                        AddLog("" + estateCounter);
                        estates.Add(estate);
                    }
                    estateCounter++;

                    r += rowspan;
                }

                return estates;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        Estate ParseSingleEstate(int estateIndex, IEnumerable<IElement> rows)
        {
            try
            {
                // элементы ДО ограничения права

                string kadastrNum = rows.ElementAt(0).QuerySelector("td:nth-child(4)").Text();
                string name = rows.ElementAt(1).QuerySelector("td:nth-child(2)").Text();
                string purpose = rows.ElementAt(2).QuerySelector("td:nth-child(2)").Text();
                string area = rows.ElementAt(3).QuerySelector("td:nth-child(2)").Text();
                string address = rows.ElementAt(4).QuerySelector("td:nth-child(2)").Text();
                string rightType = rows.ElementAt(5).QuerySelector("td:nth-child(3)").Text();
                string gosRegDate = rows.ElementAt(6).QuerySelector("td:nth-child(2)").Text();
                string gosRegNum = rows.ElementAt(7).QuerySelector("td:nth-child(2)").Text();
                string gosRegBasis = rows.ElementAt(8).QuerySelector("td:nth-child(2)").Text();

                //ограничение права
                List<Estate.RightsRestriction> rR = new List<Estate.RightsRestriction>();
                for (int e = 10; e < rows.Count(); e += 2)
                {
                    string rRType = rows.ElementAt(e).QuerySelector("td:nth-child(3)").Text();
                    string rRGosRegNum = rows.ElementAt(e + 1).QuerySelector("td:nth-child(2)").Text();

                    rR.Add(new Estate.RightsRestriction(rRType, rRGosRegNum));
                }

                return new Estate(estateIndex, kadastrNum, name, purpose, area, address, rightType, gosRegDate, gosRegNum, gosRegBasis, rR);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        void WriteEstatesToConsole(List<Estate> estates)
        {

            foreach (var e in estates)
            {
                Console.WriteLine(e.ToConsoleLine());
            }
        }

        #endregion

        #region AngleSharp

        IHtmlCollection<IElement> GetElements(string html, string selector)
        {
            try
            {
                var parser = new HtmlParser();
                var document = parser.ParseDocument(html);

                //return document.Body.SelectNodes("/html/body/div/div/table[2]/tbody/tr");
                return document.QuerySelectorAll(selector);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region Log

        public void AddLog(string msg, bool isError = false)
        {
            if (isError) msg = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] ERROR: " + msg;

            Console.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + msg);
        }

        #endregion
    }
}
