using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

class Program
{
    const string SERVER_IP = "127.0.0.1";
    const int SERVER_PORT = 9000;
    const string HASH = "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824";


    static void Main()
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(SERVER_IP, SERVER_PORT);
                Console.WriteLine($"Pripojené na {SERVER_IP}:{SERVER_PORT}");

                using (NetworkStream stream = client.GetStream())
                {
                    while (true)
                    {
                        Console.WriteLine("\nPrikaz(list, get, delete, upload, exit): ");
                        string? input = Console.ReadLine();
                        if (input == null) continue;
                        input = input.Trim();

                        if (input.ToLower() == "exit") break;

                        string[] cmd = input.Split(' ', 3);
                        string command = cmd[0].ToLower();

                        if (cmd[0] == "list")
                        {
                            SendLine(stream, "LIST");

                            string header = ReadLine(stream);
                            Console.Write(header);

                            if (!header.StartsWith("200")) continue;

                            int count = int.Parse(header.Split(' ')[2]);

                            for (int i = 0; i < count; i++)
                            {
                                Console.WriteLine(ReadLine(stream));
                            }
                        }

                        else if (cmd[0] == "get" && cmd.Length >= 2)
                        {
                            SendLine(stream, $"GET {cmd[1]}");
                            string header = ReadLine(stream);
                            if (!header.StartsWith("200")) { Console.WriteLine(header); continue; }

                            string[] parts = header.Split(new char[] { ' ' }, 4);
                            int length = int.Parse(parts[2]);
                            string description = parts.Length > 3 ? parts[3] : "file";
                            

                            byte[] data = ReadExact(stream, length);

                            string fileName = "down_" + description.Replace(" ", "_");
                            File.WriteAllBytes(fileName, data);

                            Console.WriteLine("ulozene ako: " + fileName);
                        }

                        else if (cmd[0] == "upload" && cmd.Length >= 3)
                        {
                            string path = cmd[1];
                            string description = cmd[2];

                            if (!File.Exists(path))
                            {
                                Console.WriteLine("Subor neexistuje");
                                continue;
                            }

                            byte[] data = File.ReadAllBytes(path);

                            SendLine(stream, $"UPLOAD {data.Length} {description}");
                            stream.Write(data, 0, data.Length);

                            Console.WriteLine(ReadLine(stream));
                        }

                        else if (cmd[0] == "delete" && cmd.Length >= 2)
                        {
                            SendLine(stream, $"DELETE {cmd[1]}");
                            Console.WriteLine(ReadLine(stream));
                        }
                        else
                        {
                            Console.WriteLine("Neznamy prikaz");
                        }

                    }
                }

                Console.WriteLine("Spojenie zatvorené");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba: {ex.Message}");
        }

        static void SendLine(NetworkStream stream, string line)
        {
            byte[] data = Encoding.UTF8.GetBytes(line + "\n");
            stream.Write(data, 0, data.Length);
        }

        static string ReadLine(NetworkStream stream)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1 || b == '\n') break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        static byte[] ReadExact(NetworkStream stream, int length)
        {
            byte[] data = new byte[length];
            int total = 0;

            while (total < length)
            {
                int read = stream.Read(data, total, length - total);

                if (read == 0)
                    throw new Exception("Spojenie ukoncene");

                total += read;

            }
            return data;
        }
    }
}





