using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.OpenGL.Surfaces;
using Oktato_ora_konyvelo.ViewModels;

namespace Oktato_ora_konyvelo.Classes
{
    public partial class Lesson : ObservableObject
    {
        [ObservableProperty] private DateOnly date;
        [ObservableProperty] private TimeOnly startTime;
        [ObservableProperty] private TimeOnly endTime;
        [ObservableProperty] private int drivenMinutes;
        [ObservableProperty] private int allDrivenMinutes;
        [ObservableProperty] private Student student;
        [ObservableProperty] private LessonType type;
        [ObservableProperty] private string startPlace;
        [ObservableProperty] private string endPlace;
        [ObservableProperty] private int meterAtStart;
        [ObservableProperty] private int meterAtEnd;
        [ObservableProperty] private int drivenKm;
        [ObservableProperty] private int allKm;
        [ObservableProperty] private bool isRecordedInKVAR;

        private ObservableCollection<Lesson> PreviousLessons = [];
        private ObservableCollection<Lesson> AllLessons;
        public Lesson(DateOnly date,
                      TimeOnly start,
                      TimeOnly end,
                      Student student,
                      LessonType lessonType,
                      string startPlace,
                      string endPlace,
                      int drivenKm,
                      bool isRecordedInKvar,
                      ObservableCollection<Lesson> allLessons,
                      int meterAtStart)
        {
            Date = date;
            StartTime = start;
            EndTime = end;
            Student = student;
            Type = lessonType;
            StartPlace = startPlace;
            EndPlace = endPlace;
            DrivenKm = drivenKm;
            IsRecordedInKVAR = isRecordedInKvar;
            AllLessons = allLessons;
            
            DrivenMinutes = Convert.ToInt32((EndTime - StartTime).TotalMinutes);
            AllDrivenMinutes = DrivenMinutes;
            MeterAtStart = meterAtStart;
            MeterAtEnd = MeterAtStart/*TODO: Kicserélni a settings-ben eltárolt értékre!*/ + DrivenKm;
            AllKm = DrivenKm;
        }
        public void GetCurrentStudentPreviousLessons(ObservableCollection<Lesson> AllLessons)
        {
            foreach (Lesson item in AllLessons.Where(x => x.Student.Id.Equals(Student.Id)).Where(x => x.Date <= Date))//Jelen tanuló összes órája
            {
                PreviousLessons.Add(item);
            }

            foreach (Lesson item in AllLessons.Where(x=>x.Date.Equals(Date) && x.StartTime >= EndTime))//Ha a mai napon van és későbbi óra ennél
            {
                PreviousLessons.Remove(item);//Törlés
            }

            PreviousLessons = new ObservableCollection<Lesson>(PreviousLessons.OrderBy(x=>x.Date).ThenBy(x=>x.StartTime));//Rendezés dátum és idő szerint
        }
        public void UpdateData()
        {
            DrivenMinutes = Convert.ToInt32((EndTime - StartTime).TotalMinutes);
            AllDrivenMinutes = PreviousLessons.Sum(x => x.DrivenMinutes) + DrivenMinutes;
            MeterAtEnd = MeterAtStart + DrivenKm;
            AllKm = PreviousLessons.Sum(x => x.DrivenKm) + DrivenKm;
        }
    }
}