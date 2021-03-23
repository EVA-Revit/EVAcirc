using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EVA_Gen.WPF.ViewModels.Base
{
    internal abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; //событие, реализация интерфейса

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            //Генерация события
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        //Запретить кольцевые обновление свойств
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false; //если значение поля которое мы хотим обновить, уже обновлено
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
    }
}
