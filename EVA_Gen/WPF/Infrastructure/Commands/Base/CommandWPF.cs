using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EVA_Gen.WPF.Infrastructure.Commands.Base
{
    internal abstract class CommandWPF : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            //Отдаем управление событием классу CommandManager WPF генерирует это событие во всех командах
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public abstract bool CanExecute(object parameter);



        public abstract void Execute(object parameter);

    }
}
