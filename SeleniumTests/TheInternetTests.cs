using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    // Варіант 4: тестування сторінок сайту https://the-internet.herokuapp.com/
    public class TheInternetTests
    {
        private const string BaseUrl = "https://the-internet.herokuapp.com";

        private IWebDriver driver = null!;
        private WebDriverWait wait = null!;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            // дозволяємо читати логи консолі браузера (потрібно для тесту JavaScript Errors)
            options.SetLoggingPreference(LogType.Browser, LogLevel.All);

            // драйвер ініціалізується для роботи у браузері Chrome
            driver = new ChromeDriver(options);
            driver.Manage().Window.Size = new System.Drawing.Size(1280, 900);

            // явне очікування: чекаємо елемент не довше 5 секунд
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Add/Remove Elements: після трьох натискань "Add Element" з'являються 3 кнопки Delete,
        /// після видалення однієї залишається 2.
        /// </summary>
        [Test]
        public void AddRemoveElements_AddThreeDeleteOne_TwoButtonsLeft()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/add_remove_elements/");

            // тричі натискаємо кнопку Add Element
            var addButton = driver.FindElement(By.XPath("//button[text()='Add Element']"));
            addButton.Click();
            addButton.Click();
            addButton.Click();
            Assert.That(driver.FindElements(By.CssSelector("#elements .added-manually")).Count, Is.EqualTo(3));

            // видаляємо одну кнопку Delete
            driver.FindElement(By.CssSelector("#elements .added-manually")).Click();

            Assert.That(driver.FindElements(By.CssSelector("#elements .added-manually")).Count, Is.EqualTo(2));
        }

        /// <summary>
        /// Checkboxes: спочатку перший чекбокс вимкнений, а другий увімкнений;
        /// після кліку по обох їхній стан змінюється на протилежний.
        /// </summary>
        [Test]
        public void Checkboxes_ClickBoth_StatesAreInverted()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/checkboxes");

            var checkboxes = driver.FindElements(By.CssSelector("#checkboxes input[type='checkbox']"));
            Assert.That(checkboxes[0].Selected, Is.False);
            Assert.That(checkboxes[1].Selected, Is.True);

            checkboxes[0].Click();
            checkboxes[1].Click();

            Assert.That(checkboxes[0].Selected, Is.True);
            Assert.That(checkboxes[1].Selected, Is.False);
        }

        /// <summary>
        /// Dropdown: за замовчуванням вибрано "Please select an option",
        /// після вибору "Option 2" вибраним стає саме цей пункт зі значенням "2".
        /// </summary>
        [Test]
        public void Dropdown_SelectOption2_Option2IsSelected()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/dropdown");

            var dropdown = new SelectElement(driver.FindElement(By.Id("dropdown")));
            Assert.That(dropdown.SelectedOption.Text, Is.EqualTo("Please select an option"));

            dropdown.SelectByText("Option 2");

            Assert.That(dropdown.SelectedOption.Text, Is.EqualTo("Option 2"));
            Assert.That(dropdown.SelectedOption.GetDomProperty("value"), Is.EqualTo("2"));
        }

        /// <summary>
        /// Inputs: у числове поле вводимо 42, натискаємо стрілку вгору – значення стає 43.
        /// </summary>
        [Test]
        public void Inputs_TypeNumberAndPressArrowUp_ValueIncreased()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/inputs");

            var input = driver.FindElement(By.CssSelector("input[type='number']"));
            input.SendKeys("42");
            Assert.That(input.GetDomProperty("value"), Is.EqualTo("42"));

            // стрілка вгору збільшує число на 1
            input.SendKeys(Keys.ArrowUp);

            Assert.That(input.GetDomProperty("value"), Is.EqualTo("43"));
        }

        /// <summary>
        /// Status Codes: після кліку на посилання "404" відкривається сторінка /status_codes/404
        /// з повідомленням про код 404.
        /// </summary>
        [Test]
        public void StatusCodes_Open404_ShowsStatusMessage()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/status_codes");

            driver.FindElement(By.LinkText("404")).Click();

            Assert.That(driver.Url, Is.EqualTo(BaseUrl + "/status_codes/404"));
            Assert.That(driver.FindElement(By.CssSelector(".example p")).Text,
                        Does.Contain("This page returned a 404 status code."));
        }

        /// <summary>
        /// Drag and Drop: після перетягування колонки A на колонку B вони міняються місцями,
        /// тобто в першій колонці з'являється заголовок "B".
        /// </summary>
        [Test]
        public void DragAndDrop_DragAToB_ColumnsSwapped()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/drag_and_drop");

            var columnA = driver.FindElement(By.Id("column-a"));
            var columnB = driver.FindElement(By.Id("column-b"));
            Assert.That(columnA.Text, Is.EqualTo("A"));

            // затискаємо колонку A, ведемо на колонку B і відпускаємо (з невеликими паузами,
            // щоб браузер встиг обробити події перетягування)
            new Actions(driver)
                .ClickAndHold(columnA)
                .Pause(TimeSpan.FromMilliseconds(300))
                .MoveToElement(columnB)
                .Pause(TimeSpan.FromMilliseconds(300))
                .Release(columnB)
                .Perform();

            Assert.That(driver.FindElement(By.Id("column-a")).Text, Is.EqualTo("B"));
            Assert.That(driver.FindElement(By.Id("column-b")).Text, Is.EqualTo("A"));
        }

        /// <summary>
        /// Shifting Content: меню містить 5 пунктів, серед яких є пункт "Gallery",
        /// що зсувається на сторінці.
        /// </summary>
        [Test]
        public void ShiftingContent_Menu_HasFiveItemsWithGallery()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/shifting_content/menu");

            var menuItems = driver.FindElements(By.CssSelector(".example ul li a"));
            var texts = menuItems.Select(item => item.Text).ToList();

            Assert.That(menuItems.Count, Is.EqualTo(5));
            Assert.That(texts, Does.Contain("Gallery"));
            Assert.That(driver.FindElement(By.LinkText("Gallery")).Displayed, Is.True);
        }

        /// <summary>
        /// Geolocation: підміняємо геолокацію браузера на координати Києва;
        /// після натискання "Where am I?" сторінка показує саме ці координати.
        /// </summary>
        [Test]
        public void Geolocation_WhereAmI_ShowsMockedCoordinates()
        {
            var chrome = (ChromeDriver)driver;

            // через Chrome DevTools дозволяємо геолокацію і задаємо координати Києва
            chrome.ExecuteCdpCommand("Browser.grantPermissions", new Dictionary<string, object>
            {
                { "origin", BaseUrl },
                { "permissions", new[] { "geolocation" } }
            });
            chrome.ExecuteCdpCommand("Emulation.setGeolocationOverride", new Dictionary<string, object>
            {
                { "latitude", 50.4501 },
                { "longitude", 30.5234 },
                { "accuracy", 100 }
            });

            driver.Navigate().GoToUrl(BaseUrl + "/geolocation");
            driver.FindElement(By.XPath("//button[text()='Where am I?']")).Click();

            // координати з'являються не одразу, тому чекаємо на елемент
            var latitude = wait.Until(d => d.FindElement(By.Id("lat-value")));

            Assert.That(latitude.Text, Is.EqualTo("50.4501"));
            Assert.That(driver.FindElement(By.Id("long-value")).Text, Is.EqualTo("30.5234"));
        }

        /// <summary>
        /// JavaScript Errors: сторінка під час завантаження кидає помилку JavaScript,
        /// і вона потрапляє в лог консолі браузера.
        /// </summary>
        [Test]
        public void JavaScriptErrors_OpenPage_ErrorIsLoggedInConsole()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/javascript_error");

            var logs = driver.Manage().Logs.GetLog(LogType.Browser);
            var errors = logs.Where(entry => entry.Level == LogLevel.Severe).ToList();

            Assert.That(driver.FindElement(By.TagName("p")).Text,
                        Does.Contain("This page has a JavaScript error in the onload event."));
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors[0].Message, Does.Contain("Cannot read properties of undefined"));
        }

        /// <summary>
        /// Exit Intent: коли курсор виходить за межі вікна, з'являється модальне вікно
        /// із заголовком "This is a modal window".
        /// </summary>
        [Test]
        public void ExitIntent_MouseLeavesWindow_ModalIsShown()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/exit_intent");

            var modal = driver.FindElement(By.Id("ouibounce-modal"));
            Assert.That(modal.Displayed, Is.False);

            // Selenium не може вивести курсор за межі вікна, тому подію mouseleave
            // (курсор пішов угору за межі сторінки) викликаємо через JavaScript
            ((IJavaScriptExecutor)driver).ExecuteScript(
                "document.documentElement.dispatchEvent(new MouseEvent('mouseleave', { clientY: 0 }));");

            // вікно з'являється з анімацією, тому чекаємо, поки стане видимим заголовок
            var title = driver.FindElement(By.CssSelector("#ouibounce-modal .modal-title h3"));
            wait.Until(d => title.Displayed);

            Assert.That(modal.Displayed, Is.True);
            // CSS показує заголовок великими літерами, тому порівнюємо без урахування регістру
            Assert.That(title.Text, Is.EqualTo("This is a modal window").IgnoreCase);
        }

        [TearDown]
        public void TearDown()
        {
            // зберігаємо скріншот сторінки після кожного тесту (папка Screenshots)
            var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
            Directory.CreateDirectory(folder);
            var file = Path.Combine(folder, TestContext.CurrentContext.Test.Name + ".png");
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(file);

            // закриває всі вікна браузера і завершує сесію WebDriver
            driver.Quit();
            driver.Dispose();
        }
    }
}
