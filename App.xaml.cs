using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace clnt0309
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        /*함수 이름: Application_Startup
        기능: 프로그램 실행 시 홈화면을 먼저 띄우는 로직
        반환 값: 없음
        만든 이: 규비*/
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            home H = new home();
            H.Title = "BTS 봉준호 손흥민 이상복 최상문 레고(Let's go)_home";
            H.Show();
        }
    }
}
