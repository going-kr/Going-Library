using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;

using var view = new GoViewWindow(1024, 768, WindowBorder.Resizable);
view.Debug = false;
view.Title = "Sample OpenTK";
view.TItleIconString = "fa-check";
var pnl = new GoPanel { Left = 20, Top = 20, Width = 450, Height = 300, IconString = "fa-check", ButtonWidth = 60, IconSize = 14 };
pnl.Buttons.Add(new GoButtonInfo { Name = "add", IconString = "fa-plus", Size = "50%" });
pnl.Buttons.Add(new GoButtonInfo { Name = "del", IconString = "fa-minus", Size = "50%" });

view.Childrens.Add(pnl);
pnl.Childrens.Add(new GoLabel { Left = 10, Top = 50, Width = 120, Height = 140, LabelColor = "Base3", Text = "���ع��� ��λ��� ������ �⵵�� \r\n�ϳ����� �����ϻ� �츮 ���� ����\r\n����ȭ ��õ�� ȭ������ \r\n���� ��� �������� ���� ���� �ϼ�", BorderOnly = true, ContentAlignment = GoContentAlignment.TopLeft, TextPadding = new GoPadding(10) });
pnl.Childrens.Add(new GoButton { Left = 140, Top = 50, Width = 90, Height = 40, IconString = "fa-check", Text = "�׽�Ʈ 1" });
pnl.Childrens.Add(new GoButton { Left = 240, Top = 50, Width = 90, Height = 40, IconString = "fa-ban", Text = "�׽�Ʈ 2", ButtonColor = "danger" });
pnl.Childrens.Add(new GoInputString { Left = 140, Top = 100, Width = 300, Height = 40, TitleSize = 90, Title = "���ڿ� 1", Value = "Test1", ButtonSize = 80, Buttons = [new GoButtonInfo { IconString = "fa-download", Size = "50%" }, new GoButtonInfo { IconString = "fa-upload", Size = "50%" }] });
pnl.Childrens.Add(new GoInputString { Left = 140, Top = 150, Width = 300, Height = 40, TitleSize = 90, Title = "���ڿ� 2", Value = "Test2", ButtonSize = 80, Buttons = [new GoButtonInfo { IconString = "fa-download", Size = "50%" }, new GoButtonInfo { IconString = "fa-upload", Size = "50%" }] });

view.Childrens.Add(new GoInputString { Left = 20, Top = 330, Width = 300, Height = 40, TitleSize = 90, Title = "�Է� 1", });
view.Childrens.Add(new GoInputString { Left = 20, Top = 380, Width = 300, Height = 40, TitleSize = 90, Title = "�Է� 2", });
view.Childrens.Add(new GoInputString { Left = 20, Top = 430, Width = 300, Height = 40, TitleSize = 90, Title = "�Է� 3", });
view.Childrens.Add(new GoInputString { Left = 20, Top = 480, Width = 300, Height = 40, TitleSize = 90, Title = "�Է� 4", });

view.Childrens.Add(new GoInputInt { Left = 330, Top = 330, Width = 300, Height = 40, TitleSize = 90, Title = "����", });
view.Childrens.Add(new GoInputInt { Left = 330, Top = 380, Width = 300, Height = 40, TitleSize = 90, Title = "����\r\n( 0 ~ 100)", Minimum  = 0, Maximum = 100 });
view.Childrens.Add(new GoInputDouble{ Left = 330, Top = 430, Width = 300, Height = 40, TitleSize = 90, Title = "�Ǽ�", });
view.Childrens.Add(new GoInputDouble { Left = 330, Top = 480, Width = 300, Height = 40, TitleSize = 90, Title = "�Ǽ�\r\n( -20 ~ 50 )", Minimum = -20, Maximum = 100 });

var lb = new GoListBox { Left = 480, Top = 20, Width = 450, Height = 300, SelectionMode = GoItemSelectionMode.Multi };
view.Childrens.Add(lb);
for (int i = 1; i <= 100; i++) lb.Items.Add(new() { Text = $"�׽�Ʈ {i}" });

view.VSync = VSyncMode.On;
view.CenterWindow();
view.Run();