using System;
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

        wait.Until(x =>
        {
            LoginButton.Click(); //Gomb megnyomási próbálkozás
            
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

    private IList<Student> GetAllStudents(ObservableCollection<Lesson> allLessons)
    {
        IList<Student> AllStudents = []; //Kigyűjtött tanulók
        
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

        foreach (IWebDriver row in table.FindElements(By.TagName("tr"))) //Végiglépkedés a tanulók listáján
        {
            //A tanuló neve és kartonazonosítójával létrehozásra kerül egy új Student példány
            AllStudents.Add(new Student(row.FindElement(By.XPath("td[1]")).Text, row.FindElement(By.XPath("td[5]")).Text, allLessons));
        }

        return AllStudents;
    }
    private IList<Lesson> GetAllLessons(ObservableCollection<Student> allStudents)
    {
        IList<Lesson> allLessons = []; //Kigyűjtött órák
        
        ChangePage(Pages.VezetesiKarton); //Vezetési karton oldalra váltás

        #region Gombok, Útvonalak
        IWebElement SearchBtn = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/form/div/div/div[2]/div[3]/button[2]")); //Szűrés gomb (összes adatot megjeleníti paraméterek nélkül)

        IWebElement table = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/div/div[2]/table")); //Tanulók table

        Student? CurrentStudent = null; //Adatgyűjtés során a jelen tanuló
        
        IWebElement? StudentBtn = null; //A tanuló során az alkalmakra megjelenítésére szóló gomb
        
        IWebElement? LessonBtn = null; //Az alkalom során a részletes adatok megjelenítésére szolgáló gomb
        IWebElement? LessonsTable = null; //Tanuló alkalmak table
        IWebElement? LessonForm = null; //Részletes alkalom adatok form a megnyíló modal-on 
        IWebElement? LessonFormCloseBtn = null; //A részletes adatok bezárási gombja

        LessonType CurrentType = LessonType.A; //Jelen alkalom jellege (Az A csak azért, hogy ne legyen null)

        Dictionary<string, string> LessonModalFields = new(); //Részletes alkalom adatok elérési utak
        LessonModalFields.Add("date", "div[1]/div[1]/div[2]/div/span"); //Dátum (2025. 01. 01)
        LessonModalFields.Add("times", "div[1]/div[1]/div[3]/div/span"); //Alkalom kezdet/vég (08:00 - 09:40)
        LessonModalFields.Add("type", "div[2]/div[1]/div[1]/div/span[1]/span/span[1]"); //Vezetés jellege (F/v, A)
        LessonModalFields.Add("vehicle", "div[2]/div[1]/div[2]/div/span[1]/span/span[1]"); //Jármű rendszám (ABC123 (B))
        LessonModalFields.Add("startplace", "div[1]/div[2]/div[3]/div/span"); //Indulási helyszín (Autósiskola)
        LessonModalFields.Add("endplace", "div[1]/div[2]/div[4]/div/span"); //Érkezési helyszín (Autósiskola)
        LessonModalFields.Add("startkm", "div[2]/div[2]/div[1]/div/div[1]/span[1]/input"); //Alkalom kezdeti kilóméteróra (530930)
        LessonModalFields.Add("drivenkm", "div[2]/div[2]/div[2]/div/div[1]/div/span[1]/input"); //Alkalom befejezési kilóméteróra (530951)
        LessonModalFields.Add("closebtn", "/html/body/div[1]/div[2]/div[1]/div/a[2]"); //Modal bezárás gomb
        #endregion

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
                table.FindElements(By.TagName("tr")); //Keresd a tanulók table sorait
                return true; //Megvan, kilépés
            }
            catch (Exception e) //Ha nincsenek meg
            {
                return false; //Újra
            }
        });
        #endregion
        
        foreach (IWebDriver studentRow in table.FindElements(By.TagName("tr"))) //Végiglépkedés a tanulók listáján
        {
            CurrentStudent = allStudents.First(x => x.Id.Equals(studentRow.FindElement(By.XPath("td[5]")).Text)); //Jelen tanuló lokális megkeresése karton azonosító alapján
            
            StudentBtn = studentRow.FindElement(By.XPath("td[9]/a")); //Alkalmak gomb megkeresése
            StudentBtn.Click(); //Gomb megnyomása
            
            LessonsTable = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div/div[1]/div[3]/div[2]/table")); //Alkalmak table megkeresése
            
            #region Várakozás a tanuló alkalmainak betöltésére

            WebDriverWait WaitForLessons = new WebDriverWait(driver, TimeSpan.FromSeconds(10)) //Várj max 10 másodpercet
            {
                PollingInterval = TimeSpan.FromMilliseconds(200) //0,2 másodpercenként nézd az oldal változásait
            };
            WaitForLessons.IgnoreExceptionTypes(typeof(NoSuchElementException)); //Ignoráld, ha nincs meg az element

            //TODO: Ha nincsenek alkalmak, megjelenik egy div, ami erről tájékoztat. Ha vannak alkalmak, megjelennek a table sorai.
            //El kell dönteni, hogy melyik, és kezelni kell, mind a kettő async töltődik.
            #endregion
            
            foreach (IWebElement lessonRow in LessonsTable.FindElements(By.TagName("tr"))) //Végiglépkedés a tanuló alkalmain
            {
                LessonBtn = lessonRow.FindElement(By.XPath("td[6]/a")); //Részl. adatok gomb megkeresése
                LessonBtn.Click(); //Gomb megnyomása
                
                LessonForm = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[2]/form")); //Részl. adatok form (modal-on)

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
                
                allLessons.Add(new Lesson(
                    DateOnly.Parse(LessonForm.FindElement(By.XPath(LessonModalFields["date"])).Text),
                    TimeOnly.Parse(""),
                    TimeOnly.Parse(""),
                    CurrentStudent,
                    CurrentType,
                    LessonForm.FindElement(By.XPath(LessonModalFields["startplace"])).Text,
                    LessonForm.FindElement(By.XPath(LessonModalFields["endplace"])).Text,
                    Convert.ToInt32(LessonForm.FindElement(By.XPath(LessonModalFields["drivenkm"])).Text),
                    true,
                    Convert.ToInt32(LessonForm.FindElement(By.XPath(LessonModalFields["startkm"])).Text)));
            }
        }

        return allLessons;
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