using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oktato_ora_konyvelo.Classes
{
    public partial class ToBeAddedLesson : ObservableValidator
    {
        private DateOnly date;
        [CustomValidation(typeof(ToBeAddedLesson), nameof(ValidateDate))]
        public DateOnly Date
        {
            get => date;
            set
            {
                if (SetProperty(ref date, value, true)) CalculateData();
            }
        }

        private TimeOnly startTime;
        public TimeOnly StartTime
        {
            get => startTime;
            set
            {
                if (SetProperty(ref startTime, value, true))
                {
                    CalculateData();
                }
            }
        }

        private TimeOnly endTime;
        public TimeOnly EndTime
        {
            get => endTime;
            set
            {
                if (SetProperty(ref endTime, value, true))
                {
                    CalculateData();
                }
            }
        }

        private int drivenMinutes;
        public int DrivenMinutes
        {
            get => drivenMinutes;
            set
            {
                if (SetProperty(ref drivenMinutes, value, true))
                {
                    CalculateData();
                }
            }
        }

        private int allDrivenMinutes;
        public int AllDrivenMinutes
        {
            get => allDrivenMinutes;
            set
            {
                if (SetProperty(ref allDrivenMinutes, value, true))
                {
                    CalculateData();
                }
            }
        }

        private Student? student;
        public Student? CurrentStudent
        {
            get => student;
            set
            {
                if (!SetProperty(ref student, value, true)) return;
                GetCurrentStudentPreviousLessons(allLessons);
                CalculateData();
            }
        }

        private string? lessonType;
        public string? LessonType
        {
            get => lessonType;
            set
            {
                if (SetProperty(ref lessonType, value, true))
                {
                    CalculateData();
                }
            }
        }

        private string? startPlace;
        public string? StartPlace
        {
            get => startPlace;
            set => SetProperty(ref startPlace, value);
        }

        private string? endPlace;
        public string? EndPlace
        {
            get => endPlace;
            set => SetProperty(ref endPlace, value);
        }

        private int meterAtStart;
        public int MeterAtStart
        {
            get => meterAtStart;
            set => SetProperty(ref meterAtStart, value);
        }

        private int? meterAtEnd;
        public int? MeterAtEnd
        {
            get => meterAtEnd;
            set => SetProperty(ref meterAtEnd, value);
        }

        private int? drivenKm = 0;
        [CustomValidation(typeof(ToBeAddedLesson), nameof(ValidateDrivenKm))]
        public int? DrivenKm
        {
            get => drivenKm;
            set
            {
                if (SetProperty(ref drivenKm, value, true))
                {
                    CalculateData();
                }
            }
        }

        private int? allKm;
        public int? AllKm
        {
            get => allKm;
            set => SetProperty(ref allKm, value);
        }

        private ObservableCollection<Lesson> previousLessons = [];
        private ObservableCollection<Lesson> allLessons;

        public ToBeAddedLesson(ObservableCollection<Lesson> allLessons)
        {
            this.allLessons = allLessons;

            GetCurrentStudentPreviousLessons(this.allLessons);
            CalculateData();
            //TODO: mindent ami bemenet nullable értékké varázsolni, a validatorok kezelik okosan.
            //Messaging-el értesíteni a felületet, hogy amíg az összes érték nem helyes, addig nem lehet órát hozzáadni.
            //Minden fontos UI-on látszódó érték változásakor újraszámolni mindent.
        }
        public void GetCurrentStudentPreviousLessons(ObservableCollection<Lesson> allLessons)
        {
            if (CurrentStudent is null)
            {
                return;
            }

            foreach (Lesson item in allLessons.Where(x => x.Student.Id.Equals(CurrentStudent.Id)).Where(x => x.Date <= new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)))//Jelen tanuló összes órája
            {
                previousLessons.Add(item);
            }

            previousLessons = new(previousLessons.OrderBy(x => x.Date).ThenBy(x => x.StartTime));//Rendezés dátum és idő szerint
        }
        public void CalculateData()
        {
            MeterAtStart = previousLessons.Any() ? previousLessons.Last().MeterAtEnd : 0;//Km óra tanóra elején = utolsó óra + vezetett km
            MeterAtEnd = DrivenKm is null ? meterAtStart : meterAtStart + drivenKm;//Km óra tanóra végén = Km óra tanóra elején + vezetett km
            AllKm = previousLessons.Any() ? previousLessons.Sum(x => x.DrivenKm) + DrivenKm : 0 + DrivenKm;//Göngyölt km = Eddigi összes vezetett km + vezetett km
            EndTime = !StartTime.Equals(new(0,00)) ? StartTime.AddMinutes(DrivenMinutes) : new(0, 00);//Óra végidőpontja = Óra kezdete + vezetett percek
            AllDrivenMinutes = previousLessons.Any() ? previousLessons.Sum(x => x.DrivenMinutes) + DrivenMinutes : DrivenMinutes;//Göngyölt perc = Eddigi összes vezetett perc + vezetett perc
        }

        public static ValidationResult? ValidateDate(DateTime date, ValidationContext validationContext)
        {
            if (DateTime.Compare(date, DateTime.Now) < 0)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new("A dátumnak régebbinek kell lennie a mai napnál!");
            }
        }
        public static ValidationResult? ValidateDrivenKm(int? km, ValidationContext validationContext)
        {
            if (km is null)
            {
                return new("Nem megfelelő érték!");
            }
            
            if (km < 1)
            {
                return new("A megadott úthossz túl kevés!");
            }
            
            if (km > 100)
            {
                 return new("A megadott úthossz túl sok!");
            }
            
            return ValidationResult.Success;
        }
    }
}