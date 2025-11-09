namespace ZAShinyStoragePlugin;

public static class WinFormsUtil
{
    internal static void CenterToForm(this Control child, Control parent)
    {
        int x = parent.Location.X + (parent.Width - child.Width) / 2;
        int y = parent.Location.Y + (parent.Height - child.Height) / 2;
        child.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
    }
}
