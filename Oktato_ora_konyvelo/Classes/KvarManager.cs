using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oktato_ora_konyvelo.ViewModels;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Oktato_ora_konyvelo.Classes;

public class KvarManager
{
    private enum Pages
    {
        VezetesiKarton,
        GyakorlatiOra,
        Helyszin,
        Jarmu
    }
    private ChromeDriver driver { get; set; }
    private IDictionary<string, IWebElement> NavBarButtons { get; set; }
    private string KvarUser { get; set; }
    private string KvarPwd { get; set; }
    
    private void ReadCredentials()
    {
        string creds = File.ReadAllText("./creds.txt", Encoding.Default);//Bejelentkezési adatok helye
        KvarUser = creds.Split(';')[0];//Felh. név
        KvarPwd = creds.Split(';')[1];//Jelszó
    }

    private void Login()
    {
        driver.Navigate().GoToUrl(@"https://ekapuauth.kavk.hu/Szakoktato");

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
        driver.FindElement(By.Name("username")).SendKeys(KvarUser);//Username mező megkeresése és user beleírása
        driver.FindElement(By.Name("password")).SendKeys(KvarPwd);//Ugyanez a jelszóval

        IWebElement LoginButton = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/main/div/div/div/div[3]/form/div[4]"));//Bejelentkezés gomb
        
        //A wait.Until(...) újrakezdi a futását false visszatérésnél, és kilép a true-nál
        
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5))//Várj x másodpercet, amíg megnyomható lesz a login gomb
        {
            PollingInterval = TimeSpan.FromMilliseconds(200) //0,2 másodpercenként nézd az oldal változásait
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(ElementNotInteractableException));//Nem baj, ha nem megnyomható a gomb, vagy nem látszik a kijelentkezés, próbáld

        LoginButton.Click(); //Gomb megnyomása
        
        wait.Until(x =>
        {
            try
            {
                driver.FindElement(By.Id("logout")); //Ha exception-t dob 
            }
            catch (Exception e)
            {
                return false;//nincs még ilyen, akkor újrapróbálkozás
            }
            
            return true;//Ha látszik a kijelentkezés gomb, akkor kilépés
        });
        
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(300));//Várj max 5 percet, amíg a kétlépcsős bejelentkezés sikerül
        wait.Until(x =>
        {
            if (x.Url.Equals(@"https://kokeny.kavk.hu/szakoktato")) return true;//Ha ez a jelenlegi url, siker

            return false;//Újra
        });
    }

    private void InitDriver()
    {
        ChromeOptions options = new();
        options.AddArgument("--start-maximized");//Fullscreen legyen
        options.LeaveBrowserRunning = false;//Bezáródjon a programmal együtt
        options.PageLoadStrategy = PageLoadStrategy.Normal;//Várd meg amíg teljesen betölt az oldal
        
        driver = new ChromeDriver(options);
    }

    public IList<Place> GetLocations()
    {
        ChangePage(Pages.Helyszin);
        IList<Place> Places = [];

        IWebElement table = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/div[1]/div[2]/table"));//Helyek table
        IList<IWebElement> rows = table.FindElements(By.TagName("tr"));//Sorok

        foreach (IWebElement row in rows)//Végiglépkedés a sorokon
        {
            IList<IWebElement> cells = row.FindElements(By.TagName("td"));//Sorok cellái
            Places.Add(new Place(cells[0].Text,//Hely neve
                cells[1].FindElement(By.TagName("i"))//Indulási helyszín-e
                    .GetDomAttribute("oldTitle")
                    .Equals("Igen")//Ha igen a tooltip (ha a kurzor felette van, igennel jelez amikor be van pipálva, de valamiért oldTitle a tulajdonság neve (???))
                    ? true
                    : false,
                cells[2].FindElement(By.TagName("i"))//Érkezési helyszín-e
                    .GetDomAttribute("oldTitle")
                    .Equals("Igen")
                    ? true
                    : false,
                cells[3].Text));//Hely címe
        }
        return Places;
    }

    public IList<Vehicle> GetVehicles()
    {
        ChangePage(Pages.Jarmu);
        IList<Vehicle> Vehicles = [];

        IWebElement table = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/div[1]/div[2]/table"));//Járművek table
        IList<IWebElement> rows = table.FindElements(By.TagName("tr"));//Sorok

        foreach (IWebElement row in rows)//Végiglépkedés a sorokon
        {
            IList<IWebElement> cells = row.FindElements(By.TagName("td"));//Sorok cellái
            Vehicles.Add(new Vehicle(cells[0].Text/*Rendszám*/, Vehicle.VehicleCategory.B/*Kategória*/, false/*Vontathat-e*/));//TODO: Megcsinálni rendesen, jelenleg csak tesztelésre!
        }
        
        return Vehicles;
    }

    public Task<List<Student>> GetAllStudents(ObservableCollection<Lesson> allLessons)
    {
        List<Student> AllStudents = []; //Kigyűjtött tanulók
        
        ChangePage(Pages.VezetesiKarton); //Lapváltás a Vezetési kartonokra

        IWebElement SearchBtn = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/form/div/div/div[2]/div[3]/button[2]")); //Szűrés gomb (összes adatot megjeleníti paraméterek nélkül)
        
        SearchBtn.Click();//Tanulói adatok frissítése
        
        IWebElement table = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/div/div[2]/table")); //Tanulók table

        #region Várakozás a table betöltésére
        WebDriverWait WaitForStudents = new WebDriverWait(driver, TimeSpan.FromSeconds(10)) //Várj max x másodpercet
        {
            PollingInterval = TimeSpan.FromMilliseconds(200) //0,2 másodpercenként nézd az oldal változásait
        };
        
        WaitForStudents.IgnoreExceptionTypes(typeof(NoSuchElementException));

        WaitForStudents.Until(x =>
        {
            try
            {
                table.FindElements(By.TagName("tr")); //Keresd a tanulók table sorait
                return true; //Megvan, kilépés
            }
            catch (Exception e) //Ha nincsenek meg
            {
                return false; //Újra
            }
        });
        #endregion

        foreach (IWebElement row in table.FindElements(By.TagName("tr"))) //Végiglépkedés a tanulók listáján
        {
            //A tanuló neve és kartonazonosítójával létrehozásra kerül egy új Student példány
            AllStudents.Add(new Student(row.FindElement(By.XPath("td[1]")).Text, row.FindElement(By.XPath("td[5]")).Text, allLessons));
        }

        return Task.FromResult(AllStudents);
    }
    public Task<List<Lesson>> GetAllLessons(ObservableCollection<Student> allStudents)
    {
        List<Lesson> allLessons = []; //Kigyűjtött órák
        
        ChangePage(Pages.VezetesiKarton); //Vezetési karton oldalra váltás
        Student? CurrentStudent = null; //Adatgyűjtés során a jelen tanuló
        
        IWebElement SearchBtn = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/form/div/div/div[2]/div[3]/button[2]")); //Szűrés gomb (összes adatot megjeleníti paraméterek nélkül)
        IWebElement StudentsTable = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/div/div[2]/table")); //Tanulók table
        
        IWebElement? StudentBtn = null; //A tanuló során az alkalmakra megjelenítésére szóló gomb
        
        SearchBtn.Click(); //Tanulói adatok frissítése
        
        #region Várakozás a tanulók table betöltésére
        WebDriverWait WaitForStudents = new WebDriverWait(driver, TimeSpan.FromSeconds(10)) //Várj max x másodpercet
        {
            PollingInterval = TimeSpan.FromMilliseconds(200) //0,2 másodpercenként nézd az oldal változásait
        };
        
        WaitForStudents.IgnoreExceptionTypes(typeof(NoSuchElementException));

        WaitForStudents.Until(x =>
        {
            try
            {
                StudentsTable.FindElements(By.TagName("tr")); //Keresd a tanulók table sorait
                return true; //Megvan, kilépés
            }
            catch (Exception e) //Ha nincsenek meg
            {
                return false; //Újra
            }
        });
        #endregion

        List<Lesson>? StudentLessons = [];
        
        foreach (IWebElement studentRow in StudentsTable.FindElements(By.TagName("tr"))) //Végiglépkedés a tanulók listáján
        {
            CurrentStudent = allStudents.First(x => x.Id.Equals(studentRow.FindElement(By.XPath("td[5]")).Text)); //Jelen tanuló lokális megkeresése karton azonosító alapján
            StudentBtn = studentRow.FindElement(By.XPath("td[9]/a")); //Alkalmak gomb megkeresése
            StudentBtn.Click(); //Gomb megnyomása

            StudentLessons = GetStudentLessons(driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div[1]/div[3]/div[2]/table")), CurrentStudent); //Tanuló óráinak megkeresése
            if (StudentLessons is not null) //Ha vannak alkalmak
                allLessons.AddRange(StudentLessons); //Talált alkalmak hozzáadása az eddig talált alkalmakhoz
            
            ChangePage(Pages.VezetesiKarton);
            SearchBtn = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/form/div/div/div[2]/div[3]/button[2]"));
            SearchBtn.Click();
            
            WaitForStudents.Until(x => {
                try
                {
                    StudentsTable.FindElements(By.TagName("tr")); //Keresd a tanulók table sorait
                    return true; //Megvan, kilépés
                }
                catch (Exception e) //Ha nincsenek meg
                {
                    return false; //Újra
                }
            }); //Várakozás a tanulók table betöltésére
        }

        return Task.FromResult(allLessons);
    }

    private List<Lesson>? GetStudentLessons(IWebElement LessonsTable, Student CurrentStudent)
    {
        List<Lesson> StudentLessons = new List<Lesson>();
        
        #region Várakozás az alkalmak, vagy hiányuk betöltésére
        WebDriverWait WaitForLessons = new WebDriverWait(driver, TimeSpan.FromSeconds(10)) //Várj max 10 másodpercet
        {
            PollingInterval = TimeSpan.FromMilliseconds(200) //0,2 másodpercenként nézd az oldal változásait
        };
        WaitForLessons.IgnoreExceptionTypes(typeof(NoSuchElementException)); //Ignoráld, ha nincs meg az element
        
        bool AreThereLessons = false;
        WaitForLessons.Until(x =>
        {
            if (driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div[1]/div[3]/div[2]/div[1]")) is not null) //Ha megtalálható a nincsenek alkalmak div
            {
                AreThereLessons = false; //Nincsenek alkalmak
                return true; //Megvan az eredmény, kilépés
            }
            else if (LessonsTable.FindElements(By.TagName("tr")) is not null) //Ha megtalálhatóak az alkalmak
            {
                AreThereLessons = true; //Vannak alkalmak
                return true; //Megvan az eredmény, kilépés
            }

            return false; //Még nem töltött be egyik sem, újraellenőrzés
        });
        #endregion

        if (AreThereLessons) //Ha vannak elemek
        {
            IWebElement? LessonBtn = null; //Az alkalom során a részletes adatok megjelenítésére szolgáló gomb
            IWebElement? LessonForm = null; //Részletes alkalom adatok form a megnyíló modal-on 
            IWebElement? LessonFormCloseBtn = null; //A részletes adatok bezárási gombja

            LessonType CurrentType = LessonType.A; //Jelen alkalom jellege (Az A csak azért, hogy ne legyen null)

            Dictionary<string, string> LessonModalFields = new()
            {
                { "date", "div[1]/div[1]/div[2]/div/span" }, //Dátum (2025. 01. 01)
                { "times", "div[1]/div[1]/div[3]/div/span" }, //Alkalom kezdet/vég (08:00 - 09:40)
                { "type", "div[2]/div[1]/div[1]/div/span[1]/span/span[1]" }, //Vezetés jellege (F/v, A)
                { "vehicle", "div[2]/div[1]/div[2]/div/span[1]/span/span[1]" }, //Jármű rendszám (ABC123 (B))
                { "startplace", "div[1]/div[2]/div[3]/div/span" }, //Indulási helyszín (Autósiskola)
                { "endplace", "div[1]/div[2]/div[4]/div/span" }, //Érkezési helyszín (Autósiskola)
                { "startkm", "div[2]/div[2]/div[1]/div/div[1]/span[1]/input" }, //Alkalom kezdeti kilóméteróra (530930)
                { "drivenkm", "div[2]/div[2]/div[2]/div/div[1]/div/span[1]/input" }, //Alkalom befejezési kilóméteróra (530951)
                { "closebtn", "/html/body/div[1]/div[2]/div[1]/div/a[2]" } //Modal bezárás gomb
            }; //Részletes alkalom adatok elérési utak

            WebDriverWait WaitForDetails = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            {
                PollingInterval = TimeSpan.FromMilliseconds(200) //Nézd az oldal változásait 0,2 másodpercenként
            };
            WaitForDetails.IgnoreExceptionTypes(typeof(NoSuchElementException)); //Ignoráld, ha nincs meg a modal

            IWebElement DetailsBtn = null;
            foreach (IWebElement lessonRow in LessonsTable.FindElements(By.TagName("tr"))) //Végiglépkedés a tanuló alkalmain
            {
                LessonBtn = lessonRow.FindElement(By.XPath("td[6]/a")); //Részl. adatok gomb megkeresése
                LessonBtn.Click(); //Gomb megnyomása

                #region Várakozás a részletek modal betöltésére
                WaitForDetails.Until(x =>
                {
                    try
                    {
                        LessonForm = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[2]/form")); //Részl. adatok form (modal-on)
                        return true; //Betöltött a modal, kilépés
                    }
                    catch (Exception e)
                    {
                        return false; //Nem töltött még be a modal, újraellenőrzés
                    }
                });
                #endregion
                
                #region Alkalom jelleg eldöntés
                switch (LessonForm.FindElement(By.XPath(LessonModalFields["type"])).Text)
                {
                    case "A":
                        CurrentType = LessonType.A;
                        break;
                    case "F/v":
                        CurrentType = LessonType.Fv;
                        break;
                    case "F/o":
                        CurrentType = LessonType.Fo;
                        break;
                    case "F/é":
                        CurrentType = LessonType.Fe;
                        break;
                }
                #endregion

                TimeOnly[] SplitTimes = new TimeOnly[2];

                string[] timesstring = LessonForm.FindElement(By.XPath(LessonModalFields["times"])).Text.Split('-'); //Időpontok feldarabolása
                SplitTimes[0] = TimeOnly.Parse(timesstring[0].Trim()); //Tisztítás és átalakítás (Kezdet)
                SplitTimes[1] = TimeOnly.Parse(timesstring[1].Trim()); //Tisztítás és átalakítás (Befejezés)
                
                StudentLessons.Add(new Lesson(
                    DateOnly.Parse(LessonForm.FindElement(By.XPath(LessonModalFields["date"])).Text), //Dátum
                    SplitTimes[0], //Kezdeti időpont
                    SplitTimes[1], //Befejezési időpont
                    CurrentStudent, //Tanuló
                    CurrentType, //Alkalom jelleg
                    LessonForm.FindElement(By.XPath(LessonModalFields["startplace"])).Text, //Indulás helyszín
                    LessonForm.FindElement(By.XPath(LessonModalFields["endplace"])).Text, //Érkezés helyszín
                    Convert.ToInt32(LessonForm.FindElement(By.XPath(LessonModalFields["drivenkm"])).Text), //Vezetett úthossz
                    true, //Fel van-e töltve a KVAR-ba
                    Convert.ToInt32(LessonForm.FindElement(By.XPath(LessonModalFields["startkm"])).Text))); //Kilóméteróra az óra kezdetekor
            }
            return StudentLessons!; //Órák visszaadása
        }
        else //Ha nincsenek
            return null;
    }

    private void ChangePage(Pages changeTo)
    {
        string baseurl = @"https://kokeny.kavk.hu/Szakoktato";
        driver.Navigate().GoToUrl($@"{baseurl}/{Enum.GetName(changeTo)}");
    }

    public void Start()
    {
        ReadCredentials();
        
        Login();
    }

    public KvarManager()
    {
        InitDriver();
        NavBarButtons = new Dictionary<string, IWebElement>();
    }
}