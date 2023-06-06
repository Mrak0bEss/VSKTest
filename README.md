# VSKTest
## Первое задание - SQL
Ну тут все просто, самые базовые запросы по изменению значения в ячейках.
```sql
UPDATE Order
SET OrderID = 2
WHERE OrderID = 1;

UPDATE OrderItem
SET OrderID = 2
WHERE OrderID = 1;
```
## Второе задание - определение проблем в коде

Я не работал до этого с БД-запросами, но то что я нагуглил говорит мне о том что:
1-Обычный поиск эксепшенов лучше чем Debug.Assert, так как он работает только на дебаге\
2-SqlCommand.Parameters для SQL запросов использовать лучше чем strinf.format\
3-Надо прервать соединение с БД и освободить SqlDataReader с помощью using\
4-Надо обработать ошибки связанные с обработкой БД-запросов - catch (Exception ex)\
```csharp
[WebMethod]
public Order LoadOrderInfo(string orderCode)
{
    if (string.IsNullOrEmpty(orderCode))
        throw new ArgumentException("orderCode cannot be null or empty", nameof(orderCode));

    Stopwatch stopWatch = new Stopwatch();
    stopWatch.Start();

    try
    {
        lock (cache)
        {
            if (cache.ContainsKey(orderCode))
            {
                stopWatch.Stop();
                logger.Log("INFO", "Elapsed - {0}", stopWatch.Elapsed);

                return cache[orderCode];
            }
        }

        string query = "SELECT OrderID, CustomerID, TotalMoney FROM dbo.Orders where OrderCode=@orderCode";
        using (SqlConnection connection = new SqlConnection(this.connectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@orderCode", orderCode);
            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Order order = new Order((string)reader[0], (string)reader[1], (int)reader[2]);

                    lock (cache)
                    {
                        if (!cache.ContainsKey(orderCode))
                            cache[orderCode] = order;
                    }
                    stopWatch.Stop();
                    logger.Log("INFO", "Elapsed - {0}", stopWatch.Elapsed);

                    return order;
                }
            }
        }

        stopWatch.Stop();
        logger.Log("INFO", "Elapsed - {0}", stopWatch.Elapsed);

        return null;
    }
    catch (Exception ex)
    {
        logger.Log("ERROR", ex.Message);
        throw;
    }
}
```
## Третье задание - закончите реализацию
В User мы просто реализовали интерфейс IUser и агрегрировали  IAuthenticationService согласно заданию. Что - то интересное началось потом. Так как я не писал подобные юнит тесты для меня это было что - то новое. Сначала я пытался создать отдельное решение с тестами, но не мог связать два dotnet решения, хотя все должно было работать. После этого я просто засунул класс с тестом в основной класс, и тестировал с помощью средств Visual Studio, используя NUnit, тест прошел успешно. Я сделал только один тест по двум причинам - я еще плохо умею их делать и не знаю на какие места кода стоит ставить больше тестов, и вторая причина в том что у нас только один метод, соответсвенно протестировать (как я понимаю) можно только его.
Вот реализация Ussr с тестами:
```csharp
class User : IUser
    {
        private readonly IAuthenticationService _authService;
        private AuthToken _authToken;

        public User(IAuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public string AuthToken => _authToken?.Token;

        public void Authenticate(string username, string password)
        {
            _authToken = _authService.Authenticate(username, password);
        }
    }
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Authenticate_WithValidCredentials_SetsAuthToken()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.Authenticate("username", "password"))
                .Returns(new AuthToken { Token = "token" });
            var user = new User(authServiceMock.Object);

            // Act
            user.Authenticate("username", "password");

            // Assert
            Assert.AreEqual("token", user.AuthToken);
        }
    }
```
