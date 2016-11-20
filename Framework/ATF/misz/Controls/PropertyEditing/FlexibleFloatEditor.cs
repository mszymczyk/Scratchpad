using System;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace misz.Controls.PropertyEditing
{
    public class FlexibleFloatValue
    {
        public float value = 0;
        public float softMin = 0;
        public float softMax = 0;
        public float step = 0;
        public string extraName = null;
        public bool check = false;
    }

    /// <summary>
    /// Flexible bounded floats property editor that supplies FlexibleFloatControl instances to embed in complex
    /// property editing controls. These display a slider, textbox and customize button in the GUI.
    /// Based on Sce.Atf.Controls.PropertyEditing.BoundedFloatEditor
    /// </summary>
    public class FlexibleFloatEditor : IPropertyEditor
    {
        public FlexibleFloatEditor( float min, float max, bool checkBox
            //, AttributeInfo softMinAttribute, AttributeInfo softMaxAttribute, AttributeInfo stepAttribute, AttributeInfo extraNameAttribute, AttributeInfo checkedAttribute
            )
        {
            if ( min >= max )
                throw new ArgumentOutOfRangeException( "min must be less than max" );
            m_min = min;
            m_max = max;
            m_hasCheckBox = checkBox;
            //m_softMinAttribute = softMinAttribute;
            //m_softMaxAttribute = softMaxAttribute;
            //m_stepAttribute = stepAttribute;
            //m_extraNameAttribute = extraNameAttribute;
            //m_checkedAttribute = checkedAttribute;
        }

        /// <summary>
        /// Gets the editor's minimum value</summary>
        public float Min
        {
            get { return m_min; }
        }

        /// <summary>
        /// Gets the editor's maximum value</summary>
        public float Max
        {
            get { return m_max; }
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
            var control = new FlexibleFloatControl( context, m_min, m_max, m_hasCheckBox
                //, m_softMinAttribute, m_softMaxAttribute, m_stepAttribute, m_extraNameAttribute, m_checkedAttribute
                );
            SkinService.ApplyActiveSkin( control );
            return control;
        }

        #endregion

        /// <summary>
        /// Control for editing a bounded float</summary>
        protected class FlexibleFloatControl : FlexibleFloatInputControl, ICacheablePropertyControl
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="context">Context for property editing control</param>
            /// <param name="min">Minimum value</param>
            /// <param name="max">Maximum value</param>
            public FlexibleFloatControl( PropertyEditorControlContext context, float min, float max, bool checkBox
                //, AttributeInfo softMinAttribute, AttributeInfo softMaxAttribute, AttributeInfo stepAttribute, AttributeInfo extraNameAttribute, AttributeInfo checkedAttribute
                )
                : base( min, min, max, checkBox )
            {
                m_context = context;
                //m_softMinAttribute = softMinAttribute;
                //m_softMaxAttribute = softMaxAttribute;
                //m_stepAttribute = stepAttribute;
                //m_extraNameAttribute = extraNameAttribute;
                //m_checkedAttribute = checkedAttribute;
                PropertyName = context.Descriptor.DisplayName;
                DrawBorder = false;
                DoubleBuffered = true;
                RefreshValue();
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

            /// <summary>
            /// Raises the ValueChanged event</summary>
            /// <param name="e">Event args</param>
            protected override void OnValueChanged( EventArgs e )
            {
                if ( !m_refreshing )
                {
                    //m_context.SetValue( Value );
                    SetFlexibleFloat();
                }

                base.OnValueChanged( e );
            }

            /// <summary>
            /// Raises the ValueChanged event</summary>
            /// <param name="e">Event args</param>
            protected override void OnDescChanged( EventArgs e )
            {
                if ( !m_refreshing )
                {
                    //m_context.TransactionContext.DoTransaction( delegate
                    //{
                    //    AttributePropertyDescriptor apd = m_context.Descriptor.Cast<AttributePropertyDescriptor>();

                    //    foreach ( object selectedObject in m_context.SelectedObjects )
                    //    {
                    //        DomNode domNode = apd.GetNode( selectedObject );
                    //        domNode.SetAttribute( m_softMinAttribute, SoftMin );
                    //        domNode.SetAttribute( m_softMaxAttribute, SoftMax );
                    //        domNode.SetAttribute( m_stepAttribute, StepSize );
                    //        domNode.SetAttribute( m_extraNameAttribute, ExtraName );
                    //        domNode.SetAttribute( m_checkedAttribute, CheckBoxEnabled );
                    //    }
                    //}, string.Format( "Edit Desc: {0}".Localize(), m_context.Descriptor.DisplayName ) );

                    SetFlexibleFloat();
                }

                base.OnDescChanged( e );
            }

            private void SetFlexibleFloat()
            {
                FlexibleFloatValue v = new FlexibleFloatValue();
                v.value = Value;
                v.softMin = SoftMin;
                v.softMax = SoftMax;
                v.step = StepSize;
                v.extraName = ExtraName;
                v.check = CheckBoxEnabled;
                m_context.SetValue( v );
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
                        //AttributePropertyDescriptor apd = m_context.Descriptor.Cast<AttributePropertyDescriptor>();
                        //DomNode domNode = apd.GetNode( m_context.LastSelectedObject );

                        //float softMin = (float)domNode.GetAttribute( m_softMinAttribute );
                        //float softMax = (float)domNode.GetAttribute( m_softMaxAttribute );
                        //float step = (float)domNode.GetAttribute( m_stepAttribute );
                        //string extraName = (string)domNode.GetAttribute( m_extraNameAttribute );
                        //bool checkBoxChecked = (bool)domNode.GetAttribute( m_checkedAttribute );

                        //Value = (float)value;

                        //SoftMin = softMin;
                        //SoftMax = softMax;
                        //StepSize = step;
                        //ExtraName = extraName;
                        //CheckBoxEnabled = checkBoxChecked;

                        FlexibleFloatValue v = value as FlexibleFloatValue;
                        Value = v.value;
                        SoftMin = v.softMin;
                        SoftMax = v.softMax;
                        StepSize = v.step;
                        ExtraName = v.extraName;
                        CheckBoxEnabled = v.check;

                        Enabled = !m_context.IsReadOnly;
                    }
                }
                finally
                {
                    m_refreshing = false;
                }
            }

            private readonly PropertyEditorControlContext m_context;
            private bool m_refreshing;
            //private AttributeInfo m_softMinAttribute;
            //private AttributeInfo m_softMaxAttribute;
            //private AttributeInfo m_stepAttribute;
            //private AttributeInfo m_extraNameAttribute;
            //private AttributeInfo m_checkedAttribute;
        }

        private float m_min;
        private float m_max = 100.0f;
        private bool m_hasCheckBox = false;
        //private AttributeInfo m_softMinAttribute;
        //private AttributeInfo m_softMaxAttribute;
        //private AttributeInfo m_stepAttribute;
        //private AttributeInfo m_extraNameAttribute;
        //private AttributeInfo m_checkedAttribute;
    }
}
