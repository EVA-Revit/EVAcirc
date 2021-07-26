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
            //выполнение расчетов
            //сортировка щитов
            var calcBoardsList = boards.OrderByDescending(x => x.ParentNumber);
            


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
