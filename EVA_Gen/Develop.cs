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

            var view = new WPF.Views.MainWindow();
            view.ShowDialog();
            return true;
        }
    }
}
