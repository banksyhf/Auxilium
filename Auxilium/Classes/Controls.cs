using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;

public class HiddenTab : TabControl
{

    public int DesignerIndex
    {
        get { return SelectedIndex; }
        set
        {
            if (DesignMode)
            {
                SelectedIndex = value;
            }
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 4904)
        {
            m.Result = IntPtr.Zero;
        }
        else
        {
            base.WndProc(ref m);
        }
    }

}
public sealed class ListView : System.Windows.Forms.ListView
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
    protected override void CreateHandle()
    {
        base.CreateHandle();
        SetWindowTheme(this.Handle, "explorer", null);
    }
    public ListView()
    {
        this.DoubleBuffered = true;
        this.View = System.Windows.Forms.View.Details;
        this.FullRowSelect = true;
    }
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 0x83: // WM_NCCALCSIZE
                int style = (int)GetWindowLong(this.Handle, GwlStyle);
                if ((style & WsHscroll) == WsHscroll)
                    SetWindowLong(this.Handle, GwlStyle, style & ~WsHscroll);
                base.WndProc(ref m);
                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }
    const int GwlStyle = -16;
    public const int WsHscroll = 0x00100000;

    public static int GetWindowLong(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 4)
            return (int)GetWindowLong32(hWnd, nIndex);
        else
            return (int)(long)GetWindowLongPtr64(hWnd, nIndex);
    }

    public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
    {
        if (IntPtr.Size == 4)
            return (int)SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        else
            return (int)(long)SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
    public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
    public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);
}
sealed class SmoothLabel : Control
{

	private readonly Bitmap _textBitmap;

	private readonly Graphics _textGraphics;
	public SmoothLabel()
	{
		SetStyle((ControlStyles)139270, true);

		_textBitmap = new Bitmap(1, 1);
		_textGraphics = Graphics.FromImage(_textBitmap);

		BackColor = Color.FromArgb(250, 250, 200);
		ForeColor = Color.FromArgb(150, 150, 120);
		_p1 = new Pen(Color.FromArgb(200, 200, 160));
	}

	private Pen _p1 = Pens.Black;
	public Color BorderColor {
		get { return _p1.Color; }
		set {
			_p1 = new Pen(value);
			Invalidate();
		}
	}

	private SolidBrush _b1;
	public override Color ForeColor {
		get { return base.ForeColor; }
		set {
			base.ForeColor = value;
			_b1 = new SolidBrush(value);
			Invalidate();
		}
	}

	private Size _textSize;
	public override string Text {
		get { return base.Text; }
		set {
			base.Text = value;
			_textSize = _textGraphics.MeasureString(base.Text, base.Font).ToSize();
			Invalidate();
		}
	}

	public override Font Font {
		get { return base.Font; }
		set {
			base.Font = value;
			_textSize = _textGraphics.MeasureString(base.Text, base.Font).ToSize();
			Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor);
		e.Graphics.DrawString(Text, Font, _b1, Width / 2 - _textSize.Width / 2, Height / 2 - _textSize.Height / 2);
		e.Graphics.DrawRectangle(_p1, 0, 0, Width - 1, Height - 1);
	}

}
public sealed class ChangerControl : Control
{
    #region " Control Help "
    public GraphicsPath RoundRect(Rectangle Rectangle, int Curve)
    {
        GraphicsPath P = new GraphicsPath();
        int ArcRectangleWidth = Curve * 2;
        P.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
        P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
        P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
        P.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
        P.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
        return P;
    }
    public GraphicsPath RoundRect(int X, int Y, int Width, int Height, int Curve)
    {
        return RoundRect(new Rectangle(X, Y, Width, Height), Curve);
    }
    public enum MouseState : byte
    {
        None = 0,
        Over = 1,
        Down = 2,
        Block = 3
    }
    #endregion

    #region " Variables "
    private TextBox withEventsField_tbChange;
    public TextBox tbChange
    {
        get { return withEventsField_tbChange; }
        set
        {
            if (withEventsField_tbChange != null)
            {
                withEventsField_tbChange.KeyUp -= OnKeyUp;
            }
            withEventsField_tbChange = value;
            if (withEventsField_tbChange != null)
            {
                withEventsField_tbChange.KeyUp += OnKeyUp;
            }
        }
    }
    private bool editing = false;
    #endregion
    MouseState State = new MouseState();

    #region " Constructor "
    public ChangerControl()
        : base()
    {
        tbChange = new TextBox
        {
            Text = Text,
            Size = Size,
            ForeColor = ForeColor,
            Visible = false,
            Width = 150
        };
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.FixedHeight, true);
        DoubleBuffered = true;
        ForeColor = Color.Black;
        Width = 200;
        Controls.Add(tbChange);
    }
    #endregion

    #region " Methods "

    protected override void CreateHandle()
    {
        base.CreateHandle();
        tbChange.Text = Text;
    }
    protected override void OnResize(System.EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
        Height = 23;
    }
    protected override void OnTextChanged(System.EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
    protected override void OnForeColorChanged(System.EventArgs e)
    {
        base.OnForeColorChanged(e);
        Invalidate();
    }
    protected override void OnBackColorChanged(System.EventArgs e)
    {
        base.OnBackColorChanged(e);
        Invalidate();
    }
    protected override void OnFontChanged(System.EventArgs e)
    {
        base.OnFontChanged(e);
        Invalidate();
    }
    protected override void OnParentBackColorChanged(System.EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }
    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (e.Location.X < CreateGraphics().MeasureString(Text, Font).Width + 2)
        {
            State = MouseState.Over;
        }
        else
        {
            State = MouseState.None;
        }
        Invalidate();
    }
    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }
    protected override void OnMouseLeave(System.EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }
    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        Invalidate();
    }
    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        base.OnPaint(e);
        Font fontButtons = new Font("Marlett", 8.5F, FontStyle.Regular);
        Bitmap B = new Bitmap(Width, Height);
        Graphics G = Graphics.FromImage(B);
        var _with1 = G;
        _with1.SmoothingMode = SmoothingMode.HighQuality;
        _with1.Clear(BackColor);
        if (!editing)
        {
            if (State == MouseState.Over)
            {
                G.FillPath(new SolidBrush(Color.FromArgb(220, 220, 220)), RoundRect(new Rectangle(0, 3, (int)G.MeasureString(Text, Font).Width + 1, Height - 5), 3));
            }
        }
        _with1.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(1, 6), StringFormat.GenericDefault);

        LinearGradientBrush buttonGrad = new LinearGradientBrush(new Rectangle(0, 2, Height - 7, Height - 1), Color.White, Color.FromArgb(225, 225, 225), 90);
        if (editing)
        {
            _with1.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            _with1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            _with1.FillEllipse(buttonGrad, new Rectangle(tbChange.Size.Width + 4, 3, Height - 7, Height - 7));
            _with1.DrawEllipse(Pens.Black, new Rectangle(tbChange.Size.Width + 4, 3, Height - 7, Height - 7));
            _with1.DrawString("a", new Font(fontButtons.FontFamily, 14), Brushes.Black, new Point(tbChange.Size.Width + 1, 1));

            _with1.FillEllipse(buttonGrad, new Rectangle(tbChange.Size.Width + 5 + (Height - 7) + 3, 3, Height - 7, Height - 7));
            _with1.DrawEllipse(Pens.Black, new Rectangle(tbChange.Size.Width + 5 + (Height - 7) + 3, 3, Height - 7, Height - 7));
            _with1.DrawString("r", new Font(fontButtons.FontFamily, 9), Brushes.Black, new Point(tbChange.Size.Width + 5 + (Height - 7) + 4, 5));
        }
        G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
        e.Graphics.DrawImage(B, new Point(0, 0));
        _with1.Dispose();
        B.Dispose();

    }
    protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseClick(e);
        Point mouseL = e.Location;

        var _with2 = tbChange;
        if (e.Location.X < CreateGraphics().MeasureString(Text, Font).Width + 2)
        {
            _with2.Location = new Point(0, 0);
            _with2.Size = new Size(Width - 50, Height);
            _with2.Visible = true;
            editing = _with2.Visible;
            _with2.Focus();
            _with2.SelectAll();
        }

        if ((editing))
        {
            if (mouseL.X >= (Width - 45) && mouseL.X <= (Width - 45) + 18)
            {
                editing = false;
                Text = tbChange.Text;
            }
            else if (mouseL.X >= (Width - 45) + 18)
            {
                editing = false;
            }
            tbChange.Visible = editing;
        }

        Invalidate();
    }

    private void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (editing)
        {
            if (e.KeyCode == Keys.Enter)
            {
                editing = false;
                Text = tbChange.Text;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                editing = false;
            }

            tbChange.Visible = editing;
        }

        Invalidate();
    }

    #endregion

}

#region Aero Toolbar Themes
// Thanks for fixes:
//  * Marco Minerva, jachymko - http://www.codeplex.com/windowsformsaero
//  * Ben Ryves - http://www.benryves.com/
//
// ** Note for anyone considering using this: **
//
// A better alternative to using this class is to use the MainMenu and ContextMenu
// controls instead of MenuStrip and ContextMenuStrip, as they provide true native
// rendering. If you require icons, try this:
//
// http://wyday.com/blog/2009/making-the-menus-in-your-net-app-look-professional/
public enum ToolbarTheme
{
    Toolbar,
    MediaToolbar,
    CommunicationsToolbar,
    BrowserTabBar,
    HelpBar
}

/// <summary>Renders a toolstrip using the UxTheme API via VisualStyleRenderer and a specific style.</summary>
/// <remarks>Perhaps surprisingly, this does not need to be disposable.</remarks>
public class ToolStripAeroRenderer : ToolStripSystemRenderer
{
    VisualStyleRenderer renderer;

    public ToolStripAeroRenderer(ToolbarTheme theme, bool drawBackground)
    {
        Theme = theme;
        DrawBackground = drawBackground;
    }

    /// <summary>
    /// It shouldn't be necessary to P/Invoke like this, however VisualStyleRenderer.GetMargins
    /// misses out a parameter in its own P/Invoke.
    /// </summary>
    static internal class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("uxtheme.dll")]
        public extern static int GetThemeMargins(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, IntPtr rect, out MARGINS pMargins);
    }

    // See http://msdn2.microsoft.com/en-us/library/bb773210.aspx - "Parts and States"
    // Only menu-related parts/states are needed here, VisualStyleRenderer handles most of the rest.
    enum MenuParts : int
    {
        ItemTMSchema = 1,
        DropDownTMSchema = 2,
        BarItemTMSchema = 3,
        BarDropDownTMSchema = 4,
        ChevronTMSchema = 5,
        SeparatorTMSchema = 6,
        BarBackground = 7,
        BarItem = 8,
        PopupBackground = 9,
        PopupBorders = 10,
        PopupCheck = 11,
        PopupCheckBackground = 12,
        PopupGutter = 13,
        PopupItem = 14,
        PopupSeparator = 15,
        PopupSubmenu = 16,
        SystemClose = 17,
        SystemMaximize = 18,
        SystemMinimize = 19,
        SystemRestore = 20
    }

    enum MenuBarStates : int
    {
        Active = 1,
        Inactive = 2
    }

    enum MenuBarItemStates : int
    {
        Normal = 1,
        Hover = 2,
        Pushed = 3,
        Disabled = 4,
        DisabledHover = 5,
        DisabledPushed = 6
    }

    enum MenuPopupItemStates : int
    {
        Normal = 1,
        Hover = 2,
        Disabled = 3,
        DisabledHover = 4
    }

    enum MenuPopupCheckStates : int
    {
        CheckmarkNormal = 1,
        CheckmarkDisabled = 2,
        BulletNormal = 3,
        BulletDisabled = 4
    }

    enum MenuPopupCheckBackgroundStates : int
    {
        Disabled = 1,
        Normal = 2,
        Bitmap = 3
    }

    enum MenuPopupSubMenuStates : int
    {
        Normal = 1,
        Disabled = 2
    }

    enum MarginTypes : int
    {
        Sizing = 3601,
        Content = 3602,
        Caption = 3603
    }

    static readonly int RebarBackground = 6;

    Padding GetThemeMargins(IDeviceContext dc, MarginTypes marginType)
    {
        NativeMethods.MARGINS margins;
        try
        {
            IntPtr hDC = dc.GetHdc();
            if (0 == NativeMethods.GetThemeMargins(renderer.Handle, hDC, renderer.Part, renderer.State, (int)marginType, IntPtr.Zero, out margins))
                return new Padding(margins.cxLeftWidth, margins.cyTopHeight, margins.cxRightWidth, margins.cyBottomHeight);
            return new Padding(0);
        }
        finally
        {
            dc.ReleaseHdc();
        }
    }

    private static int GetItemState(ToolStripItem item)
    {
        bool hot = item.Selected;

        if (item.IsOnDropDown)
        {
            if (item.Enabled)
                return hot ? (int)MenuPopupItemStates.Hover : (int)MenuPopupItemStates.Normal;
            return hot ? (int)MenuPopupItemStates.DisabledHover : (int)MenuPopupItemStates.Disabled;
        }
        else
        {
            if (item.Pressed)
                return item.Enabled ? (int)MenuBarItemStates.Pushed : (int)MenuBarItemStates.DisabledPushed;
            if (item.Enabled)
                return hot ? (int)MenuBarItemStates.Hover : (int)MenuBarItemStates.Normal;
            return hot ? (int)MenuBarItemStates.DisabledHover : (int)MenuBarItemStates.Disabled;
        }
    }

    public ToolbarTheme Theme
    {
        get;
        set;
    }

    public bool DrawBackground
    {
        get;
        set;
    }

    private string RebarClass
    {
        get
        {
            return SubclassPrefix + "Rebar";
        }
    }

    private string ToolbarClass
    {
        get
        {
            return SubclassPrefix + "ToolBar";
        }
    }

    private string MenuClass
    {
        get
        {
            return SubclassPrefix + "Menu";
        }
    }

    private string SubclassPrefix
    {
        get
        {
            switch (Theme)
            {
                case ToolbarTheme.MediaToolbar: return "Media::";
                case ToolbarTheme.CommunicationsToolbar: return "Communications::";
                case ToolbarTheme.BrowserTabBar: return "BrowserTabBar::";
                case ToolbarTheme.HelpBar: return "Help::";
                default: return string.Empty;
            }
        }
    }

    private VisualStyleElement Subclass(VisualStyleElement element)
    {
        return VisualStyleElement.CreateElement(SubclassPrefix + element.ClassName,
                element.Part, element.State);
    }

    private bool EnsureRenderer()
    {
        if (!IsSupported)
            return false;

        if (renderer == null)
            renderer = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);

        return true;
    }

    // Gives parented ToolStrips a transparent background.
    protected override void Initialize(ToolStrip toolStrip)
    {
        if (toolStrip.Parent is ToolStripPanel)
            toolStrip.BackColor = Color.Transparent;

        base.Initialize(toolStrip);
    }

    // Using just ToolStripManager.Renderer without setting the Renderer individually per ToolStrip means
    // that the ToolStrip is not passed to the Initialize method. ToolStripPanels, however, are. So we can
    // simply initialize it here too, and this should guarantee that the ToolStrip is initialized at least
    // once. Hopefully it isn't any more complicated than this.
    protected override void InitializePanel(ToolStripPanel toolStripPanel)
    {
        foreach (Control control in toolStripPanel.Controls)
            if (control is ToolStrip)
                Initialize((ToolStrip)control);

        base.InitializePanel(toolStripPanel);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            renderer.SetParameters(MenuClass, (int)MenuParts.PopupBorders, 0);
            if (e.ToolStrip.IsDropDown)
            {
                Region oldClip = e.Graphics.Clip;

                // Tool strip borders are rendered *after* the content, for some reason.
                // So we have to exclude the inside of the popup otherwise we'll draw over it.
                Rectangle insideRect = e.ToolStrip.ClientRectangle;
                insideRect.Inflate(-1, -1);
                e.Graphics.ExcludeClip(insideRect);

                renderer.DrawBackground(e.Graphics, e.ToolStrip.ClientRectangle, e.AffectedBounds);

                // Restore the old clip in case the Graphics is used again (does that ever happen?)
                e.Graphics.Clip = oldClip;
            }
        }
        else
        {
            base.OnRenderToolStripBorder(e);
        }
    }

    Rectangle GetBackgroundRectangle(ToolStripItem item)
    {
        if (!item.IsOnDropDown)
            return new Rectangle(new Point(), item.Bounds.Size);

        // For a drop-down menu item, the background rectangles of the items should be touching vertically.
        // This ensures that's the case.
        Rectangle rect = item.Bounds;

        // The background rectangle should be inset two pixels horizontally (on both sides), but we have
        // to take into account the border.
        rect.X = item.ContentRectangle.X + 1;
        rect.Width = item.ContentRectangle.Width - 1;

        // Make sure we're using all of the vertical space, so that the edges touch.
        rect.Y = 0;
        return rect;
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            int partID = e.Item.IsOnDropDown ? (int)MenuParts.PopupItem : (int)MenuParts.BarItem;
            renderer.SetParameters(MenuClass, partID, GetItemState(e.Item));

            Rectangle bgRect = GetBackgroundRectangle(e.Item);
            renderer.DrawBackground(e.Graphics, bgRect, bgRect);
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }

    protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            // Draw the background using Rebar & RP_BACKGROUND (or, if that is not available, fall back to
            // Rebar.Band.Normal)
            if (VisualStyleRenderer.IsElementDefined(VisualStyleElement.CreateElement(RebarClass, RebarBackground, 0)))
            {
                renderer.SetParameters(RebarClass, RebarBackground, 0);
            }
            else
            {
                renderer.SetParameters(RebarClass, 0, 0);
            }

            if (renderer.IsBackgroundPartiallyTransparent())
                renderer.DrawParentBackground(e.Graphics, e.ToolStripPanel.ClientRectangle, e.ToolStripPanel);

            renderer.DrawBackground(e.Graphics, e.ToolStripPanel.ClientRectangle);

            e.Handled = true;
        }
        else
        {
            base.OnRenderToolStripPanelBackground(e);
        }
    }

    // Render the background of an actual menu bar, dropdown menu or toolbar.
    protected override void OnRenderToolStripBackground(System.Windows.Forms.ToolStripRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            if (e.ToolStrip.IsDropDown)
            {
                renderer.SetParameters(MenuClass, (int)MenuParts.PopupBackground, 0);
            }
            else
            {
                // It's a MenuStrip or a ToolStrip. If it's contained inside a larger panel, it should have a
                // transparent background, showing the panel's background.

                if (e.ToolStrip.Parent is ToolStripPanel)
                {
                    // The background should be transparent, because the ToolStripPanel's background will be visible.
                    // (Of course, we assume the ToolStripPanel is drawn using the same theme, but it's not my fault
                    // if someone does that.)
                    return;
                }
                else
                {
                    // A lone toolbar/menubar should act like it's inside a toolbox, I guess.
                    // Maybe I should use the MenuClass in the case of a MenuStrip, although that would break
                    // the other themes...
                    if (VisualStyleRenderer.IsElementDefined(VisualStyleElement.CreateElement(RebarClass, RebarBackground, 0)))
                        renderer.SetParameters(RebarClass, RebarBackground, 0);
                    else
                        renderer.SetParameters(RebarClass, 0, 0);
                }
            }

            if (renderer.IsBackgroundPartiallyTransparent())
                renderer.DrawParentBackground(e.Graphics, e.ToolStrip.ClientRectangle, e.ToolStrip);
            if (DrawBackground)
                renderer.DrawBackground(e.Graphics, e.ToolStrip.ClientRectangle, e.AffectedBounds);
        }
        else
        {
            base.OnRenderToolStripBackground(e);
        }
    }

    // The only purpose of this override is to change the arrow colour.
    // It's OK to just draw over the default arrow since we also pass down arrow drawing to the system renderer.
    protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            ToolStripSplitButton sb = (ToolStripSplitButton)e.Item;
            base.OnRenderSplitButtonBackground(e);

            // It doesn't matter what colour of arrow we tell it to draw. OnRenderArrow will compute it from the item anyway.
            OnRenderArrow(new ToolStripArrowRenderEventArgs(e.Graphics, sb, sb.DropDownButtonBounds, Color.Red, ArrowDirection.Down));
        }
        else
        {
            base.OnRenderSplitButtonBackground(e);
        }
    }

    Color GetItemTextColor(ToolStripItem item)
    {
        int partId = item.IsOnDropDown ? (int)MenuParts.PopupItem : (int)MenuParts.BarItem;
        renderer.SetParameters(MenuClass, partId, GetItemState(item));
        return renderer.GetColor(ColorProperty.TextColor);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (EnsureRenderer())
            e.TextColor = GetItemTextColor(e.Item);

        base.OnRenderItemText(e);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            if (e.ToolStrip.IsDropDown)
            {
                renderer.SetParameters(MenuClass, (int)MenuParts.PopupGutter, 0);
                // The AffectedBounds is usually too small, way too small to look right. Instead of using that,
                // use the AffectedBounds but with the right width. Then narrow the rectangle to the correct edge
                // based on whether or not it's RTL. (It doesn't need to be narrowed to an edge in LTR mode, but let's
                // do that anyway.)
                // Using the DisplayRectangle gets roughly the right size so that the separator is closer to the text.
                Padding margins = GetThemeMargins(e.Graphics, MarginTypes.Sizing);
                int extraWidth = (e.ToolStrip.Width - e.ToolStrip.DisplayRectangle.Width - margins.Left - margins.Right - 1) - e.AffectedBounds.Width;
                Rectangle rect = e.AffectedBounds;
                rect.Y += 2;
                rect.Height -= 4;
                int sepWidth = renderer.GetPartSize(e.Graphics, ThemeSizeType.True).Width;
                if (e.ToolStrip.RightToLeft == RightToLeft.Yes)
                {
                    rect = new Rectangle(rect.X - extraWidth, rect.Y, sepWidth, rect.Height);
                    rect.X += sepWidth;
                }
                else
                {
                    rect = new Rectangle(rect.Width + extraWidth - sepWidth, rect.Y, sepWidth, rect.Height);
                }
                renderer.DrawBackground(e.Graphics, rect);
            }
        }
        else
        {
            base.OnRenderImageMargin(e);
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        if (e.ToolStrip.IsDropDown && EnsureRenderer())
        {
            renderer.SetParameters(MenuClass, (int)MenuParts.PopupSeparator, 0);
            Rectangle rect = new Rectangle(e.ToolStrip.DisplayRectangle.Left, 0, e.ToolStrip.DisplayRectangle.Width, e.Item.Height);
            renderer.DrawBackground(e.Graphics, rect, rect);
        }
        else
        {
            base.OnRenderSeparator(e);
        }
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            Rectangle bgRect = GetBackgroundRectangle(e.Item);
            bgRect.Width = bgRect.Height;

            // Now, mirror its position if the menu item is RTL.
            if (e.Item.RightToLeft == RightToLeft.Yes)
                bgRect = new Rectangle(e.ToolStrip.ClientSize.Width - bgRect.X - bgRect.Width, bgRect.Y, bgRect.Width, bgRect.Height);

            renderer.SetParameters(MenuClass, (int)MenuParts.PopupCheckBackground, e.Item.Enabled ? (int)MenuPopupCheckBackgroundStates.Normal : (int)MenuPopupCheckBackgroundStates.Disabled);
            renderer.DrawBackground(e.Graphics, bgRect);

            Rectangle checkRect = e.ImageRectangle;
            checkRect.X = bgRect.X + bgRect.Width / 2 - checkRect.Width / 2;
            checkRect.Y = bgRect.Y + bgRect.Height / 2 - checkRect.Height / 2;

            // I don't think ToolStrip even supports radio box items, so no need to render them.
            renderer.SetParameters(MenuClass, (int)MenuParts.PopupCheck, e.Item.Enabled ? (int)MenuPopupCheckStates.CheckmarkNormal : (int)MenuPopupCheckStates.CheckmarkDisabled);

            renderer.DrawBackground(e.Graphics, checkRect);
        }
        else
        {
            base.OnRenderItemCheck(e);
        }
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        // The default renderer will draw an arrow for us (the UXTheme API seems not to have one for all directions),
        // but it will get the colour wrong in many cases. The text colour is probably the best colour to use.
        if (EnsureRenderer())
            e.ArrowColor = GetItemTextColor(e.Item);
        base.OnRenderArrow(e);
    }

    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
    {
        if (EnsureRenderer())
        {
            // BrowserTabBar::Rebar draws the chevron using the default background. Odd.
            string rebarClass = RebarClass;
            if (Theme == ToolbarTheme.BrowserTabBar)
                rebarClass = "Rebar";

            int state = VisualStyleElement.Rebar.Chevron.Normal.State;
            if (e.Item.Pressed)
                state = VisualStyleElement.Rebar.Chevron.Pressed.State;
            else if (e.Item.Selected)
                state = VisualStyleElement.Rebar.Chevron.Hot.State;

            renderer.SetParameters(rebarClass, VisualStyleElement.Rebar.Chevron.Normal.Part, state);
            renderer.DrawBackground(e.Graphics, new Rectangle(Point.Empty, e.Item.Size));
        }
        else
        {
            base.OnRenderOverflowButtonBackground(e);
        }
    }

    public bool IsSupported
    {
        get
        {
            if (!VisualStyleRenderer.IsSupported)
                return false;

            // Needs a more robust check. It seems mono supports very different style sets.
            return
                    VisualStyleRenderer.IsElementDefined(
                            VisualStyleElement.CreateElement("Menu",
                                    (int)MenuParts.BarBackground,
                                    (int)MenuBarStates.Active));
        }
    }
}
#endregion

//    #region " Sort "
//    /// <summary>
//    /// This class is an implementation of the 'IComparer' interface.
//    /// </summary>
//    public class ListViewColumnSorter : IComparer
//    {
//        /// <summary>
//        /// Specifies the column to be sorted
//        /// </summary>
//        private int ColumnToSort;
//        /// <summary>
//        /// Specifies the order in which to sort (i.e. 'Ascending').
//        /// </summary>
//        private SortOrder OrderOfSort;
//        /// <summary>
//        /// Case insensitive comparer object
//        /// </summary>
//        //private CaseInsensitiveComparer ObjectCompare;
//        private NumberCaseInsensitiveComparer ObjectCompare;
//        private ImageTextComparer FirstObjectCompare;

//        /// <summary>
//        /// Class constructor.  Initializes various elements
//        /// </summary>
//        public ListViewColumnSorter()
//        {
//            // Initialize the column to '0'
//            ColumnToSort = 0;

//            // Initialize the sort order to 'none'
//            //OrderOfSort = SortOrder.None;
//            OrderOfSort = SortOrder.Ascending;

//            // Initialize the CaseInsensitiveComparer object
//            ObjectCompare = new NumberCaseInsensitiveComparer();//CaseInsensitiveComparer();
//            FirstObjectCompare = new ImageTextComparer();
//        }

//        /// <summary>
//        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
//        /// </summary>
//        /// <param name="x">First object to be compared</param>
//        /// <param name="y">Second object to be compared</param>
//        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
//        public int Compare(object x, object y)
//        {
//            int compareResult;
//            ListViewItem listviewX, listviewY;

//            // Cast the objects to be compared to ListViewItem objects
//            listviewX = (ListViewItem)x;
//            listviewY = (ListViewItem)y;

//            if (ColumnToSort == 0)
//            {
//                compareResult = FirstObjectCompare.Compare(x, y);
//            }
//            else
//            {
//                // Compare the two items
//                compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
//            }

//            // Calculate correct return value based on object comparison
//            if (OrderOfSort == SortOrder.Ascending)
//            {
//                // Ascending sort is selected, return normal result of compare operation
//                return compareResult;
//            }
//            else if (OrderOfSort == SortOrder.Descending)
//            {
//                // Descending sort is selected, return negative result of compare operation
//                return (-compareResult);
//            }
//            else
//            {
//                // Return '0' to indicate they are equal
//                return 0;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
//        /// </summary>
//        public int SortColumn
//        {
//            set
//            {
//                ColumnToSort = value;
//            }
//            get
//            {
//                return ColumnToSort;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
//        /// </summary>
//        public SortOrder Order
//        {
//            set
//            {
//                OrderOfSort = value;
//            }
//            get
//            {
//                return OrderOfSort;
//            }
//        }

//    }

//    public class ImageTextComparer : IComparer
//    {
//        //private CaseInsensitiveComparer ObjectCompare;
//        private NumberCaseInsensitiveComparer ObjectCompare;

//        public ImageTextComparer()
//        {
//            // Initialize the CaseInsensitiveComparer object
//            ObjectCompare = new NumberCaseInsensitiveComparer();//CaseInsensitiveComparer();
//        }

//        public int Compare(object x, object y)
//        {
//            //int compareResult;
//            int image1, image2;
//            ListViewItem listviewX, listviewY;

//            // Cast the objects to be compared to ListViewItem objects
//            listviewX = (ListViewItem)x;
//            image1 = listviewX.ImageIndex;
//            listviewY = (ListViewItem)y;
//            image2 = listviewY.ImageIndex;

//            if (image1 < image2)
//            {
//                return -1;
//            }
//            else if (image1 == image2)
//            {
//                return ObjectCompare.Compare(listviewX.Text, listviewY.Text);
//            }
//            else
//            {
//                return 1;
//            }
//        }
//    }

//    public class NumberCaseInsensitiveComparer : CaseInsensitiveComparer
//    {
//        public NumberCaseInsensitiveComparer()
//        {

//        }

//        public new int Compare(object x, object y)
//        {
//            if ((x is System.String) && IsWholeNumber((string)x) && (y is System.String) && IsWholeNumber((string)y))
//            {
//                return base.Compare(System.Convert.ToInt32(x), System.Convert.ToInt32(y));
//            }
//            else
//            {
//                return base.Compare(x, y);
//            }
//        }

//        private bool IsWholeNumber(string strNumber)
//        {
//            Regex objNotWholePattern = new Regex("[^0-9]");
//            return !objNotWholePattern.IsMatch(strNumber);
//        }
//    }
//    #endregion
//}
