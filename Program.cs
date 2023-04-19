using System.Text;

string testStr = "114514【管理员】矢速[爆肝] 2023/4/17 5:20:04\r\n全场延迟最高的仔\r\n【苦力】夏梦寻Official[巨菜剪辑] 2023/4/18 11:33:27\r\n【苦力】夏梦寻Official[巨菜剪辑] 2023/4/18 11:33:34\r\n任重道远\r\n【苦力】秋野Akino 2023/4/18 11:35:08\r\n1919810";
Encoding encode = Encoding.Default;
Console.WriteLine(testStr);
Console.WriteLine("---------------------------------------------");
string binStr = BinaryString.FromBytes(encode.GetBytes(testStr));
Console.WriteLine(binStr);
Console.WriteLine($"encoding: {encode.EncodingName}\nbit count: {binStr.Length}");
Console.WriteLine("---------------------------------------------");
Console.WriteLine(encode.GetString(BinaryString.Parse(binStr)));
