using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Runtime.InteropServices;
using EVA_Gen.WPF.Models;

namespace EVA_Gen
{
    static class Utilits
    { 

        private static Document doc;

        public static Document Doc
        {
            get { return doc; }
            set { doc = value; }
        }


        public static List<Element> GetElectricalEquipment(Document doc)
        {
            FilteredElementCollector collector = GetElementsOfType(doc, typeof(FamilyInstance), BuiltInCategory.OST_ElectricalEquipment);
            //возращаем список вместо IList
            return new List<Element>(collector.ToElements());
        }

        public static List<PanelItem> GetPanelsItems(Document doc)
        {
            List<PanelItem> panelItems = new List<PanelItem>();
            FilteredElementCollector collector = GetElementsOfType(doc, typeof(FamilyInstance), BuiltInCategory.OST_ElectricalEquipment);
            foreach (Element board in collector)
            {
                //PanelItem panelItem = new PanelItem(board);
                if (board.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM).AsDouble() != 0)
                { 
                    panelItems.Add(new PanelItem(board));
                }
            }
            //возращаем список вместо IList
            return panelItems;
        }


        public static FilteredElementCollector GetElementsOfType(Document doc, Type type, BuiltInCategory bic)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            collector.OfCategory(bic);
            collector.OfClass(type);

            return collector;
        }
        public static double Ft(double mm)
        {
            return (mm * 0.001) * (1 / 0.3048);
        }



        public static Element FamType(Family fam, string name)
        {
            Element ans = null;
            var id = fam.GetFamilySymbolIds();
            foreach (ElementId i in id)
            {
                Element el = doc.GetElement(i);
                string s = el.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
                if (s == name)
                {
                    ans = el;
                    break;
                }
            }
            return ans;
        }



        public static FamilySymbol GetFamAn(string name)
        {
            IList<ElementId> cats = new List<ElementId>();
            cats.Add(new ElementId(BuiltInCategory.OST_GenericAnnotation));
            cats.Add(new ElementId(BuiltInCategory.OST_DetailComponents));
            FilterCategoryRule r = new FilterCategoryRule(cats);
            ElementParameterFilter f = new ElementParameterFilter(r);
            FilteredElementCollector collector = new FilteredElementCollector(doc).WherePasses(f);
            //collector.OfCategory(BuiltInCategory.OST_GenericAnnotation);
            collector.WhereElementIsElementType();

            FamilySymbol fam = null;
            foreach (Element el in collector)
            {
                ElementType elAnSType = el as ElementType;

                string famName = elAnSType.FamilyName;
                if (famName == name)
                {
                    fam = elAnSType as FamilySymbol;
                    break;
                }
            }
            return fam;
        }

        public static IEnumerable<ElectricalSystem> GetSortedCircuits(FamilyInstance board, out ElectricalSystem circBoard)
        {
            circBoard = null;
            ElectricalSystemSet fullCircuits = board.MEPModel.ElectricalSystems; //Получение всех цепей щита
            IList<ElectricalSystem> sortCircuit = new List<ElectricalSystem>();
            string boardName = board.Name;
            foreach (ElectricalSystem circ in fullCircuits)
            {
                string s = circ.PanelName;
                if (s == boardName) sortCircuit.Add(circ);
                else circBoard = circ;
            }
            return sortCircuit.OrderBy(x => x.StartSlot);
        }

        public static ObservableCollection<CircItem> GetSortedCircuits(Element element, out CircItem circBoard)
        {
            FamilyInstance board = element as FamilyInstance;
            circBoard = null;
            ObservableCollection<CircItem> circItems = new ObservableCollection<CircItem>();
            ElectricalSystemSet fullCircuits = board.MEPModel.ElectricalSystems; //Получение всех цепей щита
            List<ElectricalSystem> sortCircuit = new List<ElectricalSystem>();
            string boardName = board.Name;
            foreach (ElectricalSystem circ in fullCircuits)
            {
                string s = circ.PanelName;
                if (s == boardName) sortCircuit.Add(circ);
                else
                {
                    circBoard = new CircItem(circ);
                    if (circ.BaseEquipment != null)
                    circBoard.ParentBoardId = circ.BaseEquipment.Id;
                }
                    
            }
            sortCircuit.OrderBy(x => x.StartSlot);
            foreach (ElectricalSystem circ in sortCircuit)
            {
                circItems.Add(new CircItem(circ));
            }
            return circItems; 
        }

        public static void UseParamViewAppZ(Element el, string nazn)
        {
            el.LookupParameter("QF_EVA").Set(0);
            el.LookupParameter("QF+Н.Р._EVA").Set(0);
            el.LookupParameter("QFD_EVA").Set(0);
            el.LookupParameter("QFD+Н.Р._EVA").Set(0);
            el.LookupParameter("КМ_EVA").Set(0);
            el.LookupParameter("FU_EVA").Set(0);
            el.LookupParameter("Мотор_привод_EVA").Set(0);
            el.LookupParameter("QS_EVA").Set(0);
            el.LookupParameter("Wh_EVA").Set(0);
            el.LookupParameter("КТ_EVA").Set(0);
            el.LookupParameter("QD_EVA").Set(0);
            try
            {
                el.LookupParameter(nazn).Set(1);
            }
            catch { TaskDialog.Show("Debug", "worning paramAppZ"); }
        }
        public static void UseParamViewUgo(Element el, string nazn)
        {
            ZeroingViewUgo(el); //обнуление видимости УГО
            try
            {
                el.LookupParameter(nazn).Set(1);
            }
            catch { TaskDialog.Show("Debug", "worning paramUgo " + nazn); }
        }

        public static void UseParamViewUgoInTypeLoad(Element el, string tepeLoad)
        {
            ZeroingViewUgo(el); //обнуление видимости УГО
            try
            {   
                switch(tepeLoad)
                {
                    case "Вентиляция общеобменная":
                        el.LookupParameter("Эл.двигатель").Set(1);
                        break;
                    case "Вентиляция противодымная":
                        el.LookupParameter("Эл.двигатель").Set(1);
                        break;
                    case "Заград. Огни":
                        el.LookupParameter("Освещение_Рабочее").Set(1);
                        break;
                    case "Квартиры по удельной нагрузки":
                        el.LookupParameter("УЭР(Б,В,К,М,Н)").Set(1);
                        break;
                    case "Квартиры повышенной комфортности":
                        el.LookupParameter("УЭР(Б,В,К,М,Н)").Set(1);
                        break;
                    case "Лифты":
                        el.LookupParameter("Эл.двигатель").Set(1);
                        break;
                    case "Оборудование насосное":
                        el.LookupParameter("Эл.двигатель").Set(1);
                        break;
                    case "Освещение аварийное":
                        el.LookupParameter("Освещение_Авар").Set(1);
                        break;
                    case "Освещение наружное":
                        el.LookupParameter("Освещение_Рабочее").Set(1);
                        break;
                    case "Освещение наружное(реклама)":
                        el.LookupParameter("Освещение_Рабочее").Set(1);
                        break;
                    case "Освещение рабочее":
                        el.LookupParameter("Освещение_Рабочее").Set(1);
                        break;
                    case "Противопожарные клапаны":
                        el.LookupParameter("Клапан").Set(1);
                        break;
                    case "Противопожные слаботочные системы":
                        el.LookupParameter("Блок_Управления").Set(1);
                        break;
                    case "Слаботочные системы":
                        el.LookupParameter("ЩУ").Set(1);
                        break;
                    case "Уборочные механизмы":
                        el.LookupParameter("Розетка_1Ф_IP20").Set(1);
                        break;
                    case "Установки пожаротушения":
                        el.LookupParameter("Эл.двигатель").Set(1);
                        break;
                    case "Холодоснабжение":
                        el.LookupParameter("Эл.двигатель").Set(1);
                        break;
                    case "Электрообогрев":
                        el.LookupParameter("ТЭН").Set(1);
                        break;

                    default:
                        el.LookupParameter("Устройство").Set(1);
                        break;
                }
            }
            catch { TaskDialog.Show("Debug", "worning paramUgoTypeLoad"); }
        }






        private static void ZeroingViewUgo(Element el)
        {
            el.LookupParameter("Блок_Управления").Set(0);
            el.LookupParameter("Блок_питания").Set(0);
            el.LookupParameter("Клапан").Set(0);
            el.LookupParameter("Освещение_Авар").Set(0);
            el.LookupParameter("Освещение_Рабочее").Set(0);
            el.LookupParameter("Розетка_1Ф_IP20").Set(0);
            el.LookupParameter("Розетка_1Ф_IP40").Set(0);
            el.LookupParameter("Розетка_3Ф_IP40").Set(0);
            el.LookupParameter("ТЭН").Set(0);
            el.LookupParameter("УЭР(Б,В,К,М,Н)").Set(0);
            el.LookupParameter("Устройство").Set(0);
            el.LookupParameter("ЩАО").Set(0);
            el.LookupParameter("ЩО").Set(0);
            el.LookupParameter("ЩР").Set(0);
            el.LookupParameter("ЩУ").Set(0);
            el.LookupParameter("Эл.вывод").Set(0);
            el.LookupParameter("Эл.двигатель").Set(0);
            el.LookupParameter("ЯТП").Set(0);
        }
    }
}
