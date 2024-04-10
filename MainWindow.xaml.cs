using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using OpenCvSharp;
using System.Windows.Threading;


namespace clnt0309
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        // 영상용
        VideoCapture video = new VideoCapture(0);
        Mat frame = new Mat();

        public MainWindow()
        {
            InitializeComponent();
        }
        /*함수 이름: Window_Loaded
        만든 이: 규비
        기능: 해당 윈도우 다른 곳에 호출 시 웹캠 영상 호출과 영상 송수신 시작하게 만드는 로직*/
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(0.01);
            timer.Tick += new EventHandler(Camera);
            timer.Start();
        }
        /*함수 이름: Camera
        만든 이: 전원
        기능: 웹캠 영상에서 모양 검출 로직 호출과 색상 검출하는 함수*/
        private void Camera(object sender, EventArgs e)
        {
            video.Read(frame); // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌

            // 색상 공간 변환 함수(Cv2.CvtColor)를 활용해 색상, 채도, 명도로 변경
            Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2HSV);

            Mat mask1 = new Mat();      //빨강
            Mat mask2 = new Mat();      //노랑
            Mat mask3 = new Mat();      //초록
            Mat mask4 = new Mat();      //파랑

            // 원하는 범위의 색상 출력
            Cv2.InRange(frame, new Scalar(0, 50, 100), new Scalar(10, 255, 255), mask1);        //빨강
            Cv2.InRange(frame, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask2);        //노랑
            Cv2.InRange(frame, new Scalar(40, 70, 80), new Scalar(70, 255, 255), mask3);        //초록
            Cv2.InRange(frame, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4);        //파랑

            // 윤곽선 검출 함수 Cv2.FindContours(원본 배열, 검출된 윤곽선, 계층 구조, 검색 방법, 근사 방법, 오프셋)
            Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);      //빨강
            Cv2.FindContours(mask2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);      //노랑
            Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);      //초록
            Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);      //파랑

            Cv2.CvtColor(frame, frame, ColorConversionCodes.HSV2BGR);
            Mat test = new Mat();
            Cv2.CvtColor(frame, test, ColorConversionCodes.BGR2HSV);

            // 원하는 범위의 색상 출력
            Mat color = new Mat();
            Cv2.InRange(test, new Scalar(0, 100, 100), new Scalar(360 / 2, 255, 255), color);

            // 윤곽선의 실제 값이 저장될 contours
            // 윤곽선은 n개 이상이 될 수 있으므로 Point[]를 묶는 Point[][]가 됨
            OpenCvSharp.Point[][] contours;

            // 그 윤곽선들의 계층 구조를 저장할 hierarchy
            HierarchyIndex[] hierarchy;

            // 윤곽선 검출 함수 Cv2.FindContours
            Cv2.FindContours(color, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);

            // 색상 변수화
            string lego_color1 = "Red";
            string lego_color2 = "Yellow";
            string lego_color3 = "Green";
            string lego_color4 = "Blue";

            // 색상 리스트
            List<string> now_color = new List<string>();

            // 최종 진위여부
            bool check = false;

            // PutText 출력 1번만 되게 bool 선언
            bool output_Red = false;
            bool output_Yellow = false;
            bool output_Green = false;
            bool output_Blue = false;
            bool output_shape = false;


            // List 형식의 Point[] 배열을 선언
            List<OpenCvSharp.Point[]> new_contours1 = new List<OpenCvSharp.Point[]>();
            // 빨간색 검출
            foreach (var c in contours1)
            {
                // 좌표
                var m = Cv2.Moments(c);
                var x = (int)(m.M10 / m.M00);
                var y = (int)(m.M01 / m.M00);

                // 윤곽선 넓이 함수(Cv2.ContourArea)는 윤곽선의 면적을 계산
                var area = Cv2.ContourArea(c);

                // Cv2.ArcLength length가 100 이상의 값만 new_contours에 추가
                double length = Cv2.ArcLength(c, true);

                // 구역 내 색상 검출
                if ((x > 130 && x < 530) && (y > 230 && y < 430))
                {
                    if (area > 4000 && length > 100)
                    {
                        if (!output_Red)
                        {
                            // new_contours1 변수에 일정 조건 이상의 윤곽선만 포함
                            new_contours1.Add(c);
                            // 윤곽선 그리기 함수(Cv2.DrawContours)
                            Cv2.DrawContours(frame, new_contours1, -1, Scalar.Red, 2, LineTypes.Link8, null, 1);
                            // Cv2.PutText(image, string text, Point org, HersheyFont, double fontScale, Scalar Color, int thickness, LineType)
                            //  이미지에 org위치부터 HersheyFont, fontScale크기, Color, thicknes두께, LineType보정으로 text를 그림
                            Cv2.PutText(frame, "true " + lego_color1 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                            now_color.Add(lego_color1);
                            output_Red = true;
                        }
                    }
                }
                else if (area > 4000 && length > 100)
                {
                    if (!output_Red)
                    {
                        new_contours1.Add(c);
                        Cv2.DrawContours(frame, new_contours1, -1, Scalar.Red, 2, LineTypes.Link8, null, 1);
                        Cv2.PutText(frame, "false " + lego_color1 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                        output_Red = true;
                    }
                }
            }

            // 노란색 검출
            List<OpenCvSharp.Point[]> new_contours2 = new List<OpenCvSharp.Point[]>();
            foreach (var c in contours2)
            {
                var m = Cv2.Moments(c);
                var x = (int)(m.M10 / m.M00);
                var y = (int)(m.M01 / m.M00);
                var area = Cv2.ContourArea(c);
                double length = Cv2.ArcLength(c, true);

                if ((x > 130 && x < 530) && (y > 10 && y < 230))
                {
                    if (area > 4000 && length > 100)
                    {
                        new_contours2.Add(c);
                        Cv2.DrawContours(frame, new_contours1, -1, Scalar.Yellow, 2, LineTypes.Link8, null, 1);
                        Cv2.PutText(frame, "true " + lego_color2 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                        now_color.Add(lego_color2);
                    }
                }
                else if (area > 4000 && length > 100)
                {
                    if (!output_Yellow)
                    {
                        new_contours2.Add(c);
                        Cv2.DrawContours(frame, new_contours1, -1, Scalar.Yellow, 2, LineTypes.Link8, null, 1);
                        Cv2.PutText(frame, "false " + lego_color2 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                        output_Yellow = true;
                    }
                }
            }

            // 초록색 검출
            List<OpenCvSharp.Point[]> new_contours3 = new List<OpenCvSharp.Point[]>();
            foreach (var c in contours3)
            {
                var m = Cv2.Moments(c);
                var x = (int)(m.M10 / m.M00);
                var y = (int)(m.M01 / m.M00);
                var area = Cv2.ContourArea(c);
                double length = Cv2.ArcLength(c, true);

                if (area > 4000 && length > 100)
                {
                    if (!output_Green)
                    {
                        new_contours3.Add(c);
                        Cv2.DrawContours(frame, new_contours3, -1, Scalar.Green, 2, LineTypes.Link8, null, 1);
                        Cv2.PutText(frame, "false " + lego_color3 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                        output_Green = true;
                    }
                }
            }

            // 파란색 검출
            List<OpenCvSharp.Point[]> new_contours4 = new List<OpenCvSharp.Point[]>();
            foreach (var c in contours4)
            {

                var m = Cv2.Moments(c);
                var x = (int)(m.M10 / m.M00);
                var y = (int)(m.M01 / m.M00);
                var area = Cv2.ContourArea(c);
                double length = Cv2.ArcLength(c, true);

                if ((x > 130 && x < 530) && (y > 430 && y < 476))
                {
                    if (area > 4000 && length > 100)
                    {
                        if (!output_Blue)
                        {
                            new_contours4.Add(c);
                            Cv2.DrawContours(frame, new_contours4, -1, Scalar.Blue, 2, LineTypes.Link8, null, 1);
                            Cv2.PutText(frame, "true " + lego_color4 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                            now_color.Add(lego_color4);
                            output_Blue = true;
                        }
                    }
                }
                else if (area > 4000 && length > 100)
                {
                    if (!output_Blue)
                    {
                        new_contours4.Add(c);
                        Cv2.DrawContours(frame, new_contours4, -1, Scalar.Blue, 2, LineTypes.Link8, null, 1);
                        Cv2.PutText(frame, "false " + lego_color4 + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheyComplex, 0.7, Scalar.Blue, 1, LineTypes.Link8);
                        output_Blue = true;
                    }
                }
            }
            // 도형 검출
            List<OpenCvSharp.Point[]> new_contours = new List<OpenCvSharp.Point[]>();
            foreach (var c in contours)
            {
                var m = Cv2.Moments(c);
                var x = (int)(m.M10 / m.M00);
                var y = (int)(m.M01 / m.M00);
                var area = Cv2.ContourArea(c, true);
                double length = Cv2.ArcLength(c, true);
                if (area > 4000 && length > 100)
                {
                    // 형상구분
                    string shape = GetShape(c);
                    if (!output_shape)
                    {
                        // 좌표에 쓰일 빨간점 그리는 함수
                        //  Cv2.Circle(image, int CenterX, int CenterY, int radius, Scalar Color, int thickness, LineType)
                        // 이미지에 Center(X,Y)를 중심으로 radius반지름으로 Color로 thickness두께로 LineType보정으로 원을 그림
                        Cv2.Circle(frame, new OpenCvSharp.Point(x, y), 5, Scalar.Red, -1);
                        Cv2.PutText(frame, shape + "(x: " + x + " y: " + y + ")", new OpenCvSharp.Point(x, y), HersheyFonts.HersheySimplex, 1.0, Scalar.Black, 2);
                        new_contours.Add(c);
                        output_shape = true;
                    }
                    if (shape == "circle" && (now_color.Contains("Red") && now_color.Contains("Blue") && now_color.Contains("Yellow")))
                    {
                        //저장할 리스트
                        ing_1.Content = "정상 제품입니다.";

                        //파일경로
                        string path = "C:/Users/lms/Documents/Visual Studio 2017/Backup Files/clnt0309/clnt0309/bin/Debug";
                        path += "/";
                        path += DateTime.Now.ToString("yyyy.MM.dd"); // 디렉토리 이름
                        path += "/";
                        path += DateTime.Now.ToString("yyyy.MM.dd - hh시mm분ss초"); //파일 이름
                        path += ".png"; //png지정

                        Cv2.ImWrite(path, frame); //캡처 저장
                        Thread.Sleep(1000);
                        check = true;
                    }
                    else if (shape == "rectangle" && (now_color.Contains("Red") && now_color.Contains("Blue") && now_color.Contains("Yellow")))
                    {
                        //저장할 리스트
                        ing_1.Content = "정상 제품입니다.";

                        //파일경로
                        string path = "C:/Users/lms/Documents/Visual Studio 2017/Backup Files/clnt0309/clnt0309/bin/Debug";
                        path += "/";
                        path += DateTime.Now.ToString("yyyy.MM.dd"); // 디렉토리 이름
                        path += "/";
                        path += DateTime.Now.ToString("yyyy.MM.dd - hh시mm분ss초"); //파일 이름
                        path += ".png"; //png지정

                        Cv2.ImWrite(path, frame); //캡처 저장
                        Thread.Sleep(1000);
                        check = true;
                    }
                    else
                    {
                        ing_1.Content = "불량 제품입니다.";
                        Thread.Sleep(1000);
                        check = false;
                    }
                }
            }
            web_Cam.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);

        }
        /*함수 이름: GetShape
        기능: 모양 검출하는 로직
        반환 값: 어떤 모형인지 확인 후 그 문자열 반환
        만든 이: 민형*/
        private string GetShape(OpenCvSharp.Point[] c)
        {
            string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            OpenCvSharp.Point[] approx = Cv2.ApproxPolyDP(c, 0.04 * peri, true);

            if (approx.Length == 3)
            {
                shape = "triangle";
            }
            else if (approx.Length == 4)
            {
                shape = "rectangle";
            }
            else if (approx.Length == 5)
            {
                shape = "pentagon";
            }
            else 
            {
                shape = "circle";
            }
            return shape;
        }
        /*함수 이름: End_btn_Click
        기능: 종료버튼 클릭 시 저장된 이미지 디렉토리 송신하는 함수 호출 및 해당 화면 끄기
        반환값: 없음
        만든 이: 규비*/
        private void End_btn_Click(object sender, RoutedEventArgs e)
        {
            Tcp_Program.Tcp_Class.Tcp_main();
            Close();       
        }
    }
}
