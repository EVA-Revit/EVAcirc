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
using System.Collections.ObjectModel;
using EVA_Gen.WPF.Models;
using EVA_Gen.WPF.ViewModels;
using EVA_Gen.WPF.Infrastructure.Commands;

namespace EVA_Gen
{
    class Develop
    {
        private static Document doc;
        private static UIDocument uidoc;
        protected static List<Element> boards;

        //Получение id категории
        //private static Categories categories = doc.Settings.Categories;
        //private static ElementId electricalEquipmentCategoryId = categories.get_Item(BuiltInCategory.OST_ElectricalEquipment).Id;
        //protected static List<PanelItem> panelItems;

        public static Result SomeCode (ExternalCommandData commandData, ref string message)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            Utilits.Doc = doc;
            try
            {
                //Здесь пишется код или метод boolean
                if (!CodeMetod())
                {
                    return Result.Failed;
                }
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message + ex.StackTrace;
                return Result.Failed;
            }
           
        }


        

        



        /// <summary>
        /// Метод в котором пишется основной код
        /// </summary>
        /// <returns></returns>
        static Boolean CodeMetod()
        {

            boards = Utilits.GetElectricalEquipment(doc);//получаем лист элементов электрооборудования

            GenCommand.doc = doc;
            GenCommand.uidoc = uidoc;

            List<ElementId> equipmentChildren = new List<ElementId>();

            foreach (FamilyInstance fi in boards)
            {
                //проверка если в щите нет нагрузки
                if (fi.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM).AsDouble() != 0)
                {
                    //Перебор коннекторов щита (исходящие + входящий)
                    foreach (Connector c in fi.MEPModel.ConnectorManager.Connectors)
                    {
                        ConnectorSet refs = c.AllRefs; //Все ссылки на коннектор щита
                        foreach (Connector c2 in refs) //Перебор из всех ссылок на коннектор щита
                        {
                            Debug.Assert(null != c2.Owner, "ожидается владелец коннектора");

                            Debug.Assert(c2.Owner is ElectricalSystem, "ожидаемый элемент панели будет эл. цепью");

                            ElectricalSystem eq = c2.Owner as ElectricalSystem; //Получения из каждой ссылки на коннектор щита его владельца(элцепь)

                            if (eq.CircuitType != CircuitType.Circuit) { continue; }  //добавлена проверка из-зи 18го revit

                            foreach (Element e2 in eq.Elements) //из всех элементов электроцепи
                            {
                                Debug.Assert(e2 is FamilyInstance, "ожидаемый элемент эл.цепи будет экземпляром семейства");

                                if (!e2.Id.Equals(fi.Id)) //если id элемента цепи не будет равен id щита то(т.е не питающая цепь):
                                {
                                    //Если в листе щитов содержится эквивалент элемента эл.цепи то он является дочерним true:
                                    if (boards.Exists(delegate (Element e) { return e.Id.Equals(e2.Id); }))
                                    {
                                        //equipmentParents[e2.Id] = eq.Id; //в словарь записать значение id элемента цепи (дочернего)
                                        equipmentChildren.Add(e2.Id);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<PanelItem> panelItems = new List<PanelItem>();

            foreach (FamilyInstance fi in boards)
            {
                if (fi.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM).AsDouble() != 0)
                {
                    //Если в списке дочерних щитов не содержится щит из списка щитов, то он щитается родительским и:
                    if (!equipmentChildren.Contains(fi.Id))
                    {
                        var panelItem = GetPanelItems(fi);
                        panelItems.Add(panelItem);
                    }

                }
            }


            var mVm = new MainWindowViewModel();
            mVm.Panels = panelItems;
            var view = new WPF.Views.MainWindow();
            view.DataContext = mVm;
            view.ShowDialog();
            return true;
        }



        private static PanelItem GetPanelItems(FamilyInstance board)
        {
            PanelItem panelItem = new PanelItem(board);

            Categories categories = doc.Settings.Categories;
            ElementId electricalEquipmentCategoryId = categories.get_Item(BuiltInCategory.OST_ElectricalEquipment).Id;

            foreach (Connector c in board.MEPModel.ConnectorManager.Connectors)
            {
                ConnectorSet refs = c.AllRefs; //все ссылки на коннектор
                foreach (Connector c2 in refs)
                {
                    Debug.Assert(null != c2.Owner,
                        "ожидается владелец соединителя");

                    Debug.Assert(c2.Owner is ElectricalSystem,
                        "ожидаемый элемент щита будет ел.цепью");

                    ElectricalSystem eq = c2.Owner as ElectricalSystem; //Получения из каждой ссылки на коннектор щита его владельца(элцепь)

                    if (eq.CircuitType != CircuitType.Circuit) { continue; }  //добавлена проверка из-зи 18го revit

                    foreach (Element e2 in eq.Elements)// Перебор элементов цепи
                    {
                        Debug.Assert(e2 is FamilyInstance, "ожидаемый элемент эл.цепи будет экземпляром семейства");

                        //если id элемента цепи не будет равен id щита и элемент будет щитом то:
                        if (!e2.Id.Equals(board.Id) && e2.Category.Id == electricalEquipmentCategoryId)
                        {
                            //потомок
                            var pe = GetPanelItems(e2 as FamilyInstance);
                            panelItem.SubPanels.Add(pe);

                        }
                    }
                }
            }
            return panelItem;
        }

    }
}
