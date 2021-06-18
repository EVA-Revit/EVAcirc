using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System.Windows;

namespace EVA_Gen.WPF.Models
{
    internal class ElectricalInfo
    {
        public string Name { get; set; }
        public int P_ust { get; set; }
        public string P_r { get; set; }

        public int elId { get; set; }

        public Element Element { get; set; }
        public ElementId ParentBoardId { get; set; }
        public ElementId Id { get; set; }

    }

    internal class ElementItem : ElectricalInfo
    {



    }


    internal class CircItem : ElectricalInfo
    {
        //public static string DeviceMark = "Марка_аппарата";
        //public static string DeviceType = "Т_аппарата";

        public bool Rez { get; set; }
        public string AppZ { get; set; }
        public int Length { get; set; }
        //public string Cable_Mark_2 { get; set; }
        public string Cable_Mark_1 { get; set; }
        public string Cable_In_Tray_Pipe { get; set; }
        public int Сable_S_1_1 { get; set; }
        public int Сable_S_1_2 { get; set; }
        public double Сable_S_1_3 { get; set; }
        public int Сable_S_2_1 { get; set; }
        public int Сable_S_2_2 { get; set; }
        public double Сable_S_2_3 { get; set; }

        public double Pipe_L { get; set; }
        public double P1_Calculated { get; set; }
        public double P_Installed { get; set; }
        public double I1_Calculated { get; set; }
        public double Cos { get; set; }
        public double Ik_End_Line { get; set; }
        public double Cable_Calculated_L { get; set; }
        public double DU_Calculated { get; set; }
        public double Cable_L_1 { get; set; }
        public double Cable_L_2 { get; set; }
        public int Number_Of_Phase { get; set; }
        public string Phase_Connection { get; set; }
        public string Device_Type_1 { get; set; }
        public string Device_Type_2 { get; set; }
        public string Device_Type_3 { get; set; }
        public string Device_Mark_1 { get; set; }
        public string Device_Mark_2 { get; set; }
        public string Device_Mark_3 { get; set; }
        public double Device_I_1 { get; set; }
        public double Device_I_2 { get; set; }
        public double Device_I_3 { get; set; }
        public string Device_Curve_1 { get; set; }
        public string Device_Curve_2 { get; set; }
        public string Device_Curve_3 { get; set; }
        public double Device_Break_1 { get; set; }
        public double Device_Break_2 { get; set; }
        public double Device_Break_3 { get; set; }
        public double Device_dI_bodyI_1 { get; set; }
        public double Device_dI_bodyI_2 { get; set; }
        public double Device_dI_bodyI_3 { get; set; }
        public string Load_Type { get; set; }
        public string Load_Name { get; set; }
        public string Ugo { get; set; }

        public List<ElementItem> ElementList { get; set; }

        public CircItem(ElectricalSystem rCirc)
        {
            //заполнение свойств
            if(rCirc.CircuitType == CircuitType.Circuit)
            {
                Name = rCirc.LookupParameter("Имя_цепи_EVA").AsString();
                Rez = false;
                Id = rCirc.Id;
                //Cable_Mark_2 = rCirc.LookupParameter("Марка_кабеля_2_EVA").AsString();
                //Cable_Mark_1 = rCirc.LookupParameter("Марка_кабеля_1_EVA").AsString();
                Cable_In_Tray_Pipe = rCirc.LookupParameter("Способ_прокладки_EVA").AsString();

                Сable_S_1_1 = rCirc.LookupParameter("Сечение_кабеля_1_1_EVA").AsInteger();
                Сable_S_1_2 = rCirc.LookupParameter("Сечение_кабеля_1_2_EVA").AsInteger();
                //Сable_S_1_3 = rCirc.LookupParameter("Сечение_кабеля_1_3_EVA").AsDouble();
                Сable_S_2_1 = rCirc.LookupParameter("Сечение_кабеля_2_1_EVA").AsInteger();
                Сable_S_2_2 = rCirc.LookupParameter("Сечение_кабеля_2_2_EVA").AsInteger();
                Сable_S_2_3 = rCirc.LookupParameter("Сечение_кабеля_2_3_EVA").AsDouble();

                Pipe_L = rCirc.LookupParameter("L_трубы_EVA").AsDouble();
                P1_Calculated = rCirc.LookupParameter("Pр_отх_линии_EVA").AsDouble();
                P_Installed = rCirc.LookupParameter("Pу_EVA").AsDouble();
                I1_Calculated = rCirc.LookupParameter("Iр_отх_линии_EVA").AsDouble();
                Cos = rCirc.LookupParameter("Cos_EVA").AsDouble();
                Ik_End_Line = rCirc.LookupParameter("I_1ф_кз_EVA").AsDouble();
                Cable_Calculated_L = rCirc.LookupParameter("L_расч_кабеля_EVA").AsDouble();
                DU_Calculated = rCirc.LookupParameter("ΔU_EVA").AsDouble();
                Cable_L_1 = rCirc.LookupParameter("L_факт_кабеля_1_EVA").AsDouble();
                Cable_L_2 = rCirc.LookupParameter("L_факт_кабеля_2_EVA").AsDouble();
                Number_Of_Phase = rCirc.PolesNumber;
                Phase_Connection = rCirc.LookupParameter("Фаза_подключения_EVA").AsValueString();
                Device_Type_1 = rCirc.LookupParameter("Тип_аппарата_1_EVA").AsValueString();
                Device_Type_2 = rCirc.LookupParameter("Тип_аппарата_2_EVA").AsValueString();
                Device_Type_3 = rCirc.LookupParameter("Тип_аппарата_3_EVA").AsValueString();
                Device_Mark_1 = rCirc.LookupParameter("Марка_аппарата_1_EVA").AsString();
                Device_Mark_2 = rCirc.LookupParameter("Марка_аппарата_2_EVA").AsString();
                Device_Mark_3 = rCirc.LookupParameter("Марка_аппарата_3_EVA").AsString();
                Device_I_1 = rCirc.LookupParameter("I_расцепителя_аппарата_1_EVA").AsDouble();
                Device_I_2 = rCirc.LookupParameter("I_расцепителя_аппарата_2_EVA").AsDouble();
                Device_I_3 = rCirc.LookupParameter("I_расцепителя_аппарата_3_EVA").AsDouble();
                Device_Curve_1 = rCirc.LookupParameter("Характеристика_срабатывания_аппарат_1_EVA").AsValueString();
                Device_Curve_2 = rCirc.LookupParameter("Характеристика_срабатывания_аппарат_2_EVA").AsValueString();
                Device_Curve_3 = rCirc.LookupParameter("Характеристика_срабатывания_аппарат_3_EVA").AsValueString();
                Device_Break_1 = rCirc.LookupParameter("Ном_откл_способн_аппарата_1_EVA").AsDouble();
                Device_Break_2 = rCirc.LookupParameter("Ном_откл_способн_аппарата_2_EVA").AsDouble();
                Device_Break_3 = rCirc.LookupParameter("Ном_откл_способн_аппарата_3_EVA").AsDouble();
                Device_dI_bodyI_1 = rCirc.LookupParameter("I_утеч-корп_аппарата-транф_1_EVA").AsDouble();
                Device_dI_bodyI_2 = rCirc.LookupParameter("I_утеч-корп_аппарата-транф_2_EVA").AsDouble();
                Device_dI_bodyI_3 = rCirc.LookupParameter("I_утеч-корп_аппарата-транф_3_EVA").AsDouble();
                Load_Type = rCirc.LookupParameter("Тип_Нагрузки_EVA").AsValueString();
                Load_Name = rCirc.LookupParameter("Наименование_нагрузки_EVA").AsString();
                Ugo = rCirc.LookupParameter("УГО_EVA").AsValueString();


            }
            
            else 
            {
                Rez = true;

                //подключения расширенного хранилища для получения информации из резервных цепей
            }




        }
        /// <summary>
        /// Получение свойства string по имени и порядковому номеру
        /// </summary>
        /// <param name="property"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string GetProp(Device property, int order)
        {
            switch(property)
            {
                case Device.Mark:
                    switch(order)
                    {
                        case 1: return Device_Mark_1;
                        case 2: return Device_Mark_2;
                        case 3: return Device_Mark_2;
                        default: return "ошибка";
                    }
                case Device.Type:
                    switch (order)
                    {
                        case 1: return Device_Type_1;
                        case 2: return Device_Type_2;
                        case 3: return Device_Type_3;
                        default: return "ошибка";
                    }
                case Device.Curve:
                    switch (order)
                    {
                        case 1: return Device_Curve_1;
                        case 2: return Device_Curve_2;
                        case 3: return Device_Curve_3;
                        default: return "ошибка";
                    }
                case Device.I:
                    switch (order)
                    {
                        case 1: return Device_I_1.ToString();
                        case 2: return Device_I_2.ToString();
                        case 3: return Device_I_3.ToString();
                        default: return "ошибка";
                    }
                case Device.Break:
                    switch (order)
                    {
                        case 1: return Device_Break_1.ToString();
                        case 2: return Device_Break_2.ToString();
                        case 3: return Device_Break_3.ToString();
                        default: return "ошибка";
                    }
                case Device.Body:
                    switch (order)
                    {
                        case 1: return Device_dI_bodyI_1.ToString();
                        case 2: return Device_dI_bodyI_2.ToString();
                        case 3: return Device_dI_bodyI_3.ToString();
                        default: return "ошибка";
                    }

                default:
                    return "ошибка";
            }
        }     
    }


    internal class PanelItem : ElectricalInfo
    {
        public string AppVvod { get; set; }
        public int CountGroup { get; set; }

        public Element Rboard { get; set; }

        public CircItem CircBoard { get; set; }
        

        private bool _Is_Checked;
        public bool Is_Checked
        {
            get => _Is_Checked;


            set
            {
                if (Equals(_Is_Checked, value)) return;
                _Is_Checked = value;
                if (value) SelectedPanels.Panels.Add(this);
                else SelectedPanels.Panels.Remove(this);
                
            }
        }

        public List<PanelItem> SubPanels { get; set; } = new List<PanelItem>();


        //Цепи щита
        public ObservableCollection<CircItem> Circuits { get; set; }

        //управление видимостью чекбокса
        private System.Windows.Visibility _visibility;
        public System.Windows.Visibility Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
            }
        }




        //Конструктор
        public PanelItem(Element panelRevit)
        {
            //SubPanels = new List<PanelItem>();
            Circuits = new ObservableCollection<CircItem>();

            //Заполнение свойств паенели
            Name = panelRevit.Name;
            Id = panelRevit.Id;
            //ParentBoardId = ElementId.InvalidElementId;

            //Заполнение свойств цепей
            Circuits = Utilits.GetSortedCircuits(panelRevit, out CircItem circBoard); //отсортированные цепи
            if(circBoard != null)
            {
                CircBoard = circBoard;
                ParentBoardId = circBoard.ParentBoardId;
            }
            
            CountGroup = Circuits.Count;

            //Rboard = panelRevit;

            //CountGroup=panelRevit.

            //управление видимостью чекбоксов
            //Visibility = System.Windows.Visibility.Collapsed;
            Visibility = System.Windows.Visibility.Visible;


        }

        //Конструктор для объеденения панелей ВРУ
        //public PanelItem(PanelItem panelItem)
        //{
        //    Name = panelItem.Name.Remove(3);
        //    //SubPanels.Add(panelItem);
        //    SubPanels.Add(this);
        //}

        //public PanelItem()
        //{

        //}

    }

    //internal class VruItem : ElectricalInfo
    //{

    //}


    enum Device
    {
        Mark,
        Type,
        Curve,
        I,
        Break,
        Body
    }

    internal static class SelectedPanels
    {
        public static List<PanelItem> Panels { get; set; } = new List<PanelItem>();
    }
}
