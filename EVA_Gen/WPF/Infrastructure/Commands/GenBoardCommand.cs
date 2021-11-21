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
    class GenBoardCommand : CommandWPF
    {
        public static Document doc;
        public static UIDocument uidoc;
        public static MainWindow window;
        public override bool CanExecute(object parameter)
        {
            if (SelectedPanels.Panels.Count > 0) return true;
            return false;
        }

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
                        view.Scale = 1;
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

                                //создание копии нового  вида
                                view = ViewDrafting.Create(doc, viewFamilyType.Id);
                                //view= View3D.CreateIsometric(doc, viewFamilyType.Id);
                                view.Name = selectedPanel.Name + " копия " + i.ToString();
                                view.Scale = 1;
                                break;
                            }

                        }


                    }



                    newTran.Commit();

                }



                uidoc.RequestViewChange(view);
                double step = Utilits.Ft(30);

                List<CircItem> circItemsBoard = selectedPanel.Circuits.ToList();
                int nn = selectedPanel.CountGroup;
                XYZ pt = new XYZ();

                double y1 = 0;
                //проверка, есть ли 3ий аппарат
                bool ap3 = circItemsBoard.Where(x => x.Device_Type_3 != "No").Count() > 0;
                if (ap3) y1 = Utilits.Ft(15);

                //Получение семейств анотаций
                var fam_shina = Utilits.FamType(Utilits.GetFamAn("EVA_Щит_Шина").Family, "-")
                    as FamilySymbol;

                var fam_othLine = Utilits.FamType(Utilits.GetFamAn("EVA_Щит_Отходящая_линия").Family, "-")
                  as FamilySymbol;

                var fam_app = Utilits.FamType(Utilits.GetFamAn("EVA_Щит_Аппараты").Family, "-")
                 as FamilySymbol;

                var fam_zagolovok = Utilits.FamType(Utilits.GetFamAn("EVA_Щит_Заголовки").Family, "-")
                 as FamilySymbol;

                var fam_legenda = Utilits.FamType(Utilits.GetFamAn("EVA_Легенда_Нагрузка").Family, "-")
                as FamilySymbol;

                //var fam_UGO = Utilits.FamType(Utilits.GetFamAn("EVA_Щит_УГО").Family, "-")
                // as FamilySymbol;



                //Создание
                using (Transaction newTran = new Transaction(doc, "GenerShemsBoard"))
                {
                    newTran.Start();
                    //Активация семейств
                    if (!fam_shina.IsActive) fam_shina.Activate();
                    if (!fam_othLine.IsActive) fam_othLine.Activate();
                    if (!fam_app.IsActive) fam_app.Activate();
                    if (!fam_zagolovok.IsActive) fam_zagolovok.Activate();
                    if (!fam_legenda.IsActive) fam_legenda.Activate();
                    //if (!fam_UGO.IsActive) fam_UGO.Activate();


                    FamilyInstance zagolovok = doc.Create.NewFamilyInstance(pt, fam_zagolovok, view);
                    zagolovok.LookupParameter("Длина_до_кабеля_EVA").Set((Utilits.Ft(60) + y1));
                    //zagolovok.LookupParameter("Проект_EVA").Set(selectedPanel.Proekt);
                    //zagolovok.LookupParameter("Имя_щита_EVA").Set(selectedPanel.Name);
                    //zagolovok.LookupParameter("ID_IN_EVA").Set(selectedPanel.Id.ToString());

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
                    XYZ ptLegend = new XYZ(pt.X + Utilits.Ft(150), pt.Y + Utilits.Ft(150), pt.Z);
                    GenCommand.LegendBoard(ptLegend, fam_legenda, view, selectedPanel);

                    for (int i = 1; i < nn + 1; i++)
                    {

                        double y;
                        double x;
                        string str1 = "";
                        string str2 = "";


                        FamilyInstance othLine = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2 + pt.X, pt.Y, 0), fam_othLine, view);

                        var circ = circItemsBoard[i - 1];
                        if (circ.Rez) //если цепь резервная
                        {
                            othLine.LookupParameter("Резерв_EVA").Set(1);

                            //othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Load_Name);
                            continue;

                        }

                        if (circ.Load_Type == "Сигнал от")
                        {
                            othLine.LookupParameter("Реле_EVA").Set(1);
                            othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Load_Name);
                        }
                        else if (circ.Load_Type == "Резерв")
                        {
                            othLine.LookupParameter("Резерв_EVA").Set(1);
                            othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Load_Name);
                        }
                        else
                        {
                            if (circ.Cable_S_1_1 != 0)
                            {
                                if (circ.Cable_S_1_1 == 1)
                                {
                                    str1 = circ.Cable_S_1_2 + "x" + circ.Cable_S_1_3;
                                }
                                else
                                {
                                    str1 = circ.Cable_S_1_1 + "x" + circ.Cable_S_1_2 + "x" + circ.Cable_S_1_3;
                                }
                                
                            }

                            if (circ.Cable_S_2_1 != 0)
                            {
                                if (circ.Cable_S_2_1 == 1)
                                {
                                    str2 = circ.Cable_S_2_2 + "x" + circ.Cable_S_2_3;

                                }
                                else
                                {
                                    str2 = circ.Cable_S_2_1 + "x" + circ.Cable_S_2_2 + "x" + circ.Cable_S_2_3;
                                }
                                if (circ.Cable_S_2_2 == 1)
                                {
                                    othLine.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1 + "+" + str2);
                                }
                                else
                                {
                                    othLine.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1 + "/" + str2);
                                    if (circ.Cable_L_2 != 0) othLine.LookupParameter("Длина_факт_2_EVA").Set(circ.Cable_L_2.ToString());
                                }
                            }
                            else
                            {
                                othLine.LookupParameter("Кол-во_жил_сечение_EVA").Set(str1);
                            }


                            //назначение кабельной линии



                            othLine.LookupParameter("Название_кабельной_линии_EVA").Set(circ.Name);
                            othLine.LookupParameter("Марка_кабеля_EVA").Set(circ.Cable_Mark);
                            othLine.LookupParameter("Способ_прокладки_EVA").Set(circ.Cable_In_Tray_Pipe);





                            if (circ.Pipe_L != 0) othLine.LookupParameter("Длина_трубы_EVA").Set(circ.Pipe_L.ToString());
                            othLine.LookupParameter("Число_жил_EVA").Set(circ.Number_Of_Phase);

                            if (circ.P1 != 0) othLine.LookupParameter("Рр_EVA").Set(circ.P1_Visable);
                            if (circ.I1_Max != 0) othLine.LookupParameter("Iр_EVA").Set(circ.I1_Max_Visable);
                            if (circ.Cos != 0) othLine.LookupParameter("cos_EVA").Set(circ.Cos_Visable);
                            if (circ.Ik_End_Line != 0) othLine.LookupParameter("Однофазный_ток_КЗ_EVA").Set(circ.Ik_End_Line_Visable);
                            if (circ.Cable_Calculated_L != 0) othLine.LookupParameter("Длина_расч_EVA").Set(circ.Cable_Calculated_L.ToString());
                            if (circ.DU_Calculated != 0) othLine.LookupParameter("ΔU_EVA").Set(circ.DU_Calculated.ToString());
                            if (circ.Cable_L_1 != 0) othLine.LookupParameter("Длина_факт_1_EVA").Set(circ.Cable_L_1.ToString());
                            if (circ.Number_Of_Phase != 0) othLine.LookupParameter("Фаза_EVA").Set(circ.Phase_Connection);


                            othLine.LookupParameter("Длина_до_кабеля_EVA").Set(Utilits.Ft(60) + y1);
                            othLine.LookupParameter("Длина_линии_до_УГО_EVA").Set(Utilits.Ft(70));

                            othLine.LookupParameter("Ширина_таблицы_EVA").Set(step);

                            othLine.LookupParameter("Проект_EVA").Set(circ.Proekt);
                            othLine.LookupParameter("Имя_щита_EVA").Set(circ.PanelName);
                            othLine.LookupParameter("ID_IN_EVA").Set(circ.Id.ToString());

                            othLine.LookupParameter("Имя_потребителя_EVA").Set(circ.Load_Name);


                        }

                        if (circ.Device_Type_1 != "No")
                        {
                            y = Utilits.Ft(-16);

                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение
                            Utilits.SetParamApZ(app, circ, numAppStr, i, 1);
                            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(9));
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
                                app.LookupParameter("Проект_EVA").Set(circ.Proekt);
                                app.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                app.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString() + "$" + i.ToString() + i.ToString());

                                //TODO: добавить параметры для спеки
                            }
                        }
                        if (circ.Device_Type_2 != "No")
                        {
                            y = Utilits.Ft(-40);

                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение
                            Utilits.SetParamApZ(app, circ, numAppStr, i, 2);
                            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(9));
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
                                app.LookupParameter("Проект_EVA").Set(circ.Proekt);
                                app.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                app.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString() + "$" + i.ToString() + i.ToString());

                                //TODO: добавить параметры для спеки
                            }
                        }

                        if (circ.Device_Type_3 != "No")
                        {
                            y = Utilits.Ft(-64);


                            FamilyInstance app = doc.Create.NewFamilyInstance(new XYZ((step * i) - step / 2, y, 0), fam_app, view);

                            //Назначение

                            Utilits.SetParamApZ(app, circ, numAppStr, i, 3);
                            app.LookupParameter("Перемещение_по_Y_EVA").Set(Utilits.Ft(9));
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
                                app.LookupParameter("Проект_EVA").Set(circ.Proekt);
                                app.LookupParameter("Имя_щита_EVA ").Set(circ.PanelName);
                                app.LookupParameter("ID_IN_EVA ").Set(circ.Id.ToString() + "$" + i.ToString() + i.ToString());

                                //TODO: добавить параметры для спеки
                            }
                        }


                        //УГО
                        if (circ.Ugo != "No")
                        {
                            //установка видимости уго из параметра
                            Utilits.UseParamViewUgo(othLine, circ.Ugo);
                        }
                        else
                        {

                            Utilits.UseParamViewUgoInTypeLoad(othLine, circ.Load_Type);
                        }



                    }









                    newTran.Commit();



                }




            }


        }


    }
}
