using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using TrivialViewer.Trivial;


namespace TrivialViewer
{
	public partial class ViewerWindow : Form
	{
		const int WINDOW_GRIP_SIZE = 5;

		const string DEFAULT_IMAGE_PATH = "DoNotBuy.jpg";

		protected delegate void PaintDelegate(PaintEventArgs ev);

		protected PaintDelegate paintMethod;

		/// <summary>
		/// The image being displayed
		/// </summary>
		protected Bitmap image;

		protected bool cropMode = false;

		/// <summary>
		/// The original ratio of width to height
		/// </summary>
		protected float ratio;

		/// <summary>
		/// Desktop dimensions
		/// </summary>
		protected Rectangle screen;

		/// <summary>
		/// The intended width and height of the image - since Windows forces the actual
		/// width and height to not exceed the size of the screen, we need to play some
		/// tricks when one of these exceeds the screen size.
		/// </summary>
		protected int documentWidth;
		protected int documentHeight;
		protected int documentTop;
		protected int documentLeft;


		#region App status vars
		// Drag info - only active during dragging
		protected DragMode dragMode = DragMode.None;

		// The location of the mouse relative to the desktop when dragging started
		protected Point screenStartMouse;

		// The boundaries of the document when dragging started
		protected Rectangle documentStartBox;
		#endregion

		public ViewerWindow()
		{
			InitializeComponent();
			init(Application.StartupPath + "\\..\\..\\" + DEFAULT_IMAGE_PATH);
		}

		public ViewerWindow(string imagePath)
		{
			InitializeComponent();
			init(imagePath);
		}

		/*
		 * TODO: Make stretching a little nicer - something to do with distance of mouse cursor
		 * away from center, but should it be center based on initial mousedown or center from
		 * mousemove to mousemove? Iterative function problem...
		 * Probably event to event, and draw a line from the new cursor position to the current
		 * center. The opposite edge it hits is fixed and won't move as a result of this resize.
		 * Every other edge is a candidate for resize based on a percentage of distance from
		 * the place where that line hits that opposite edge - so the edge the cursor is
		 * dragging is farthest, then the one it's sort of dragging, then the one that is most
		 * in tandem with the fixed one, and finally the fixed one.
		 * 
		 * Then the image shrinks or grows based on the mouse cursor's position.
		 * 
		 * TODO: Mouse-over controls like 1x, current %zoom, cursor draws coords right next to
		 * it
		 * 
		 * TODO: Make loading small, translucent, or all-white, etc images more noticeable by highlighting them on the screen
		 * briefly, and maybe doing so again when they regain focus via the Windows taskbar etc.
		 * 
		 * TODO: Test pinch to zoom on Win8 - might already work with the way events tie to resize
		 */

		protected void init(string imagePath)
		{
			ControlBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.None;
			AutoScaleMode = AutoScaleMode.None;
			AutoScroll = false;
			DoubleBuffered = true;

			// For some reason the Windows Forms Designer in Visual Studio 2008 does not offer
			// the MouseWheel event. Tack it on here manually.
			this.MouseWheel += new MouseEventHandler(onMouseWheel);


			this.Text = imagePath;



			MinimumSize = new Size(3 * WINDOW_GRIP_SIZE, 3 * WINDOW_GRIP_SIZE);

			image = new Bitmap(imagePath);

			// Prepare the initial image dimensions at pixel for pixel
			int width = image.Width;
			int height = image.Height;
			ratio = (float)width / (float)height;

			// Get the desktop it's mostly inside of
			screen = Screen.GetWorkingArea(this);

			// If it's too wide, shrink it to the width of the desktop
			if (width > screen.Width)
			{
				width = screen.Width - 4;
				// integer division - multiply first, no big deal
				height = width * image.Height / image.Width;
			}

			// If it's still too tall, shrink it to the height of the desktop
			if (height > screen.Height)
			{
				height = screen.Height - 4;
				// integer division - multiply first, no big deal
				width = height * image.Width / image.Height;
			}

			//DesktopBounds = new Rectangle(DesktopBounds.X, DesktopBounds.Y, width, height);
			this.Size = new Size(width, height);
			this.BackgroundImageLayout = ImageLayout.Stretch;
			this.BackgroundImage = image;

			paintMethod = paintNormal;

			documentWidth = Width;
			documentHeight = Height;
			documentLeft = Left;
			documentTop = Top;
		}

		protected void onMouseWheel(object sender, MouseEventArgs ev)
		{
			/*
			if (ev.Delta == 0)
				return;

			double changeBy;
			if (ev.Delta > 0)
				changeBy = Math.Pow(1.05, ev.Delta);
			else
				changeBy = Math.Pow(.95, -ev.Delta);

			if (Height > Width)
			{
				Height = (int)(Height * changeBy);
				Width = (int)(Width * Height * ratio);
			}
			else
			{
				Width = (int)(Width * changeBy);
				Height = (int)(Height * Width / ratio);
			}
			*/
		}

		protected void onMouseClick(object sender, MouseEventArgs ev)
		{
			if (ev.Button == MouseButtons.Right)
				onRightClick(ev);
		}

		protected void onRightClick(MouseEventArgs ev)
		{
			Close();
		}

		protected void onMouseDown(object sender, MouseEventArgs ev)
		{
			if (ev.Button != MouseButtons.Left)
				return;

			dragMode = GetDragMode(ev, Size);
			screenStartMouse = Control.MousePosition;
			documentStartBox = new Rectangle(documentLeft, documentTop, documentWidth, documentHeight);
		}

		protected void onMouseMove(object sender, MouseEventArgs ev)
		{
			switch (dragMode)
			{
				case DragMode.None:
					DragMode borderCheck = GetDragMode(ev, Size);
					switch (borderCheck)
					{
						case DragMode.Move:
							Cursor = Cursors.Default;
							break;
						case DragMode.Top:
						case DragMode.Bottom:
							Cursor = Cursors.SizeNS;
							break;
						case DragMode.TopRight:
						case DragMode.BottomLeft:
							Cursor = Cursors.SizeNESW;
							break;
						case DragMode.Right:
						case DragMode.Left:
							Cursor = Cursors.SizeWE;
							break;
						case DragMode.BottomRight:
						case DragMode.TopLeft:
							Cursor = Cursors.SizeNWSE;
							break;
					}
					break;

				case DragMode.Move:
					Point screenCurrentMouse = Control.MousePosition;
					documentLeft = documentStartBox.X + screenCurrentMouse.X - screenStartMouse.X;
					documentTop = documentStartBox.Y + screenCurrentMouse.Y - screenStartMouse.Y;
					updateDimensions();
					break;

				/*
				default:
					PointF mouse = new PointF(Control.MousePosition.X, Control.MousePosition.Y);
					PointF oldCenter = new PointF(Left + Width / 2f, Top + Height / 2f);
					PointF mouseDirection = new PointF(mouse.X - oldCenter.X, mouse.Y - oldCenter.Y);

					float newTop = Top;
					float newLeft = Left;
					float newBottom = Top + Height;
					float newRight = Left + Width;
					if (mouseDirection.X > 0)
						newRight = mouse.X;
					else
						newLeft = mouse.X;

					if (mouseDirection.Y > 0)
						newBottom = mouse.Y;
					else
						newTop = mouse.Y;

					float calcWidth = (newBottom - newTop) * ratio;
					float calcHeight = (newRight - newLeft) / ratio;


					break;
				*/
				
				case DragMode.Right:
					documentWidth = Control.MousePosition.X - Location.X;
					documentHeight = (int)(documentWidth / ratio);
					documentTop = documentStartBox.Y + documentStartBox.Height / 2 - documentHeight / 2;
					updateDimensions();
					break;

				case DragMode.Bottom:
					documentHeight = Control.MousePosition.Y - Location.Y;
					documentWidth = (int)(documentHeight * ratio);
					documentLeft = documentStartBox.X + documentStartBox.Width / 2 - documentWidth / 2;
					updateDimensions();
					break;

				case DragMode.Left:
					documentLeft = Control.MousePosition.X;
					documentWidth = documentStartBox.X + documentStartBox.Width - Control.MousePosition.X;
					documentHeight = (int)(documentWidth / ratio);
					documentTop = documentStartBox.Y + documentStartBox.Height / 2 - documentHeight / 2;
					updateDimensions();
					break;

				case DragMode.Top:
					documentTop = Control.MousePosition.Y;
					documentHeight = documentStartBox.Y + documentStartBox.Height - Control.MousePosition.Y;
					documentWidth = (int)(documentHeight * ratio);
					documentLeft = documentStartBox.X + documentStartBox.Width / 2 - documentWidth / 2;
					updateDimensions();
					break;
				

				/*
				// This works, it's just weird to enable it when the other corners aren't coded
				case DragMode.BottomRight:
					// What each would be if we just followed the mouse for both, ignoring ratio
					int dragWidth = Control.MousePosition.X - Location.X;
					int dragHeight = Control.MousePosition.Y - Location.Y;

					// What each would be if we had one follow the other
					int calcWidth = (int)(dragHeight * ratio);
					int calcHeight = (int)(dragWidth / ratio);

					// We want the smallest to follow the mouse, so that the other extends
					// beyond the mouse; otherwise the other would recede away from the mouse,
					// creating a weird detached-from-cursor effect
					if (calcWidth > dragWidth)
					{
						// Width is larger; drag Height
						Height = dragHeight;
						Width = calcWidth;
					}
					else
					{
						Width = dragWidth;
						Height = calcHeight;
					}
					break;
				 */
			}
		}

		protected void onMouseUp(object sender, MouseEventArgs ev)
		{
			if (ev.Button != MouseButtons.Left)
				return;

			dragMode = DragMode.None;
		}



		protected DragMode GetDragMode(MouseEventArgs ev, Size size)
		{
			int x = ev.X;
			int y = ev.Y;
			int w = Size.Width - WINDOW_GRIP_SIZE;
			int h = Size.Height - WINDOW_GRIP_SIZE;

			if (x <= WINDOW_GRIP_SIZE)
			{
				/*
				if (y <= WINDOW_GRIP_SIZE)
					return DragMode.TopLeft;

				if (y >= h)
					return DragMode.BottomLeft;
				*/

				return DragMode.Left;
			}

			if (x >= w)
			{
				/*
				if (y <= WINDOW_GRIP_SIZE)
					return DragMode.TopRight;

				if (y >= h)
					return DragMode.BottomRight;
				*/

				return DragMode.Right;
			}

			if (y <= WINDOW_GRIP_SIZE)
				return DragMode.Top;

			if (y >= h)
				return DragMode.Bottom;

			return DragMode.Move;
		}

		protected void onKeyDown(object sender, KeyEventArgs ev)
		{
			switch (ev.KeyCode)
			{
				case Keys.Left:
					rotateLeft();
					break;
				case Keys.Right:
					rotateRight();
					break;
			}
		}

		protected void rotateLeft()
		{
			this.BackgroundImage.RotateFlip(RotateFlipType.Rotate270FlipNone);

			int temp = documentWidth;
			documentWidth = documentHeight;
			documentHeight = temp;
			updateDimensions();

			ratio = 1 / ratio;
		}

		protected void rotateRight()
		{
			this.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

			int temp = documentWidth;
			documentWidth = documentHeight;
			documentHeight = temp;
			updateDimensions();

			ratio = 1 / ratio;
		}

		protected void updateDimensions()
		{
			if (documentWidth <= screen.Width && documentHeight <= screen.Height)
			{
				if (cropMode)
				{
					cropMode = false;
					BackgroundImage = image;
					paintMethod = paintNormal;
				}

				Width = documentWidth;
				Height = documentHeight;
				Left = documentLeft;
				Top = documentTop;
			}
			else
			{
				if (!cropMode)
				{
					cropMode = true;
					BackgroundImage = null;
					paintMethod = paintScaled;
				}

				int left = Left;
				int width = Width;
				int top = Top;
				int height = Height;

				calculateLeftAndWidth(documentLeft, documentWidth, screen.Width, image.Width, ref left, ref width);

				calculateLeftAndWidth(documentTop, documentHeight, screen.Height, image.Height, ref top, ref height);
	
				Left = left;
				Width = width;
				Top = top;
				Height = height;

				Invalidate();
			}
		}

		protected static void calculateLeftAndWidth(int documentLeft, int documentWidth, int screenWidth, int imageWidth, ref int left, ref int width)
		{
			if (documentWidth < screenWidth)
			{
				// Let the OS figure it out
				width = documentWidth;
				left = documentLeft;
			}
			else
			{
				// OS stopped cooperating, start faking it
				if (documentLeft > 0)
				{
					// The left edge of the image is not touching the left edge of the screen yet
					// Fill the remainder of the screen to the right
					left = documentLeft;
					width = screenWidth - documentLeft;
				}
				else if (documentLeft + documentWidth > screenWidth)
				{
					// The image fills the entire width of the screen
					// Fill the entire width of the screen
					width = screenWidth;
					left = 0;
				}
				else
				{
					// The right edge of the image is not touching the right edge of the screen yet
					// Fill the remainder of the screen to the left
					left = 0;
					width = documentLeft + documentWidth;
				}
			}
		}

		protected static void calculatePaintSrcLeftAndWidth(int documentLeft, int documentWidth, int screenWidth, int imageWidth, int width, ref int srcLeft, ref int srcWidth)
		{
			if (documentWidth < screenWidth)
			{
				// Let the OS figure it out
				srcWidth = imageWidth;
				srcLeft = 0;
			}
			else
			{
				// Draw it all fancy
				srcWidth = imageWidth * width / documentWidth;
				srcLeft = documentLeft < 0 ? imageWidth * -documentLeft / documentWidth : 0;
			}
		}


		protected void onPaint(object sender, PaintEventArgs ev)
		{
			paintMethod(ev);
		}

		protected void paintNormal(PaintEventArgs ev)
		{
		}

		protected void paintScaled(PaintEventArgs ev)
		{
			int srcLeft = 0;
			int srcWidth = 0;
			calculatePaintSrcLeftAndWidth(documentLeft, documentWidth, screen.Width, image.Width, Width, ref srcLeft, ref srcWidth);

			int srcTop = 0;
			int srcHeight = 0;
			calculatePaintSrcLeftAndWidth(documentTop, documentHeight, screen.Height, image.Height, Height, ref srcTop, ref srcHeight);

			Graphics g = ev.Graphics;

			var srcRect = new Rectangle(srcLeft, srcTop, srcWidth, srcHeight);
			var destRect = new Rectangle(0, 0, Width, Height);
			g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
		}

		/*
		// Never called
		private void ViewerWindow_ResizeBegin(object sender, EventArgs e)
		{

		}

		// Never called
		private void ViewerWindow_Scroll(object sender, ScrollEventArgs e)
		{

		}

		// Called multiple times at init, and anytime we resize the window in code
		private void ViewerWindow_Resize(object sender, EventArgs e)
		{

		}
		*/
	}
}
