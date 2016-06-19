using System;
using System.Windows.Forms;
using Sce.Atf;

namespace misz.Gui
{
    public partial class FlexibleFloatEditorForm : Form
    {
        public FlexibleFloatEditorForm( float min, float max, float softMin, float softMax, float step, string extraName )
        {
            InitializeComponent();

            m_min = min;
            m_max = max;
            m_softMin = softMin;
            m_softMax = softMax;
            m_step = step;
            m_extraName = extraName;

            textBoxMin.Value = min;
            textBoxMax.Value = max;
            textBoxSoftMin.Value = softMin;
            textBoxSoftMax.Value = softMax;
            textBoxStep.Value = step;
            textBoxName.Text = extraName;

            textBoxSoftMin.ValueEdited += textBoxSoftMin_ValueEdited;
            textBoxSoftMax.ValueEdited += textBoxSoftMax_ValueEdited;
            textBoxStep.ValueEdited += textBoxStep_ValueEdited;
            textBoxName.TextChanged += textBoxName_TextChanged;
        }

        void textBoxSoftMin_ValueEdited( object sender, EventArgs e )
        {
            float newMin = (float) textBoxSoftMin.Value;
            if ( newMin < Min || newMin > Max )
            {
                newMin = MathUtil.Clamp<float>( newMin, Min, Max );
                textBoxSoftMin.Value = newMin;
            }
            if ( newMin > SoftMax )
            {
                newMin = SoftMax;
                textBoxSoftMin.Value = newMin;
            }

            m_softMin = newMin;
        }

        void textBoxSoftMax_ValueEdited( object sender, EventArgs e )
        {
            float newMax = (float) textBoxSoftMax.Value;
            if ( newMax < Min || newMax > Max )
            {
                newMax = MathUtil.Clamp<float>( newMax, Min, Max );
                textBoxSoftMax.Value = newMax;
            }
            if ( newMax < SoftMin )
            {
                newMax = SoftMin;
                textBoxSoftMax.Value = newMax;
            }

            m_softMax = newMax;
        }

        void textBoxStep_ValueEdited( object sender, EventArgs e )
        {
            float newStep = (float) textBoxStep.Value;
            if ( newStep <= 0 )
            {
                newStep = 0.0001f;
                textBoxStep.Value = newStep;
            }

            m_step = newStep;
        }

        private void textBoxName_TextChanged( object sender, EventArgs e )
        {
            m_extraName = textBoxName.Text;
        }

        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            if ( keyData == Keys.Escape )
            {
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
                return true;
            }
            return base.ProcessCmdKey( ref msg, keyData );
        }

        public float Min
        {
            get { return m_min; }
        }

        public float Max
        {
            get { return m_max; }
        }

        public float SoftMin
        {
            get { return m_softMin; }
        }

        public float SoftMax
        {
            get { return m_softMax; }
        }

        public float StepSize
        {
            get { return m_step; }
        }

        public string ExtraName
        {
            get { return m_extraName; }
        }

        private float m_min;
        private float m_max;
        private float m_softMin;
        private float m_softMax;
        private float m_step;
        private string m_extraName;
    }
}
