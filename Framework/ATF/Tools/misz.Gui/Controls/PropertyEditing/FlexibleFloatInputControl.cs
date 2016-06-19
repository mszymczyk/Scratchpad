using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Controls;
using Sce.Atf;
using Sce.Atf.Dom;

namespace misz.Gui
{

    /// <summary>
    /// Control for editing a bounded float value with modal dialog to customize slider range</summary>
    public class FlexibleFloatInputControl : UserControl
    {
        /// <summary>
        /// Constructor with initial and bounding values</summary>
        /// <param name="value">Initial value</param>
        /// <param name="min">Minimum value, hard limit</param>
        /// <param name="max">Maximum value, hard limit</param>
        /// <param name="softMin">Minimum value, soft limit, can be modified via dialog</param>
        /// <param name="softMax">Maximum value, soft limit, can be modified via dialog</param>
        /// <param name="stepSize">Maximum value, delta for spinner control, can be modified via dialog</param>
        public FlexibleFloatInputControl( float value, float min, float max, bool checkBox )
        {
            if ( min >= max )
                throw new ArgumentException( "min must be less than max" );
            DoubleBuffered = true;
            m_min = min;
            m_max = max;
            m_softMin = m_min;
            m_softMax = m_max;
            m_stepSize = (m_softMax - m_softMin) * 0.01f;
            m_value = MathUtil.Clamp( value, m_softMin, m_softMax );
            m_value = MathUtil.Clamp( value, m_min, m_max );
            m_textBox = new NumericTextBox();
            m_textBox.BorderStyle = BorderStyle.None;
            m_textBox.Name = "m_textBox";

            m_button = new EditButton();
            m_button.Name = "m_button";
            m_button.Modal = true;
            m_button.Click += ( sender, e ) =>
            {
                FlexibleFloatEditorForm dialog = new FlexibleFloatEditorForm( m_min, m_max, m_softMin, m_softMax, m_stepSize, m_extraName );
                dialog.Text = "Customize - " + PropertyName;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ShowDialog();
                if ( dialog.DialogResult == DialogResult.OK )
                {
                    SetDesc( dialog.SoftMin, dialog.SoftMax, dialog.StepSize, dialog.ExtraName, false );
                }
            };

            m_spinner = new CompactSpinner();
            m_spinner.GotFocus += ( sender, e ) => m_textBox.Focus();

            if (checkBox)
            {
                m_checkBox = new CheckBox();
                m_checkBox.Checked = true;
                m_checkBox.Size = m_checkBox.PreferredSize;
                m_checkBox.CheckAlign = ContentAlignment.MiddleLeft;
                SetCheckBoxChecked( true );
                m_checkBox.CheckedChanged += ( sender, e ) =>
                {
                    SetCheckBoxChecked( m_checkBox.Checked );
                };
            }

            SuspendLayout();
            UpdateTextBox();
            Controls.Add( m_textBox );
            if ( checkBox )
            	Controls.Add( m_checkBox );
            Controls.Add( m_button );
            Controls.Add( m_spinner );
            ResumeLayout( false );
            PerformLayout();

            m_textBox.ValueEdited += ( sender, e ) =>
            {
                float val = (float) m_textBox.Value;
                SetValue( val, false );
            };

            m_spinner.Changed += ( sender, e ) =>
            {
                float newValue = Value + (float) e.Value * m_stepSize;
                SetValue( newValue, false );
            };

            m_textBox.SizeChanged += ( sender, e ) => Height = m_textBox.Height + 3;
            SizeChanged += ( sender, e ) =>
            {
                int x = 0;
                int textBoxOffset = 0;
                if ( m_checkBox != null )
                {
                    // + 2 - copied from BoolEditor, m_topAndLeftMargin
                    x += Height + 2;
                    textBoxOffset = 5; // 5 looks ok
                    m_checkBox.Bounds = new Rectangle( 2, 0, Height, Height );
                }
                m_spinner.Bounds = new Rectangle( x, 0, Height, Height );
                m_button.Bounds = new Rectangle( Width - Height, 0, Height, Height );
                m_textBox.Bounds = new Rectangle( m_spinner.Width + x + textBoxOffset, 0, Width - m_spinner.Width - Height - x - textBoxOffset, m_textBox.Height );
            };
        }

        /// <summary>
        /// Gets and sets the current value of the control</summary>
        public float Value
        {
            get { return m_value; }
            set
            {
                SetValue( value, false );
            }
        }

        /// <summary>
        /// Gets or sets the minimum value</summary>
        public float SoftMin
        {
            get { return m_softMin; }
            set
            {
                if ( value >= m_softMax )
                    throw new ArgumentException( "SoftMin" );
                m_softMin = MathUtil.Max<float>( value, m_min );
                if ( m_value < m_softMin )
                    Value = m_softMin;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value</summary>
        public float SoftMax
        {
            get { return m_softMax; }
            set
            {
                if ( value <= m_softMin )
                    throw new ArgumentException( "SoftMax" );
                m_softMax = MathUtil.Min<float>( value, m_max );
                if ( m_value > m_softMax )
                    Value = m_softMax;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the step size</summary>
        public float StepSize
        {
            get { return m_stepSize; }
            set
            {
                m_stepSize = MathUtil.Max<float>( value, 0.0001f );
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the step size</summary>
        public string ExtraName
        {
            get { return m_extraName; }
            set
            {
                m_extraName = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the step size</summary>
        public bool CheckBoxEnabled
        {
            get { return m_checkBoxEnabled; }
            set
            {
                SetCheckBoxChecked( value );
            }
        }

        private void SetCheckBoxChecked( bool checkedOrNot )
        {
            if (checkedOrNot != m_checkBoxEnabled)
            {
                m_checkBoxEnabled = checkedOrNot;

                bool enableControls = true;
                if (m_checkBox != null)
                {
                    if ( m_checkBox.Checked != m_checkBoxEnabled )
                        m_checkBox.Checked = m_checkBoxEnabled;

                    if (!m_checkBoxEnabled)
                    {
                        enableControls = false;
                        m_spinner.Enabled = false;
                        m_textBox.Enabled = false;
                        m_button.Enabled = false;
                    }
                }

                if (enableControls)
                {
                    m_spinner.Enabled = true;
                    m_textBox.Enabled = true;
                    m_button.Enabled = true;
                }

                OnValueChanged( EventArgs.Empty );
            }

            this.Invalidate();
        }

        public string PropertyName
        {
            get { return m_propertyName; }
            set { m_propertyName = value; }
        }

        /// <summary>
        /// Gets and sets whether to draw a border around the control</summary>        
        public bool DrawBorder
        {
            get { return m_drawBorder; }
            set { m_drawBorder = value; }
        }

        /// <summary>
        /// Event that is raised after value is changed</summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Event that is raised after control description/metadata is changed</summary>
        public event EventHandler DescChanged;

        /// <summary>
        /// Translates from normalized trackbar position to value</summary>
        /// <param name="position">The normalized trackbar position, in [0..1]</param>
        /// <returns>The value (should be between SoftMin and SoftMax)</returns>
        /// <remarks>Override this method to change the mapping between position and
        /// value, for example, for a logarithmic slider.</remarks>
        protected virtual float GetValue( float position )
        {
            return Constrain( m_softMin + position * ( m_softMax - m_softMin ) );
        }

        /// <summary>
        /// Translates from value to normalized position</summary>
        /// <param name="value">The value, in [SoftMin..SoftMax]</param>
        /// <returns>The normalized trackbar position (should be in [0..1])</returns>
        /// <remarks>Override this method to change the mapping between position and
        /// value, for example, for a logarithmic slider. This should be the inverse
        /// of GetValue().</remarks>
        protected virtual float GetPosition( float value )
        {
            value = Constrain( value );
            return ( value - m_softMin ) / ( m_softMax - m_softMin );
        }


        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.IntInputControl.ValueChanged"/> event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected virtual void OnValueChanged( EventArgs e )
        {
            ValueChanged.Raise( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.IntInputControl.ValueChanged"/> event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected virtual void OnDescChanged( EventArgs e )
        {
            DescChanged.Raise( this, e );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Windows.Forms.Control.Paint"></see> event and performs custom processing</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        protected override void OnPaint( PaintEventArgs e )
        {
            float r = (float) ( m_value - m_softMin ) / (float) ( m_softMax - m_softMin );
            int w = (int) ( r * m_textBox.Width );
            Rectangle rec
                = new Rectangle( m_textBox.Location.X, m_textBox.Height, w, 3 );
            e.Graphics.FillRectangle( Brushes.LightBlue, rec );
            if ( m_drawBorder )
                ControlPaint.DrawBorder3D( e.Graphics, ClientRectangle, Border3DStyle.Flat );
        }

        private void SetValue( float value, bool forceNewValue )
        {
            value = Sce.Atf.MathUtil.Clamp( value, m_softMin, m_softMax );
            if ( forceNewValue ||
                value != m_value )
            {
                m_value = value;
                OnValueChanged( EventArgs.Empty );
            }

            // Update the user interface to make sure the displayed text is in sync.
            //  If these two methods are in the above 'if', then typing in the same
            //  out-of-range value twice in a row persists, indicating that m_textBox.Text
            //  is out of sync with m_value.
            UpdateTextBox();
            this.Invalidate();
        }

        private void SetDesc( float softMin, float softMax, float stepSize, string extraName, bool force )
        {
            if ( force
                || softMin != m_softMin
                || softMax != m_softMax
                || stepSize != m_stepSize
                || extraName != m_extraName
                )
            {
                m_softMin = softMin;
                m_softMax = softMax;
                m_stepSize = stepSize;
                m_extraName = extraName;
                OnDescChanged( EventArgs.Empty );
            }

            this.Invalidate();
        }

        private void UpdateTextBox()
        {
            m_textBox.Value = m_value;
        }

        private float Constrain( float value )
        {
            return MathUtil.Clamp( value, m_softMin, m_softMax );
        }

        private float m_value;
        private float m_min;
        private float m_max;
        private float m_softMin;
        private float m_softMax;
        private float m_stepSize;
        private string m_extraName = "";
        private bool m_checkBoxEnabled = true; // used only when m_checkBox is present
        private string m_propertyName;

        private bool m_drawBorder = true;

        private CompactSpinner m_spinner;
        private NumericTextBox m_textBox;
        private EditButton m_button;
        private CheckBox m_checkBox;
    }
}
