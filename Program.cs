using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Serv1
{
    enum State // 상태 열거형
    {
        STATE,
        FILENAMESIZE,
        FILENAME,
        FILESIZE,
        FILEDOWNLOAD
    }
    class File // 파일 클래스
    {	
        protected State state = State.STATE; // 상태
        public byte[] FileName { get; set; }  // 파일 이름
        public byte[] Binary { get; set; } // 파일
    }	
    class Client : File // 파일 클래스를 상속 받음
    {
        private Socket socket; // 클라이언트 소켓
        private byte[] buffer; // 버퍼	
        private int seek = 0; // 파일 다운로드 위치	
        // 다운로드 디렉토리	
        private string SaveDirectory = @"C:\Users\lms\Documents\Visual Studio 2017\Backup Files\Serv1\Clnt_file\";
	
        /*함수이름: Client
         기능: 생성자 소켓받고, 버퍼로 메세지 받고 메세지 대기 하는 로직
         반환값: 없음*/
        public Client(Socket socket)
        {
            this.socket = socket;
            buffer = new byte[1];	
            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, this);

            var remoteAddr = (IPEndPoint)socket.RemoteEndPoint;
            Console.WriteLine($"Client:(From:{remoteAddr.Address.ToString()}:{remoteAddr.Port},Connection time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")})");
        }
        /*함수이름: Receive
         기능: 데이터 수신(파일이름, 파일이름크기, 파일크기, 파일저장)
         반환값: 없음
         만든 이: 규비*/
        private void Receive(IAsyncResult result)
        {	
            if (socket.Connected) //커넥트되면
            {
                int size = this.socket.EndReceive(result); //보류 중인 비동기 읽기 끝내기 데이터 사이즈받기

                if (state == State.STATE) //상태
                {
                    switch (buffer[0])
                    {
                        case 0: //파일 이름 크기
                            state = State.FILENAMESIZE;
                            buffer = new byte[4];
                            break;	
                        case 1: //파일 이름
                            state = State.FILENAME;
                            buffer = new byte[FileName.Length];
                            seek = 0; //다운로드 위치
                            break;
                        case 2: //파일 크기
                            state = State.FILESIZE;
                            buffer = new byte[4];
                            break;	
                        case 3: //파일
                            state = State.FILEDOWNLOAD;
                            buffer = new byte[Binary.Length];
                            seek = 0; //다운로드 위치
                            break;
                    }
                }
                else if (state == State.FILENAMESIZE) // 파일 이름 사이즈
                {
                    FileName = new byte[BitConverter.ToInt32(buffer, 0)]; //바이트배열로 4비트에서 변환된
                    buffer = new byte[1]; //초기화
                    state = State.STATE;
                }
                else if (state == State.FILENAME)  // 파일 이름
                {	
                    Array.Copy(buffer, 0, FileName, seek, size); //배열 복사 
                    seek += size; // 받은 만큼 위치 옮긴	
                    
                    if (seek >= FileName.Length) // 위치와 파일 이름 크기가 같으면 종료	
                    {
                        buffer = new byte[1]; //초기화
                        state = State.STATE;
                    }
                }
                else if (state == State.FILESIZE) // 파일 사이즈	
                {

                    Binary = new byte[BitConverter.ToInt32(buffer, 0)]; //바이트배열로 4비트에서 변환된
                    buffer = new byte[1]; //초기화
                    state = State.STATE;
                }
                else if (state == State.FILEDOWNLOAD) // 파일 다운로드	
                {
                    Array.Copy(buffer, 0, Binary, seek, size); //배열 복사 
                    seek += size; // 받은 만큼 위치 옮긴	
                    
                    if (seek >= Binary.Length) // 위치와 파일 크기가 같으면 종료	
                    {
                        string name = Encoding.UTF8.GetString(FileName);
                        name += ".png"; //확장자가 짤려서 올때 대비 내가 추가해줌

                        var filepath = SaveDirectory + name;
                        var subDir = Path.GetDirectoryName(filepath); // 파일의 디렉토리를 확인
                        
                        if (!Directory.Exists(subDir)) // 디렉토리가 없으면
                        {
                            Directory.CreateDirectory(subDir);
                        }

                        using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write)) //바이너리를 파일에 저장 경로,생성,쓰기
                        {
                            stream.Write(Binary, 0, Binary.Length); // 쓰기
                        }
                        Console.WriteLine($"Download file - ${SaveDirectory + Encoding.UTF8.GetString(FileName)}");
	
                        buffer = new byte[1]; //초기화
                        state = State.STATE;
                    }
                }
	
                this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, this); //버퍼로 받고 메세지대기
            }
        }
    }
    class Program : Socket
    {
        /*함수이름: program()
         기능: 생성자 포트 리슨 서버 대기 요청으로 대기
         반환값: 없음
         만든 이: 규비*/
        public Program() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            base.Bind(new IPEndPoint(IPAddress.Any, 10000)); //포트 바인드
            base.Listen(10);
            BeginAccept(Accept, this); //대기
        }
        /*함수이름: Accept
         기능: 클라이언트 접속 요청시 로직 함수
         반환값: 없음
         만든 이: 규비*/
        private void Accept(IAsyncResult result)
        {
            var client = new Client(EndAccept(result)); //연결 수라ㅣㄱ
            BeginAccept(Accept, this); //다른 연결요청을 수락할 수 있게
        }
        /*함수이름: main()
         기능: 프로그램 시작하는 로직 다른 함수들 호출 로직
         반환값: 없음
         만든 이: 규비*/
        static void Main()
        {
            new Program();
            Console.WriteLine("SERVER OPEN [Press the q key to exit]");
            while (true)
            {
                string k = Console.ReadLine();
                if ("q".Equals(k, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
        }
    }
}