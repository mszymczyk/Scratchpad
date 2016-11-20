//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace SettingsEditor
{
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
        public System.Drawing.Color CurveColor
        {
            get { return System.Drawing.Color.FromArgb( GetAttribute<int>( Schema.curveType.colorAttribute ) ); }
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
}
