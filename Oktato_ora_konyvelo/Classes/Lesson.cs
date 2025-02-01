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
        [ObservableProperty] private DateOnly date; //Dátum
        [ObservableProperty] private TimeOnly startTime; //Kezdő időpont
        [ObservableProperty] private TimeOnly endTime; //Befejezési időpont
        [ObservableProperty] private int drivenMinutes; //Az alkalmon vezetett percek száma
        [ObservableProperty] private int allDrivenMinutes; //Eddig az összes alkalmakon vezetett percek száma
        [ObservableProperty] private Student student; //Tanuló, aki részt vett az órán
        [ObservableProperty] private LessonType type; //Óra jellege
        [ObservableProperty] private string startPlace; //Kezdő helyszín
        [ObservableProperty] private string endPlace; //Befejezési helyszín
        [ObservableProperty] private int meterAtStart; //Kilóméteróra állás az alkalom elején
        [ObservableProperty] private int meterAtEnd; //Kilóméteróra állás az alkalom végén
        [ObservableProperty] private int drivenKm; //Az alkalom alatt vezetett út hossza
        [ObservableProperty] private int allKm; //Eddig az összes alkalmakon vezetett út hossza
        [ObservableProperty] private bool isRecordedInKVAR; //Rögzítve van-e az óra a KVAR rendszerben

        private ObservableCollection<Lesson> PreviousLessons = []; //Az eddigi órák listája
        public Lesson(DateOnly date,
                      TimeOnly start,
                      TimeOnly end,
                      Student student,
                      LessonType lessonType,
                      string startPlace,
                      string endPlace,
                      int drivenKm,
                      bool isRecordedInKvar,
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
                PreviousLessons.Add(item); //Eddigi órákhoz adás
            }

            foreach (Lesson item in AllLessons.Where(x=>x.Date.Equals(Date) && x.StartTime >= EndTime))//Ha a mai napon van és későbbi óra ennél
            {
                PreviousLessons.Remove(item);//Törlés
            }

            PreviousLessons = new ObservableCollection<Lesson>(PreviousLessons.OrderBy(x=>x.Date).ThenBy(x=>x.StartTime));//Rendezés dátum és idő szerint
        }
        public void UpdateData()
        {
            DrivenMinutes = Convert.ToInt32((EndTime - StartTime).TotalMinutes); //Az alkalmon vezetett idő (Befejezési időpont - Kezdeti időpont)
            AllDrivenMinutes = PreviousLessons.Sum(x => x.DrivenMinutes) + DrivenMinutes; //Az eddig összes vezetett percek száma (Az eddigi alkalmakon vezetett percek + A jelen alkalom időtartama)
            MeterAtEnd = MeterAtStart + DrivenKm; //A kilóméteróraállás az alkalom végén (A kmóraállás az óra elején + a vezetett km)
            AllKm = PreviousLessons.Sum(x => x.DrivenKm) + DrivenKm; //Az eddig vezetett összes úthossz (Az eddigi alkalmakon levezetett úthossz + a jelen alkalom vezetett hossza)
        }
    }
}