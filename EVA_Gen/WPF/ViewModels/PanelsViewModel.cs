using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EVA_Gen.WPF.Infrastructure.Commands;
using EVA_Gen.WPF.ViewModels.Base;

namespace EVA_Gen.WPF.ViewModels
{
    class PanelsViewModel 
    {
        public string NameS;



        public string Name { get => NameS; set { NameS = value; } }

        public ObservableCollection<PanelsViewModel> Children { get; set; }


        public PanelsViewModel()
        {
            Children = new ObservableCollection<PanelsViewModel>();
        }


            


    }
}
