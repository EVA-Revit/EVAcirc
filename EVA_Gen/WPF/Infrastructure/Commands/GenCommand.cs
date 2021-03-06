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

                PanelItem selectedPanel = item;
                window.Close();
                var view = Utilits.GetDrawingsView(selectedPanel.Name);
                using (Transaction newTran = new Transaction(doc, "Создание вида EVA"))
                {
                    newTran.Start();


                    if (view == null)
                    {
                        var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                             .FirstOrDefault(x => x.ViewFamily == ViewFamily.Drafting);

                        //создание нового 2D вида
                        view = ViewDrafting.Create(doc, viewFamilyType.Id);
                        //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                        view.Name = selectedPanel.Name;
                    }

                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            view = Utilits.GetDrawingsView(selectedPanel.Name + " копия " + i.ToString());
                            if (view == null)
                            {
                                var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                                        .FirstOrDefault(x => x.ViewFamily == ViewFamily.Drafting);

                                //создание нового  вида
                                view = ViewDrafting.Create(doc, viewFamilyType.Id);
                                //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                                view.Name = selectedPanel.Name + " копия " + i.ToString();
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
                List<CircItem> circItemsBoard = selectedPanel.Circuits.ToList();
                int nn = selectedPanel.CountGroup;
                XYZ pt = new XYZ();


                double y1 = 0;
                //проверка, есть ли 3ий аппарат
                bool ap3 = circItemsBoard.Where(x => x.Device_Type_3 != "No").Count() > 0;
                if(ap3) y1 = Utilits.Ft(25);

                //Получение семейств анотаций
                var fam_shina = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Шина").Family, "-")
                    as FamilySymbol;

                var fam_othLine = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Отходящая_линия").Family, "-")
                  as FamilySymbol;

                var fam_app = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Аппараты").Family, "-")
                 as FamilySymbol;

                var fam_UGO = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_УГО").Family, "-")
                 as FamilySymbol;

                var fam_othLine_add = Utilits.FamType(Utilits.GetFamAn("EVA_Панель_Отходящая_линия_Доп").Family, "-")
                 as FamilySymbol;

                var fam_legenda = Utilits.FamType(Utilits.GetFamAn("EVA_Легенда_Нагрузка").Family, "-")
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
                    if (!fam_othLine_add.IsActive) fam_othLine_add.Activate();
                    if (!fam_legenda.IsActive) fam_legenda.Activate();


                    FamilyInstance shina = doc.Create.NewFamilyInstance(pt, fam_shina, view);

                    if (nn == 0) TaskDialog.Show("Отладка", "Кол-во груп равно 0");
                    shina.LookupParameter("L_шины_EVA").Set(nn * step);
                    shina.LookupParameter("Проект_EVA").Set(selectedPanel.Proekt);
                    shina.LookupParameter("Имя_щита_EVA").Set(selectedPanel.Name);
                    shina.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id.ToString());

                    string numAppStr = "";
                    string panelName = selectedPanel.Name;
                    if (panelName.Contains("РП-"))
                    {

                        string rez = panelName.Substring(panelName.IndexOf("РП-") + 3);
                        numAppStr = rez.Split(' ')[0];
                        numAppStr = rez.Split('(')[0];
                        if (numAppStr.Contains("-")) numAppStr = numAppStr.Remove(numAppStr.IndexOf("-"));
                    }
                    else if (panelName.Contains("РП."))
                    {
                        string rez = panelName.Substring(panelName.IndexOf("РП.") + 3);
                        numAppStr = rez.Split(' ')[0];
                        numAppStr = rez.Split('(')[0];
                        if (numAppStr.Contains("-")) numAppStr = numAppStr.Remove(numAppStr.IndexOf("-"));
                    }
                    else if (panelName.Contains("РП"))
                    {
                        string rez = panelName.Substring(panelName.IndexOf("РП") + 2);
                        numAppStr = rez.Split(' ')[0];
                        numAppStr = rez.Split('(')[0];
                        if (numAppStr.Contains("-")) numAppStr = numAppStr.Remove(numAppStr.IndexOf("-"));
                    }

                    //legend
                    LegendBoard(pt, fam_legenda, view, selectedPanel);

                    





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

                        //TaskDialog.Show("adad", circ.Cable_type.ToString());

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
                            if(circ.Cable_S_1_1 != 0)
                            {
                                if (circ.Cable_S_1_1 == 1)
                                {
                                    str1 = circ.Cable_S_1_2 + " x " + circ.Cable_S_1_3;
                                }
                                else
                                {
                                    str1 = circ.Cable_S_1_1 + " x " + circ.Cable_S_1_2 + " x " + circ.Cable_S_1_3;
                                }
                            }
    
                            if (circ.Cable_S_2_1 != 0)
                            {
                                if (circ.Cable_S_2_1 == 1)
                                {
                                    str2 = circ.Cable_S_1_2 + " x " + circ.Cable_S_1_3;

                                }
                                else
                                {
                                    str2 = circ.Cable_S_1_1 + " x " + circ.Cable_S_1_2 + " x " + circ.Cable_S_1_3;
                                }
                                if (circ.Cable_S_2_2 == 1)
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

                            if(circ.P1 != 0) othLine.LookupParameter("Рр_EVA").Set(circ.P1_Visable);
                            if (circ.I1_Max != 0) othLine.LookupParameter("Iр_EVA").Set(circ.I1_Max_Visable);
                            if (circ.Cos != 0) othLine.LookupParameter("cos_EVA").Set(circ.Cos.ToString());
                            if (circ.Ik_End_Line != 0) othLine.LookupParameter("Однофазный_ток_КЗ_EVA").Set(circ.Ik_End_Line_Visable);
                            if (circ.Cable_Calculated_L != 0) othLine.LookupParameter("Длина_расч_EVA").Set(circ.Cable_Calculated_L.ToString());
                            if (circ.DU_Calculated != 0) othLine.LookupParameter("ΔU_EVA").Set(circ.DU_Calculated.ToString());
                            if (circ.Cable_L_1 != 0) othLine.LookupParameter("Длина_факт_1_EVA").Set(circ.Cable_L_1.ToString());
                            if (circ.Number_Of_Phase != 0) othLine.LookupParameter("Фаза_EVA").Set(circ.Phase_Connection);
                        

                            othLine.LookupParameter("Длина_до_кабеля_EVA").Set(Utilits.Ft(50) + y1);
                            othLine.LookupParameter("Длина_линии_до_УГО_EVA").Set(Utilits.Ft(75));

                            othLine.LookupParameter("Проект_EVA").Set(circ.Proekt);
                            othLine.LookupParameter("Имя_щита_EVA").Set(circ.PanelName);
                            othLine.LookupParameter("ID_IN_EVA").Set(circ.Id.ToString());
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

                        if (circ.Device_Type_1 != "No")
                        {
                            y = Utilits.Ft(13);

                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение
                            Utilits.SetParamApZ(app, circ, numAppStr, i, 1);
                            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(16));
                            if (circ.Device_Type_1 == "Wh+TT")
                            {
                                app = doc.Create.NewFamilyInstance(new XYZ((step * i), y, 0), fam_app, view);
                                //SetParameters
                                app.LookupParameter("Перемещение_по_Х_EVA").Set(Utilits.Ft(16));
                                Utilits.UseParamViewAppZ(app, "Wh");
                                //1
                                app.LookupParameter("Строка1_EVA").Set(numAppStr + "Wh" + i.ToString());
                                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Break, 1));

                                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Счетчик эл. энергии");
                                app.LookupParameter("Тип_марка_обозначение_документа_EVA").Set(circ.GetProp(Device.Break, 1));
                                app.LookupParameter("Проект_EVA ").Set(circ.Proekt);
                                app.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                app.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString()+"$" + i.ToString() + i.ToString());

                                //TODO: добавить параметры для спеки
                            }

                        }


                        if (circ.Device_Type_2 != "No")
                        {
                            y = Utilits.Ft(37);

                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение
                            Utilits.SetParamApZ(app, circ, numAppStr, i, 2);
                            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(16));
                            if (circ.Device_Type_2 == "Wh+TT")
                            {
                                app = doc.Create.NewFamilyInstance(new XYZ((step * i), y, 0), fam_app, view);
                                //SetParameters
                                app.LookupParameter("Перемещение_по_Х_EVA").Set(Utilits.Ft(16));
                                Utilits.UseParamViewAppZ(app, "Wh");
                                //1
                                app.LookupParameter("Строка1_EVA").Set(numAppStr + "Wh" + i.ToString());
                                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Break, 1));

                                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Счетчик эл. энергии");
                                app.LookupParameter("Тип_марка_обозначение_документа_EVA").Set(circ.GetProp(Device.Break, 1));
                                app.LookupParameter("Проект_EVA ").Set(circ.Proekt);
                                app.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                app.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString() + "$" + i.ToString() + i.ToString());

                                //TODO: добавить параметры для спеки
                            }
                        }

                        if (circ.Device_Type_3 != "No")
                        {
                            y = Utilits.Ft(61);


                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение

                            Utilits.SetParamApZ(app, circ, numAppStr, i, 3);
                            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(16));
                            if (circ.Device_Type_3 == "Wh+TT")
                            {
                                app = doc.Create.NewFamilyInstance(new XYZ((step * i), y, 0), fam_app, view);
                                //SetParameters
                                app.LookupParameter("Перемещение_по_Х_EVA").Set(Utilits.Ft(16));
                                Utilits.UseParamViewAppZ(app, "Wh");
                                //1
                                app.LookupParameter("Строка1_EVA").Set(numAppStr + "Wh" + i.ToString());
                                app.LookupParameter("Строка2_EVA").Set(circ.GetProp(Device.Break, 1));

                                app.LookupParameter("Наименование_и_техническая_характеристика_EVA").Set("Счетчик эл. энергии");
                                app.LookupParameter("Тип_марка_обозначение_документа_EVA").Set(circ.GetProp(Device.Break, 1));
                                app.LookupParameter("Проект_EVA ").Set(circ.Proekt);
                                app.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                app.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString() + "$" + i.ToString() + i.ToString());

                                //TODO: добавить параметры для спеки
                            }
                        }

                        //UGO
                        if (ap3) y = Utilits.Ft(155.5);
                        else y = Utilits.Ft(130.5);
                        FamilyInstance ugo = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_UGO, view);

                        ugo.LookupParameter("Проект_EVA").Set(circ.Proekt);
                        ugo.LookupParameter("Имя_щита_EVA").Set(circ.PanelName);
                        ugo.LookupParameter("ID_IN_EVA").Set(circ.Id.ToString());


                        //SelectedPanel.Name == "ЩУ"
                        //Если имеются отходящие линии 
                        if (circ.Out_Line_panel != null)
                        {
                            
                           
                            ugo.LookupParameter("Строка1_EVA").Set(panelName + ", " + "text1");
                            ugo.LookupParameter("Строка2_EVA").Set("text2");
                            ugo.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(6));
                            ugo.LookupParameter("Перемещение_по_X_EVA").Set(Utilits.Ft(10));
                            //установка видимости уго из параметра
                            //Utilits.UseParamViewUgo(ugo, circ.Ugo);
                            Utilits.UseParamViewUgo(ugo, "ЩУ");

                            double x1 = (step * i) - step / 2;
                            double y2 = 0;

                            var panelAdd = Develop.boards.FirstOrDefault(pe => pe.Name == circ.Out_Line_panel);

                            for (int i2 = 0; i2 < panelAdd.CountGroup; i2++)
                            {
                                if (panelAdd.CountGroup != 1)
                                {
                                    x1 = x1 - Utilits.Ft(10);
                                    y2 = Utilits.Ft(25);

                                }


                                str1 = "";
                                str2 = "";
                                //создание
                                FamilyInstance othLine_add = doc.Create.NewFamilyInstance(new XYZ(x1, Utilits.Ft(130.5) + y1 + y2, 0), fam_othLine_add, view);
                                FamilyInstance ugo2 = doc.Create.NewFamilyInstance(new XYZ(x1, Utilits.Ft(190.5) + y1 + y2, 0), fam_UGO, view);

                                othLine_add.LookupParameter("Проект_EVA ").Set(circ.Proekt);
                                othLine_add.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                othLine_add.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString());
                                ugo2.LookupParameter("Проект_EVA ").Set(circ.Proekt);
                                ugo2.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                ugo2.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString());


                                x1 = x1 + Utilits.Ft(30);

                                //назначение
                                var circAdd = panelAdd.Circuits[i2];



                                //if (circAdd.Cable_S_1_1 == 0)
                                //{

                                //}
                                //else
                                //{
                                //    if (circAdd.Cable_S_1_1 == 1)
                                //    {

                                //    }
                                //    else
                                //    {
                                //        str1 = circAdd.Cable_S_1_1 + "x" + circAdd.Cable_S_1_2 + "x" + circAdd.Cable_S_1_3;
                                //    }
                                //}
                                //if(circAdd.Cable_S_2_1 == 0)
                                //{

                                //}
                                if (circAdd.Load_Type == "Сигнал от") continue;
                                if (circAdd.Load_Type == "Резерв") continue;

                                if (circAdd.Cable_S_1_1 != 0)
                                {
                                    if (circAdd.Cable_S_1_1 == 1)
                                    {
                                        str1 = circAdd.Cable_S_1_2 + " x " + circAdd.Cable_S_1_3;
                                    }
                                    else
                                    {
                                        str1 = circAdd.Cable_S_1_1 + " x " + circAdd.Cable_S_1_2 + " x " + circAdd.Cable_S_1_3;
                                    }
                                }

                                if (circAdd.Cable_S_2_1 != 0)
                                {
                                    if (circAdd.Cable_S_2_1 == 1)
                                    {
                                        str2 = circAdd.Cable_S_1_2 + " x " + circAdd.Cable_S_1_3;

                                    }
                                    else
                                    {
                                        str2 = circAdd.Cable_S_1_1 + " x " + circAdd.Cable_S_1_2 + " x " + circAdd.Cable_S_1_3;
                                    }
                                    if (circAdd.Cable_S_2_2 == 1)
                                    {
                                        othLine_add.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1 + " + " + str2);
                                    }
                                    else
                                    {
                                        othLine_add.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1 + "/" + str2);
                                        if (circAdd.Cable_L_2 != 0) othLine_add.LookupParameter("Длина_факт_2_EVA").Set(circAdd.Cable_L_2);
                                    }
                                }
                                //назначение кабельной линии

                                othLine_add.LookupParameter("Название_кабельной_линии_EVA").Set(circAdd.Name);
                                othLine_add.LookupParameter("Марка_кабеля_EVA").Set(circAdd.Cable_Mark_1);
                                othLine_add.LookupParameter("Способ_прокладки_EVA").Set(circAdd.Cable_In_Tray_Pipe);





                                othLine_add.LookupParameter("Длина_трубы_EVA").Set(circAdd.Pipe_L.ToString());
                                othLine_add.LookupParameter("Число_жил_EVA").Set(circAdd.Number_Of_Phase);

                                if (circAdd.P1 != 0) othLine_add.LookupParameter("Рр_EVA").Set(circAdd.P1_Visable);
                                if (circAdd.I1_Max != 0) othLine_add.LookupParameter("Iр_EVA").Set(circAdd.I1_Max_Visable);
                                if (circAdd.Cos != 0) othLine_add.LookupParameter("cos_EVA").Set(circAdd.Cos.ToString());
                                if (circAdd.Ik_End_Line != 0) othLine_add.LookupParameter("Однофазный_ток_КЗ_EVA").Set(circAdd.Ik_End_Line_Visable);
                                if (circAdd.Cable_Calculated_L != 0) othLine_add.LookupParameter("Длина_расч_EVA").Set(circAdd.Cable_Calculated_L.ToString());
                                if (circAdd.DU_Calculated != 0) othLine_add.LookupParameter("ΔU_EVA").Set(circAdd.DU_Calculated.ToString());
                                if (circAdd.Cable_L_1 != 0) othLine_add.LookupParameter("Длина_факт_1_EVA").Set(circAdd.Cable_L_1.ToString());
                                if (circAdd.Number_Of_Phase != 0) othLine_add.LookupParameter("Фаза_EVA").Set(circAdd.Phase_Connection);


                                
                                othLine_add.LookupParameter("Длина_линии_до_УГО_EVA").Set(Utilits.Ft(60));

                                //назначение УГОдоп 
                                ugo2.LookupParameter("Строка1_EVA").Set(circAdd.Load_Name);
                                ugo2.LookupParameter("Строка2_EVA").Set("Py= " + circAdd.P_Visable + "кВт");
                                ugo2.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(25));
                                ugo2.LookupParameter("Перемещение_по_X_EVA").Set(0);


                                if (circAdd.Ugo != "No")
                                {
                                    //установка видимости уго из параметра
                                    Utilits.UseParamViewUgo(ugo2, circAdd.Ugo);
                                }
                                else
                                {

                                    Utilits.UseParamViewUgoInTypeLoad(ugo2, circAdd.Load_Type);
                                }
                            }



                        }
                        else
                        {
                            //if (ap3) y = Utilits.Ft(150.5);
                            //else y = Utilits.Ft(115.5);
                            //FamilyInstance ugo = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_UGO, view);

                            //Назначение
                            ugo.LookupParameter("Строка1_EVA").Set(circ.Load_Name);
                            ugo.LookupParameter("Строка2_EVA").Set("Py= " + circ.P_Visable + " кВт");
                            ugo.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(25));
                            ugo.LookupParameter("Перемещение_по_X_EVA").Set(0);


                            if (circ.Ugo != "No")
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

        public static void LegendBoard(XYZ pt, FamilySymbol fam_legenda,ViewDrafting view, PanelItem selectedPanel)
        {
            FamilyInstance legenda = doc.Create.NewFamilyInstance(new XYZ(pt.X, pt.Y - Utilits.Ft(50), 0), fam_legenda, view);

            //назначение легенды
            if (selectedPanel.P2W == selectedPanel.P2S)
            {
                legenda.LookupParameter("kс_EVA").Set(selectedPanel.K2W_Visable);
                legenda.LookupParameter("Рр_EVA").Set(selectedPanel.P2W_Visable);
                legenda.LookupParameter("Iр_EVA").Set(selectedPanel.I2W_max_Visable);
                legenda.LookupParameter("cos_EVA").Set(selectedPanel.cos2W_Visable);
                legenda.LookupParameter("Sр_EVA").Set(selectedPanel.S2W_Visable);
                legenda.LookupParameter("Ру_EVA").Set(selectedPanel.P_Visable);
                legenda.LookupParameter("Имя_Панели_EVA").Set(selectedPanel.Name);
                
                legenda.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id + "Р");

                if (selectedPanel.P2W != selectedPanel.P2SF)
                {
                    legenda.LookupParameter("Режим_EVA").Set("Рабочий режим");
                }
                else return;
            }
            else
            {
                legenda.LookupParameter("kс_EVA").Set(selectedPanel.K2S_Visable);
                legenda.LookupParameter("Рр_EVA").Set(selectedPanel.P2S_Visable);
                legenda.LookupParameter("Iр_EVA").Set(selectedPanel.I2S_max_Visable);
                legenda.LookupParameter("cos_EVA").Set(selectedPanel.cos2S_Visable);
                legenda.LookupParameter("Sр_EVA").Set(selectedPanel.S2S_Visable);
                legenda.LookupParameter("Ру_EVA").Set(selectedPanel.P_Visable);
                legenda.LookupParameter("Имя_Панели_EVA").Set(selectedPanel.Name);
                legenda.LookupParameter("Режим_EVA").Set("Рабочий режим(лето)");
                legenda.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id + "Р_Л");


                legenda = doc.Create.NewFamilyInstance(new XYZ(pt.X, pt.Y - Utilits.Ft(100), 0), fam_legenda, view);

                legenda.LookupParameter("kс_EVA").Set(selectedPanel.K2W_Visable);
                legenda.LookupParameter("Рр_EVA").Set(selectedPanel.P2W_Visable);
                legenda.LookupParameter("Iр_EVA").Set(selectedPanel.I2W_max_Visable);
                legenda.LookupParameter("cos_EVA").Set(selectedPanel.cos2W_Visable);
                legenda.LookupParameter("Sр_EVA").Set(selectedPanel.S2W_Visable);
                legenda.LookupParameter("Ру_EVA").Set(selectedPanel.P_Visable);
                legenda.LookupParameter("Имя_Панели_EVA").Set(selectedPanel.Name);
                legenda.LookupParameter("Режим_EVA").Set("Рабочий режим(зима)");
                legenda.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id + "Р_З");

                if (selectedPanel.P2S == selectedPanel.P2SF) return;
            }
            
            
            legenda = doc.Create.NewFamilyInstance(new XYZ(pt.X + Utilits.Ft(50), pt.Y - Utilits.Ft(50), 0), fam_legenda, view);

            if (selectedPanel.P2WF != selectedPanel.P2SF)
            {
                legenda.LookupParameter("kс_EVA").Set(selectedPanel.K2WF_Visable);
                legenda.LookupParameter("Рр_EVA").Set(selectedPanel.P2WF_Visable);
                legenda.LookupParameter("Iр_EVA").Set(selectedPanel.I2WF_max_Visable);
                legenda.LookupParameter("cos_EVA").Set(selectedPanel.cos2WF_Visable);
                legenda.LookupParameter("Sр_EVA").Set(selectedPanel.S2WF_Visable);
                legenda.LookupParameter("Ру_EVA").Set(selectedPanel.P_Visable);
                legenda.LookupParameter("Имя_Панели_EVA").Set(selectedPanel.Name);
                legenda.LookupParameter("Режим_EVA").Set("Аварийный режим");
                legenda.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id + "А");
                return;
            }
            else
            {
                legenda.LookupParameter("kс_EVA").Set(selectedPanel.K2SF_Visable);
                legenda.LookupParameter("Рр_EVA").Set(selectedPanel.P2SF_Visable);
                legenda.LookupParameter("Iр_EVA").Set(selectedPanel.I2SF_max_Visable);
                legenda.LookupParameter("cos_EVA").Set(selectedPanel.cos2SF_Visable);
                legenda.LookupParameter("Sр_EVA").Set(selectedPanel.S2SF_Visable);
                legenda.LookupParameter("Ру_EVA").Set(selectedPanel.P_Visable);
                legenda.LookupParameter("Имя_Панели_EVA").Set(selectedPanel.Name);
                legenda.LookupParameter("Режим_EVA").Set("Аварийный режим(лето)");
                legenda.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id + "А_Л");

                legenda = doc.Create.NewFamilyInstance(new XYZ(pt.X + Utilits.Ft(150), pt.Y - Utilits.Ft(100), 0), fam_legenda, view);

                legenda.LookupParameter("kс_EVA").Set(selectedPanel.K2WF_Visable);
                legenda.LookupParameter("Рр_EVA").Set(selectedPanel.P2WF_Visable);
                legenda.LookupParameter("Iр_EVA").Set(selectedPanel.I2WF_max_Visable);
                legenda.LookupParameter("cos_EVA").Set(selectedPanel.cos2WF_Visable);
                legenda.LookupParameter("Sр_EVA").Set(selectedPanel.S2WF_Visable);
                legenda.LookupParameter("Ру_EVA").Set(selectedPanel.P_Visable);
                legenda.LookupParameter("Имя_Панели_EVA").Set(selectedPanel.Name);
                legenda.LookupParameter("Режим_EVA").Set("Аварийный режим(зима)");
                legenda.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id + "А_З");
            }
        }
    }
}
