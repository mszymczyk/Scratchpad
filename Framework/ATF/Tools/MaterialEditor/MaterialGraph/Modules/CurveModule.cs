using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to a control point</summary>
    public class ControlPoint : DomNodeAdapter, IControlPoint
    {
        #region IControlPoint Members

        /// <summary>
        /// Gets parent curve for this control point</summary>
        public ICurve Parent
        {
            get { return GetParentAs<ICurve>(); }
        }

        /// <summary>
        /// Gets or sets x-coordinate</summary>
        public float X
        {
            get { return GetAttribute<float>( Schema.controlPointType.xAttribute ); }
            set
            {
                SetAttribute( Schema.controlPointType.xAttribute, value );
            }
        }

        /// <summary>
        /// Gets or sets y-coordinate</summary>
        public float Y
        {
            get { return GetAttribute<float>( Schema.controlPointType.yAttribute ); }
            set
            {
                SetAttribute( Schema.controlPointType.yAttribute, value );
            }

        }

        /// <summary>
        /// Gets or sets "tangent in" value</summary>
        public Vec2F TangentIn
        {
            get
            {
                float[] tan = GetAttribute<float[]>( Schema.controlPointType.tangentInAttribute );
                return new Vec2F( tan );
            }
            set
            {
                float[] tan = new float[] { value.X, value.Y };
                SetAttribute( Schema.controlPointType.tangentInAttribute, tan );
            }
        }

        /// <summary>
        /// Gets or sets "tangent in" type</summary>
        public CurveTangentTypes TangentInType
        {
            get
            {
                string str = GetAttribute<string>( Schema.controlPointType.tangentInTypeAttribute );
                return (CurveTangentTypes)Enum.Parse( typeof( CurveTangentTypes ), str );
            }
            set { SetAttribute( Schema.controlPointType.tangentInTypeAttribute, value.ToString() ); }
        }

        /// <summary>
        /// Gets or sets "tangent out"</summary>
        public Vec2F TangentOut
        {
            get
            {
                float[] tan = GetAttribute<float[]>( Schema.controlPointType.tangentOutAttribute );
                return new Vec2F( tan );
            }
            set
            {
                float[] tan = new float[] { value.X, value.Y };
                SetAttribute( Schema.controlPointType.tangentOutAttribute, tan );
            }
        }

        /// <summary>
        /// Gets or sets "tanget out" type</summary>
        public CurveTangentTypes TangentOutType
        {
            get
            {
                string str = GetAttribute<string>( Schema.controlPointType.tangentOutTypeAttribute );
                return (CurveTangentTypes)Enum.Parse( typeof( CurveTangentTypes ), str );
            }
            set { SetAttribute( Schema.controlPointType.tangentOutTypeAttribute, value.ToString() ); }

        }

        /// <summary>
        /// Gets or sets whether the tangents are broken</summary>
        public bool BrokenTangents
        {
            get { return GetAttribute<bool>( Schema.controlPointType.brokenTangentsAttribute ); }
            set { SetAttribute( Schema.controlPointType.brokenTangentsAttribute, value ); }

        }

        private PointEditorData m_editorData = new PointEditorData();
        /// <summary>
        /// Gets editor's data.
        /// PointEditorData is used by the editor and does not need to be persisted.</summary>
        public PointEditorData EditorData
        {
            get { return m_editorData; }
        }

        /// <summary>
        /// Clones this control point</summary>
        /// <returns>Cloned control point</returns>
        public IControlPoint Clone()
        {
            DomNode node = new DomNode( Schema.controlPointType.Type );
            // clone local attributes
            foreach ( AttributeInfo attributeInfo in DomNode.Type.Attributes )
            {
                object value = DomNode.GetLocalAttribute( attributeInfo );
                if ( value != null )
                    node.SetAttribute( attributeInfo, attributeInfo.Type.Clone( value ) );
            }

            node.InitializeExtensions();
            return node.As<IControlPoint>();
        }

        #endregion
    }

    /// <summary>
    /// Adapts DomNode to a Curve</summary>
    public class Curve : DomNodeAdapter, ICurve
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_pointList = GetChildList<IControlPoint>( Schema.curveType.controlPointChild );
            m_readonlyPointList = new ReadOnlyCollection<IControlPoint>( m_pointList );
        }

        #region ICurve Members

        /// <summary>
        /// Gets curve name</summary>
        public string Name
        {
            get { return GetAttribute<string>( Schema.curveType.nameAttribute ); }
            set { SetAttribute( Schema.curveType.nameAttribute, value ); }
        }

        /// <summary>
        /// Gets or sets curve display name</summary>
        public string DisplayName
        {
            get { return GetAttribute<string>( Schema.curveType.displayNameAttribute ); }
            set { SetAttribute( Schema.curveType.displayNameAttribute, value ); }
        }

        /// <summary>
        /// Gets or sets curve Interpolation type</summary>        
        public InterpolationTypes CurveInterpolation
        {
            get { return m_interpolationType; }
            set { m_interpolationType = value; }
        }

        /// <summary>
        /// Gets or sets visibility</summary>
        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }

        /// <summary>
        /// Gets or sets minimum x value</summary>
        public float MinX
        {
            get { return GetAttribute<float>( Schema.curveType.minXAttribute ); }
            set { SetAttribute( Schema.curveType.minXAttribute, value ); }
        }

        /// <summary> 
        /// Gets or sets maximum x value</summary>
        public float MaxX
        {
            get { return GetAttribute<float>( Schema.curveType.maxXAttribute ); }
            set { SetAttribute( Schema.curveType.maxXAttribute, value ); }
        }

        /// <summary> 
        /// Gets or sets minimum y value</summary>
        public float MinY
        {
            get { return GetAttribute<float>( Schema.curveType.minYAttribute ); }
            set { SetAttribute( Schema.curveType.minYAttribute, value ); }
        }

        /// <summary> 
        /// Gets or sets maximum y value</summary>
        public float MaxY
        {
            get { return GetAttribute<float>( Schema.curveType.maxYAttribute ); }
            set { SetAttribute( Schema.curveType.maxYAttribute, value ); }
        }

        /// <summary>
        /// Gets x axis label</summary>
        public string XLabel
        {
            get { return GetAttribute<string>( Schema.curveType.xLabelAttribute ); }
            set { SetAttribute( Schema.curveType.xLabelAttribute, value ); }
        }

        /// <summary>
        /// Gets y axis label</summary>
        public string YLabel
        {
            get { return GetAttribute<string>( Schema.curveType.yLabelAttribute ); }
            set { SetAttribute( Schema.curveType.yLabelAttribute, value ); }
        }

        /// <summary>
        /// Gets or sets curve color</summary>
        public Color CurveColor
        {
            get { return Color.FromArgb( GetAttribute<int>( Schema.curveType.colorAttribute ) ); }
            set { SetAttribute( Schema.curveType.colorAttribute, value.ToArgb() ); }
        }

        /// <summary>
        /// Gets or sets values before first control point</summary>
        public CurveLoopTypes PreInfinity
        {
            get
            {
                string str = GetAttribute<string>( Schema.curveType.preInfinityAttribute );
                return (CurveLoopTypes)Enum.Parse( typeof( CurveLoopTypes ), str );
            }
            set { SetAttribute( Schema.curveType.preInfinityAttribute, value.ToString() ); }
        }

        /// <summary>
        /// Gets or sets values after last control point</summary>
        public CurveLoopTypes PostInfinity
        {
            get
            {
                string str = GetAttribute<string>( Schema.curveType.postInfinityAttribute );
                return (CurveLoopTypes)Enum.Parse( typeof( CurveLoopTypes ), str );
            }
            set { SetAttribute( Schema.curveType.postInfinityAttribute, value.ToString() ); }

        }

        /// <summary>
        /// Creates a control point</summary>
        /// <returns>Control point</returns>
        public IControlPoint CreateControlPoint()
        {
            DomNode node = new DomNode( Schema.controlPointType.Type );
            node.InitializeExtensions();
            IControlPoint cpt = node.As<IControlPoint>();
            cpt.TangentInType = CurveTangentTypes.Spline;
            cpt.TangentIn = new Vec2F( 0.5f, 0.5f );
            cpt.TangentOutType = CurveTangentTypes.Spline;
            cpt.TangentOut = new Vec2F( 0.5f, 0.5f );
            return cpt;
        }

        /// <summary>
        /// Adds control point at the end of the internal list</summary>
        /// <param name="cp">Control point</param> 
        public void AddControlPoint( IControlPoint cp )
        {
            m_pointList.Add( cp );
        }

        /// <summary>
        /// Inserts control point with the specified index into the internal list</summary>
        /// <param name="index">Index</param>
        /// <param name="cp">Control point</param>
        public void InsertControlPoint( int index, IControlPoint cp )
        {
            m_pointList.Insert( index, cp );
        }

        /// <summary>
        /// Removes given control point from the internal list</summary>
        /// <param name="cp">Control point</param>
        public void RemoveControlPoint( IControlPoint cp )
        {
            m_pointList.Remove( cp );
        }

        /// <summary>
        /// Gets readonly list of control points</summary>
        public ReadOnlyCollection<IControlPoint> ControlPoints
        {
            get { return m_readonlyPointList; }
        }

        /// <summary>
        /// Deletes all the control points</summary>
        public void Clear()
        {
            m_pointList.Clear();
        }

        private IList<IControlPoint> m_pointList;
        private ReadOnlyCollection<IControlPoint> m_readonlyPointList;
        private bool m_visible = true;
        private InterpolationTypes m_interpolationType = InterpolationTypes.Hermite;
        #endregion        
    }

    /// <summary>
    /// Adapts DomNode to a CurveModule</summary>
    public class CurveModule : MaterialGraphModuleAdapter, ICurveSet
    {
        public static DomNodeType curveType;
        public static ChildInfo curveChild;
        public static AttributeInfo inputModeAttribute;

        public static MaterialGraphPin pinOut_Red;

        //public static MaterialGraphPin pinIn_R;

        public static void DefineDomNodeType()
        {
            pinOut_Red = MaterialGraphPin.CreateMaterialGraphInputPin( "Out", 0, MaterialGraphPin.ComponentType.Red );

            //pinIn_R = MaterialGraphPin.CreateMaterialGraphInputPin( "T", 0, MaterialGraphPin.ComponentType.Red );

            string prettyName = "Curve".Localize();

            DomNodeType dnt = DefineModuleType(
                new XmlQualifiedName( "curveType", Schema.NS ),
                prettyName,
                prettyName,
                Resources.LightImage,
                new MaterialGraphPin[]
                {
                    //pinIn_R
                },
                new MaterialGraphPin[]
                {
                    pinOut_Red,
                },
                "Util" );

            curveType = dnt;

            curveChild = new ChildInfo( "curve", Schema.curveType.Type, true );
            dnt.Define( curveChild );

            inputModeAttribute = CreateEnumAttribute( dnt, "inputMode", new string[] { "Global Time", "Level Time", "Manual Trigger", "Manual" }, "General", "Input Mode",
                "Determines how input to this node is provided" );

            curveType.Define( new ExtensionInfo<CurveModule>() );
        }
        
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            m_curves = GetChildList<ICurve>( curveChild );
            if ( m_curves.Count > 0 )
                return;

            // add sample curve

            // add x channel
            Curve curve = (new DomNode( Schema.curveType.Type )).As<Curve>();
            curve.Name = Id;
            curve.DisplayName = Id;
            curve.MinX = 0;
            curve.MaxX = 1;
            curve.MinY = 0;
            curve.MaxY = 1;
            curve.CurveColor = Color.Red;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "x-Time";
            curve.YLabel = "x-Pos";

            IControlPoint cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 0;
            curve.AddControlPoint( cp );

            cp = curve.CreateControlPoint();
            cp.X = 1;
            cp.Y = 1;
            curve.AddControlPoint( cp );

            CurveUtils.ComputeTangent( curve );
            m_curves.Add( curve );
        }

        #region ICurveSet Members

            /// <summary>
            /// Gets list of the curves associated with an animation.</summary>
        public IList<ICurve> Curves
        {
            get { return m_curves; }
        }

        private IList<ICurve> m_curves;
        #endregion

        public override string EvaluatePin( MaterialGraphPin pin, ShaderSourceCode sourceCode )
        {
            if ( pin == pinOut_Red )
            {
                string uniform = sourceCode.AddUniformParameter( Id, UniformType.Float );

                string outputVarName = sourceCode.AddComputedValue( this, pin );
                sourceCode.ComputedValuesEval.Append( outputVarName );
                sourceCode.ComputedValuesEval.Append( " = " );
                sourceCode.ComputedValuesEval.Append( uniform );
                sourceCode.ComputedValuesEval.AppendLine( ";" );

                return outputVarName;
            }
            //else if ( pin == pinIn_R )
            //{
            //    string outputVarName = sourceCode.AddComputedValue( this, pin );
            //    sourceCode.ComputedValuesEval.Append( outputVarName );
            //    sourceCode.ComputedValuesEval.Append( " = 0;" );
            //    sourceCode.ComputedValuesEval.AppendLine();

            //    return outputVarName;
            //}

            throw new MaterialEvaluationException( this, pin, sourceCode );
        }
    }
}



