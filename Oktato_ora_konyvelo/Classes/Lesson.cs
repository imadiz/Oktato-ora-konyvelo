using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [ObservableProperty] private string lessonType;
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
                      string lessonType,
                      string startPlace,
                      string endPlace,
                      int drivenKm,
                      bool isRecordedInKvar,
                      ObservableCollection<Lesson> allLessons,
                      bool firstLesson = false,
                      int meterAtStart = 0)
        {
            Date = date;
            StartTime = start;
            EndTime = end;
            Student = student;
            LessonType = lessonType;
            StartPlace = startPlace;
            EndPlace = endPlace;
            DrivenKm = drivenKm;
            IsRecordedInKVAR = isRecordedInKvar;
            AllLessons = allLessons;

            if (!firstLesson)
            {//Ha nem az első óra
                GetCurrentStudentPreviousLessons(allLessons);
                UpdateData();
            }
            else//Ha az első óra
            {
                DrivenMinutes = Convert.ToInt32((EndTime - StartTime).TotalMinutes);
                AllDrivenMinutes = DrivenMinutes;
                MeterAtEnd = MeterAtStart/*TODO: Kicserélni a settings-ben eltárolt értékre!*/ + DrivenKm;
                AllKm = DrivenKm;
            }

            if (!meterAtStart.Equals(0))
            {
                MeterAtStart = meterAtStart;
            }
            else
            {
                MeterAtStart = meterAtStart;
            }
            
        }
        public void GetCurrentStudentPreviousLessons(ObservableCollection<Lesson> AllLessons)
        {
            foreach (Lesson item in AllLessons.Where(x => x.Student.Id.Equals(Student.Id)).Where(x => x.Date <= Date))//Jelen tanuló összes órája
            {
                PreviousLessons.Add(item);
            }

            PreviousLessons = new ObservableCollection<Lesson>(PreviousLessons.OrderBy(x=>x.Date).OrderBy(x=>x.StartTime));//Rendezés dátum és idő szerint
        }
        public void UpdateData()
        {
            DrivenMinutes = Convert.ToInt32((EndTime - StartTime).TotalMinutes);
            AllDrivenMinutes = PreviousLessons.Sum(x => x.DrivenMinutes) + DrivenMinutes;
            MeterAtStart = AllLessons.OrderBy(x => x.Date).OrderBy(x => x.StartTime).Last().MeterAtEnd;
            MeterAtEnd = MeterAtStart + DrivenKm;
            AllKm = PreviousLessons.Sum(x => x.DrivenKm) + DrivenKm;
        }
    }
}