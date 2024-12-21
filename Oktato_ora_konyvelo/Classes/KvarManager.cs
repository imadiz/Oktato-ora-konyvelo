using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Oktato_ora_konyvelo.Classes;

public class KvarManager
{
    private ChromeDriver driver { get; set; }
    private string KvarUser { get; set; }
    private string KvarPwd { get; set; }
    
    public void ReadCredentials()
    {
        string creds = File.ReadAllText("./creds.txt", Encoding.Default);//Bejelentkezési adatok helye
        KvarUser = creds.Split(';')[0];//Felh. név
        KvarPwd = creds.Split(';')[1];//Jelszó
    }

    public Task Login()
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
        wait.IgnoreExceptionTypes(typeof(ElementNotInteractableException), typeof(NoSuchElementException));//Nem baj, ha nem megnyomható a gomb, vagy nem látszik a kijelentkezés, próbáld

        wait.Until(x =>
        {
            LoginButton.Click(); //Gomb megnyomási próbálkozás
            driver.FindElement(By.Id("logout"));//Ha exception-t dob (nincs még ilyen, akkor újrapróbálkozás)
            return true;//Ha látszik a kijelentkezés gomb, akkor kilépés
        });
        
        

        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(300));//Várj max 5 percet, amíg a kétlépcsős bejelentkezés sikerül
        wait.Until(x =>
        {
            if (x.Url.Equals(@"https://kokeny.kavk.hu/szakoktato"))//Ha ez a jelenlegi url
            {
                return true;//Siker
            }

            return false;//Újra
        });
        
        return Task.CompletedTask;//Login siker
    }

    public KvarManager()
    {
        ReadCredentials();
        
        ChromeOptions options = new();
        options.AddArgument("--start-maximized");//Fullscreen legyen
        options.LeaveBrowserRunning = false;//Bezáródjon a programmal együtt
        options.PageLoadStrategy = PageLoadStrategy.Normal;//Várd meg amíg teljesen betölt az oldal
        
        driver = new ChromeDriver(options);
    }
}