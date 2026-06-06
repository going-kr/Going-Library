using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;

namespace SampleBinding;

/// <summary>창 없이 디자인을 펌프해 바인딩 결과를 콘솔로 확인하는 자가 점검(헤드리스).</summary>
internal static class Smoke
{
    public static void Run(GoDesign design, AppHub hub)
    {
        design.Init();
        design.WireBindings(hub);

        var page = design.Pages["Main"];
        hub.Logs.Add(new LogVM { Message = "부팅 완료" });
        hub.Logs.Add(new LogVM { Message = "펌프 A 기동" });

        // 두 번 펌프: 바인딩/ItemsSource 전달 + 행 생성 → 값 반영
        page.FireUpdate();
        page.FireUpdate();

        Console.WriteLine("=== gudx binding smoke ===");
        Console.WriteLine($"status      : {((GoLabel)page.Childrens[0]).Text}");
        PrintCard("card PumpA  ", (GoComponentInstance)page.Childrens[1]);
        PrintCard("card PumpB  ", (GoComponentInstance)page.Childrens[2]);

        var list = (GoItemList)page.Childrens[4];
        Console.WriteLine($"log rows    : {list.Childrens.Count}");
        foreach (var row in list.Childrens)
            Console.WriteLine($"  - {RowText(row)}");

        // live 갱신
        hub.PumpA.Rpm = 1600;
        hub.Logs.Add(new LogVM { Message = "경고: 과속" });
        page.FireUpdate();
        page.FireUpdate();

        Console.WriteLine("--- after live update ---");
        PrintCard("card PumpA  ", (GoComponentInstance)page.Childrens[1]);
        Console.WriteLine($"log rows    : {((GoItemList)page.Childrens[4]).Childrens.Count}");
        Console.WriteLine("OK");
    }

    private static void PrintCard(string label, GoComponentInstance card)
    {
        var box = (GoBoxPanel)card.Childrens[0];
        var ls = box.Childrens.OfType<GoLabel>().ToList();
        Console.WriteLine($"{label}: {ls[0].Text} / {ls[1].Text}");
    }

    private static string RowText(Going.UI.Controls.IGoControl row)
        => row is GoBoxPanel b ? ((GoLabel)b.Childrens[0]).Text
         : row is GoLabel l ? l.Text : row.GetType().Name;
}
