using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Oktato_ora_konyvelo.Classes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Oktato_ora_konyvelo.ViewModels;

namespace Oktato_ora_konyvelo.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        #region Data
        [ObservableProperty]
        ObservableCollection<Lesson> allLessons = new();

        [ObservableProperty]
        ObservableCollection<Student> allStudents = new();

        [ObservableProperty]
        Settings allSettings = new("18519", "Anfi1978");

        [ObservableProperty]
        ToBeAddedLesson tempLesson;
        #endregion

        #region Manage files
        public void ReadAllData()
        {

        }
        public void SaveAllData()
        {

        }
        #endregion

        public void AddLesson()
        {
            Student CurrentStudent = AllStudents[new Random().Next(2)];
            AllLessons.Add(new Lesson(new DateOnly(2024, 08, 01),
                                      new TimeOnly(08, 00),
                                      new TimeOnly(9, 40),
                                      CurrentStudent,
                                      "F/o",
                                      "Autósiskola",
                                      "Autósiskola",
                                      new Random().Next(1, 30),
                                      Convert.ToBoolean(new Random().Next(2)),
                                      AllLessons));
        }

        #region Tanuló adatok frissítése
        public void UpdateStudentData()
        {
            foreach (Student item in AllStudents)
            {
                item.UpdateData();
            }
            TempLesson.GetCurrentStudentPreviousLessons(AllLessons);
        }

        private void AllLessons_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateStudentData();
        }
        #endregion

        public MainViewModel()
        {
            TempLesson = new ToBeAddedLesson(AllLessons);

            AllStudents.Add(new Student("Teszt Elek (12345678)", AllLessons));
            AllStudents.Add(new Student("Michael Myers (87654321)", AllLessons));

            AllLessons.CollectionChanged += AllLessons_CollectionChanged;

            AllLessons.Add(new Lesson(new DateOnly(2024, 08, 01),
                                      new TimeOnly(08, 00),
                                      new TimeOnly(9, 40),
                                      AllStudents[0],
                                      "A",
                                      "Autósiskola",
                                      "Autósiskola",
                                      18,
                                      false,
                                      AllLessons,
                                      true,
                                      511000));

            AllLessons.Add(new Lesson(new DateOnly(2024, 09, 03),
                                      new TimeOnly(10, 00),
                                      new TimeOnly(11, 40),
                                      AllStudents[1],
                                      "F/o",
                                      "Autósiskola",
                                      "Autósiskola",
                                      511050,
                                      true,
                                      AllLessons));
        }
    }
}
