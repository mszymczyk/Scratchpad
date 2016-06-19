using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace misz.Gui
{
    /// <summary>
    /// Edit button used to add "three dot button" on the right side of property to customize control or edit value.
    /// Copied from Sce.Atf.Controls.PropertyEditing.PropertyEditingControl
    /// </summary>
    class EditButton : Button
    {
        public EditButton()
        {
            base.SetStyle( ControlStyles.Selectable, true );
            base.BackColor = SystemColors.Control;
            base.ForeColor = SystemColors.ControlText;
            base.TabStop = false;
            base.IsDefault = false;
        }

        /// <summary>
        /// Gets or sets whether the button is for a modal dialog or a drop down control</summary>
        public bool Modal
        {
            get { return m_modal; }
            set
            {
                m_modal = value;
                Invalidate();
            }
        }

        protected override void OnMouseDown( MouseEventArgs arg )
        {
            base.OnMouseDown( arg );
            if (arg.Button == MouseButtons.Left)
            {
                m_pushed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp( MouseEventArgs arg )
        {
            base.OnMouseUp( arg );
            if (arg.Button == MouseButtons.Left)
            {
                m_pushed = false;
                Invalidate();
            }
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            Graphics g = e.Graphics;
            Rectangle r = ClientRectangle;

            if (m_modal)
            {
                base.OnPaint( e );
                // draws "..."
                int x = r.X + r.Width / 2 - 5;
                int y = r.Bottom - 5;
                using (Brush brush = new SolidBrush( Enabled ? SystemColors.ControlText : SystemColors.GrayText ))
                {
                    g.FillRectangle( brush, x, y, 2, 2 );
                    g.FillRectangle( brush, x + 4, y, 2, 2 );
                    g.FillRectangle( brush, x + 8, y, 2, 2 );
                }
            }
            else
            {
                if (Application.RenderWithVisualStyles)
                {
                    ComboBoxRenderer.DrawDropDownButton(
                        g,
                        ClientRectangle,
                        !Enabled ? ComboBoxState.Disabled : (m_pushed ? ComboBoxState.Pressed : ComboBoxState.Normal) );
                }
                else
                {
                    ControlPaint.DrawButton(
                        g,
                        ClientRectangle,
                        !Enabled ? ButtonState.Inactive : (m_pushed ? ButtonState.Pushed : ButtonState.Normal) );
                }
            }
        }

        private bool m_modal;
        private bool m_pushed;
    }
}
