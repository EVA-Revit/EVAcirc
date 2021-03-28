using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVA_Gen.WPF.Models
{
    internal class ElectricalInfo
    {
        public string Name { get; set; }
        public string P_ust { get; set; }
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

        public ObservableCollection<PanelItem> Nodes { get; set; }

    }
}
