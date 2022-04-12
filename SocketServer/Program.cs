using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace SocketServer
{
    class MainClass
    {
        static async Task Main(string[] args)
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    string resultText = default;

                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine(data);
                    if (data.Contains("none"))
                    {
                        string reply = "HTTP/1.1 200 Ok\r\nDate: Sun, 07 Jul 2013 17:13:10 GMT\r\nServer: Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16\r\nLast-Modified: Sat, 30 Mar 2013 11:28:59 GMT\r\nETag: \"ca-4d922b19fd4c0\"\r\nAccept-Ranges: bytes\r\nContent-Length: 202\r\nKeep-Alive: timeout=5, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n\r\n" +
                "hello ";
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }

                    var request = data.Split(' ');

                    // Показываем данные на консоли
                    Console.Write("Полученное имя файла: " + request[5] + "\n\n");

                    if (request[0] == "GET")
                    {
                        try
                        {
                            using (FileStream fstream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + $@"{request[5]}"))
                            {
                                // выделяем массив для считывания данных из файла
                                byte[] buffer = new byte[fstream.Length];
                                // считываем данные
                                await fstream.ReadAsync(buffer, 0, buffer.Length);
                                // декодируем байты в строку
                                string textFromFile = Encoding.Default.GetString(buffer);
                                resultText = textFromFile;
                                Console.WriteLine($"Текст из файла: {textFromFile}");
                            }
                        }
                        catch
                        {
                            resultText = "Error";
                        }
                    }
                    else if(request[0] == "POST")
                    {
                        using (FileStream fstream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + $@"{request[5]}", FileMode.OpenOrCreate))
                        {
                            // преобразуем строку в байты
                            byte[] buffer = Encoding.Default.GetBytes(request[6]);
                            // запись массива байтов в файл
                            await fstream.WriteAsync(buffer, 0, buffer.Length);
                            Console.WriteLine("Текст записан в файл");
                            resultText = "true";
                        }
                    }
                    else
                    {
                        resultText = "Error";
                    }

                    // Отправляем ответ клиенту\
                    if (resultText == "Error" )
                    {
                        string reply  = "HTTP/1.1 400 Not Found\r\nDate: Sun, 07 Jul 2013 17:13:10 GMT\r\nServer: Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16\r\nLast-Modified: Sat, 30 Mar 2013 11:28:59 GMT\r\nETag: \"ca-4d922b19fd4c0\"\r\nAccept-Ranges: bytes\r\nContent-Length: 202\r\nKeep-Alive: timeout=5, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n\r\n" +
                "<html>";
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }
                    else
                    {
                        string reply = "HTTP/1.1 200 OK\r\nDate: Sun, 07 Jul 2013 17:13:10 GMT\r\nServer: Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16\r\nLast-Modified: Sat, 30 Mar 2013 11:28:59 GMT\r\nETag: \"ca-4d922b19fd4c0\"\r\nAccept-Ranges: bytes\r\nContent-Length: 202\r\nKeep-Alive: timeout=5, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n\r\n" +
                resultText;
                        byte[] msg = Encoding.UTF8.GetBytes(reply);
                        handler.Send(msg);
                    }

                    if (data.IndexOf("<TheEnd>") > -1)
                    {
                        Console.WriteLine("Сервер завершил соединение с клиентом.");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static string CreateHTTPResponce(string status, string body)
        {
            string responce;
            var requestLine = $"HTTP/1.1 " + status;

            responce = "HTTP/1.1 200 OK\r\nDate: Sun, 07 Jul 2013 17:13:10 GMT\r\nServer: Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16\r\nLast-Modified: Sat, 30 Mar 2013 11:28:59 GMT\r\nETag: \"ca-4d922b19fd4c0\"\r\nAccept-Ranges: bytes\r\nContent-Length: 202\r\nKeep-Alive: timeout=5, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n\r\n" +
                "< html >";

            return responce;
        }
    }
}
