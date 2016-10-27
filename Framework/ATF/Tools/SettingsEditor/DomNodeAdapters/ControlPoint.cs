//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts DomNode to a control point</summary>
    public class ControlPoint : DomNodeAdapter , IControlPoint
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
            get { return GetAttribute<float>(Schema.controlPointType.xAttribute); }
            set 
            {                
                SetAttribute(Schema.controlPointType.xAttribute, value); 
            }
        }

        /// <summary>
        /// Gets or sets y-coordinate</summary>
        public float Y
        {
            get { return GetAttribute<float>(Schema.controlPointType.yAttribute); }
            set 
            {                
                SetAttribute(Schema.controlPointType.yAttribute, value); 
            }
            
        }

        /// <summary>
        /// Gets or sets "tangent in" value</summary>
        public Vec2F TangentIn
        {
            get 
            {
                float[] tan = GetAttribute<float[]>(Schema.controlPointType.tangentInAttribute);
                return new Vec2F(tan);
            }
            set 
            {
                float[] tan = new float[] { value.X, value.Y };
                SetAttribute(Schema.controlPointType.tangentInAttribute, tan); 
            }            
        }

        /// <summary>
        /// Gets or sets "tangent in" type</summary>
        public CurveTangentTypes TangentInType
        {
            get 
            {
                string str = GetAttribute<string>(Schema.controlPointType.tangentInTypeAttribute);
                return (CurveTangentTypes)Enum.Parse(typeof(CurveTangentTypes), str);
            }
            set { SetAttribute(Schema.controlPointType.tangentInTypeAttribute, value.ToString()); }            
        }

        /// <summary>
        /// Gets or sets "tangent out"</summary>
        public Vec2F TangentOut
        {
            get 
            {
                float[] tan = GetAttribute<float[]>(Schema.controlPointType.tangentOutAttribute);
                return new Vec2F(tan);
            }
            set 
            {
                float[] tan = new float[] { value.X, value.Y };
                SetAttribute(Schema.controlPointType.tangentOutAttribute, tan); 
            }            
        }

        /// <summary>
        /// Gets or sets "tanget out" type</summary>
        public CurveTangentTypes TangentOutType
        {
            get 
            {
                string str = GetAttribute<string>(Schema.controlPointType.tangentOutTypeAttribute);
                return (CurveTangentTypes)Enum.Parse(typeof(CurveTangentTypes), str);
            }
            set { SetAttribute(Schema.controlPointType.tangentOutTypeAttribute, value.ToString()); }
            
        }

        /// <summary>
        /// Gets or sets whether the tangents are broken</summary>
        public bool BrokenTangents
        {
            get { return GetAttribute<bool>(Schema.controlPointType.brokenTangentsAttribute); }
            set { SetAttribute(Schema.controlPointType.brokenTangentsAttribute, value); }
            
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
            DomNode node = new DomNode(Schema.controlPointType.Type);
            // clone local attributes
            foreach (AttributeInfo attributeInfo in DomNode.Type.Attributes)
            {
                object value = DomNode.GetLocalAttribute(attributeInfo);
                if (value != null)
                    node.SetAttribute(attributeInfo, attributeInfo.Type.Clone(value));
            }

            node.InitializeExtensions();
            return node.As<IControlPoint>();
        }

        #endregion
    }
}
