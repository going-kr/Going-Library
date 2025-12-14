using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UIEditor.Utils
{
    public class LM
    {
        static Lang lang => Program.DataMgr?.Language ?? Lang.KO;

        #region 메뉴
        #region 파일
        #region File
        public static string File
        {
            get
            {
                if (lang == Lang.KO) return "파일";
                else if (lang == Lang.EN) return "File";
                else return "";
            }
        }
        #endregion
        #region NewFile
        public static string NewFile
        {
            get
            {
                if (lang == Lang.KO) return "새 프로젝트";
                else if (lang == Lang.EN) return "New Project";
                else return "";
            }
        }
        #endregion
 
        #region Save
        public static string Save
        {
            get
            {
                if (lang == Lang.KO) return "저장";
                else if (lang == Lang.EN) return "Save";
                else return "";
            }
        }
        #endregion
        #region SaveAs
        public static string SaveAs
        {
            get
            {
                if (lang == Lang.KO) return "다른 이름으로 저장";
                else if (lang == Lang.EN) return "Save As";
                else return "";
            }
        }
        #endregion
        #region Open
        public static string Open
        {
            get
            {
                if (lang == Lang.KO) return "열기";
                else if (lang == Lang.EN) return "Open";
                else return "";
            }
        }
        #endregion
        #region Close
        public static string Close
        {
            get
            {
                if (lang == Lang.KO) return "닫기";
                else if (lang == Lang.EN) return "Close";
                else return "";
            }
        }
        #endregion
        #region Exit
        public static string Exit
        {
            get
            {
                if (lang == Lang.KO) return "끝내기";
                else if (lang == Lang.EN) return "Exit";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 편집
        #region Edit
        public static string Edit
        {
            get
            {
                if (lang == Lang.KO) return "편집";
                else if (lang == Lang.EN) return "Edit";
                else return "";
            }
        }
        #endregion
        #region Undo
        public static string Undo
        {
            get
            {
                if (lang == Lang.KO) return "실행 취소";
                else if (lang == Lang.EN) return "Undo";
                else return "";
            }
        }
        #endregion
        #region Redo
        public static string Redo
        {
            get
            {
                if (lang == Lang.KO) return "다시 실행";
                else if (lang == Lang.EN) return "Redo";
                else return "";
            }
        }
        #endregion
        #region Copy
        public static string Copy
        {
            get
            {
                if (lang == Lang.KO) return "복사";
                else if (lang == Lang.EN) return "Copy";
                else return "";
            }
        }
        #endregion
        #region Cut
        public static string Cut
        {
            get
            {
                if (lang == Lang.KO) return "잘라내기";
                else if (lang == Lang.EN) return "Cut";
                else return "";
            }
        }
        #endregion
        #region Paste
        public static string Paste
        {
            get
            {
                if (lang == Lang.KO) return "붙여넣기";
                else if (lang == Lang.EN) return "Paste";
                else return "";
            }
        }
        #endregion
        #region Delete
        public static string Delete
        {
            get
            {
                if (lang == Lang.KO) return "삭제";
                else if (lang == Lang.EN) return "Delete";
                else return "";
            }
        }
        #endregion
        #region SelectAll
        public static string SelectAll
        {
            get
            {
                if (lang == Lang.KO) return "모두 선택";
                else if (lang == Lang.EN) return "Select All";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 뷰
        #region View
        public static string View
        {
            get
            {
                if (lang == Lang.KO) return "보기";
                else if (lang == Lang.EN) return "View";
                else return "";
            }
        }
        #endregion
        #region Explorer
        public static string Explorer
        {
            get
            {
                if (lang == Lang.KO) return "탐색기";
                else if (lang == Lang.EN) return "Explorer";
                else return "";
            }
        }
        #endregion
        #region Properties
        public static string Properties
        {
            get
            {
                if (lang == Lang.KO) return "속성";
                else if (lang == Lang.EN) return "Properties";
                else return "";
            }
        }
        #endregion
        #region ToolBox
        public static string ToolBox
        {
            get
            {
                if (lang == Lang.KO) return "도구 상자";
                else if (lang == Lang.EN) return "ToolBox";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 프로젝트
        #region Project
        public static string Project
        {
            get
            {
                if (lang == Lang.KO) return "프로젝트";
                else if (lang == Lang.EN) return "Project";
                else return "";
            }
        }
        #endregion
        #region Validation
        public static string Validation
        {
            get
            {
                if (lang == Lang.KO) return "유효성 체크";
                else if (lang == Lang.EN) return "Validation";
                else return "";
            }
        }
        #endregion
        #region Deploy
        public static string Deploy
        {
            get
            {
                if (lang == Lang.KO) return "배포";
                else if (lang == Lang.EN) return "Deploy";
                else return "";
            }
        }
        #endregion
        #region ProjectProps
        public static string ProjectProps
        {
            get
            {
                if (lang == Lang.KO) return "프로젝트 속성";
                else if (lang == Lang.EN) return "Properties";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 도구
        #region Tool
        public static string Tool
        {
            get
            {
                if (lang == Lang.KO) return "도구";
                else if (lang == Lang.EN) return "Tool";
                else return "";
            }
        }
        #endregion
        #region Resources
        public static string Resources
        {
            get
            {
                if (lang == Lang.KO) return "리소스";
                else if (lang == Lang.EN) return "Resources";
                else return "";
            }
        }
        #endregion
        #region ProgramSetting
        public const string ProgramSettingK = "프로그램 설정";
        public const string ProgramSettingE = "Program Setting";
        public static string ProgramSetting
        {
            get
            {
                if (lang == Lang.KO) return ProgramSettingK;
                else if (lang == Lang.EN) return ProgramSettingE;
                else return "";
            }
        }
        #endregion
        #endregion

        #region 도움말
        #region Help
        public static string Help
        {
            get
            {
                if (lang == Lang.KO) return "도움말";
                else if (lang == Lang.EN) return "Help";
                else return "";
            }
        }
        #endregion
        #region ProgramInfo
        public static string ProgramInfo
        {
            get
            {
                if (lang == Lang.KO) return "프로그램 정보";
                else if (lang == Lang.EN) return "Program Information";
                else return "";
            }
        }
        #endregion
        #endregion
        #endregion

        #region 다이얼로그
        #region Ok
        public const string OkK = "확인";
        public const string OkE = "Ok";
        public static string Ok
        {
            get
            {
                if (lang == Lang.KO) return OkK;
                else if (lang == Lang.EN) return OkE;
                else return "";
            }
        }
        #endregion
        #region Cancel
        public const string CancelK = "취소";
        public const string CancelE = "Cancel";
        public static string Cancel
        {
            get
            {
                if (lang == Lang.KO) return CancelK;
                else if (lang == Lang.EN) return CancelE;
                else return "";
            }
        }
        #endregion
        #region Yes
        public const string YesK = "예";
        public const string YesE = "Yes";
        public static string Yes
        {
            get
            {
                if (lang == Lang.KO) return YesK;
                else if (lang == Lang.EN) return YesE;
                else return "";
            }
        }
        #endregion
        #region No
        public const string NoK = "아니요";
        public const string NoE = "No";
        public static string No
        {
            get
            {
                if (lang == Lang.KO) return NoK;
                else if (lang == Lang.EN) return NoE;
                else return "";
            }
        }
        #endregion
        #endregion

        #region 프로그램 설정 창
        #region Language
        public const string LanguageK = "언어";
        public const string LanguageE = "Language";
        public static string Language
        {
            get
            {
                if (lang == Lang.KO) return LanguageK;
                else if (lang == Lang.EN) return LanguageE;
                else return "";
            }
        }
        #endregion
        #region ProjectFolder
        public const string ProjectFolderK = "프로젝트 폴더";
        public const string ProjectFolderE = "Project Folder";
        public static string ProjectFolder
        {
            get
            {
                if (lang == Lang.KO) return ProjectFolderK;
                else if (lang == Lang.EN) return ProjectFolderE;
                else return "";
            }
        }
        #endregion
        #region ProjectPath
        public static string ProjectPath
        {
            get
            {
                if (lang == Lang.KO) return "프로젝트 경로";
                else if (lang == Lang.EN) return "Project Path";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 새 프로젝트 창
        #region ProjectName
        public static string ProjectName
        {
            get
            {
                if (lang == Lang.KO) return "프로젝트 명";
                else if (lang == Lang.EN) return "Project Name";
                else return "";
            }
        }
        #endregion
        #region ScreenSize
        public static string ScreenSize
        {
            get
            {
                if (lang == Lang.KO) return "화면 크기";
                else if (lang == Lang.EN) return "Screen Size";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 메시지
        #region SaveQuestion
        public static string SaveQuestion
        {
            get
            {
                if (lang == Lang.KO) return "저장 하시겠습니까?";
                else if (lang == Lang.EN) return "Would you like me to save it?";
                else return "";
            }
        }
        #endregion
        #region SavePermissions
        public static string SavePermissions
        {
            get
            {
                if (lang == Lang.KO) return "권한 부족으로 저장할 수 없습니다.";
                else if (lang == Lang.EN) return "Insufficient permissions to save.";
                else return "";
            }
        }
        #endregion

        #region ExistsControl
        public static string ExistsControl
        {
            get
            {
                if (lang == Lang.KO) return "동일한 이름의 컨트롤이 존재합니다.";
                else if (lang == Lang.EN) return "The control with the same name already exists.";
                else return "";
            }
        }
        #endregion
        #region ExistsPage
        public static string ExistsPage
        {
            get
            {
                if (lang == Lang.KO) return "동일한 이름의 페이지가 존재합니다.";
                else if (lang == Lang.EN) return "The page with the same name already exists.";
                else return "";
            }
        }
        #endregion
        #region ExistsWindows
        public static string ExistsWindows
        {
            get
            {
                if (lang == Lang.KO) return "동일한 이름의 윈도우가 존재합니다.";
                else if (lang == Lang.EN) return "The window with the same name already exists.";
                else return "";
            }
        }
        #endregion
        #region DeletePage
        public static string DeletePage
        {
            get
            {
                if (lang == Lang.KO) return "페이지를 삭제하시겠습니까?";
                else if (lang == Lang.EN) return "Would you like to delete the page?";
                else return "";
            }
        }
        #endregion
        #region DeleteWindow
        public static string DeleteWindow
        {
            get
            {
                if (lang == Lang.KO) return "윈도우를 삭제하시겠습니까?";
                else if (lang == Lang.EN) return "Would you like to delete the window?";
                else return "";
            }
        }
        #endregion

        #region ValidationOK
        public static string ValidationOK
        {
            get
            {
                if (lang == Lang.KO) return "유효성 체크 결과 정상입니다.";
                else if (lang == Lang.EN) return "The validity check results are normal.";
                else return "";
            }
        }
        #endregion
        #region ValidationFail
        public static string ValidationFail
        {
            get
            {
                if (lang == Lang.KO) return "유효성 체크 결과 문제가 확인 되었습니다.";
                else if (lang == Lang.EN) return "The validity check results have identified an issue.";
                else return "";
            }
        }
        #endregion

        #region InvalidProjectFolder
        public static string InvalidProjectFolder
        {
            get
            {
                if (lang == Lang.KO) return "프로젝트 폴더 경로가 유효하지 않습니다.";
                else if (lang == Lang.EN) return "The project folder path is not valid.";
                else return "";
            }
        }
        #endregion

        #region DeployComplete
        public static string DeployComplete
        {
            get
            {
                if (lang == Lang.KO) return "배포를 완료하였습니다.";
                else if (lang == Lang.EN) return "The deployment has been completed.";
                else return "";
            }
        }
        #endregion

        #region InputError
        public static string InputError
        {
            get
            {
                if (lang == Lang.KO) return "입력 오류";
                else if (lang == Lang.EN) return "Input Error";
                else return "";
            }
        }
        #endregion
        #region InputError_EmptyName
        public static string InputError_EmptyName
        {
            get
            {
                if (lang == Lang.KO) return "'Name' 을 입력하십시오.";
                else if (lang == Lang.EN) return "Please enter your 'Name'.";
                else return "";
            }
        }
        #endregion
        #region InputError_DupName
        public static string InputError_DupName
        {
            get
            {
                if (lang == Lang.KO) return "중복된 'Name' 이 존재합니다.";
                else if (lang == Lang.EN) return "There is a duplicate 'Name'.";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 탐색기
        #region Add
        public static string Add
        {
            get
            {
                if (lang == Lang.KO) return "추가";
                else if (lang == Lang.EN) return "Add";
                else return "";
            }
        }
        #endregion
        #region Rename
        public static string Rename
        {
            get
            {
                if (lang == Lang.KO) return "이름 바꾸기";
                else if (lang == Lang.EN) return "Rename";
                else return "";
            }
        }
        #endregion

        #region Page
        public static string Page
        {
            get
            {
                if (lang == Lang.KO) return "페이지";
                else if (lang == Lang.EN) return "Page";
                else return "";
            }
        }
        #endregion
        #region Window
        public static string Window
        {
            get
            {
                if (lang == Lang.KO) return "윈도우";
                else if (lang == Lang.EN) return "Window";
                else return "";
            }
        }
        #endregion
        #region Duplication
        public static string Duplication
        {
            get
            {
                if (lang == Lang.KO) return "중복";
                else if (lang == Lang.EN) return "Duplication";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 도구상자
        #region Controls
        public static string Controls
        {
            get
            {
                if (lang == Lang.KO) return "Controls";
                else if (lang == Lang.EN) return "Controls";
                else return "";
            }
        }
        #endregion
        #region Containers
        public static string Containers
        {
            get
            {
                if (lang == Lang.KO) return "Containers";
                else if (lang == Lang.EN) return "Containers";
                else return "";
            }
        }
        #endregion
        #region ImageCanvas
        public static string ImageCanvas
        {
            get
            {
                if (lang == Lang.KO) return "ImageCanvas";
                else if (lang == Lang.EN) return "ImageCanvas";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 컬렉션 에디터
        #region Item
        public static string Item
        {
            get
            {
                if (lang == Lang.KO) return "항목";
                else if (lang == Lang.EN) return "Item";
                else return "";
            }
        }
        #endregion
        #region Index
        public static string Index
        {
            get
            {
                if (lang == Lang.KO) return "순번";
                else if (lang == Lang.EN) return "No";
                else return "";
            }
        }
        #endregion
        #region Height
        public static string Height
        {
            get
            {
                if (lang == Lang.KO) return "높이";
                else if (lang == Lang.EN) return "Height";
                else return "";
            }
        }
        #endregion
        #region Unit
        public static string Unit
        {
            get
            {
                if (lang == Lang.KO) return "단위";
                else if (lang == Lang.EN) return "Unit";
                else return "";
            }
        }
        #endregion
        #region Column
        public static string Column
        {
            get
            {
                if (lang == Lang.KO) return "컬럼";
                else if (lang == Lang.EN) return "Column";
                else return "";
            }
        }
        #endregion
        #region Value
        public static string Value
        {
            get
            {
                if (lang == Lang.KO) return "값";
                else if (lang == Lang.EN) return "Value";
                else return "";
            }
        }
        #endregion
        #region Items
        public static string Items
        {
            get
            {
                if (lang == Lang.KO) return "목록";
                else if (lang == Lang.EN) return "Items";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 리소스 메니저
        #region ImageList
        public static string ImageList
        {
            get
            {
                if (lang == Lang.KO) return "이미지 목록";
                else if (lang == Lang.EN) return "Images";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 테마 
        #region ThemeEditor
        public static string ThemeEditor
        {
            get
            {
                if (lang == Lang.KO) return "테마 에디터";
                else if (lang == Lang.EN) return "Theme Editor";
                else return "";
            }
        }
        #endregion
        #region NoUse
        public static string NoUse
        {
            get
            {
                if (lang == Lang.KO) return "사용 안함";
                else if (lang == Lang.EN) return "Not Used";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 폰트
        #region FontAdd
        public static string FontAdd
        {
            get
            {
                if (lang == Lang.KO) return "폰트 추가";
                else if (lang == Lang.EN) return "Add Font";
                else return "";
            }
        }
        #endregion
        #region FontList
        public static string FontList
        {
            get
            {
                if (lang == Lang.KO) return "폰트 목록";
                else if (lang == Lang.EN) return "Font List";
                else return "";
            }
        }
        #endregion
        #region FontName
        public static string FontName
        {
            get
            {
                if (lang == Lang.KO) return "폰트명";
                else if (lang == Lang.EN) return "Font Name";
                else return "";
            }
        }
        #endregion
        #region Example
        public static string Example
        {
            get
            {
                if (lang == Lang.KO) return "예시";
                else if (lang == Lang.EN) return "Example";
                else return "";
            }
        }
        #endregion
        #region CantDeleteDefaultFont
        public static string CantDeleteDefaultFont
        {
            get
            {
                if (lang == Lang.KO) return "'나눔고딕'은 기본 폰트로 삭제할 수 없습니다.";
                else if (lang == Lang.EN) return "'NanumGothic' is a default font and cannot be removed.";
                else return "";
            }
        }
        #endregion
        #region ErrorFontCount
        public static string ErrorFontCount
        {
            get
            {
                if (lang == Lang.KO) return "폰트는 5종류 이상 추가할 수 없습니다";
                else if (lang == Lang.EN) return "You cannot add more than 5 fonts.";
                else return "";
            }
        }
        #endregion
        #region DefaultFont
        public static string DefaultFont
        {
            get
            {
                if (lang == Lang.KO) return "기본 폰트";
                else if (lang == Lang.EN) return "Default Font";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 새 페이지
        #region NewPage
        public static string NewPage
        {
            get
            {
                if (lang == Lang.KO) return "새 페이지";
                else if (lang == Lang.EN) return "New Page";
                else return "";
            }
        }
        #endregion
        #region PageName
        public static string PageName
        {
            get
            {
                if (lang == Lang.KO) return "페이지 명";
                else if (lang == Lang.EN) return "Page Name";
                else return "";
            }
        }
        #endregion
        #region PageType
        public static string PageType
        {
            get
            {
                if (lang == Lang.KO) return "페이지 유형";
                else if (lang == Lang.EN) return "Page Type";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 선택기
        #region ImageSelector
        public static string ImageSelector
        {
            get
            {
                if (lang == Lang.KO) return "이미지 선택기";
                else if (lang == Lang.EN) return "Image Selector";
                else return "";
            }
        }
        #endregion
        #region FontSelector
        public static string FontSelector
        {
            get
            {
                if (lang == Lang.KO) return "폰트 선택기";
                else if (lang == Lang.EN) return "Font Selector";
                else return "";
            }
        }
        #endregion
        #endregion

        #region 배포
        #region FaildDeploy
        public static string FaildDeploy
        {
            get
            {
                if (lang == Lang.KO) return "배포 실패";
                else if (lang == Lang.EN) return "Failed to Deploy";
                else return "";
            }
        }
        #endregion
        #endregion
    }

    #region enum : Lang
    public enum Lang { NONE, KO, EN }
    #endregion
}
