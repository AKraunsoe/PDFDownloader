
Console.InputEncoding = new System.Text.UTF8Encoding(false);
Console.OutputEncoding = new System.Text.UTF8Encoding(false);

try
{
   Menu menu = new Menu();
   menu.StartMenu();
}
catch(Exception e)
{
    Console.WriteLine("Exception: " + e.StackTrace);
}