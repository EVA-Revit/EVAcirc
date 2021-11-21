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

        public static IEnumerable<ElectricalSystem> GetSortedCircuits(FamilyInstance board)
        {
            ElectricalSystemSet fullCircuits = board.MEPModel.ElectricalSystems; //Получение всех цепей щита
            IList<ElectricalSystem> sortCircuit = new List<ElectricalSystem>();
            string boardName = board.Name;
            foreach (ElectricalSystem circ in fullCircuits)
            {
                string s = circ.PanelName;
                if (s == boardName) sortCircuit.Add(circ);
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
            sortCircuit = sortCircuit.OrderBy(x => x.StartSlot).ToList();
            foreach (ElectricalSystem circ in sortCircuit)
            {
                circItems.Add(new CircItem(circ));
            }
            return circItems; 
        }

        public static void UseParamViewAppZ(Element el, string nazn)
        {
            el.LookupParameter("AFDD").Set(0);
            el.LookupParameter("QF").Set(0);
            el.LookupParameter("QF+Н.Р.").Set(0);
            el.LookupParameter("QFD").Set(0);
            el.LookupParameter("QFD+Н.Р.").Set(0);
            el.LookupParameter("QS+N").Set(0);
            el.LookupParameter("TT").Set(0);
            el.LookupParameter("КМ").Set(0);
            el.LookupParameter("FU").Set(0);
            el.LookupParameter("QS").Set(0);
            el.LookupParameter("Wh").Set(0);
            el.LookupParameter("КТ").Set(0);
            el.LookupParameter("QD").Set(0);
            el.LookupParameter("QF_Выкатной").Set(0);
            el.LookupParameter("QF+N").Set(0);
            try
            {
                el.LookupParameter(nazn).Set(1);
            }
            catch { TaskDialog.Show("Debug", "worning paramAppZ " + nazn); }
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

        public static double Voltage(double volt)
        {
            //return UnitUtils.ConvertToInternalUnits(volt, DisplayUnitType.DUT_VOLTS);
            return UnitUtils.ConvertFromInternalUnits(volt, DisplayUnitType.DUT_VOLTS);
        }

        public static double VoltAmperage(double voltAmper)
        {
            //return UnitUtils.ConvertToInternalUnits(volt, DisplayUnitType.DUT_VOLTS);
            return UnitUtils.ConvertFromInternalUnits(voltAmper, DisplayUnitType.DUT_VOLT_AMPERES);
        }

        /// <summary>
        /// Получение элементов ключевой спецификации по её имени
        /// </summary>
        public static Element ViewKeyElement(string nameTabl)
        {
            FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(BuiltInParameter.VIEW_NAME), nameTabl, false);
            var filter = new ElementParameterFilter(rule);

            FilteredElementCollector keysp = new FilteredElementCollector(doc);
            keysp.OfClass(typeof(ViewSchedule)).WherePasses(filter);
            if (keysp.Count() != 1) return null;

            //var sd = filter.PassesFilter(keysp.FirstElement());
            return keysp.FirstElement();
        }


        public static ViewDrafting GetDrawingsView(string nameView)
        {
            var view = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).FirstOrDefault(x => x.Name == nameView);

            return view as ViewDrafting;
        }

        public static void SetParamApZ(FamilyInstance app, CircItem circ, string numApStg, int i, int order)
        {
            //Назначение
            //app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(16));
            app.LookupParameter("Перемещение_по_Х_EVA").Set(Utilits.Ft(16));

            //if (circ.Device_Type_1 == "QF")
            if (circ.GetProp(Device.Type, order) == "QF")
            {
                UseParamViewAppZ(app, "QF");
                //1
                app.LookupParameter("Строка1_EVA").Set(numApStg + "QF" + i.ToString());

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

                if (circ.GetProp(Device.Body, order) != "")
                {
                    var el = doc.GetElement(circ.Id);

                    TaskDialog.Show("ss", el.LookupParameter("I_утеч-корп_аппарата-транф_1_EVA").AsString());
                    app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "А");
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "A характеристика" 
                        + circ.GetProp(Device.Curve, order) + "кА");
                }
                else
                {
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Break, order) + "кА");
                }



            }

            else if (circ.GetProp(Device.Type, order) == "QF+Н.Р.")
            {
                UseParamViewAppZ(app, "QF+Н.Р.");
                //1
                app.LookupParameter("Строка1_EVA").Set(numApStg + "QF" + i.ToString());

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

                if (circ.GetProp(Device.Body, order) != "")
                {
                    app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "А");
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                       + (circ.Number_Of_Phase + 1).ToString() + "x" + circ.GetProp(Device.I, order) + "A характеристика"
                       + circ.GetProp(Device.Curve, order) + "кА");
                }
                else
                {
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + (circ.Number_Of_Phase + 1).ToString() + "x" + circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Break, order) + "кА");
                }

            }

            else if (circ.GetProp(Device.Type, order) == "QFD")
            {
                UseParamViewAppZ(app, "QFD");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QFD" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "мА");
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Дифф. автоматический выключатель " +
                    circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "А, характеристика " + circ.GetProp(Device.Curve, order) 
                    + ", " + circ.GetProp(Device.Body, order) + "мА, " + circ.GetProp(Device.Break, order) + "кА");
            }

            else if (circ.GetProp(Device.Type, order) == "QFD+Н.Р.")
            {
                UseParamViewAppZ(app, "QFD+Н.Р.");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QFD" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "мА");
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Дифф. автоматический выключатель " +
                    (circ.Number_Of_Phase +1).ToString() + "x" + circ.GetProp(Device.I, order) + "А, характеристика " + circ.GetProp(Device.Curve, order)
                    + ", " + circ.GetProp(Device.Body, order) + "мА, " + circ.GetProp(Device.Break, order) + "кА");

            }

            else if (circ.GetProp(Device.Type, order) == "FU")
            {
                UseParamViewAppZ(app, "FU");


                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Плавкая вставка " 
                    + circ.GetProp(Device.I, order) + "А");

                if (circ.Number_Of_Phase == 3)
                {
                    app.LookupParameter("Строка1_EVA").Set(numApStg + "FU" + i.ToString() + ".1 " + numApStg + "FU" +
                        i.ToString() + ".3");
                }
                else app.LookupParameter("Строка1_EVA").Set(numApStg + "FU" + i.ToString());
            }

            else if (circ.GetProp(Device.Type, order) == "QD")
            {
                UseParamViewAppZ(app, "QD");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QD" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Break, order) + "мА");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Устр. защитного отключения "
                    + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "А");

            }


            else if (circ.GetProp(Device.Type, order) == "QS")
            {
                UseParamViewAppZ(app, "QS");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QS" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Рубильник "
                    + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "А");
            }

            else if (circ.GetProp(Device.Type, order) == "KM")
            {
                UseParamViewAppZ(app, "КМ");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "KM" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Контактор "
                    + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "А");
            }


            else if (circ.GetProp(Device.Type, order) == "Wh")
            {
                UseParamViewAppZ(app, "Wh");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "Wh" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Счетчик эл. энергии");
            }

            else if (circ.GetProp(Device.Type, order) == "QF+N")
            {
                UseParamViewAppZ(app, "QF+N");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QF" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");
                if (circ.GetProp(Device.Body, order) != "")
                {
                    app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "А");
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "A характеристика"
                        + circ.GetProp(Device.Curve, order) + "кА");
                }
                else
                {
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Break, order) + "кА");
                }
            }

            else if (circ.GetProp(Device.Type, order) == "QF_Выкатной")
            {
                UseParamViewAppZ(app, "QF_Выкатной");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QF" + i.ToString());

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

                if (circ.GetProp(Device.Body, order) != "")
                {
                    app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "А");
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "A характеристика"
                        + circ.GetProp(Device.Curve, order) + "кА");
                }
                else
                {
                    app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Автоматический выключатель "
                        + circ.Number_Of_Phase + "x" + circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Break, order) + "кА");
                }
            }

            else if (circ.GetProp(Device.Type, order) == "AFDD")
            {
                UseParamViewAppZ(app, "AFDD");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "AFDD" + i.ToString());

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A ");
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Устр. защиты при дуговой пробое");

            }
            else if (circ.GetProp(Device.Type, order) == "Wh+TT")
            {
                UseParamViewAppZ(app, "TT");

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));

                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Тр-тр тока");

                if (circ.Number_Of_Phase == 3)
                {
                    app.LookupParameter("Строка1_EVA").Set(numApStg + "Т" + i.ToString() + ".1 " + numApStg + "Т" +
                        i.ToString() + ".3");
                }
                else app.LookupParameter("Строка1_EVA").Set(numApStg + "Т" + i.ToString());


            }

            else if (circ.GetProp(Device.Type, order) == "QS+N")
            {
                UseParamViewAppZ(app, "QS+N");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QS" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Рубильник "
                    + (circ.Number_Of_Phase + 1).ToString() + "x" + circ.GetProp(Device.I, order) + "А");
            }

            //Общее назначения
            app.LookupParameter("Тип_марка_обозначение_документа_EVA").Set(circ.GetProp(Device.Mark, order));
            app.LookupParameter("Код_оборудования_EVA").Set(circ.GetProp(Device.Article, order));
            app.LookupParameter("Завод-изготовитель_EVA").Set(circ.GetProp(Device.Manufacturer, order));
            app.LookupParameter("Проект_EVA").Set(circ.Proekt);
            app.LookupParameter("Имя_щита_EVA").Set(circ.PanelName);
            app.LookupParameter("ID_IN_EVA").Set(circ.Id.ToString() + "$" + i.ToString());



        }





    }
}
