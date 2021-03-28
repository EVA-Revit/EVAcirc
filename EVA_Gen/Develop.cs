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

namespace EVA_Gen
{
    class Develop
    {
        public static Result SomeCode (ExternalCommandData commandData, ref string message)
        {
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
            
            var fg = new PanelItem();
            fg.Name = "Hello";
           
            var pop = new PanelItem();
            pop.Name = "Hello2";
            fg.Nodes.Add(pop);

            var df = new List<PanelItem>();
            df.Add(fg);

            var mVm = new MainWindowViewModel();
            mVm.Panels = df;
            var view = new WPF.Views.MainWindow();
            view.DataContext = mVm;
            view.ShowDialog();
            return true;
        }
    }
}
