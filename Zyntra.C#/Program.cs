using Newtonsoft.Json;
using Zyntra.CS;

string token = ;
AccessPoint accessPoint = new (){ ID =  };

var bot = new Client();
var result = await bot.Login(token);

if (!result.status)
{
    Console.WriteLine("Login failed: " + result.reason);
    Environment.Exit(0);
}

var result2 = await bot.SendMessage(accessPoint, "Hello, world!");
if (!result2.status)
{
    Console.WriteLine("Send failed: " + result2.reason);
    Environment.Exit(0);
}
else if (result2.msg != null)
{
    var result3 = await bot.GetMessage(result2.msg);
    Console.WriteLine(JsonConvert.SerializeObject(result3, Formatting.Indented));
}