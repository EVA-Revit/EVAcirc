using EVA_Gen.WPF.Infrastructure.Commands.Base;
using EVA_Gen.WPF.Models;
using EVA_Gen.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

namespace EVA_Gen.WPF.Infrastructure.Commands
{
    class GenCommand : CommandWPF
    {
        public override bool CanExecute(object parameter) => true;


        public static Document doc;
        public static UIDocument uidoc;
        public static PanelItem SelectedPanelCommand;

        //логика
        public override void Execute(object parameter)
        {
            //TaskDialog.Show("Testing", "Hello world");
            var view = GetDrawingsView(SelectedPanelCommand.Name);
            using (Transaction newTran = new Transaction(doc, "Изоляция элементов цепей"))
            {
                newTran.Start();


                if (view == null)
                {
                    var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                         .FirstOrDefault(x => x.ViewFamily == ViewFamily.Drafting);

                    //создание нового 3D вида
                    view = ViewDrafting.Create(doc, viewFamilyType.Id);
                    //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                    view.Name = SelectedPanelCommand.Name;
                }
                
                else
                {
                    for (int i = 1; i < 100; i++)
                    {
                        view = GetDrawingsView(SelectedPanelCommand.Name + " копия " + i.ToString());
                        if (view == null)
                        {
                            var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                                    .FirstOrDefault(x => x.ViewFamily == ViewFamily.Drafting);

                            //создание нового 3D вида
                            view = ViewDrafting.Create(doc, viewFamilyType.Id);
                            //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                            view.Name = SelectedPanelCommand.Name + " копия " + i.ToString();
                            break;
                        }

                    }
                   
                    
                }
                


                newTran.Commit();
                
            }
            
            uidoc.RequestViewChange(view);
            double step = Utilits.Ft(30);

            ElectricalSystem circBoard = null;
            IEnumerable<ElectricalSystem> circSortBoard = Utilits.GetSortedCircuits(SelectedPanelCommand.Rboard as FamilyInstance, out circBoard); //отсортированные цепи
            int nn = circSortBoard.Count();
            XYZ pt = new XYZ();
           


            //Получение семейств анотаций
            var fam_shina = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Шина").Family, "-")
                as FamilySymbol;

            var fam_othLine = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Отходящая_линия").Family, "-")
              as FamilySymbol;

            var fam_app = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Аппараты").Family, "-")
             as FamilySymbol;

            var fam_UGO = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_УГО").Family, "-")
             as FamilySymbol;

            //var fam_el = Utilits.FamType(Utilits.GetFamAn(FNt1.el).Family, "-")
            //    as FamilySymbol;



            //Создание
            using (Transaction newTran = new Transaction(doc, "GenerShems"))
            {
                newTran.Start();
                //Активация семейств
                if (!fam_shina.IsActive) fam_shina.Activate();
                if (!fam_othLine.IsActive) fam_othLine.Activate();
                if (!fam_app.IsActive) fam_app.Activate();
                if (!fam_UGO.IsActive) fam_UGO.Activate();


                FamilyInstance shina = doc.Create.NewFamilyInstance(pt, fam_shina, view);

                if (nn == 0) TaskDialog.Show("Отладка", "Кол-во груп равно 0");
                shina.LookupParameter("L_шины_EVA").Set(nn * step);


                for (int i = 1; i < nn + 1; i++)
                {


                    FamilyInstance othLine = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2 + pt.X, pt.Y, 0), fam_othLine, view);


                    //var circ = ElPanel.ElCircuetsList[i - 1];
                    //string str1Eva = circ.Name_Line + " " + circ.Cable_Mark_1 + " " + circ.Сable_S_1 + " " + circ.Сable_In_Tray + " " + circ.Сable_In_Pipe + circ.Pipe_D;

                    //string str2Eva = "Pp= " + circ.Calculated_Р + "кВт Ip= " + circ.Calculated_I + "A cosF= " + circ.COS;
                    //othLine.LookupParameter("Строка1_EVA").Set(str1Eva);
                    //othLine.LookupParameter("Строка2_EVA").Set(str2Eva);


                    //othLine.LookupParameter("Кол-во_фаз_EVA").Set(circ.Number_Of_Phase);
                    //othLine.LookupParameter("Фаза_EVA").Set(circ.Connection_Phase);
                    //othLine.LookupParameter("Длина_до_кабеля_EVA").Set(Utilits.Ft(50));
                    //othLine.LookupParameter("Длина_линии_до_УГО_EVA").Set(Utilits.Ft(60));



                    //FamilyInstance app = Doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2 + pt.X, pt.Y + Utilits.Ft(13), 0), fam_app, view);

                    //app.LookupParameter("Строка1_EVA").Set(circ.Device_Number_1);
                    //app.LookupParameter("Строка2_EVA").Set(circ.Device_Type_1);
                    //app.LookupParameter("Строка3_EVA").Set(circ.Device_1 + "A");

                    //app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(16));
                    //app.LookupParameter("Перемещение_по_Х_EVA").Set(Utilits.Ft(16));
                    //app.LookupParameter("QF_EVA").Set(1);


                    //FamilyInstance ugo = Doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2 + pt.X, pt.Y + Utilits.Ft(115.5), 0), fam_UGO, view);

                    //ugo.LookupParameter("Строка1_EVA").Set(circ.Load_Name);
                    //string pu = "Pу= " + circ.P_Installed + "кВт";
                    //ugo.LookupParameter("Строка2_EVA").Set(pu);
                    //ugo.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(25));
                    //ugo.LookupParameter("Перемещение_по_X_EVA").Set(Utilits.Ft(0));
                    //ugo.LookupParameter("ЩР_EVA").Set(1);
                    //ugo.LookupParameter("ТЭН_EVA").Set(0);
                }








                newTran.Commit();
            }
        }




        private static ViewDrafting GetDrawingsView(string userName)
        {
            var view = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).FirstOrDefault(x => x.Name == userName);

            return view as ViewDrafting;
        }
    }
}
