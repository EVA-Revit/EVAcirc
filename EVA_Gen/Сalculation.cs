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
                        circ.P += elItem.S * elItem.Cos;
                        circ.Q += elItem.S * Math.Sqrt(1 - elItem.Cos * elItem.Cos);

                        switch (circ.Phase_Connection)
                        {
                            case "L1":
                                circ.P_L1 += elItem.S * elItem.Cos;
                                break;
                            case "L2":
                                circ.P_L2 += elItem.S * elItem.Cos;
                                break;
                            case "L3":
                                circ.P_L3 += elItem.S * elItem.Cos;
                                break;
                            default:
                                circ.P_L1 += (elItem.S * elItem.Cos) / 3;
                                circ.P_L2 += (elItem.S * elItem.Cos) / 3;
                                circ.P_L3 += (elItem.S * elItem.Cos) / 3;
                                break;
                        }
                    }
                    else
                    {

                    }
                   
                }
                circ.S = Math.Sqrt(circ.P * circ.P + circ.Q * circ.Q);
                circ.Cos = circ.P / circ.S;
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
            pi.cos2W = pi.P2W / pi.S2W;
            pi.cos2WF = pi.P2WF / pi.S2WF;
            pi.cos2S = pi.P2S / pi.S2S;
            pi.cos2SF = pi.P2SF / pi.S2SF;
            pi.K2W = pi.P2W / pi.P;
            pi.K2WF = pi.P2WF / pi.P;
            pi.K2S = pi.P2S / pi.P;
            pi.K2SF = pi.P2SF / pi.P;

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



        }


    }
}
