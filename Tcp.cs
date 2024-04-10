using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace clnt0309
{
    class Tcp_Program
    {
        /*함수이름: Send()
         기능: 파일 전송하는 함수
         반환값: 없음
         만든 이: 규비*/
        static void Send(Socket client, string name, string filename)
        {
            var file = new FileInfo(filename);
            if (file.Exists) //파일 존재여부
            {
                var binary = new byte[file.Length];
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    stream.Read(binary, 0, binary.Length);
                    client.Send(new byte[] { 0 }); // 파일 이름 크기야
                    client.Send(BitConverter.GetBytes(name.Length)); //파일이름 크기 송신
                    client.Send(new byte[] { 1 }); //파일 이름이야
                    client.Send(Encoding.UTF8.GetBytes(name)); //보낼 파일의 이름 인코딩
                    client.Send(new byte[] { 2 }); //파일 크기야
                    client.Send(BitConverter.GetBytes(binary.Length)); //파일크기 송신
                    client.Send(new byte[] { 3 }); //파일 송신할거야
                    client.Send(binary); //파일 송신
                }
            }
            else
            {
                Console.WriteLine("The file is not exists. - " + filename);
            }
        }
        /*함수이름: GetFileList()
         기능: 해당 디렉토리 압축하기
         반환값: 리스트(디렉토리의 하위 모든 파일)
         만든 이: 규비*/
        static List<String> GetFileList(String rootPath, List<String> fileList)
        {
            if (fileList == null)
            {
                return null;
            }
            var attr = File.GetAttributes(rootPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dirInfo = new DirectoryInfo(rootPath);
                foreach (var dir in dirInfo.GetDirectories())
                {
                    GetFileList(dir.FullName, fileList);
                }
                foreach (var file in dirInfo.GetFiles())
                {
                    GetFileList(file.FullName, fileList);
                }
            }
            else
            {
                var fileInfo = new FileInfo(rootPath);
                fileList.Add(fileInfo.FullName);
            }
            return fileList;
        }
        public static Tcp_Program Tcp_Class = new Tcp_Program();
        /*함수이름: Tcp_main()
         기능: 이 함수 호출 시 클라이언트 네트워크의 순서대로 다른 함수 호출하는 로직
         반환값: 없음
         만든 이: 규비*/
        public void Tcp_main()
        {
            var sourcePath = @"C:\Users\lms\Documents\Visual Studio 2017\Backup Files\clnt0309\clnt0309\bin\Debug\2024.03.11";
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(ipep);
                foreach (var file in GetFileList(sourcePath, new List<string>()))
                {
                    Send(client, file.Substring(sourcePath.Length + 1), file);
                }
            }
            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }
    }
}