using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Oktato_ora_konyvelo.ViewModels;

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
                CalculateData();
            }
        }

        private LessonType? type;
        public LessonType? Type
        {
            get => type;
            set
            {
                if (SetProperty(ref type, value, true))
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

        private int? previousLessonCount;
        public int? PreviousLessonCount
        {
            get => previousLessonCount;
            set => SetProperty(ref previousLessonCount, value);
        }

        private int? occasionCount;
        public int? OccasionCount
        {
            get => occasionCount;
            set => SetProperty(ref occasionCount, value);
        }

        private ObservableCollection<Lesson> allLessons;
        public ToBeAddedLesson(ObservableCollection<Lesson> allLessons)
        {
            this.allLessons = allLessons;
            CalculateData();
            //TODO: mindent ami bemenet nullable értékké varázsolni, a validatorok kezelik okosan.
            //Messaging-el értesíteni a felületet, hogy amíg az összes érték nem helyes, addig nem lehet órát hozzáadni.
            //Minden fontos UI-on látszódó érték változásakor újraszámolni mindent.
            //BlackoutDates a CalendarDatePicker-en!
        }
        public void CalculateData()
        {
            if (CurrentStudent is null) return;//Ha nincs tanuló kiválasztva, akkor ne számolj semmit
            
            MeterAtStart = CurrentStudent.StudentLessons.Any() ? CurrentStudent.StudentLessons.Last().MeterAtEnd : 0;//Km óra tanóra elején = utolsó óra + vezetett km
            MeterAtEnd = DrivenKm is null ? meterAtStart : meterAtStart + drivenKm;//Km óra tanóra végén = Km óra tanóra elején + vezetett km
            AllKm = CurrentStudent.StudentLessons.Any() ? CurrentStudent.StudentLessons.Sum(x => x.DrivenKm) + DrivenKm : 0 + DrivenKm;//Göngyölt km = Eddigi összes vezetett km + vezetett km
            
            EndTime = !StartTime.Equals(new(0,00)) ? StartTime.AddMinutes(DrivenMinutes) : new(0, 00);//Óra végidőpontja = Óra kezdete + vezetett percek
            AllDrivenMinutes = CurrentStudent.StudentLessons.Any() ? CurrentStudent.StudentLessons.Sum(x => x.DrivenMinutes) + DrivenMinutes : DrivenMinutes;//Göngyölt perc = Eddigi összes vezetett perc + vezetett perc

            PreviousLessonCount = CurrentStudent is null ? null : AllDrivenMinutes / 50;//Eddigi órák száma (Az alkalom 100 perc(ergo. 2 óra)!)
            OccasionCount = CurrentStudent!.StudentLessons.Count;//Eddigi alkalmak száma
        }

        #region Validation
        public static ValidationResult? ValidateDate(DateOnly? dateinput, ValidationContext validationContext)
        {
            ToBeAddedLesson instance = (ToBeAddedLesson)validationContext.ObjectInstance;
            
            if (dateinput is null) return null;//Ha nincs érték, nincs visszajelzés
            
            if (instance.allLessons.Count(x=>x.Date.Equals(dateinput)) >= 5) //Ha már megvan az 5 tanulós limit egy napra
                return new ValidationResult("Nem lehet több tanuló a kiválasztott napon!");
            
            return ValidationResult.Success;
        }
        public static ValidationResult? ValidateDrivenKm(int? km, ValidationContext validationContext)
        {
            if (km is null)//Ha nincs érték, nincs visszajelzés
                return null;
            
            if (km < 1)
                return new("A megadott úthossz túl kevés!");
            
            if (km > 100)
                return new("A megadott úthossz túl sok!");
            
            return ValidationResult.Success;
        }
        #endregion
    }
}