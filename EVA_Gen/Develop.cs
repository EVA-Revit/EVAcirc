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
using System.Threading;

namespace EVA_Gen
{
    class Develop
    {
        private static Document doc;
        private static UIDocument uidoc;
        public static List<PanelItem> boards;
        //private static Categories categories; 
        private static ElementId electricalEquipmentCategoryId;
        private static ConnectorSet refs;
        private static ElectricalSystem eq;
        private static PanelItem panelItem;
        private static PanelItem panelItem2;



        public static Result SomeCode(ExternalCommandData commandData, ref string message)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            Utilits.Doc = doc;
            Calculation.Doc = doc;
            Categories categories = doc.Settings.Categories;
            electricalEquipmentCategoryId = categories.get_Item(BuiltInCategory.OST_ElectricalEquipment).Id;
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

            boards = Utilits.GetPanelsItems(doc);//получаем лист элементов электрооборудования

            GenCommand.doc = doc;
            GenCommand.uidoc = uidoc;
            GenBoardCommand.doc = doc;
            GenBoardCommand.uidoc = uidoc;


            List<ElementId> equipmentChildren = new List<ElementId>();
            ObservableCollection<PanelItem> panelItems = new ObservableCollection<PanelItem>();

            foreach (var fi in boards)
            {
                //проверка если в щите нет нагрузки

                //Перебор коннекторов щита (исходящие + входящий)

                //Если отсутствует родительский ID, то этот щит является родительским:
                if (fi.ParentBoardId == null)
                {
                    //equipmentParents[e2.Id] = eq.Id; //в словарь записать значение id элемента цепи (дочернего)
                    //equipmentChildren.Add(fi.Id);
                    var panelItem = GetPanelItems(fi, 0);
                    panelItems.Add(panelItem);
                }
                
                
            }

            //Заполнение комбобоксов
            Element view_Combo = Utilits.ViewKeyElement("Тип_Нагрузки_EVA");
            FilteredElementCollector string_table_combo = new FilteredElementCollector(doc, view_Combo.Id);
            CircItem.TypeLoadCB.Add(new TypeLoadItem());
            foreach (var item in string_table_combo)
            {
                CircItem.TypeLoadCB.Add(new TypeLoadItem(item));
            }

            view_Combo = Utilits.ViewKeyElement("УГО_EVA");
            string_table_combo = new FilteredElementCollector(doc, view_Combo.Id);
            foreach (var item in string_table_combo)
            {
                CircItem.UgoCB.Add(item.Name);
            }





            //выполнение расчетов
            //сортировка щитов
            var calcBoardsList = boards.OrderByDescending(x => x.ParentNumber);

            //получения id строк ключевой спецификации
            Element view_AAZGL = Utilits.ViewKeyElement("Фаза_подключения_EVA");
            FilteredElementCollector string_table_app = new FilteredElementCollector(doc, view_AAZGL.Id);
            Calculation.L1_id = string_table_app.FirstOrDefault(x => x.Name == "L1").Id;
            Calculation.L2_id = string_table_app.FirstOrDefault(x => x.Name == "L2").Id;
            Calculation.L3_id = string_table_app.FirstOrDefault(x => x.Name == "L3").Id;
            Calculation.L123_id = string_table_app.FirstOrDefault(x => x.Name == "-").Id;

            view_AAZGL = Utilits.ViewKeyElement("Режим_работы_Зима_Лето_EVA");
            string_table_app = new FilteredElementCollector(doc, view_AAZGL.Id);
            Calculation.Load_Winter = string_table_app.FirstOrDefault(x => x.Name == "зима").Id;
            Calculation.Load_Summer = string_table_app.FirstOrDefault(x => x.Name == "лето").Id;
            Calculation.Load_All_Year = string_table_app.FirstOrDefault(x => x.Name == "круглый год").Id;

            view_AAZGL = Utilits.ViewKeyElement("Режим_учета_нагрузки_EVA");
            string_table_app = new FilteredElementCollector(doc, view_AAZGL.Id);
            Calculation.Load_Mode_Double = string_table_app.FirstOrDefault(x => x.Name == "в обоих режимах").Id;
            Calculation.Load_Mode_Fire = string_table_app.FirstOrDefault(x => x.Name == "в пожарном режиме").Id;
            Calculation.Load_Mode_Work = string_table_app.FirstOrDefault(x => x.Name == "в рабочем режиме").Id;
            Calculation.Load_Mode_Ignore = string_table_app.FirstOrDefault(x => x.Name == "не учитывать").Id;


            using (Transaction newTran = new Transaction(doc, "RecordingCalc"))
            {
                newTran.Start();

                foreach (var pitem in calcBoardsList)
                {
                    Calculation.Circuits(pitem);
                    Calculation.Panels(pitem);
                }

                newTran.Commit();
            }



           


            //foreach (var fi in boards)
            //{

            //    //Если в списке дочерних щитов не содержится щит из списка щитов, то он щитается родительским и:
            //    if (!equipmentChildren.Contains(fi.Id))
            //    {
            //        //Thread tr = new Thread()
            //        //{

            //        //}
            //        //var panelItem = GetPanelItems(fi); //
            //        var panelItem = GetPanelItems(fi);
            //        panelItems.Add(panelItem);
            //    }


            //}
            #region Объеденение панелелей в ВРУ


            //ObservableCollection<PanelItem> nPanel = new ObservableCollection<PanelItem>();
            //ObservableCollection<PanelItem> dPanel = new ObservableCollection<PanelItem>();
            //var dd = panelItems.Where(x => x.Name.StartsWith("ВРУ"));

            //var gf = panelItems.OfType<PanelItem>();
            //TaskDialog.Show("sadad", gf.Count().ToString());
            ////foreach (PanelItem item in panelItems)
            //foreach (PanelItem item in gf)
            //{
            //    if (item.Name.StartsWith("ВРУ"))
            //    {
            //        TaskDialog.Show("sfs", "VRU");
            //        dPanel.Add(item);
            //        var asf = nPanel.Where(x => x.Name.StartsWith(item.Name.Remove(3)));
            //        if (asf.Count() > 0) asf.First().SubPanels.Add(item);
            //        else nPanel.Add(new PanelItem(item));

            //        //panelItems.Remove(item);
            //    }
            //    //var lf = UnionPanels(item)
            //}

            //foreach (var item in dPanel)
            //{
            //    panelItems.Remove(item);
            //}
            //foreach (var item in nPanel)
            //{
            //    panelItems.Add(item);
            //}

            #endregion

            var mVm = new MainWindowViewModel();
            mVm.Panels = panelItems;
            var view = new WPF.Views.MainWindow();
            view.DataContext = mVm;
            GenCommand.window = view;
            GenBoardCommand.window = view;
            view.ShowDialog();

            return true;
        }


        private static PanelItem GetPanelItems(PanelItem board, int parNumber)
        {
            board.ParentNumber = parNumber;
            parNumber += 1;
            foreach (var c in boards)
            {
                //если id элемента цепи не будет равен id щита и элемент будет щитом то:
                if (board.Id == c.ParentBoardId)
                {
                    //потомок
                    panelItem2 = GetPanelItems(c, parNumber); //рекурсия
                    board.SubPanels.Add(panelItem2);
                }   
            }
            return board;
        }

    }






    public class DicParentToChildren : Dictionary<ElementId, List<Element>>
    {
        //Заменяем метод ADD
        public void Add(ElementId parentId, Element child)
        {
            if (!this.ContainsKey(parentId)) //Проверяем, есть ли ключ с добавляемым значением
            {
                this.Add(parentId, new List<Element>()); //если не содержит, то добавляем ключ и создаем лист в value
            }
            if (null != child) //если дочернего элемента нет
            {
                this[parentId].Add(child); //Добавляет по значению ключа(к родительскому щиту) дочерний щит 
                //не создавая новый лист
            }
        }
    }


}
