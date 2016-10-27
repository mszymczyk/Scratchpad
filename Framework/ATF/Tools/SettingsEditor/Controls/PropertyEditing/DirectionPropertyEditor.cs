using System;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Adaptation;
using System.Drawing;
using Sce.Atf.Controls;
using Sce.Atf.VectorMath;

namespace SettingsEditor
{
    /// <summary>
    /// Property editor that supplies DirectionControl instances to embed in complex
    /// property editing controls. These display a NumericTupleControl and D3D pane for controlling direction in the GUI.
    /// </summary>
    public class DirectionPropertyEditor : IPropertyEditor
    {
        public DirectionPropertyEditor( D3DManipulator d3dManipulator )
        {
            m_d3dManipulator = d3dManipulator;
        }

        #region IPropertyEditor Members

        /// <summary>
        /// Obtains a control to edit a given property. Changes to the selection set
        /// cause this method to be called again (and passed a new 'context'),
        /// unless ICacheablePropertyControl is implemented on the control. For
        /// performance reasons, it is highly recommended that the control implement
        /// the ICacheablePropertyControl interface.</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public virtual Control GetEditingControl( PropertyEditorControlContext context )
        {
            var control = new DirectionControl( context, m_d3dManipulator );
            SkinService.ApplyActiveSkin( control );
            return control;
        }

        #endregion

        /// <summary>
        /// Control for editing a bounded float</summary>
        protected class DirectionControl : UserControl, ICacheablePropertyControl
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="context">Context for property editing control</param>
            /// <param name="min">Minimum value</param>
            /// <param name="max">Maximum value</param>
            public DirectionControl( PropertyEditorControlContext context, D3DManipulator d3dManipulator )
            {
                m_d3dManipulator = d3dManipulator;
                m_context = context;

                m_numericXYZ = new NumericTupleControl();
                m_numericXYZ.ValueChanged += numericXYZ_ValueChanged;
                m_editButton = new misz.Gui.EditButton();
                m_editButton.Modal = true;

                base.SuspendLayout();

                m_editButton.Left = base.Right - 18;
                m_editButton.Size = new Size( 18, 18 );
                m_editButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                m_editButton.Visible = true;

                m_editButton.Click += editButton_Click;

                Controls.Add( m_numericXYZ );
                Controls.Add( m_editButton );

                base.ResumeLayout();

                m_numericXYZ.SizeChanged += ( sender, e ) => Height = m_numericXYZ.Height + 3;
                SizeChanged += ( sender, e ) =>
                {
                    int x = 0;
                    int textBoxOffset = 0;
                    m_editButton.Bounds = new Rectangle( Width - Height, 0, Height, Height );
                    m_numericXYZ.Bounds = new Rectangle( x + textBoxOffset, 0, Width - Height - x - textBoxOffset, m_numericXYZ.Height );
                };

                DoubleBuffered = true;
                RefreshValue();
            }

            private void numericXYZ_ValueChanged( object sender, EventArgs e )
            {
                float[] xyz = (float[])m_numericXYZ.Value;
                SetValue( xyz[0], xyz[1], xyz[2], false );
            }

            private void editButton_Click( object sender, EventArgs e )
            {
                Point location = m_editButton.PointToScreen( new Point(m_editButton.Size.Width, m_editButton.Size.Height ) );
                DirectionForm form = new DirectionForm( m_d3dManipulator, this, location );
                form.Show();
            }

            #region ICacheablePropertyControl

            /// <summary>
            /// Gets true iff this control can be used indefinitely, regardless of whether the associated
            /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
            /// This property must be constant for the life of this control.</summary>
            public virtual bool Cacheable
            {
                get { return true; }
            }

            #endregion

            public PropertyEditorControlContext Context
            {
                get { return m_context; }
            }

            /// <summary>
            /// Gets and sets the current value of the control</summary>
            public float[] Value
            {
                get { return new float[3] { m_value[0], m_value[1], m_value[2] }; }
                set
                {
                    SetValue( value[0], value[1], value[2], false );
                }
            }

            /// <summary>
            /// Raises the ValueChanged event</summary>
            /// <param name="e">Event args</param>
            protected void OnValueChanged( EventArgs e )
            {
                if ( !m_refreshing )
                {
                    m_context.SetValue( Value );
                }
            }

            /// <summary>
            /// Refreshes the control</summary>
            public override void Refresh()
            {
                RefreshValue();

                base.Refresh();
            }

            private void RefreshValue()
            {
                try
                {
                    m_refreshing = true;

                    object value = m_context.GetValue();
                    if ( value == null )
                        Enabled = false;
                    else
                    {
                        float[] fvalue = (float[])value;
                        Value = new float[3] { fvalue[0], fvalue[1], fvalue[2] };
                        Enabled = !m_context.IsReadOnly;
                    }
                }
                finally
                {
                    m_refreshing = false;
                }
            }


            private void SetValue( float x, float y, float z, bool forceNewValue )
            {
                Vec3F vec = new Vec3F( x, y, z );
                Vec3F curVec = new Vec3F( m_value[0], m_value[1], m_value[2] );
                vec.Normalize();
                if ( forceNewValue || !vec.Equals(curVec, 0.001f) )
                {
                    m_value[0] = vec.X;
                    m_value[1] = vec.Y;
                    m_value[2] = vec.Z;
                    OnValueChanged( EventArgs.Empty );
                }

                UpdateNumericTupleControl();
                //this.Invalidate(); // unnecessary
            }

            private void UpdateNumericTupleControl()
            {
                m_numericXYZ.Value = m_value;
                m_d3dManipulator.Direction = new Vec3F( m_value[0], m_value[1], m_value[2] );
            }

            D3DManipulator m_d3dManipulator;
            private readonly PropertyEditorControlContext m_context;
            private bool m_refreshing;
            private float[] m_value = new float[3];
            NumericTupleControl m_numericXYZ;
            misz.Gui.EditButton m_editButton;
        }

        protected class DirectionForm : Form
        {
            public DirectionForm( D3DManipulator d3dManipulator, DirectionControl directionControl, Point editButtonScreenRightBottomLocation )
            {
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                this.ControlBox = false;
                this.Size = new Size( 160, 160 );
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                Point p = editButtonScreenRightBottomLocation;
                p.X -= this.Size.Width;
                this.Location = p;
                this.KeyPreview = true; // for undo/redo

                m_directionControl = directionControl;
                m_d3dManipulator = d3dManipulator;
                m_d3dManipulator.DirectionChanged += d3dManipulator_DirectionChanged;

                Controls.Add( m_d3dManipulator.Control );
            }

            private void d3dManipulator_DirectionChanged( object sender, EventArgs e )
            {
                Vec3F dir = m_d3dManipulator.Direction;
                m_directionControl.Value = new float[3] { dir.X, dir.Y, dir.Z };
            }

            protected override void OnLeave( EventArgs e )
            {
                if ( m_d3dManipulator != null )
                {
                    m_d3dManipulator.DirectionChanged -= d3dManipulator_DirectionChanged;

                    Controls.Remove( m_d3dManipulator.Control );
                    base.OnLeave( e );
                    Close();
                }
            }

            protected override void OnDeactivate( EventArgs e )
            {
                if ( m_d3dManipulator != null )
                {
                    m_d3dManipulator.DirectionChanged -= d3dManipulator_DirectionChanged;

                    Controls.Remove( m_d3dManipulator.Control );
                    base.OnDeactivate( e );
                    Close();
                }
            }

            protected override void OnKeyUp( KeyEventArgs e )
            {
                base.OnKeyUp( e );

                if ( e.Control && (e.KeyCode == Keys.Z || e.KeyCode == Keys.Y) )
                {
                    // undo
                    IHistoryContext hc = m_directionControl.Context.TransactionContext.As<IHistoryContext>();
                    if ( hc != null && e.KeyCode == Keys.Z && hc.CanUndo )
                        hc.Undo();
                    else if ( hc != null && e.KeyCode == Keys.Y && hc.CanRedo )
                        hc.Redo();
                }
            }

            D3DManipulator m_d3dManipulator;
            DirectionControl m_directionControl;
        };

        private D3DManipulator m_d3dManipulator;
    }
}
