using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EVA_Gen.WPF.Infrastructure.Commands;
using EVA_Gen.WPF.ViewModels.Base;
using System.Windows;

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


        #region Команды

        #region ClosedWindowCommand
        //свойство
        public ICommand CloseWindowCommand { get; }

        private bool CanCloseWindowCommandExecuted(object p) => true;
        private void OnCloseWindowCommandExecuted(object p)
        {
            //Application.Current.Shutdown(); //закрытие текущего приложения
        }

        //public ICommand TestCommand { get; }

        #endregion
        #region TestCommand

        #endregion


        #endregion


        //Конструктор
        public MainWindowViewModel()
        {
            #region Команды
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecuted);

            #endregion

        }
    }
}
