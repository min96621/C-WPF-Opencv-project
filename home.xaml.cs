using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace clnt0309
{
    /// <summary>
    /// home.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class home : Window
    {
        MainWindow W = new MainWindow();
        public home()
        {
            InitializeComponent();
        }
        /*함수 이름: File_mode()
        기능: 디렉토리 생성 및 오픈
        반환값: 없음
        만든 이: 규비*/
        public void File_open()
        {
            string save = DateTime.Now.ToString("yyyy.MM.dd");
            Directory.CreateDirectory(save);
        }
        /*함수 이름: Start_Btn_Click
        기능: 홈화면에서 시작버튼 클릭 시 웹캠화면을 띄우고 홈화면을 끄는 로직
        반환 값: 없음
        만든 이: 규비*/
        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            File_open();
            W.Title = "BTS 봉준호 손흥민 이상복 최상문 레고(Let's go)_ing";
            W.Show();
            Close();
        }
    }
}
