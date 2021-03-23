using EVA_Gen.WPF.Infrastructure.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace EVA_Gen.WPF.Infrastructure.Commands
{
    class TestCommand : CommandWPF
    {
        public override bool CanExecute(object parameter) => true;


        //логика
        public override void Execute(object parameter)
        {
            TaskDialog.Show("Testing", "Hello world");
        }
    }
}
