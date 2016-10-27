using System.Drawing;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using System;
using Sce.Atf;
using pico;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.VectorMath;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts DomNode to a DynamicProperty</summary>
    public class DynamicProperty : DomNodeAdapter
    {
        /// <summary>
        /// Creates parameter based on given parameter module
        /// </summary>
        public static DynamicProperty CreateFromSetting( Group group, Setting sett, DynamicProperty dpOld = null )
        {
            DomNode node = new DomNode( Schema.dynamicPropertyType.Type, Schema.groupType.propChild );
            DynamicProperty p = node.As<DynamicProperty>();
            p.Name = sett.Name;
            p.m_setting = sett;

            if ( dpOld != null )
            {
                p.ExtraName = dpOld.ExtraName;
            }

            if ( sett is BoolSetting )
            {
                BoolSetting bsett = (BoolSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.bvalAttribute.Name;
                p.BoolValue = bsett.Value;

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.BoolValue = dpOld.BoolValue;
                }
            }
            else if ( sett is IntSetting )
            {
                IntSetting isett = (IntSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.ivalAttribute.Name;
                p.IntValue = isett.Value;

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.IntValue = dpOld.IntValue;
                }
            }
            else if ( sett is EnumSetting )
            {
                EnumSetting esett = (EnumSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.evalAttribute.Name;
                p.EnumValue = (int)esett.Value;

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.IntValue = MathUtil.Clamp<int>( dpOld.IntValue, 0, esett.NumEnumValues - 1 );
                }
            }
            else if ( sett is FloatSetting )
            {
                FloatSetting fsett = (FloatSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.fvalAttribute.Name;
                p.FloatValue = fsett.Value;
                p.SoftMinValue = fsett.SoftMinValue;
                p.SoftMaxValue = fsett.SoftMaxValue;
                p.StepValue = fsett.StepSize;
                p.Checked = fsett.ValueChecked;

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.FloatValue = MathUtil.Clamp<float>( dpOld.FloatValue, fsett.MinValue, fsett.MaxValue );
                    // if old SoftMinValue/SoftMaxValues/Step equal fsett values, then assume
                    // that user didn't change default values of these properties via three-dot-dialog
                    // and set new DynamicProperty values to fsett values
                    // this aids creating/editing new settings because "soft" values can be live-edited
                    // old behavior prevented this
                    if ( picoMath.EqualApprox( dpOld.SoftMinValue, dpOld.InitialSoftMinValue, 0.0001f ) )
                    {
                        p.SoftMinValue = fsett.SoftMinValue;
                        p.InitialSoftMinValue = fsett.SoftMinValue;
                    }
                    else
                    {
                        p.SoftMinValue = MathUtil.Clamp<float>( dpOld.SoftMinValue, fsett.MinValue, fsett.MaxValue );
                        p.InitialSoftMinValue = dpOld.InitialSoftMinValue;
                    }

                    if ( picoMath.EqualApprox( dpOld.SoftMaxValue, dpOld.InitialSoftMaxValue, 0.0001f ) )
                    {
                        p.SoftMaxValue = fsett.SoftMaxValue;
                        p.InitialSoftMaxValue = fsett.SoftMaxValue;
                    }
                    else
                    {
                        p.SoftMaxValue = MathUtil.Clamp<float>( dpOld.SoftMaxValue, fsett.MinValue, fsett.MaxValue ); ;
                        p.InitialSoftMaxValue = dpOld.InitialSoftMaxValue;
                    }

                    if ( picoMath.EqualApprox( dpOld.StepValue, fsett.StepSize, 0.0001f ) )
                    {
                        p.StepValue = fsett.StepSize;
                        p.InitialStepValue = fsett.StepSize;
                    }
                    else
                    {
                        p.StepValue = dpOld.StepValue;
                        p.InitialStepValue = dpOld.StepValue;
                    }

                    p.Checked = dpOld.Checked;
                }
                else
                {
                    p.InitialSoftMinValue = p.SoftMinValue;
                    p.InitialSoftMaxValue = p.SoftMaxValue;
                    p.InitialStepValue = p.StepValue;
                }
            }
            else if ( sett is Float4Setting )
            {
                Float4Setting f4sett = (Float4Setting)sett;

                p.ValueType = Schema.dynamicPropertyType.f4valAttribute.Name;
                p.Float4Value = new Float4( f4sett.Value );

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.Float4Value = new Float4( dpOld.Float4Value );
                }
            }
            else if ( sett is ColorSetting )
            {
                ColorSetting csett = (ColorSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.colvalAttribute.Name;
                p.ColorValue = new Color( csett.Value );

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.ColorValue = new Color( dpOld.ColorValue );
                }
            }
            else if ( sett is StringSetting )
            {
                StringSetting ssett = (StringSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.svalAttribute.Name;
                p.StringValue = ssett.Value ?? "";

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.StringValue = dpOld.StringValue;
                }
            }
            else if ( sett is DirectionSetting )
            {
                DirectionSetting dsett = (DirectionSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.dirvalAttribute.Name;
                p.DirectionValue = new Direction( dsett.Value );

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.DirectionValue = new Direction( dpOld.DirectionValue );
                }
            }
            else if ( sett is AnimCurveSetting )
            {
                AnimCurveSetting ssett = (AnimCurveSetting)sett;

                p.ValueType = Schema.dynamicPropertyType.curveChild.Name;

                if ( dpOld != null && dpOld.ValueType == p.ValueType )
                {
                    p.AnimCurveValue = dpOld.AnimCurveValue;
                }
                else
                {
                    p.AnimCurveValue = CreateSampleCurve( p.Name );
                }
            }

            p.DomNode.InitializeExtensions();
            group.Properties.Add( p );

            return p;
        }

        private static Curve CreateSampleCurve( string Name )
        {
            // add x channel
            Curve curve = (new DomNode( Schema.curveType.Type )).As<Curve>();
            curve.Name = Name;
            curve.DisplayName = "Curve:" + Name;
            curve.MinX = 0;
            curve.MaxX = 1.0f;
            curve.MinY = 0.0f;
            curve.MaxY = 1.0f;
            curve.CurveColor = System.Drawing.Color.Red;
            curve.PreInfinity = CurveLoopTypes.Constant;
            curve.PostInfinity = CurveLoopTypes.Constant;
            curve.XLabel = "Time";
            curve.YLabel = "Value";

            Vec2F pStart = new Vec2F( 0, 0 );
            Vec2F pEnd = new Vec2F( curve.MaxX, 1 );

            Vec2F p1 = Vec2F.Lerp( pStart, pEnd, 0.33f );
            Vec2F p2 = Vec2F.Lerp( pStart, pEnd, 0.66f );

            IControlPoint cp = curve.CreateControlPoint();
            cp.X = pStart.X;
            cp.Y = pStart.Y;
            curve.AddControlPoint( cp );

            cp = curve.CreateControlPoint();
            cp.X = p1.X;
            cp.Y = p1.Y;
            curve.AddControlPoint( cp );

            cp = curve.CreateControlPoint();
            cp.X = p2.X;
            cp.Y = p2.Y;
            curve.AddControlPoint( cp );

            cp = curve.CreateControlPoint();
            cp.X = pEnd.X;
            cp.Y = pEnd.Y;
            curve.AddControlPoint( cp );

            CurveUtils.ComputeTangent( curve );

            return curve;
        }

        public string Name
        {
            get { return (string)DomNode.GetAttribute( Schema.dynamicPropertyType.nameAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.nameAttribute, value ); }
        }

        public string DisplayName
        {
            get { return Setting.DisplayName; }
        }

        public string ValueType
        {
            get { return (string)DomNode.GetAttribute( Schema.dynamicPropertyType.typeAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.typeAttribute, value ); }
        }

        public SettingType PropertyType
        {
            get { return Setting.Type; }
        }

        public bool BoolValue
        {
            get { return (bool)DomNode.GetAttribute( Schema.dynamicPropertyType.bvalAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.bvalAttribute, value ); }
        }

        public int IntValue
        {
            get { return (int)DomNode.GetAttribute( Schema.dynamicPropertyType.ivalAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.ivalAttribute, value ); }
        }

        public int EnumValue
        {
            get { return (int)DomNode.GetAttribute( Schema.dynamicPropertyType.evalAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.evalAttribute, value ); }
        }

        public float FloatValue
        {
            get { return (float)DomNode.GetAttribute( Schema.dynamicPropertyType.fvalAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.fvalAttribute, value ); }
        }

        public Float4 Float4Value
        {
            get { return new Float4( (float[])DomNode.GetAttribute( Schema.dynamicPropertyType.f4valAttribute ) ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.f4valAttribute, new float[4] { value.X, value.Y, value.Z, value.W } ); }
        }

        public Color ColorValue
        {
            get { return new Color( (float[])DomNode.GetAttribute( Schema.dynamicPropertyType.colvalAttribute ) ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.colvalAttribute, new float[3] { value.R, value.G, value.B } ); }
        }

        public string StringValue
        {
            get { return (string)DomNode.GetAttribute( Schema.dynamicPropertyType.svalAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.svalAttribute, value ); }
        }

        public Direction DirectionValue
        {
            get { return new Direction( (float[])DomNode.GetAttribute( Schema.dynamicPropertyType.dirvalAttribute ) ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.dirvalAttribute, new float[3] { value.X, value.Y, value.Z } ); }
        }

        public Curve AnimCurveValue
        {
            get { return DomNode.GetChild( Schema.dynamicPropertyType.curveChild ).Cast<Curve>(); }
            set { DomNode.SetChild( Schema.dynamicPropertyType.curveChild, value.DomNode ); }
        }

        public float SoftMinValue
        {
            get { return (float)DomNode.GetAttribute( Schema.dynamicPropertyType.minAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.minAttribute, value ); }
        }

        public float SoftMaxValue
        {
            get { return (float)DomNode.GetAttribute( Schema.dynamicPropertyType.maxAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.maxAttribute, value ); }
        }

        public float StepValue
        {
            get { return (float)DomNode.GetAttribute( Schema.dynamicPropertyType.stepAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.stepAttribute, value ); }
        }

        public string ExtraName
        {
            get { return (string)DomNode.GetAttribute( Schema.dynamicPropertyType.extraNameAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.extraNameAttribute, value ); }
        }

        public bool Checked
        {
            get { return (bool)DomNode.GetAttribute( Schema.dynamicPropertyType.checkedAttribute ); }
            set { DomNode.SetAttribute( Schema.dynamicPropertyType.checkedAttribute, value ); }
        }

        public float InitialSoftMinValue { get; set; }
        public float InitialSoftMaxValue { get; set; }
        public float InitialStepValue { get; set; }

        public Setting Setting
        {
            get
            {
                if ( m_setting == null )
                {
                    Preset preset = DomNode.Parent.As<Preset>();
                    if ( preset != null )
                    {
                        m_setting = preset.Group.SettingGroup.FindSetting( Name );
                    }
                    else
                    {
                        Group group = DomNode.Parent.Cast<Group>();
                        m_setting = group.SettingGroup.FindSetting( Name );
                    }
                }

                return m_setting;
            }
        }

        private Setting m_setting;
    }
}
