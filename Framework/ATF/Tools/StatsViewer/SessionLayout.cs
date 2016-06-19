using System.Collections.Generic;
using System.Drawing;

namespace misz.StatsViewer
{
    /// <summary>
    /// A class to associate a timeline object's path with a bounding rectangle in screen space.
    /// A timeline object can appear multiple times within a TimelineControl due to referenced
    /// sub-documents, including the possibility of the same timeline sub-document being
    /// referenced multiple times. Also provides an enumeration of all pairs of paths and
    /// rectangles.</summary>
    public class SessionLayout : IEnumerable<KeyValuePair<SessionPath, RectangleF>>
    {
        /// <summary>
        /// Strictly adds a new path and associated bounding rectangle. The path must not already
        /// have been added.</summary>
        /// <param name="path">Path</param>
        /// <param name="bounds">Bounding rectangle in screen space of this visual instance of a timeline object</param>
        public void Add(SessionPath path, RectangleF bounds)
        {
            m_paths.Add(path, bounds);
        }

        /// <summary>
        /// Gets the bounding rectangle, in screen space, of the given timeline object path.
        /// The path must have been previously added.</summary>
        /// <param name="path">Path</param>
        /// <returns>Bounding rectangle in screen space</returns>
        public RectangleF GetBounds(SessionPath path)
        {
            return m_paths[path];
        }

        /// <summary>
        /// Gets the bounding rectangle, in screen space, of the given timeline object path, or
        /// returns false if the path can't be found</summary>
        /// <param name="path">Path</param>
        /// <param name="bounds">Bounding rectangle in screen space</param>
        /// <returns>True iff the bounds for this path could be found</returns>
        public bool TryGetBounds(SessionPath path, out RectangleF bounds)
        {
            return m_paths.TryGetValue(path, out bounds);
        }

        /// <summary>
        /// Returns whether or not the given path was previously added</summary>
        /// <param name="path">Path</param>
        /// <returns>True iff the given path was previously added</returns>
        public bool ContainsPath(SessionPath path)
        {
            return m_paths.ContainsKey(path);
        }

        /// <summary>
        /// Gets or sets the bounding rectangle to be associated with this path. When getting,
        /// the path must already have been added or an exception is thrown. When setting,
        /// the path may or may not have already been added; if not already present, the
        /// path is added.</summary>
        /// <param name="path">Path</param>
        public RectangleF this[SessionPath path]
        {
            get { return m_paths[path]; }
            set { m_paths[path] = value; }
        }

        /// <summary>
        /// Gets or sets the bounding rectangle, in screen space, of a timeline object that is
        /// in the main timeline document and so has no parent ITimelineReference objects in
        /// its path. A SessionPath is created around this timeline object when setting.</summary>
        /// <param name="obj">Timeline object</param>
        public RectangleF this[ISessionObject obj]
        {
            get { return m_paths[new SessionPath(obj)]; }
            set { m_paths[new SessionPath(obj)] = value; }
        }

        #region IEnumerable<KeyValuePair<SessionPath,RectangleF>> Members

        /// <summary>
        /// Gets enumerator for paths</summary>
        /// <returns>Enumerator for paths</returns>
        public IEnumerator<KeyValuePair<SessionPath, RectangleF>> GetEnumerator()
        {
            return m_paths.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private readonly Dictionary<SessionPath, RectangleF> m_paths =
            new Dictionary<SessionPath, RectangleF>();
    }
}
