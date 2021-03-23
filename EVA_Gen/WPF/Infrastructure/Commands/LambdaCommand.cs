using EVA_Gen.WPF.Infrastructure.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVA_Gen.WPF.Infrastructure.Commands
{
    internal class LambdaCommand : CommandWPF
    {
        //поля
        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;

        //Конструктор
        public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            //Получение двух делигатов
            _Execute = Execute ?? throw new ArgumentNullException(nameof(Execute)); //исключение если нет ссылки на делигат
            _CanExecute = CanExecute;
        }
        public override bool CanExecute(object parameter) => _CanExecute?.Invoke(parameter) ?? true; // Если нет делигата всё равно выполнять команду



        public override void Execute(object parameter)
        {
            _Execute(parameter);
        }
    }
}
