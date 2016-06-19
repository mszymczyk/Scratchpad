using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace misz.StatsViewer
{
    /// <summary>
    /// Class for session path, which is a sequence of objects in session, e.g., groups, tracks, events</summary>
    public class SessionPath : AdaptablePath<ISessionObject>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="last">Single object making up the path</param>
        public SessionPath(ISessionObject last)
            : base(last)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Path, as sequence of objects</param>
        public SessionPath(IEnumerable<ISessionObject> path)
            : base(path)
        {
        }

        /// <summary>
        /// Concatenates object with path</summary>
        /// <param name="lhs">Prefix object</param>
        /// <param name="rhs">Optional path</param>
        /// <returns>Concatenated path, with lhs as first object</returns>
        public static SessionPath operator +(ISessionObject lhs, SessionPath rhs)
        {
            if (rhs == null)
                return new SessionPath(lhs);
            ISessionObject[] path = new ISessionObject[1 + rhs.Count];
            path[0] = lhs;
            rhs.CopyTo(path, 1);
            return new SessionPath(path);
        }

        /// <summary>
        /// Concatenates path with object</summary>
        /// <param name="lhs">Optional path</param>
        /// <param name="rhs">Suffix object</param>
        /// <returns>Concatenated path, with rhs as last object</returns>
        public static SessionPath operator +(SessionPath lhs, ISessionObject rhs)
        {
            if (lhs == null)
                return new SessionPath(rhs);
            ISessionObject[] path = new ISessionObject[lhs.Count + 1];
            lhs.CopyTo(path, 0);
            path[lhs.Count] = rhs;
            return new SessionPath(path);
        }

        /// <summary>
        /// Concatenates two paths</summary>
        /// <param name="lhs">First path. Can be null.</param>
        /// <param name="rhs">Second path. Can be null.</param>
        /// <returns>Concatenated path, with rhs as prefix and lhs as suffix. Is null if both lhs and rhs are null.</returns>
        public static SessionPath operator +(SessionPath lhs, SessionPath rhs)
        {
            if (lhs == null)
                return rhs;
            if (rhs == null)
                return lhs;
            ISessionObject[] path = new ISessionObject[lhs.Count + rhs.Count];
            lhs.CopyTo(path, 0);
            rhs.CopyTo(path, lhs.Count);
            return new SessionPath(path);
        }
    }
}
