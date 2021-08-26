using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Runtime.InteropServices;
using EVA_Gen.WPF.Models;

namespace EVA_Gen
{
    static class Calculation
    {
        private static Document doc;
        public static Document Doc
        {
            get { return doc; }
            set { doc = value; }
        }

        const int U1f = 220;
        const int U3f= 380;

        public static void Circuits (PanelItem pi)
        {

            foreach (CircItem circ in pi.Circuits)
            {
                circ.P = 0;
                circ.Q = 0;
                circ.P_L1 = 0;
                circ.P_L2 = 0;
                circ.P_L3 = 0;

                int k_LMO;
                int k_LMO_F;
                int k_LW;
                int k_LS;



                switch(circ.Load_Mode_Operating)
                {
                    case "в обоих режимах":
                        k_LMO = 1;
                        k_LMO_F = 1;
                        break;
                    case "в пожарном режиме":
                        k_LMO = 0;
                        k_LMO_F = 1;
                        break;
                    case "в рабочем режиме":
                        k_LMO = 1;
                        k_LMO_F = 0;
                        break;
                    default:
                        k_LMO = 0;
                        k_LMO_F = 0;
                        break;
                }

                switch(circ.Load_Winter_Summer)
                {
                    case "зима":
                        k_LW = 1;
                        k_LS = 0;
                        break;
                    case "лето":
                        k_LW = 0;
                        k_LS = 1;
                        break;
                    default:
                        k_LW = 1;
                        k_LS = 1;
                        break;
                }

                foreach (ElementItem elItem in circ.ElementList)
                {
                    if (!elItem.IsPanel)
                    {
                        circ.P += Utilits.VoltAmperage(elItem.S) * elItem.Cos;
                        //TaskDialog.Show("d", Utilits.VoltAmperage(elItem.S).ToString());
                        circ.Q += Utilits.VoltAmperage(elItem.S) * Math.Sqrt(1 - elItem.Cos * elItem.Cos);

                        switch (circ.Phase_Connection)
                        {
                            case "L1":
                                circ.P_L1 += Utilits.VoltAmperage(elItem.S) * elItem.Cos;
                                break;
                            case "L2":
                                circ.P_L2 += Utilits.VoltAmperage(elItem.S) * elItem.Cos;
                                break;
                            case "L3":
                                circ.P_L3 += Utilits.VoltAmperage(elItem.S) * elItem.Cos;
                                break;
                            default:
                                circ.P_L1 += (Utilits.VoltAmperage(elItem.S) * elItem.Cos) / 3;
                                circ.P_L2 += (Utilits.VoltAmperage(elItem.S) * elItem.Cos) / 3;
                                circ.P_L3 += (Utilits.VoltAmperage(elItem.S) * elItem.Cos) / 3;
                                break;
                        }
                    }
                    else //если элемент панель
                    {
                        //переписать данные из панели
                        //TaskDialog.Show("dad", elItem.Id.IntegerValue.ToString() + elItem.Name); 
                        var ghf = Develop.boards;
                        //var panelItem = Develop.boards.FirstOrDefault(x => x.Id.IntegerValue == elItem.Id.IntegerValue);

                        //circ.P += panelItem.P;
                        //circ.Q += panelItem.Q;

                        //circ.P_L1 += panelItem.P_L1;
                        //circ.P_L2 += panelItem.P_L2;
                        //circ.P_L3 += panelItem.P_L3;
                    }
                   
                }
                circ.S = Math.Sqrt(circ.P * circ.P + circ.Q * circ.Q);
                if (circ.S == 0) circ.Cos = 1;
                else circ.Cos = circ.P / circ.S;

                //TaskDialog.Show("d", circ.Cos.ToString());
                circ.P1 = circ.P * circ.Kc1;
                circ.P2W = circ.P * circ.Kc2 * k_LMO * k_LW;
                circ.P3W = circ.P * circ.Kc3 * k_LMO * k_LW * circ.Kd;
                circ.P4W = circ.P * circ.Kc4 * k_LMO * k_LW * circ.Kd;
                circ.P2WF = circ.P * circ.Kc2 * k_LMO_F * k_LW;
                circ.P3WF = circ.P * circ.Kc3 * k_LMO_F * k_LW * circ.Kd;
                circ.P4WF = circ.P * circ.Kc4 * k_LMO_F * k_LW * circ.Kd;
                circ.P2S = circ.P * circ.Kc2 * k_LMO * k_LS;
                circ.P3S = circ.P * circ.Kc3 * k_LMO * k_LS * circ.Kd;
                circ.P4S = circ.P * circ.Kc4 * k_LMO * k_LS * circ.Kd;
                circ.P2SF = circ.P * circ.Kc2 * k_LMO_F * k_LS;
                circ.P3SF = circ.P * circ.Kc3 * k_LMO_F * k_LS * circ.Kd;
                circ.P4SF = circ.P * circ.Kc4 * k_LMO_F * k_LS * circ.Kd;

                circ.Q1 = circ.P1 * Math.Tan(Math.Acos(circ.Cos));
                circ.Q2W = circ.P2W * Math.Tan(Math.Acos(circ.Cos));
                circ.Q3W = circ.P3W * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q4W = circ.P4W * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q2WF = circ.P2W * Math.Tan(Math.Acos(circ.Cos));
                circ.Q3WF = circ.P3W * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q4WF = circ.P4W * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q2S = circ.P2S * Math.Tan(Math.Acos(circ.Cos));
                circ.Q3S = circ.P3S * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q4S = circ.P4S * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q2SF = circ.P2SF * Math.Tan(Math.Acos(circ.Cos));
                circ.Q3SF = circ.P3SF * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;
                circ.Q4SF = circ.P4SF * Math.Tan(Math.Acos(circ.Cos)) * circ.Kd;

                circ.I1_L1 = circ.P_L1 * circ.Kc1 / (U1f * circ.Cos);
                circ.I1_L2 = circ.P_L2 * circ.Kc1 / (U1f * circ.Cos);
                circ.I1_L3 = circ.P_L3 * circ.Kc1 / (U1f * circ.Cos);

                circ.S1 = circ.S * circ.Kc1;

                circ.I1_Max = circ.I1_L1;
                if (circ.I1_Max < circ.I1_L2) circ.I1_Max = circ.I1_L2;
                if (circ.I1_Max < circ.I1_L3) circ.I1_Max = circ.I1_L3;

                if (!circ.Cable_S_1_Lock)
                {
                    int i = 1;
                    double device_I = 0;
                    if (circ.Device_Type_1.Contains("QF")) device_I = circ.Device_I_1;
                    if (circ.Device_Type_2.Contains("QF")) device_I = circ.Device_I_2;
                    if (circ.Device_Type_3.Contains("QF")) device_I = circ.Device_I_3;


                    if(circ.Cable_type && circ.Cable_method)
                    {

                    }
                }

                //circ.DU_Calculated = (circ.P1 * circ.Cable_Calculated_L)/72










                //запись в ревит

                RecordingCalcCirc(circ);






            }


        }

        public static void Panels(PanelItem pi)
        {
            pi.P = 0;
            pi.P2W = 0;
            pi.P2WF = 0;
            pi.P2S = 0;
            pi.P2SF = 0;
            pi.Q = 0;
            pi.Q2W = 0;
            pi.Q2WF = 0;
            pi.Q2S = 0;
            pi.Q2SF = 0;

            pi.P_L1 = 0;
            pi.P_L2 = 0;
            pi.P_L3 = 0;

            foreach (CircItem circ in pi.Circuits)
            {
                pi.P += circ.P;
                pi.P2W += circ.P2W;
                pi.P2WF += circ.P2WF;
                pi.P2S += circ.P2S;
                pi.P2SF += circ.P2SF;

                pi.Q += circ.Q;
                pi.Q2W += circ.Q2W;
                pi.Q2WF += circ.Q2WF;
                pi.Q2S += circ.Q2S;
                pi.Q2SF += circ.Q2SF;

                pi.P_L1 += circ.P_L1;
                pi.P_L2 += circ.P_L2;
                pi.P_L3 += circ.P_L3;


            }




            pi.S2W = Math.Sqrt(pi.P2W * pi.P2W + pi.Q2W * pi.Q2W);
            pi.S2WF = Math.Sqrt(pi.P2WF * pi.P2WF + pi.Q2WF * pi.Q2WF);
            pi.S2S = Math.Sqrt(pi.P2S * pi.P2S + pi.Q2S * pi.Q2S);
            pi.S2SF = Math.Sqrt(pi.P2SF * pi.P2SF + pi.Q2SF * pi.Q2SF);
            if (pi.S2W == 0) pi.cos2W = 1;
            else pi.cos2W = pi.P2W / pi.S2W;
            if (pi.S2WF == 0) pi.cos2WF = 1;
            else pi.cos2WF = pi.P2WF / pi.S2WF;
            if (pi.S2S == 0) pi.cos2S = 1;
            else pi.cos2S = pi.P2S / pi.S2S;
            if (pi.S2SF == 0) pi.cos2SF = 1;
            else pi.cos2SF = pi.P2SF / pi.S2SF;




            //обработка ошибки 
            if (pi.P == 0)
            {
                pi.K2W = 0;
                pi.K2WF = 0;
                pi.K2S = 0;
                pi.K2SF = 0;
            }
            else
            {
                pi.K2W = pi.P2W / pi.P;
                pi.K2WF = pi.P2WF / pi.P;
                pi.K2S = pi.P2S / pi.P;
                pi.K2SF = pi.P2SF / pi.P;
            }
           

            pi.I2W_L1 = pi.P_L1 * pi.K2W / (U1f * pi.cos2W);
            pi.I2W_L2 = pi.P_L2 * pi.K2W / (U1f * pi.cos2W);
            pi.I2W_L3 = pi.P_L3 * pi.K2W / (U1f * pi.cos2W);

            pi.I2WF_L1 = pi.P_L1 * pi.K2WF / (U1f * pi.cos2WF);
            pi.I2WF_L2 = pi.P_L2 * pi.K2WF / (U1f * pi.cos2WF);
            pi.I2WF_L3 = pi.P_L3 * pi.K2WF / (U1f * pi.cos2WF);

            pi.I2S_L1 = pi.P_L1 * pi.K2S/ (U1f * pi.cos2S);
            pi.I2S_L2 = pi.P_L2 * pi.K2S / (U1f * pi.cos2S);
            pi.I2S_L3 = pi.P_L3 * pi.K2S / (U1f * pi.cos2S);

            pi.I2SF_L1 = pi.P_L1 * pi.K2SF / (U1f * pi.cos2SF);
            pi.I2SF_L2 = pi.P_L2 * pi.K2SF / (U1f * pi.cos2SF);
            pi.I2SF_L3 = pi.P_L3 * pi.K2SF / (U1f * pi.cos2SF);


            pi.I2W_max = pi.I2W_L1;
            if (pi.I2W_max < pi.I2W_L2) pi.I2W_max = pi.I2W_L2;
            if (pi.I2W_max < pi.I2W_L3) pi.I2W_max = pi.I2W_L3;

            pi.I2WF_max = pi.I2WF_L1;
            if (pi.I2WF_max < pi.I2WF_L2) pi.I2WF_max = pi.I2WF_L2;
            if (pi.I2WF_max < pi.I2WF_L3) pi.I2WF_max = pi.I2WF_L3;

            pi.I2S_max = pi.I2S_L1;
            if (pi.I2S_max < pi.I2S_L2) pi.I2S_max = pi.I2S_L2;
            if (pi.I2S_max < pi.I2S_L3) pi.I2S_max = pi.I2S_L3;

            pi.I2SF_max = pi.I2SF_L1;
            if (pi.I2SF_max < pi.I2SF_L2) pi.I2SF_max = pi.I2SF_L2;
            if (pi.I2SF_max < pi.I2SF_L3) pi.I2SF_max = pi.I2SF_L3;

            //TODO: Запись в цепи ревит

            RecordingCalcPanel(pi);

        }

        static void RecordingCalcCirc(CircItem circ)
        {
            Element rCirc = Doc.GetElement(circ.Id);

            if (rCirc.LookupParameter("Pу_EVA").AsDouble() != circ.P) rCirc.LookupParameter("Pу_EVA").Set(circ.P);
            if (rCirc.LookupParameter("Sу_EVA").AsDouble() != circ.S) rCirc.LookupParameter("Sу_EVA").Set(circ.S);
            //TaskDialog.Show("sd", rCirc.Name + circ.Cos.ToString());
            if (rCirc.LookupParameter("Cos_EVA").AsDouble() != circ.Cos) rCirc.LookupParameter("Cos_EVA").Set(circ.Cos);
            if (rCirc.LookupParameter("Pр_отх_линии_EVA").AsDouble() != circ.P1) rCirc.LookupParameter("Pр_отх_линии_EVA").Set(circ.P1);
            if (rCirc.LookupParameter("Pр_щита_Зима_EVA").AsDouble() != circ.P2W) rCirc.LookupParameter("Pр_щита_Зима_EVA").Set(circ.P2W);
            if (rCirc.LookupParameter("Pр_на_вводах_ВРУ_Зима_EVA").AsDouble() != circ.P3W) rCirc.LookupParameter("Pр_на_вводах_ВРУ_Зима_EVA").Set(circ.P3W);
            if (rCirc.LookupParameter("Pр_ВРУ_авар_реж_Зима_EVA").AsDouble() != circ.P4W) rCirc.LookupParameter("Pр_ВРУ_авар_реж_Зима_EVA").Set(circ.P4W);
            if (rCirc.LookupParameter("Pр_щита_Зима_Пожар_EVA").AsDouble() != circ.P2WF) rCirc.LookupParameter("Pр_щита_Зима_Пожар_EVA").Set(circ.P2WF);
            if (rCirc.LookupParameter("Pр_на_вводах_ВРУ_Зима_Пожар_EVA").AsDouble() != circ.P3WF) rCirc.LookupParameter("Pр_на_вводах_ВРУ_Зима_Пожар_EVA").Set(circ.P3WF);
            if (rCirc.LookupParameter("Pр_ВРУ_авар_реж_Зима_Пожар_EVA").AsDouble() != circ.P4WF) rCirc.LookupParameter("Pр_ВРУ_авар_реж_Зима_Пожар_EVA").Set(circ.P4WF);
            if (rCirc.LookupParameter("Pр_щита_Лето_EVA").AsDouble() != circ.P2S) rCirc.LookupParameter("Pр_щита_Лето_EVA").Set(circ.P2S);
            if (rCirc.LookupParameter("Pр_на_вводах_ВРУ_Лето_EVA").AsDouble() != circ.P3S) rCirc.LookupParameter("Pр_на_вводах_ВРУ_Лето_EVA").Set(circ.P3S);
            if (rCirc.LookupParameter("Pр_ВРУ_авар_реж_Лето_EVA").AsDouble() != circ.P4S) rCirc.LookupParameter("Pр_ВРУ_авар_реж_Лето_EVA").Set(circ.P4S);
            if (rCirc.LookupParameter("Pр_щита_Лето_Пожар_EVA").AsDouble() != circ.P2SF) rCirc.LookupParameter("Pр_щита_Лето_Пожар_EVA").Set(circ.P2SF);
            if (rCirc.LookupParameter("Pр_на_вводах_ВРУ_Лето_Пожар_EVA").AsDouble() != circ.P3SF) rCirc.LookupParameter("Pр_на_вводах_ВРУ_Лето_Пожар_EVA").Set(circ.P3SF);
            if (rCirc.LookupParameter("Pр_ВРУ_авар_реж_Лето_Пожар_EVA").AsDouble() != circ.P4SF) rCirc.LookupParameter("Pр_ВРУ_авар_реж_Лето_Пожар_EVA").Set(circ.P4SF);

            if (rCirc.LookupParameter("Q_отх_линии_EVA").AsDouble() != circ.Q1) rCirc.LookupParameter("Q_отх_линии_EVA").Set(circ.Q1);
            if (rCirc.LookupParameter("Qр_щита_Зима_EVA").AsDouble() != circ.Q2W) rCirc.LookupParameter("Qр_щита_Зима_EVA").Set(circ.Q2W);
            if (rCirc.LookupParameter("Qр_на_вводах_ВРУ_Зима_EVA").AsDouble() != circ.Q3W) rCirc.LookupParameter("Qр_на_вводах_ВРУ_Зима_EVA").Set(circ.Q3W);
            if (rCirc.LookupParameter("Qр_ВРУ_авар_реж_Зима_EVA").AsDouble() != circ.Q4W) rCirc.LookupParameter("Qр_ВРУ_авар_реж_Зима_EVA").Set(circ.Q4W);
            if (rCirc.LookupParameter("Qр_щита_Зима_Пожар_EVA").AsDouble() != circ.Q2WF) rCirc.LookupParameter("Qр_щита_Зима_Пожар_EVA").Set(circ.Q2WF);
            if (rCirc.LookupParameter("Qр_на_вводах_ВРУ_Зима_Пожар_EVA").AsDouble() != circ.Q3WF) rCirc.LookupParameter("Qр_на_вводах_ВРУ_Зима_Пожар_EVA").Set(circ.Q3WF);
            if (rCirc.LookupParameter("Qр_ВРУ_авар_реж_Зима_Пожар_EVA").AsDouble() != circ.Q4WF) rCirc.LookupParameter("Qр_ВРУ_авар_реж_Зима_Пожар_EVA").Set(circ.Q4WF);
            if (rCirc.LookupParameter("Qр_щита_Лето_EVA").AsDouble() != circ.Q2S) rCirc.LookupParameter("Qр_щита_Лето_EVA").Set(circ.Q2S);
            if (rCirc.LookupParameter("Qр_на_вводах_ВРУ_Лето_EVA").AsDouble() != circ.Q3S) rCirc.LookupParameter("Qр_на_вводах_ВРУ_Лето_EVA").Set(circ.Q3S);
            if (rCirc.LookupParameter("Qр_ВРУ_авар_реж_Лето_EVA").AsDouble() != circ.Q4S) rCirc.LookupParameter("Qр_ВРУ_авар_реж_Лето_EVA").Set(circ.Q4S);
            if (rCirc.LookupParameter("Qр_щита_Лето_Пожар_EVA").AsDouble() != circ.Q2SF) rCirc.LookupParameter("Qр_щита_Лето_Пожар_EVA").Set(circ.Q2SF);
            if (rCirc.LookupParameter("Qр_на_вводах_ВРУ_Лето_Пожар_EVA").AsDouble() != circ.Q3SF) rCirc.LookupParameter("Qр_на_вводах_ВРУ_Лето_Пожар_EVA").Set(circ.Q3SF);
            if (rCirc.LookupParameter("Qр_ВРУ_авар_реж_Лето_Пожар_EVA").AsDouble() != circ.Q4SF) rCirc.LookupParameter("Qр_ВРУ_авар_реж_Лето_Пожар_EVA").Set(circ.Q4SF);

            if (rCirc.LookupParameter("Iр_отх_линии_L1_EVA").AsDouble() != circ.I1_L1) rCirc.LookupParameter("Iр_отх_линии_L1_EVA").Set(circ.I1_L1);
            if (rCirc.LookupParameter("Iр_отх_линии_L2_EVA").AsDouble() != circ.I1_L2) rCirc.LookupParameter("Iр_отх_линии_L2_EVA").Set(circ.I1_L2);
            if (rCirc.LookupParameter("Iр_отх_линии_L3_EVA").AsDouble() != circ.I1_L3) rCirc.LookupParameter("Iр_отх_линии_L3_EVA").Set(circ.I1_L3);

            if (rCirc.LookupParameter("S_отх_линии_EVA").AsDouble() != circ.S1) rCirc.LookupParameter("S_отх_линии_EVA").Set(circ.S1);
            if (rCirc.LookupParameter("Iр_отх_линии_EVA").AsDouble() != circ.I1_Max) rCirc.LookupParameter("Iр_отх_линии_EVA").Set(circ.I1_Max);



            if (rCirc.LookupParameter("Iр_отх_линии_EVA").AsDouble() != circ.I1_Max) rCirc.LookupParameter("Iр_отх_линии_EVA").Set(circ.I1_Max);

        }



        static void RecordingCalcPanel(PanelItem panel)
        {
            Element rPanel = Doc.GetElement(panel.Id);

            if (rPanel.LookupParameter("Pу_EVA").AsDouble() != panel.P) rPanel.LookupParameter("Pу_EVA").Set(panel.P);
            if (rPanel.LookupParameter("Pр_щита_Зима_EVA").AsDouble() != panel.P2W) rPanel.LookupParameter("Pр_щита_Зима_EVA").Set(panel.P2W);
            if (rPanel.LookupParameter("Pр_щита_Зима_Пожар_EVA").AsDouble() != panel.P2WF) rPanel.LookupParameter("Pр_щита_Зима_EVA").Set(panel.P2WF);
            if (rPanel.LookupParameter("Pр_щита_Лето_EVA").AsDouble() != panel.P2S) rPanel.LookupParameter("Pр_щита_Лето_EVA").Set(panel.P2S);
            if (rPanel.LookupParameter("Pр_щита_Лето_Пожар_EVA").AsDouble() != panel.P2SF) rPanel.LookupParameter("Pр_щита_Зима_EVA").Set(panel.P2SF);
            if (rPanel.LookupParameter("Qу_EVA").AsDouble() != panel.Q) rPanel.LookupParameter("Qу_EVA").Set(panel.Q);
            if (rPanel.LookupParameter("Qр_щита_Зима_EVA").AsDouble() != panel.Q2W) rPanel.LookupParameter("Qр_щита_Зима_EVA").Set(panel.Q2W);
            if (rPanel.LookupParameter("Qр_щита_Зима_Пожар_EVA").AsDouble() != panel.Q2WF) rPanel.LookupParameter("Qр_щита_Зима_Пожар_EVA").Set(panel.Q2WF);
            if (rPanel.LookupParameter("Qр_щита_Лето_EVA").AsDouble() != panel.Q2S) rPanel.LookupParameter("Pр_щита_Лето_EVA").Set(panel.Q2S);
            if (rPanel.LookupParameter("Qр_щита_Лето_Пожар_EVA").AsDouble() != panel.Q2SF) rPanel.LookupParameter("Qр_щита_Лето_Пожар_EVA").Set(panel.Q2SF);
            if (rPanel.LookupParameter("Pу_L1_EVA").AsDouble() != panel.P_L1) rPanel.LookupParameter("Pу_L1_EVA").Set(panel.P_L1);
            if (rPanel.LookupParameter("Pу_L2_EVA").AsDouble() != panel.P_L2) rPanel.LookupParameter("Pу_L2_EVA").Set(panel.P_L2);
            if (rPanel.LookupParameter("Pу_L3_EVA").AsDouble() != panel.P_L3) rPanel.LookupParameter("Pу_L3_EVA").Set(panel.P_L3);

            if (rPanel.LookupParameter("Sр_щита_Зима_EVA").AsDouble() != panel.S2W) rPanel.LookupParameter("Sр_щита_Зима_EVA").Set(panel.S2W);
            if (rPanel.LookupParameter("Sр_щита_Зима_Пожар_EVA").AsDouble() != panel.S2WF) rPanel.LookupParameter("Sр_щита_Зима_Пожар_EVA").Set(panel.S2WF);
            if (rPanel.LookupParameter("Sр_щита_Лето_EVA").AsDouble() != panel.S2S) rPanel.LookupParameter("Sр_щита_Лето_EVA").Set(panel.S2S);
            if (rPanel.LookupParameter("Sр_щита_Лето_Пожар_EVA").AsDouble() != panel.S2SF) rPanel.LookupParameter("Sр_щита_Лето_Пожар_EVA").Set(panel.S2SF);

            if (rPanel.LookupParameter("Cos_щита_Зима_EVA").AsDouble() != panel.cos2W) rPanel.LookupParameter("Cos_щита_Зима_EVA").Set(panel.cos2W);
            if (rPanel.LookupParameter("Cos_щита_Зима_Пожар_EVA").AsDouble() != panel.cos2WF) rPanel.LookupParameter("Cos_щита_Зима_Пожар_EVA").Set(panel.cos2WF);
            if (rPanel.LookupParameter("Cos_щита_Лето_EVA").AsDouble() != panel.cos2S) rPanel.LookupParameter("Cos_щита_Лето_EVA").Set(panel.cos2S);
            if (rPanel.LookupParameter("Cos_щита_Лето_Пожар_EVA").AsDouble() != panel.cos2SF) rPanel.LookupParameter("Cos_щита_Лето_Пожар_EVA").Set(panel.cos2SF);
            if (rPanel.LookupParameter("Кс_щита_Зима_EVA").AsDouble() != panel.K2W) rPanel.LookupParameter("Кс_щита_Зима_EVA").Set(panel.K2W);
            if (rPanel.LookupParameter("Кс_щита_Зима_Пожар_EVA").AsDouble() != panel.K2WF) rPanel.LookupParameter("Кс_щита_Зима_Пожар_EVA").Set(panel.K2WF);
            if (rPanel.LookupParameter("Кс_щита_Лето_EVA").AsDouble() != panel.K2S) rPanel.LookupParameter("Кс_щита_Лето_EVA").Set(panel.K2S);
            if (rPanel.LookupParameter("Кс_щита_Лето_Пожар_EVA").AsDouble() != panel.K2SF) rPanel.LookupParameter("Кс_щита_Лето_Пожар_EVA").Set(panel.K2SF);
            if (rPanel.LookupParameter("Iр_щита_Зима_L1_EVA").AsDouble() != panel.I2W_L1) rPanel.LookupParameter("Iр_щита_Зима_L1_EVA").Set(panel.I2W_L1);
            if (rPanel.LookupParameter("Iр_щита_Зима_L2_EVA").AsDouble() != panel.I2W_L2) rPanel.LookupParameter("Iр_щита_Зима_L2_EVA").Set(panel.I2W_L2);
            if (rPanel.LookupParameter("Iр_щита_Зима_L3_EVA").AsDouble() != panel.I2W_L3) rPanel.LookupParameter("Iр_щита_Зима_L3_EVA").Set(panel.I2W_L3);
            if (rPanel.LookupParameter("Iр_щита_Зима_EVA").AsDouble() != panel.I2W_max) rPanel.LookupParameter("Iр_щита_Зима_EVA").Set(panel.I2W_max);
            if (rPanel.LookupParameter("Iр_щита_Зима_Пожар_L1_EVA").AsDouble() != panel.I2WF_L1) rPanel.LookupParameter("Iр_щита_Зима_Пожар_L1_EVA").Set(panel.I2WF_L1);
            if (rPanel.LookupParameter("Iр_щита_Зима_Пожар_L2_EVA").AsDouble() != panel.I2WF_L2) rPanel.LookupParameter("Iр_щита_Зима_Пожар_L2_EVA").Set(panel.I2WF_L2);
            if (rPanel.LookupParameter("Iр_щита_Зима_Пожар_L3_EVA").AsDouble() != panel.I2WF_L3) rPanel.LookupParameter("Iр_щита_Зима_Пожар_L3_EVA").Set(panel.I2WF_L3);
            if (rPanel.LookupParameter("Iр_щита_Зима_Пожар_EVA").AsDouble() != panel.I2WF_max) rPanel.LookupParameter("Iр_щита_Зима_Пожар_EVA").Set(panel.I2WF_max);
            if (rPanel.LookupParameter("Iр_щита_Лето_L1_EVA").AsDouble() != panel.I2S_L1) rPanel.LookupParameter("Iр_щита_Лето_L1_EVA").Set(panel.I2S_L1);
            if (rPanel.LookupParameter("Iр_щита_Лето_L2_EVA").AsDouble() != panel.I2S_L2) rPanel.LookupParameter("Iр_щита_Лето_L2_EVA").Set(panel.I2S_L2);
            if (rPanel.LookupParameter("Iр_щита_Лето_L3_EVA").AsDouble() != panel.I2S_L3) rPanel.LookupParameter("Iр_щита_Лето_L3_EVA").Set(panel.I2S_L3);
            if (rPanel.LookupParameter("Iр_щита_Лето_EVA").AsDouble() != panel.I2S_max) rPanel.LookupParameter("Iр_щита_Лето_EVA").Set(panel.I2S_max);
            if (rPanel.LookupParameter("Iр_щита_Лето_Пожар_L1_EVA").AsDouble() != panel.I2SF_L1) rPanel.LookupParameter("Iр_щита_Лето_Пожар_L1_EVA").Set(panel.I2SF_L1);
            if (rPanel.LookupParameter("Iр_щита_Лето_Пожар_L2_EVA").AsDouble() != panel.I2SF_L2) rPanel.LookupParameter("Iр_щита_Лето_Пожар_L2_EVA").Set(panel.I2SF_L2);
            if (rPanel.LookupParameter("Iр_щита_Лето_Пожар_L3_EVA").AsDouble() != panel.I2SF_L3) rPanel.LookupParameter("Iр_щита_Лето_Пожар_L3_EVA").Set(panel.I2SF_L3);
            if (rPanel.LookupParameter("Iр_щита_Лето_Пожар_EVA").AsDouble() != panel.I2SF_max) rPanel.LookupParameter("Iр_щита_Лето_Пожар_EVA").Set(panel.I2SF_max);


        }




    }
}
