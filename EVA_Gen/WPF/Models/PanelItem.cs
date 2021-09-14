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

        public double P2W { get; set; }
        public double P2WF { get; set; }
        public double P2S { get; set; }
        public double P2SF { get; set; }
        public double Q2W { get; set; }
        public double Q2WF { get; set; }
        public double Q2S { get; set; }
        public double Q2SF { get; set; }
        public double P_L1 { get; set; }
        public double P_L2 { get; set; }
        public double P_L3 { get; set; }

        public double S { get; set; } 
        public double Cos { get; set; } 
        public double P { get; set; }
        public string P_Visable
        {
            get { return (P / 1000).ToString(); }
        }


        public double Q { get; set; }

    }

    internal class ElementItem : ElectricalInfo
    {
        public bool IsPanel { get; set; }
        //public PanelItem PanIt { get; set; }


        public ElementItem()
        {

        }
        public ElementItem(Element el)
        {
            ConnectorSet connectors = (el as FamilyInstance).MEPModel.ConnectorManager.Connectors;
            foreach (var obj in connectors)
            {
                var conn = obj as Connector;
                var famConnInfo = conn.GetMEPConnectorInfo() as MEPFamilyConnectorInfo;

                S = (famConnInfo.GetConnectorParameterValue(new ElementId(BuiltInParameter.RBS_ELEC_APPARENT_LOAD)) as DoubleParameterValue).Value;
                Cos = (famConnInfo.GetConnectorParameterValue(new ElementId(BuiltInParameter.RBS_ELEC_POWER_FACTOR)) as DoubleParameterValue).Value;
            }
        }


    }


    internal class CircItem : ElectricalInfo
    {
        //public static string DeviceMark = "Марка_аппарата";
        //public static string DeviceType = "Т_аппарата";
        static Categories categories = Utilits.Doc.Settings.Categories;
        ElementId electricalEquipmentCategoryId = categories.get_Item(BuiltInCategory.OST_ElectricalEquipment).Id;

        public bool Rez { get; set; } 
        public bool HasValueP { get; set; }
        public string AppZ { get; set; }
        public int Length { get; set; }
        //public string Cable_Mark_2 { get; set; }
        public string Cable_Mark_1 { get; set; }
        public string Cable_In_Tray_Pipe { get; set; }
        public int Cable_S_1_1 { get; set; }
        public int Cable_S_1_2 { get; set; }
        public double Cable_S_1_3 { get; set; }
        public int Cable_S_2_1 { get; set; }
        public int Cable_S_2_2 { get; set; }
        public double Cable_S_2_3 { get; set; }

        public double Pipe_L { get; set; }
       
        //public double P { get; set; }
        public double P1 { get; set; }
        public string P1_Visable
        {
            get { return (P1 / 1000).ToString() ; }
        }



        public double I1_Max { get; set; }
        public string I1_Max_Visable
        {
            get { return Math.Round(I1_Max, 2).ToString(); }
            //set;
        }
        public double Q1 { get; set; }

        //public double Cos { get; set; }
        public double Ik_End_Line { get; set; }
        public string Ik_End_Line_Visable
        {
            get { return Math.Round(Ik_End_Line, 2).ToString(); }
            //set;
        }

        public double Cable_Calculated_L { get; set; }
        public double DU_Calculated { get; set; }
        public double Cable_L_1 { get; set; }
        public double Cable_L_2 { get; set; }
        public int Number_Of_Phase { get; set; }
        public string Phase_Connection { get; set; }
        public string Device_Type_1 { get; set; } = "нет";
        public string Device_Type_2 { get; set; } = "нет";
        public string Device_Type_3 { get; set; } = "нет";
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
        public double Device_dI_body_I_Meter_TT_1 { get; set; }
        public double Device_dI_body_I_Meter_TT_2 { get; set; }
        public double Device_dI_body_I_Meter_TT_3 { get; set; }
       
        public string PanelName { get; set; }

        public bool Cable_type { get; set; }
        public bool Cable_method { get; set; }
        public bool Cos_Lock { get; set; }
        public bool Phase_Connection_Lock { get; set; }
        public bool Cable_S_1_Lock { get; set; }

        public string Load_Winter_Summer { get; set; }
        public bool Load_Winter_Summer_Lock { get; set; }
        public string Load_Mode_Operating { get; set; }
        public bool Load_Mode_Operating_Lock { get; set; }

        public string Cable_Mark { get; set; }
        public double DU_Allowable { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        //public double Q { get; set; }
        //public double S { get; set; }
        public double S1 { get; set; }


        public string Load_Type { get; set; }
        public string Load_Name { get; set; }
        public string Ugo { get; set; }
        public string Out_Line_panel { get; set; }
        public double Kc1 { get; set; }
        public double Kc2 { get; set; } 
        public double Kc3 { get; set; } 
        public double Kc4 { get; set; } 
        public double Voltage { get; set; }

        public bool Device_I_1_Lock { get; set; }
        public bool Device_I_2_Lock { get; set; }
        public bool Device_I_3_Lock { get; set; }


        
        
        public double P3W { get; set; }
        public double P4W { get; set; }
        public double P3WF { get; set; }
        public double P4WF { get; set; }
        public double P3S { get; set; }
        public double P4S { get; set; }
        public double P3SF { get; set; }
        public double P4SF { get; set; }
        public double Q3W { get; set; }
        public double Q4W { get; set; }
        public double Q3WF { get; set; }
        public double Q4WF { get; set; }
        public double Q3S { get; set; }
        public double Q4S { get; set; }
        public double Q3SF { get; set; }
        public double Q4SF { get; set; }
        public double I1_L1 { get; set; }
        public double I1_L2 { get; set; }
        public double I1_L3 { get; set; }
        public double Kd { get; set; }
        public string Name_Load_T { get; set; } = "";


        public List<ElementItem> ElementList { get; set; } = new List<ElementItem>();

        public CircItem(ElectricalSystem rCirc)
        {
            //заполнение свойств
            if(rCirc.CircuitType == CircuitType.Circuit)
            {
                Name = rCirc.LookupParameter("Имя_цепи_EVA").AsString();
                Rez = false;
                Id = rCirc.Id;
                Cos = rCirc.LookupParameter("Cos_EVA").AsDouble();



                //Cable_Mark_2 = rCirc.LookupParameter("Марка_кабеля_2_EVA").AsString();
                //Cable_Mark_1 = rCirc.LookupParameter("Марка_кабеля_1_EVA").AsString();
                Cable_In_Tray_Pipe = rCirc.LookupParameter("Способ_прокладки_EVA").AsString();

                Cable_S_1_1 = rCirc.LookupParameter("Сечение_кабеля_1_1_EVA").AsInteger();
                Cable_S_1_2 = rCirc.LookupParameter("Сечение_кабеля_1_2_EVA").AsInteger();
                //Cable_S_1_3 = rCirc.LookupParameter("Сечение_кабеля_1_3_EVA").AsDouble();
                Cable_S_2_1 = rCirc.LookupParameter("Сечение_кабеля_2_1_EVA").AsInteger();
                Cable_S_2_2 = rCirc.LookupParameter("Сечение_кабеля_2_2_EVA").AsInteger();
                Cable_S_2_3 = rCirc.LookupParameter("Сечение_кабеля_2_3_EVA").AsDouble();

                


                Pipe_L = rCirc.LookupParameter("L_трубы_EVA").AsDouble();
                P1 = rCirc.LookupParameter("Pр_отх_линии_EVA").AsDouble();
                P = rCirc.LookupParameter("Pу_EVA").AsDouble();
                HasValueP = rCirc.LookupParameter("Pу_EVA").HasValue;
                I1_Max = rCirc.LookupParameter("Iр_отх_линии_EVA").AsDouble();
                Cos = rCirc.LookupParameter("Cos_EVA").AsDouble();
                Ik_End_Line = rCirc.LookupParameter("I_1ф_кз_EVA").AsDouble();
                Cable_Calculated_L = rCirc.LookupParameter("L_расч_кабеля_EVA").AsDouble();
                //DU_Calculated = rCirc.LookupParameter("ΔU_EVA").AsDouble();

                Cable_L_1 = rCirc.LookupParameter("L_факт_кабеля_1_EVA").AsDouble();
                Cable_L_2 = rCirc.LookupParameter("L_факт_кабеля_2_EVA").AsDouble();
                Number_Of_Phase = rCirc.PolesNumber;
                if (rCirc.get_Parameter(BuiltInParameter.RBS_ELEC_NUMBER_OF_POLES).AsInteger() == 3) Phase_Connection = "-";
                else if (rCirc.LookupParameter("Фаза_подключения_EVA").AsValueString() == "(нет)") Phase_Connection = "L1";
                else Phase_Connection = rCirc.LookupParameter("Фаза_подключения_EVA").AsValueString();

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
                Device_dI_body_I_Meter_TT_1 = rCirc.LookupParameter("I_утеч-корп_аппарата-транф_1_EVA").AsDouble();
                Device_dI_body_I_Meter_TT_2 = rCirc.LookupParameter("I_утеч-корп_аппарата-транф_2_EVA").AsDouble();
                Device_dI_body_I_Meter_TT_3 = rCirc.LookupParameter("I_утеч-корп_аппарата-транф_3_EVA").AsDouble();
                
                PanelName = rCirc.PanelName;


                Load_Type = rCirc.LookupParameter("Тип_Нагрузки_EVA").AsValueString();
                Load_Name = rCirc.LookupParameter("Наименование_нагрузки_EVA").AsString();
                Ugo = rCirc.LookupParameter("УГО_EVA").AsValueString();

                if (rCirc.LookupParameter("Кс_отх_линии_EVA").AsDouble() == 0) Kc1 = 1;
                else Kc1 = rCirc.LookupParameter("Кс_отх_линии_EVA").AsDouble();
                if (rCirc.LookupParameter("Кс_щита_EVA").AsDouble() == 0) Kc2 = 1;
                else Kc2 = rCirc.LookupParameter("Кс_щита_EVA").AsDouble();
                if (rCirc.LookupParameter("Кс_на_вводах_ВРУ_EVA").AsDouble() == 0) Kc3 = 1;
                else Kc3 = rCirc.LookupParameter("Кс_на_вводах_ВРУ_EVA").AsDouble();
                if (rCirc.LookupParameter("Кс_ВРУ_авар_реж_EVA").AsDouble() == 0) Kc4 = 1;
                else Kc4 = rCirc.LookupParameter("Кс_ВРУ_авар_реж_EVA").AsDouble();

                Voltage = rCirc.get_Parameter(BuiltInParameter.RBS_ELEC_VOLTAGE).AsDouble();
                Q1 = rCirc.LookupParameter("Q_отх_линии_EVA").AsDouble();
                Cable_type = rCirc.LookupParameter("Тип_кабеля_Алюм_EVA").AsInteger() == 1;
                Cable_method = rCirc.LookupParameter("Способ_прокладки_кабеля_Земля_EVA").AsInteger() == 1;
                Cos_Lock = rCirc.LookupParameter("Cos_Lock_EVA").AsInteger() == 1;
                Phase_Connection_Lock = rCirc.LookupParameter("Фаза_подключения_Lock_EVA").AsInteger() == 1;
                Cable_S_1_Lock = rCirc.LookupParameter("Сечение_кабеля_1_Lock_EVA").AsInteger() == 1;

                if (rCirc.LookupParameter("Режим_работы_Зима_Лето_EVA").AsValueString() == "") Load_Winter_Summer = "круглый год";
                else Load_Winter_Summer = rCirc.LookupParameter("Режим_работы_Зима_Лето_EVA").AsValueString();

                Load_Winter_Summer_Lock = rCirc.LookupParameter("Режим_работы_Зима_Лето_Lock_EVA").AsInteger() == 1;
                if (rCirc.LookupParameter("Режим_учета_нагрузки_EVA").AsValueString() == "") Load_Mode_Operating = "в обоих режимах";
                else Load_Mode_Operating = rCirc.LookupParameter("Режим_учета_нагрузки_EVA").AsValueString();

                Load_Mode_Operating_Lock = rCirc.LookupParameter("Режим_учета_нагрузки_Lock_EVA").AsInteger() == 1;

                Cable_Mark = rCirc.LookupParameter("Марка_кабеля_EVA").AsString();
                if (rCirc.LookupParameter("ΔU_допустимые_EVA").AsDouble() == 0) DU_Allowable = 3;
                else DU_Allowable = rCirc.LookupParameter("ΔU_допустимые_EVA").AsDouble();
                Text1 = rCirc.LookupParameter("Текст1_EVA").AsString();
                Text2 = rCirc.LookupParameter("Текст2_EVA").AsString();
                Q = rCirc.LookupParameter("Qу_EVA").AsDouble();
                S = rCirc.LookupParameter("Sу_EVA").AsDouble();
                S1 = rCirc.LookupParameter("S_отх_линии_EVA").AsDouble();

                Device_I_1_Lock = rCirc.LookupParameter("I_расцепителя_аппарата_1_Lock_EVA").AsInteger() == 1;
                Load_Mode_Operating_Lock = rCirc.LookupParameter("I_расцепителя_аппарата_2_Lock_EVA").AsInteger() == 1;
                Load_Mode_Operating_Lock = rCirc.LookupParameter("I_расцепителя_аппарата_3_Lock_EVA").AsInteger() == 1;


                P_L1 = rCirc.LookupParameter("Pу_L1_EVA").AsDouble();
                P_L2 = rCirc.LookupParameter("Pу_L2_EVA").AsDouble();
                P_L3 = rCirc.LookupParameter("Pу_L3_EVA").AsDouble();
                P2W = rCirc.LookupParameter("Pр_щита_Зима_EVA").AsDouble();
                P3W = rCirc.LookupParameter("Pр_на_вводах_ВРУ_Зима_EVA").AsDouble();
                P4W = rCirc.LookupParameter("Pр_ВРУ_авар_реж_Зима_EVA").AsDouble();
                P2WF = rCirc.LookupParameter("Pр_щита_Зима_Пожар_EVA").AsDouble();
                P3WF = rCirc.LookupParameter("Pр_на_вводах_ВРУ_Зима_Пожар_EVA").AsDouble();
                P4WF = rCirc.LookupParameter("Pр_ВРУ_авар_реж_Зима_Пожар_EVA").AsDouble();
                P2S = rCirc.LookupParameter("Pр_щита_Лето_EVA").AsDouble();
                P3S = rCirc.LookupParameter("Pр_на_вводах_ВРУ_Лето_EVA").AsDouble();
                P4S = rCirc.LookupParameter("Pр_ВРУ_авар_реж_Лето_EVA").AsDouble();
                P2SF = rCirc.LookupParameter("Pр_щита_Лето_Пожар_EVA").AsDouble();
                P3SF = rCirc.LookupParameter("Pр_на_вводах_ВРУ_Лето_Пожар_EVA").AsDouble();
                P4SF = rCirc.LookupParameter("Pр_ВРУ_авар_реж_Лето_Пожар_EVA").AsDouble();
                Q2W = rCirc.LookupParameter("Qр_щита_Зима_EVA").AsDouble();
                Q3W = rCirc.LookupParameter("Qр_на_вводах_ВРУ_Зима_EVA").AsDouble();
                Q4W = rCirc.LookupParameter("Qр_ВРУ_авар_реж_Зима_EVA").AsDouble();
                Q2WF = rCirc.LookupParameter("Qр_щита_Зима_Пожар_EVA").AsDouble();
                Q3WF = rCirc.LookupParameter("Qр_на_вводах_ВРУ_Зима_Пожар_EVA").AsDouble();
                Q4WF = rCirc.LookupParameter("Qр_ВРУ_авар_реж_Зима_Пожар_EVA").AsDouble();
                Q2S = rCirc.LookupParameter("Qр_щита_Лето_EVA").AsDouble();
                Q3S = rCirc.LookupParameter("Qр_на_вводах_ВРУ_Лето_EVA").AsDouble();
                Q4S = rCirc.LookupParameter("Qр_ВРУ_авар_реж_Лето_EVA").AsDouble();
                Q2SF = rCirc.LookupParameter("Qр_щита_Лето_Пожар_EVA").AsDouble();
                Q3SF = rCirc.LookupParameter("Qр_на_вводах_ВРУ_Лето_Пожар_EVA").AsDouble();
                Q4SF = rCirc.LookupParameter("Qр_ВРУ_авар_реж_Лето_Пожар_EVA").AsDouble();
                I1_L1 = rCirc.LookupParameter("Iр_отх_линии_L1_EVA").AsDouble();
                I1_L2 = rCirc.LookupParameter("Iр_отх_линии_L2_EVA").AsDouble();
                I1_L3 = rCirc.LookupParameter("Iр_отх_линии_L3_EVA").AsDouble();
                if (rCirc.LookupParameter("Кс_доп_EVA").AsDouble() == 0) Kd = 1;
                else Kd = rCirc.LookupParameter("Кс_доп_EVA").AsDouble();















                // получение параметров от элементов цепи
                foreach (Element el in rCirc.Elements)
                {
                    var eli = new ElementItem();
                    eli.Id = el.Id;
                    if (!el.Category.Id.Equals(electricalEquipmentCategoryId)) //если элемент не панель
                    {
                        eli.IsPanel = false;
                        ConnectorSet connectors = (el as FamilyInstance).MEPModel.ConnectorManager.Connectors;
                        foreach (var obj in connectors)
                        {
                            var conn = obj as Connector;
                            var famConnInfo = conn.GetMEPConnectorInfo() as MEPFamilyConnectorInfo;

                            if (conn.Domain == Domain.DomainElectrical)
                            {
                                eli.S = (famConnInfo.GetConnectorParameterValue(new ElementId(BuiltInParameter.RBS_ELEC_APPARENT_LOAD)) as DoubleParameterValue).Value;
                                eli.Cos = (famConnInfo.GetConnectorParameterValue(new ElementId(BuiltInParameter.RBS_ELEC_POWER_FACTOR)) as DoubleParameterValue).Value;
                            }
                        }
                        //ElementList.Add(new ElementItem(el));   
                        //var sd = el.LookupParameter("Имя_нагрузки_EVA").AsString();
                        if(el.LookupParameter("Имя_нагрузки_EVA").AsString() != null)
                        {
                            if (!Name_Load_T.Contains(el.LookupParameter("Имя_нагрузки_EVA").AsString()))
                            {
                                if(Name_Load_T == "") Name_Load_T = el.LookupParameter("Имя_нагрузки_EVA").AsString();
                                else Name_Load_T = Name_Load_T + ", " + el.LookupParameter("Имя_нагрузки_EVA").AsString();
                            }
                        }
                       
                    }
                    else //if(el.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM).AsDouble() != 0)
                    {
                        eli.IsPanel = true;

                        //eli.PanIt = this;
                        if (el.LookupParameter("Отходящие_линии_EVA").AsInteger() == 1)
                        {
                            //var circuits = Utilits.GetSortedCircuits(el as FamilyInstance);
                            Out_Line_panel = el.Name;                         
                        }
                    }

                    ElementList.Add(eli);
                }
                

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
                        case 1: return Device_dI_body_I_Meter_TT_1.ToString();
                        case 2: return Device_dI_body_I_Meter_TT_2.ToString();
                        case 3: return Device_dI_body_I_Meter_TT_3.ToString();
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

        //public Element Rboard { get; set; }

        public CircItem CircBoard { get; set; }
        
        public int ParentNumber { get; set; }
        public double Line_step { get; set; }
        public string circ_Text1 { get; set; }
        public string circ_Text2 { get; set; }
        public double S2W { get; set; }
        public double S2WF { get; set; }
        public double S2S { get; set; }
        public double S2SF { get; set; }
        public double cos2W { get; set; }
        public double cos2WF { get; set; }
        public double cos2S { get; set; }
        public double cos2SF { get; set; }
        public double K2W { get; set; } = 1;
        public double K2WF { get; set; } = 1;
        public double K2S { get; set; } = 1;
        public double K2SF { get; set; } = 1;
        public double I2W_L1 { get; set; }
        public double I2W_L2 { get; set; }
        public double I2W_L3 { get; set; }
        public double I2W_max { get; set; }
        public double I2WF_L1 { get; set; }
        public double I2WF_L2 { get; set; }
        public double I2WF_L3 { get; set; }
        public double I2WF_max { get; set; }
        public double I2S_L1 { get; set; }
        public double I2S_L2 { get; set; }
        public double I2S_L3 { get; set; }
        public double I2S_max { get; set; }
        public double I2SF_L1 { get; set; }
        public double I2SF_L2 { get; set; }
        public double I2SF_L3 { get; set; }
        public double I2SF_max { get; set; }
                                 


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

            Line_step = panelRevit.LookupParameter("Шаг_линии_EVA").AsDouble();
            circ_Text1 = panelRevit.LookupParameter("Текст1_EVA").AsString();
            circ_Text2 = panelRevit.LookupParameter("Текст2_EVA").AsString();
            S2W = panelRevit.LookupParameter("Sр_щита_Зима_EVA").AsDouble();
            S2WF = panelRevit.LookupParameter("Sр_щита_Зима_Пожар_EVA").AsDouble();
            S2S = panelRevit.LookupParameter("Sр_щита_Лето_EVA").AsDouble();
            S2SF = panelRevit.LookupParameter("Sр_щита_Лето_Пожар_EVA").AsDouble();
            cos2W = panelRevit.LookupParameter("Cos_щита_Зима_EVA").AsDouble();
            cos2WF = panelRevit.LookupParameter("Cos_щита_Зима_Пожар_EVA").AsDouble();
            cos2S = panelRevit.LookupParameter("Cos_щита_Лето_EVA").AsDouble();
            cos2SF = panelRevit.LookupParameter("Cos_щита_Лето_Пожар_EVA").AsDouble();
            K2W = panelRevit.LookupParameter("Кс_щита_Зима_EVA").AsDouble();
            K2WF = panelRevit.LookupParameter("Кс_щита_Зима_Пожар_EVA").AsDouble();
            K2S = panelRevit.LookupParameter("Кс_щита_Лето_EVA").AsDouble();
            K2SF = panelRevit.LookupParameter("Кс_щита_Лето_Пожар_EVA").AsDouble();
            I2W_L1 = panelRevit.LookupParameter("Iр_щита_Зима_L1_EVA").AsDouble();
            I2W_L2 = panelRevit.LookupParameter("Iр_щита_Зима_L2_EVA").AsDouble();
            I2W_L3 = panelRevit.LookupParameter("Iр_щита_Зима_L3_EVA").AsDouble();
            I2W_max = panelRevit.LookupParameter("Iр_щита_Зима_EVA").AsDouble();
            I2WF_L1 = panelRevit.LookupParameter("Iр_щита_Зима_Пожар_L1_EVA").AsDouble();
            I2WF_L2 = panelRevit.LookupParameter("Iр_щита_Зима_Пожар_L2_EVA").AsDouble();
            I2WF_L3 = panelRevit.LookupParameter("Iр_щита_Зима_Пожар_L3_EVA").AsDouble();
            I2WF_max = panelRevit.LookupParameter("Iр_щита_Зима_Пожар_EVA").AsDouble();
            I2S_L1 = panelRevit.LookupParameter("Iр_щита_Лето_L1_EVA").AsDouble();
            I2S_L2 = panelRevit.LookupParameter("Iр_щита_Лето_L2_EVA").AsDouble();
            I2S_L3 = panelRevit.LookupParameter("Iр_щита_Лето_L3_EVA").AsDouble();
            I2S_max = panelRevit.LookupParameter("Iр_щита_Лето_EVA").AsDouble();
            I2SF_L1 = panelRevit.LookupParameter("Iр_щита_Лето_Пожар_L1_EVA").AsDouble();
            I2SF_L2 = panelRevit.LookupParameter("Iр_щита_Лето_Пожар_L2_EVA").AsDouble();
            I2SF_L3 = panelRevit.LookupParameter("Iр_щита_Лето_Пожар_L3_EVA").AsDouble();
            I2SF_max = panelRevit.LookupParameter("Iр_щита_Лето_Пожар_EVA").AsDouble();













            //Заполнение свойств цепей, точка входа создания экземпляров цепей
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
