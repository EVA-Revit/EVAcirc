using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Runtime.InteropServices;

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


    }
}
