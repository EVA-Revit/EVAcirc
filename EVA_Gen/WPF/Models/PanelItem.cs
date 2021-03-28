using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace EVA_Gen.WPF.Models
{
    internal class ElectricalInfo
    {
        public string Name { get; set; }
        public int P_ust { get; set; }
        public string P_r { get; set; }
    }

    internal class CircItem : ElectricalInfo
    {
        public string AppZ { get; set; }
        public int Length { get; set; }

        
    }

    internal class PanelItem : ElectricalInfo
    {
        public string AppVvod { get; set; }
        public int CountGroup { get; set; }



        private bool _Is_Checked;
        public bool Is_Checked
        {
            get => _Is_Checked;


            set
            {
                if (Equals(_Is_Checked, value)) return;
                _Is_Checked = value;
                TaskDialog.Show("fafaf", "aafaf");
            }
        }

        public List<PanelItem> Nodes { get; set; }


        //Цепи щита
        public ObservableCollection<CircItem> Circuits { get; set; }


        //Конструктор
        public PanelItem()
        {
            Nodes = new List<PanelItem>();
            Circuits = new ObservableCollection<CircItem>();
        }

    }
}
