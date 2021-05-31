using EVA_Gen.WPF.Infrastructure.Commands.Base;
using EVA_Gen.WPF.Models;
using EVA_Gen.WPF.ViewModels;
using EVA_Gen.WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

namespace EVA_Gen.WPF.Infrastructure.Commands
{
    class GenCommand : CommandWPF
    {
        public static Document doc;
        public static UIDocument uidoc;
        public static MainWindow window;
        //public static PanelItem SelectedPanel { get; set; }



        public override bool CanExecute(object parameter)
        {
            if (SelectedPanels.Panels.Count > 0) return true;
            return false;
        }

        //private ObservableCollection<PanelItem> ChildPanel(PanelItem pi )
       


        //логика
        public override void Execute(object parameter)
        {
            

            foreach (var item in SelectedPanels.Panels)
            {

                PanelItem SelectedPanel = item;
                window.Close();
                var view = GetDrawingsView(SelectedPanel.Name);
                using (Transaction newTran = new Transaction(doc, "Изоляция элементов цепей"))
                {
                    newTran.Start();


                    if (view == null)
                    {
                        var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                             .FirstOrDefault(x => x.ViewFamily == ViewFamily.Drafting);

                        //создание нового 2D вида
                        view = ViewDrafting.Create(doc, viewFamilyType.Id);
                        //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                        view.Name = SelectedPanel.Name;
                    }

                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            view = GetDrawingsView(SelectedPanel.Name + " копия " + i.ToString());
                            if (view == null)
                            {
                                var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                                        .FirstOrDefault(x => x.ViewFamily == ViewFamily.Drafting);

                                //создание нового 3D вида
                                view = ViewDrafting.Create(doc, viewFamilyType.Id);
                                //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                                view.Name = SelectedPanel.Name + " копия " + i.ToString();
                                break;
                            }

                        }


                    }



                    newTran.Commit();

                }

                uidoc.RequestViewChange(view);
                double step = Utilits.Ft(30);

                //ElectricalSystem circBoard = null;
                //List<ElectricalSystem> circSortBoard = Utilits.GetSortedCircuits(SelectedPanelCommand.Rboard as FamilyInstance, out circBoard).ToList(); //отсортированные цепи
                List<CircItem> circItemsBoard = SelectedPanel.Circuits.ToList();
                int nn = SelectedPanel.CountGroup;
                XYZ pt = new XYZ();


                double d1 = 0;
                //проверка, есть ли 3ий аппарат
                bool ap3 = circItemsBoard.Where(x => x.Device_Type_3 != "(нет)").Count() > 0;
                if(ap3) d1 = Utilits.Ft(20);

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

                    string numAppStr = "";
                    string panelName = SelectedPanel.Name;
                    if (panelName.Contains("РП-"))
                    {
                        string rez = panelName.Substring(panelName.IndexOf("РП-") + 3);
                        numAppStr = rez.Split(' ')[0];
                        numAppStr = rez.Split('(')[0];
                        if (numAppStr.Contains("-")) numAppStr = numAppStr.Remove(numAppStr.IndexOf("-"));
                    }





                    for (int i = 1; i < nn + 1; i++)
                    {

                        double y;
                        double x;
                        string str1 = "";
                        string str2 = "";


                        FamilyInstance othLine = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2 + pt.X, pt.Y, 0), fam_othLine, view);

                        var circ = circItemsBoard[i - 1];
                        if(circ.Rez) //если цепь резервная
                        {
                            othLine.LookupParameter("Резерв_EVA").Set(1);
                            //othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Load_Name);
                            continue;
                        }



                        //назначение
                        if (circ.Load_Type == "Сигнал от") 
                        {
                            othLine.LookupParameter("Реле_EVA").Set(1);
                            othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Load_Name);
                        }
                        else if  (circ.Load_Type == "Резерв") 
                        {
                            othLine.LookupParameter("Резерв_EVA").Set(1);
                            othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Load_Name);
                        }
                        else
                        {
                            if(circ.Сable_S_1_1 != 0)
                            {
                                if (circ.Сable_S_1_1 == 1)
                                {
                                    str1 = circ.Сable_S_1_2 + " x " + circ.Сable_S_1_3;
                                }
                                else
                                {
                                    str1 = circ.Сable_S_1_1 + " x " + circ.Сable_S_1_2 + " x " + circ.Сable_S_1_3;
                                }
                            }
    
                            if (circ.Сable_S_2_1 != 0)
                            {
                                if (circ.Сable_S_2_1 == 1)
                                {
                                    str2 = circ.Сable_S_1_2 + " x " + circ.Сable_S_1_3;

                                }
                                else
                                {
                                    str2 = circ.Сable_S_1_1 + " x " + circ.Сable_S_1_2 + " x " + circ.Сable_S_1_3;
                                }
                                if (circ.Сable_S_2_2 == 1)
                                {
                                    othLine.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1 + " + " + str2);
                                }
                                else
                                {
                                    othLine.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1 + "/" + str2);
                                    if (circ.Cable_L_2 != 0) othLine.LookupParameter("Длина_факт_2_EVA").Set(circ.Cable_L_2);
                                }
                            }

                            //назначение кабельной линии

                            othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Name);
                            othLine.LookupParameter("Марка_кабеля_EVA").Set(circ.Cable_Mark_1);
                            othLine.LookupParameter("Способ_прокладки_EVA").Set(circ.Cable_In_Tray_Pipe);





                            othLine.LookupParameter("Длина_трубы_EVA").Set(circ.Pipe_L.ToString());
                            othLine.LookupParameter("Число_жил_EVA").Set(circ.Number_Of_Phase);




                            //othLine.LookupParameter("Кол-во_жил_сечение_1_EVA").Set(circ.Сable_S_1_1);
                            //othLine.LookupParameter("Кол-во_жил_сечение_2_EVA").Set(circ.Сable_S__1);

                            //othLine.LookupParameter("Рр_EVA").Set(circ.P1_Calculated.ToString());
                            //othLine.LookupParameter("Iр_EVA").Set(circ.I1_Calculated.ToString());
                            //othLine.LookupParameter("cos_EVA").Set(circ.Cos.ToString());
                            //othLine.LookupParameter("Однофазный_ток_КЗ_EVA").Set(circ.Ik_End_Line.ToString());
                            //othLine.LookupParameter("Длина_расч_EVA").Set(circ.Cable_Calculated_L.ToString());
                            //othLine.LookupParameter("ΔU_EVA").Set(circ.DU_Calculated.ToString());
                            //othLine.LookupParameter("Длина_факт_1_EVA").Set(circ.Cable_L_1.ToString());
                            //othLine.LookupParameter("Длина_факт_2_EVA").Set(circ.Cable_L_2.ToString());


                            //othLine.LookupParameter("Фаза_EVA").Set(circ.Phase_Connection);


                            othLine.LookupParameter("Длина_до_кабеля_EVA").Set(Utilits.Ft(50) + d1);
                            othLine.LookupParameter("Длина_линии_до_УГО_EVA").Set(Utilits.Ft(60));
                        }





                        //назначение
                        //othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Name);
                        //othLine.LookupParameter("Марка_кабеля_EVA").Set(circ.Cable_Mark_1);
                        //othLine.LookupParameter("Кол-во_жил_сечение_1_EVA").Set(circ.Сable_S_1_1);
                        //othLine.LookupParameter("Кол-во_жил_сечение_2_EVA").Set(circ.Сable_S__1);
                        //othLine.LookupParameter("Способ_прокладки_EVA").Set(circ.Cable_In_Tray_Pipe);
                        //othLine.LookupParameter("Рр_EVA").Set(circ.P1_Calculated.ToString());
                        //othLine.LookupParameter("Iр_EVA").Set(circ.I1_Calculated.ToString());
                        //othLine.LookupParameter("cos_EVA").Set(circ.Cos.ToString());
                        //othLine.LookupParameter("Однофазный_ток_КЗ_EVA").Set(circ.Ik_End_Line.ToString());
                        //othLine.LookupParameter("Длина_расч_EVA").Set(circ.Cable_Calculated_L.ToString());
                        //othLine.LookupParameter("ΔU_EVA").Set(circ.DU_Calculated.ToString());
                        //othLine.LookupParameter("Длина_факт_1_EVA").Set(circ.Cable_L_1.ToString());
                        //othLine.LookupParameter("Длина_факт_2_EVA").Set(circ.Cable_L_2.ToString());
                        //othLine.LookupParameter("Длина_трубы_EVA").Set(circ.Pipe_L.ToString());
                        //othLine.LookupParameter("Число_жил_EVA").Set(circ.Number_Of_Phase);
                        //othLine.LookupParameter("Фаза_EVA").Set(circ.Phase_Connection);

                        if (circ.Device_Type_1 != "(нет)")
                        {
                            y = Utilits.Ft(13);

                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение
                            SetParamApZ(app, circ, numAppStr, i, 1);
                        }


                        if (circ.Device_Type_2 != "(нет)")
                        {
                            y = Utilits.Ft(37);

                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение
                            SetParamApZ(app, circ, numAppStr, i, 2);

                        }

                        if (circ.Device_Type_3 != "(нет)")
                        {
                            y = Utilits.Ft(61);


                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение

                            SetParamApZ(app, circ, numAppStr, i, 3);

                        }

                        //UGO

                        if (circ.Load_Type == "Вентиляция противодымная" && SelectedPanel.Name == "ЩУ")
                        {

                        }
                        else
                        {
                            if (ap3) y = Utilits.Ft(150.5);
                            else y = Utilits.Ft(115.5);
                            FamilyInstance ugo = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_UGO, view);

                            //Назначение
                            ugo.LookupParameter("Строка1_EVA").Set(circ.Load_Name);
                            ugo.LookupParameter("Строка2_EVA").Set("Py= " + circ.P_Installed + "кВт");
                            ugo.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(25));
                            ugo.LookupParameter("Перемещение_по_X_EVA").Set(0);


                            if (circ.Ugo != "(нет)")
                            {
                                //установка видимости уго из параметра
                                Utilits.UseParamViewUgo(ugo, circ.Ugo);
                            }
                            else
                            {

                                Utilits.UseParamViewUgoInTypeLoad(ugo, circ.Load_Type);
                            }
                        }



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


          
           
        }




        private static ViewDrafting GetDrawingsView(string userName)
        {
            var view = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).FirstOrDefault(x => x.Name == userName);

            return view as ViewDrafting;
        }


        private static void SetParamApZ(FamilyInstance app, CircItem circ, string numApStg, int i, int order)
        {
            //Назначение
            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(16));
            app.LookupParameter("Перемещение_по_Х_EVA").Set(Utilits.Ft(16));

            //if (circ.Device_Type_1 == "QF")
            if (circ.GetProp(Device.Type, order) == "QF")
            {
                Utilits.UseParamViewAppZ(app, "QF_EVA");
                //1
                app.LookupParameter("Строка1_EVA").Set(numApStg + "QF" + i.ToString());

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + circ.Number_Of_Phase.ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

                if (circ.GetProp(Device.Body, order) != "0")
                {
                    app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "А");
                }

                


            }

            else if (circ.GetProp(Device.Type, order) == "QF+Н.Р.")
            {
                Utilits.UseParamViewAppZ(app, "QF+Н.Р._EVA");
                //1
                app.LookupParameter("Строка1_EVA").Set(numApStg + "QF" + i.ToString());

                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + circ.Number_Of_Phase.ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

                if (circ.GetProp(Device.Body, order) != "0")
                {
                    app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "А");
                }

                
            }

            else if (circ.GetProp(Device.Type, order) == "QFD")
            {
                Utilits.UseParamViewAppZ(app, "QFD_EVA");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QFD" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + (circ.Number_Of_Phase + 1).ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "мА");
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

            }

            else if (circ.GetProp(Device.Type, order) == "QFD+Н.Р.")
            {
                Utilits.UseParamViewAppZ(app, "QFD+Н.Р._EVA");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QFD" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + (circ.Number_Of_Phase + 1).ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A " + circ.GetProp(Device.Curve, order));
                app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Body, order) + "мА");
                app.LookupParameter("Строка5_EVA").Set(circ.GetProp(Device.Break, order) + "кА");

            }

            else if (circ.GetProp(Device.Type, order) == "FU")
            {
                Utilits.UseParamViewAppZ(app, "FU_EVA");


                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order));
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                if (circ.Number_Of_Phase == 3)
                {
                    app.LookupParameter("Строка1_EVA").Set(numApStg + "FU" + i.ToString() + ".1 " + numApStg + "FU" +
                        i.ToString() + ".3");
                }
                else app.LookupParameter("Строка1_EVA").Set(numApStg + "FU" + i.ToString());
            }

            else if (circ.GetProp(Device.Type, order) == "QD")
            {
                Utilits.UseParamViewAppZ(app, "QD_EVA");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QD" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + (circ.Number_Of_Phase + 1).ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
                app.LookupParameter("Строка4_EVA").Set(circ.GetProp(Device.Break, order) + "мА");

            }


            else if (circ.GetProp(Device.Type, order) == "QS")
            {
                Utilits.UseParamViewAppZ(app, "QS_EVA");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "QS" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + circ.Number_Of_Phase.ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
            }

            else if (circ.GetProp(Device.Type, order) == "KM")
            {
                Utilits.UseParamViewAppZ(app, "КМ_EVA");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "KM" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + circ.Number_Of_Phase.ToString() + "P");
                app.LookupParameter("Строка3_EVA").Set(circ.GetProp(Device.I, order) + "A");
            }


            else if (circ.GetProp(Device.Type, order) == "Wh")
            {
                Utilits.UseParamViewAppZ(app, "Wh_EVA");

                app.LookupParameter("Строка1_EVA").Set(numApStg + "Wh" + i.ToString());
                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Mark, order) + " " + circ.Number_Of_Phase.ToString() + "P");

            }
        }

    }
}
