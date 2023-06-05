/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[ToolboxItem(false)]
	public abstract class TabStyleProvider : Component
	{
		#region Constructor

		protected TabStyleProvider(CustomTabControl tabControl) {
			_TabControl = tabControl;

			_BorderColor = Color.Empty;
			_BorderColorSelected = Color.Empty;
			_FocusColor = Color.Orange;

			_ImageAlign = _TabControl.RightToLeftLayout ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;

			HotTrack = true;

			//	Must set after the _Overlap as this is used in the calculations of the actual padding
			Padding = new Point(6, 3);
		}

		#endregion

		#region Factory Methods

		public static TabStyleProvider CreateProvider(CustomTabControl tabControl) {
			TabStyleProvider provider;

			//	Depending on the display style of the tabControl generate an appropriate provider.
			switch (tabControl.DisplayStyle) {
				case TabStyle.None:
					provider = new TabStyleNoneProvider(tabControl);
					break;

				case TabStyle.Default:
					provider = new TabStyleDefaultProvider(tabControl);
					break;

				case TabStyle.Angled:
					provider = new TabStyleAngledProvider(tabControl);
					break;

				case TabStyle.Rounded:
					provider = new TabStyleRoundedProvider(tabControl);
					break;

				case TabStyle.VisualStudio:
					provider = new TabStyleVisualStudioProvider(tabControl);
					break;

				case TabStyle.Chrome:
					provider = new TabStyleChromeProvider(tabControl);
					break;

				case TabStyle.IE8:
					provider = new TabStyleIE8Provider(tabControl);
					break;

				case TabStyle.VS2010:
					provider = new TabStyleVS2010Provider(tabControl);
					break;

				default:
					provider = new TabStyleDefaultProvider(tabControl);
					break;
			}

			provider._Style = tabControl.DisplayStyle;
			return provider;
		}

		#endregion

		#region	Protected variables

		protected CustomTabControl _TabControl;
		protected Point _Padding;
		protected bool _HotTrack;
		protected TabStyle _Style = TabStyle.Default;
		protected ContentAlignment _ImageAlign;
		protected int _Radius = 1;
		protected int _Overlap;
		protected bool _FocusTrack;
		protected float _Opacity = 1;
		protected bool _ShowTabCloser;
		protected Color _BorderColorSelected = Color.Empty;
		protected Color _BorderColor = Color.Empty;
		protected Color _BorderColorHot = Color.Empty;
		protected Color _CloserColorActive = Color.Black;
		protected Color _CloserColor = Color.DarkGray;
		protected Color _FocusColor = Color.Empty;
		protected Color _TextColor = Color.Empty;
		protected Color _TextColorSelected = Color.Empty;
		protected Color _TextColorDisabled = Color.Empty;
		FontStyle _SelectedTextStyle;

		#endregion

		#region overridable Methods

		public abstract void AddTabBorder(GraphicsPath path, Rectangle tabBounds);

		public virtual Rectangle GetTabRect(int index) {
			if (index < 0) {
				return new Rectangle();
			}
			Rectangle tabBounds = _TabControl.GetTabRect(index);
			if (_TabControl.RightToLeftLayout) {
				tabBounds.X = _TabControl.Width - tabBounds.Right;
			}
			bool firstTabinRow = _TabControl.IsFirstTabInRow(index);

			//	Expand to overlap the tabpage
			switch (_TabControl.Alignment) {
				case TabAlignment.Top:
					tabBounds.Height += 2;
					break;
				case TabAlignment.Bottom:
					tabBounds.Height += 2;
					tabBounds.Y -= 2;
					break;
				case TabAlignment.Left:
					tabBounds.Width += 2;
					break;
				case TabAlignment.Right:
					tabBounds.X -= 2;
					tabBounds.Width += 2;
					break;
			}

			//	Greate Overlap unless first tab in the row to align with tabpage
			if ((!firstTabinRow || _TabControl.RightToLeftLayout) && _Overlap > 0) {
				if (_TabControl.Alignment <= TabAlignment.Bottom) {
					tabBounds.X -= _Overlap;
					tabBounds.Width += _Overlap;
				}
				else {
					tabBounds.Y -= _Overlap;
					tabBounds.Height += _Overlap;
				}
			}

			//	Adjust first tab in the row to align with tabpage
			EnsureFirstTabIsInView(ref tabBounds, index);

			return tabBounds;
		}

		protected virtual void EnsureFirstTabIsInView(ref Rectangle tabBounds, int index) {
			//	Adjust first tab in the row to align with tabpage
			//	Make sure we only reposition visible tabs, as we may have scrolled out of view.

			bool firstTabinRow = _TabControl.IsFirstTabInRow(index);

			if (firstTabinRow) {
				if (_TabControl.Alignment <= TabAlignment.Bottom) {
					if (_TabControl.RightToLeftLayout) {
						if (tabBounds.Left < _TabControl.Right) {
							int tabPageRight = _TabControl.GetPageBounds(index).Right;
							if (tabBounds.Right > tabPageRight) {
								tabBounds.Width -= (tabBounds.Right - tabPageRight);
							}
						}
					}
					else {
						if (tabBounds.Right > 0) {
							int tabPageX = _TabControl.GetPageBounds(index).X;
							if (tabBounds.X < tabPageX) {
								tabBounds.Width -= (tabPageX - tabBounds.X);
								tabBounds.X = tabPageX;
							}
						}
					}
				}
				else {
					if (_TabControl.RightToLeftLayout) {
						if (tabBounds.Top < _TabControl.Bottom) {
							int tabPageBottom = _TabControl.GetPageBounds(index).Bottom;
							if (tabBounds.Bottom > tabPageBottom) {
								tabBounds.Height -= (tabBounds.Bottom - tabPageBottom);
							}
						}
					}
					else {
						if (tabBounds.Bottom > 0) {
							int tabPageY = _TabControl.GetPageBounds(index).Location.Y;
							if (tabBounds.Y < tabPageY) {
								tabBounds.Height -= (tabPageY - tabBounds.Y);
								tabBounds.Y = tabPageY;
							}
						}
					}
				}
			}
		}

		protected virtual Brush GetTabBackgroundBrush(int index) {
			LinearGradientBrush fillBrush = null;

			//	Capture the colours dependant on selection state of the tab
			Color dark = Color.FromArgb(207, 207, 207);
			Color light = Color.FromArgb(242, 242, 242);

			if (_TabControl.SelectedIndex == index) {
				dark = SystemColors.ControlLight;
				light = SystemColors.Window;
			}
			else if (!_TabControl.TabPages[index].Enabled) {
				light = dark;
			}
			else if (_HotTrack && index == _TabControl.ActiveIndex) {
				//	Enable hot tracking
				light = Color.FromArgb(234, 246, 253);
				dark = Color.FromArgb(167, 217, 245);
			}

			//	Get the correctly aligned gradient
			Rectangle tabBounds = GetTabRect(index);
			tabBounds.Inflate(3, 3);
			tabBounds.X -= 1;
			tabBounds.Y -= 1;
			switch (_TabControl.Alignment) {
				case TabAlignment.Top:
					if (_TabControl.SelectedIndex == index) {
						dark = light;
					}
					fillBrush = new LinearGradientBrush(tabBounds, light, dark, LinearGradientMode.Vertical);
					break;
				case TabAlignment.Bottom:
					fillBrush = new LinearGradientBrush(tabBounds, light, dark, LinearGradientMode.Vertical);
					break;
				case TabAlignment.Left:
					fillBrush = new LinearGradientBrush(tabBounds, dark, light, LinearGradientMode.Horizontal);
					break;
				case TabAlignment.Right:
					fillBrush = new LinearGradientBrush(tabBounds, light, dark, LinearGradientMode.Horizontal);
					break;
			}

			//	Add the blend
			fillBrush.Blend = GetBackgroundBlend();

			return fillBrush;
		}

		#endregion

		#region	Base Properties

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TabStyle DisplayStyle {
			get => _Style;
			set => _Style = value;
		}

		[Category("Appearance")]
		public ContentAlignment ImageAlign {
			get => _ImageAlign;
			set {
				_ImageAlign = value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance")]
		public Point Padding {
			get => _Padding;
			set {
				_Padding = value;
				var c = (TabControl)_TabControl;
				//	This line will trigger the handle to recreate, therefore invalidating the control
				if (_ShowTabCloser) {
					c.Padding = value.X + (int)(_Radius / 2) < -_TabControl.TabCloserSize
						? new Point(0, value.Y)
						: new Point(value.X + (int)(_Radius / 2) + _TabControl.TabCloserSize, value.Y);
				}
				else {
					c.Padding = value.X + (int)(_Radius / 2) < 1 ? new Point(0, value.Y) : new Point(value.X + (int)(_Radius / 2) - 1, value.Y);
				}
			}
		}

		[Category("Appearance"), DefaultValue(1), Browsable(true)]
		public int Radius {
			get => _Radius;
			set {
				if (value < 1) {
					throw new ArgumentException("The radius must be greater than 1", "value");
				}
				_Radius = value;
				//	Adjust padding
				Padding = _Padding;
			}
		}

		[Category("Appearance")]
		public int Overlap {
			get => _Overlap;
			set {
				if (value < 0) {
					throw new ArgumentException("The tabs cannot have a negative overlap", "value");
				}
				_Overlap = value;
			}
		}

		[Category("Appearance")]
		public bool FocusTrack {
			get => _FocusTrack;
			set {
				_FocusTrack = value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance")]
		public bool HotTrack {
			get => _HotTrack;
			set {
				_HotTrack = value;
				((TabControl)_TabControl).HotTrack = value;
			}
		}

		[Category("Appearance")]
		public bool ShowTabCloser {
			get => _ShowTabCloser;
			set {
				_ShowTabCloser = value;
				//	Adjust padding
				Padding = _Padding;
			}
		}

		[Category("Appearance")]
		public float Opacity {
			get => _Opacity;
			set {
				if (value < 0) {
					throw new ArgumentException("The opacity must be between 0 and 1", "value");
				}
				if (value > 1) {
					throw new ArgumentException("The opacity must be between 0 and 1", "value");
				}
				_Opacity = value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color BorderColorSelected {
			get => _BorderColorSelected.IsEmpty ? ThemedColors.ToolBorder : _BorderColorSelected;
			set {
				_BorderColorSelected = value.Equals(ThemedColors.ToolBorder) ? Color.Empty : value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color BorderColorHot {
			get => _BorderColorHot.IsEmpty ? SystemColors.ControlDark : _BorderColorHot;
			set {
				_BorderColorHot = value.Equals(SystemColors.ControlDark) ? Color.Empty : value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color BorderColor {
			get => _BorderColor.IsEmpty ? SystemColors.ControlDark : _BorderColor;
			set {
				_BorderColor = value.Equals(SystemColors.ControlDark) ? Color.Empty : value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color TextColor {
			get => _TextColor.IsEmpty ? SystemColors.ControlText : _TextColor;
			set {
				_TextColor = value.Equals(SystemColors.ControlText) ? Color.Empty : value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color TextColorSelected {
			get => _TextColorSelected.IsEmpty ? SystemColors.ControlText : _TextColorSelected;
			set {
				_TextColorSelected = value.Equals(SystemColors.ControlText) ? Color.Empty : value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance")]
		public FontStyle SelectedTextStyle {
			get => _SelectedTextStyle;
			set {
				if (value != _SelectedTextStyle) {
					_SelectedTextStyle = value;
					_TabControl.Invalidate();
				}
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color TextColorDisabled {
			get => _TextColor.IsEmpty ? SystemColors.ControlDark : _TextColorDisabled;
			set {
				_TextColorDisabled = value.Equals(SystemColors.ControlDark) ? Color.Empty : value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "Orange")]
		public Color FocusColor {
			get => _FocusColor;
			set {
				_FocusColor = value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "Black")]
		public Color CloserColorActive {
			get => _CloserColorActive;
			set {
				_CloserColorActive = value;
				_TabControl.Invalidate();
			}
		}

		[Category("Appearance"), DefaultValue(typeof(Color), "DarkGrey")]
		public Color CloserColor {
			get => _CloserColor;
			set {
				_CloserColor = value;
				_TabControl.Invalidate();
			}
		}

		#endregion

		#region Painting
		public void PaintTab(int index, Graphics graphics) {
			using (GraphicsPath tabpath = GetTabBorder(index)) {
				using (Brush fillBrush = GetTabBackgroundBrush(index)) {
					//	Paint the background
					graphics.FillPath(fillBrush, tabpath);

					//	Paint a focus indication
					if (_TabControl.Focused) {
						DrawTabFocusIndicator(tabpath, index, graphics);
					}

					//	Paint the closer
					if (ShouldDrawTabCloser(index)) {
						DrawTabCloser(index, graphics);
					}
				}
			}
		}

		internal bool ShouldDrawTabCloser(int index) {
			var controls = _TabControl.TabPages[index].Controls;
			return controls.Count <= 0 || controls[0] is ITabContent c == false || c.CanClose;
		}

		protected virtual void DrawTabCloser(int index, Graphics graphics) {
			if (_ShowTabCloser) {
				Rectangle closerRect = _TabControl.GetTabCloserRect(index);
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (GraphicsPath closerPath = GetCloserPath(closerRect))
				using (Pen closerPen = new Pen(closerRect.Contains(_TabControl.MousePosition) ? _CloserColorActive : _CloserColor)) {
					graphics.DrawPath(closerPen, closerPath);
				}
			}
		}

		protected static GraphicsPath GetCloserPath(Rectangle closerRect) {
			GraphicsPath closerPath = new GraphicsPath();
			closerPath.AddLine(closerRect.X, closerRect.Y, closerRect.Right, closerRect.Bottom);
			closerPath.CloseFigure();
			closerPath.AddLine(closerRect.Right, closerRect.Y, closerRect.X, closerRect.Bottom);
			closerPath.CloseFigure();

			return closerPath;
		}

		private void DrawTabFocusIndicator(GraphicsPath tabpath, int index, Graphics graphics) {
			if (_FocusTrack && _TabControl.Focused && index == _TabControl.SelectedIndex) {
				RectangleF pathRect = tabpath.GetBounds();
				Rectangle focusRect;
				LinearGradientBrush focusBrush;
				switch (_TabControl.Alignment) {
					case TabAlignment.Top:
						focusRect = new Rectangle((int)pathRect.X, (int)pathRect.Y, (int)pathRect.Width, 4);
						focusBrush = new LinearGradientBrush(focusRect, _FocusColor, SystemColors.Window, LinearGradientMode.Vertical);
						break;
					case TabAlignment.Bottom:
						focusRect = new Rectangle((int)pathRect.X, (int)pathRect.Bottom - 4, (int)pathRect.Width, 4);
						focusBrush = new LinearGradientBrush(focusRect, SystemColors.ControlLight, _FocusColor, LinearGradientMode.Vertical);
						break;
					case TabAlignment.Left:
						focusRect = new Rectangle((int)pathRect.X, (int)pathRect.Y, 4, (int)pathRect.Height);
						focusBrush = new LinearGradientBrush(focusRect, _FocusColor, SystemColors.ControlLight, LinearGradientMode.Horizontal);
						break;
					case TabAlignment.Right:
						focusRect = new Rectangle((int)pathRect.Right - 4, (int)pathRect.Y, 4, (int)pathRect.Height);
						focusBrush = new LinearGradientBrush(focusRect, SystemColors.ControlLight, _FocusColor, LinearGradientMode.Horizontal);
						break;
					default:
						goto case TabAlignment.Top;
				}

				//	Ensure the focus stip does not go outside the tab
				using (focusBrush)
				using (Region focusRegion = new Region(focusRect)) {
					focusRegion.Intersect(tabpath);
					graphics.FillRegion(focusBrush, focusRegion);
				}
			}
		}

		#endregion

		#region Background brushes

		private Blend GetBackgroundBlend() {
			return _TabControl.Alignment == TabAlignment.Top ? new Blend {
				Factors = new float[] { 0f, 0.5f, 1f, 1f },
				Positions = new float[] { 0f, 0.5f, 0.51f, 1f }
			} : new Blend {
				Factors = new float[] { 0f, 0.7f, 1f },
				Positions = new float[] { 0f, 0.6f, 1f }
			};
		}

		public virtual Brush GetPageBackgroundBrush(int index) {
			//	Capture the colors dependent on selection state of the tab
			Color light;
			if (_TabControl.SelectedIndex == index) {
				light = SystemColors.Window;
			}
			else if (!_TabControl.TabPages[index].Enabled) {
				light = Color.FromArgb(207, 207, 207);
			}
			else if (_HotTrack && index == _TabControl.ActiveIndex) {
				//	Enable hot tracking
				light = Color.FromArgb(234, 246, 253);
			}
			else if (_TabControl.Alignment == TabAlignment.Top) {
				light = Color.FromArgb(207, 207, 207);
			}
			else {
				light = Color.FromArgb(242, 242, 242);
			}

			return new SolidBrush(light);
		}

		#endregion

		#region Tab border and rect

		public GraphicsPath GetTabBorder(int index) {
			GraphicsPath path = new GraphicsPath();

			AddTabBorder(path, GetTabRect(index));

			path.CloseFigure();
			return path;
		}

		#endregion

	}
}
