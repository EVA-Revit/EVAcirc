using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVA_Gen.WPF.ViewModels.Base; 

namespace EVA_Gen.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private string _Title = "E V A";

        #region Заголовок окна
        /// <summary>Заголовок окна</summary>
        public string Title
        {
            get => _Title;

            //Если бы не определили метод Set в базовом классе
            //set
            //{
            //    if (Equals(_Title, value)) return;
            //    _Title = value;
            //    OnPropertyChanged();
            //}

            //обычное переопределение метода
            //set
            //{
            //    Set(ref _Title, value);
            //}

            //короткое переопределение
            set => Set(ref _Title, value);
        }
        #endregion


    }
}
