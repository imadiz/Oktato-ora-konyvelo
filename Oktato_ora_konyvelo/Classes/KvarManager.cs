using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5))//Várj 2 másodpercet, amíg megnyomható lesz a login gomb
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),//0,2 másodpercenként nézd az oldal változásait
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

    private void GetAllLessons()
    {
        ChangePage(Pages.VezetesiKarton);

        IWebElement SearchBtn = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/form/div/div/div[2]/div[3]/button[2]"));//Szűrés gomb (összes adatot megjeleníti paraméterek nélkül)
        
        SearchBtn.Click();//Tanulói adatok frissítése

        IWebElement table = driver.FindElement(By.XPath(@"/html/body/div[2]/div[2]/div/div[1]/div/div[2]/table"));//Tanulók table
        
        
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
        GetLocations();
        GetVehicles();
    }

    public KvarManager()
    {
        InitDriver();
        NavBarButtons = new Dictionary<string, IWebElement>();
    }
}