using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

namespace EVA_Gen.WPF.Models
{
    internal class ElectricalInfo
    {
        public string Name { get; set; }
        public int P_ust { get; set; }
        public string P_r { get; set; }

        public int elId { get; set; }

        public Element Element { get; set; }

    }

    internal class CircItem : ElectricalInfo
    {
        public string AppZ { get; set; }
        public int Length { get; set; }
        public string Mark_Cable_2 { get; set; }
        public string Mark_Cable_1 { get; set; }
        public string WayOfLaying { get; set; }

        public string SectionCable2 { get; set; }
        public string L_pipe { get; set; }

        public CircItem(ElectricalSystem rCirc)
        {
            //заполнение свойств
            if(rCirc.CircuitType == CircuitType.Circuit)
            {
                Name = rCirc.Name;

                Mark_Cable_2 = rCirc.LookupParameter("Марка_кабеля_2_EVA").AsString();
                Mark_Cable_1 = rCirc.LookupParameter("Марка_кабеля_1_EVA").AsString();
                WayOfLaying = rCirc.LookupParameter("Способ_прокладки_EVA").AsString();

                SectionCable2 = rCirc.LookupParameter("Сечение_кабеля_2_EVA").AsString();

                L_pipe = rCirc.LookupParameter("L_трубы_EVA").ToString();
            }
            




        }

        
    }

    internal class PanelItem : ElectricalInfo
    {
        public string AppVvod { get; set; }
        public int CountGroup { get; set; }

        public Element Rboard { get; set; }

        CircItem CircBoard { get; set; }

        private bool _Is_Checked;
        public bool Is_Checked
        {
            get => _Is_Checked;


            set
            {
                if (Equals(_Is_Checked, value)) return;
                _Is_Checked = value;
            }
        }

        public List<PanelItem> SubPanels { get; set; }


        //Цепи щита
        public ObservableCollection<CircItem> Circuits { get; set; }


        //Конструктор
        public PanelItem(Element panelRevit)
        {
            SubPanels = new List<PanelItem>();
            Circuits = new ObservableCollection<CircItem>();

            //Заполнение свойств паенели
            Name = panelRevit.Name;
            elId = panelRevit.Id.IntegerValue;


            //Заполнение свойств цепей
            Circuits = Utilits.GetSortedCircuits(panelRevit, out CircItem circBoard); //отсортированные цепи
            CircBoard = circBoard;
            CountGroup = Circuits.Count;

            Rboard = panelRevit;

            //CountGroup=panelRevit.

        }

    }
}
