﻿using System;
using static System.Math;

namespace GeometRi
{
    public class Ellipse : IPlanarObject
    {

        private Point3d _point;
        private Vector3d _v1;
        private Vector3d _v2;

        public Ellipse(Point3d Center, Vector3d semiaxis_a, Vector3d semiaxis_b)
        {
            if ((!semiaxis_a.IsOrthogonalTo(semiaxis_b)))
            {
                throw new Exception("Semiaxes are not orthogonal");
            }
            _point = Center.Copy();
            if (semiaxis_a.Norm >= semiaxis_b.Norm)
            {
                _v1 = semiaxis_a.Copy();
                _v2 = semiaxis_b.Copy();
            }
            else
            {
                _v1 = semiaxis_b.Copy();
                _v2 = semiaxis_a.Copy();
            }

        }

        /// <summary>
        /// Creates copy of the object
        /// </summary>
        public Ellipse Copy()
        {
            return new Ellipse(_point.Copy(), _v1.Copy(), _v2.Copy());
        }

        #region "Properties"
        public Point3d Center
        {
            get { return _point.Copy(); }
        }

        public Vector3d MajorSemiaxis
        {
            get { return _v1.Copy(); }
        }

        public Vector3d MinorSemiaxis
        {
            get { return _v2.Copy(); }
        }

        public Vector3d Normal
        {
            get { return _v1.Cross(_v2); }
        }

        public bool IsOriented
        {
            get { return false; }
        }

        /// <summary>
        /// Length of the major semiaxis
        /// </summary>
        public double A
        {
            get { return _v1.Norm; }
        }

        /// <summary>
        /// Length of the minor semiaxis
        /// </summary>
        public double B
        {
            get { return _v2.Norm; }
        }

        /// <summary>
        /// Distance from center to focus
        /// </summary>
        public double F
        {
            get { return Sqrt(Math.Pow(_v1.Norm, 2) - Math.Pow(_v2.Norm, 2)); }
        }

        /// <summary>
        /// First focus
        /// </summary>
        public Point3d F1
        {
            get { return _point.Translate(F * _v1.Normalized); }
        }

        /// <summary>
        /// Second focus
        /// </summary>
        public Point3d F2
        {
            get { return _point.Translate(-F * _v1.Normalized); }
        }

        /// <summary>
        /// Eccentricity of the ellipse
        /// </summary>
        public double E
        {
            get { return Sqrt(1 - Math.Pow(_v2.Norm, 2) / Math.Pow(_v1.Norm, 2)); }
        }

        public double Area
        {
            get { return PI * A * B; }
        }

        /// <summary>
        /// Approximate circumference of the ellipse
        /// </summary>
        public double Perimeter
        {
            get
            {
                double a = _v1.Norm;
                double b = _v2.Norm;
                double h = Math.Pow((a - b), 2) / Math.Pow((a + b), 2);
                return PI * (a + b) * (1 + 3 * h / (10 + Sqrt(4 - 3 * h)));
            }
        }
        #endregion

        #region "ParallelMethods"
        /// <summary>
        /// Check if two objects are parallel
        /// </summary>
        public bool IsParallelTo(ILinearObject obj)
        {
            return this.Normal.IsOrthogonalTo(obj.Direction);
        }

        /// <summary>
        /// Check if two objects are NOT parallel
        /// </summary>
        public bool IsNotParallelTo(ILinearObject obj)
        {
            return !this.Normal.IsOrthogonalTo(obj.Direction);
        }

        /// <summary>
        /// Check if two objects are orthogonal
        /// </summary>
        public bool IsOrthogonalTo(ILinearObject obj)
        {
            return this.Normal.IsParallelTo(obj.Direction);
        }

        /// <summary>
        /// Check if two objects are parallel
        /// </summary>
        public bool IsParallelTo(IPlanarObject obj)
        {
            return this.Normal.IsParallelTo(obj.Normal);
        }

        /// <summary>
        /// Check if two objects are NOT parallel
        /// </summary>
        public bool IsNotParallelTo(IPlanarObject obj)
        {
            return this.Normal.IsNotParallelTo(obj.Normal);
        }

        /// <summary>
        /// Check if two objects are orthogonal
        /// </summary>
        public bool IsOrthogonalTo(IPlanarObject obj)
        {
            return this.Normal.IsOrthogonalTo(obj.Normal);
        }
        #endregion

        /// <summary>
        /// Returns point on ellipse for given parameter 't' (0 &lt;= t &lt; 2Pi)
        /// </summary>
        public Point3d ParametricForm(double t)
        {

            return _point + _v1.ToPoint * Cos(t) + _v2.ToPoint * Sin(t);

        }

        /// <summary>
        /// Orthogonal projection of the ellipse to plane
        /// </summary>
        public Ellipse ProjectionTo(Plane3d s)
        {

            Point3d c = _point.ProjectionTo(s);
            Point3d q = _point.Translate(_v1).ProjectionTo(s);
            Point3d p = _point.Translate(_v2).ProjectionTo(s);

            Vector3d f1 = new Vector3d(c, p);
            Vector3d f2 = new Vector3d(c, q);

            double t0 = 0.5 * Atan2(2 * f1 * f2, f1 * f1 - f2 * f2);
            Vector3d v1 = f1 * Cos(t0) + f2 * Sin(t0);
            Vector3d v2 = f1 * Cos(t0 + PI / 2) + f2 * Sin(t0 + PI / 2);

            return new Ellipse(c, v1, v2);
        }

        /// <summary>
        /// Intersection of ellipse with plane.
        /// Returns 'null' (no intersection) or object of type 'Ellipse', 'Point3d' or 'Segment3d'.
        /// </summary>
        public object IntersectionWith(Plane3d s)
        {


            if (this.Normal.IsParallelTo(s.Normal))
            {
                if (this.Center.BelongsTo(s))
                {
                    // coplanar objects
                    return this.Copy();
                }
                else
                {
                    // parallel objects
                    return null;
                }
            }
            else
            {
                Line3d l = (Line3d)s.IntersectionWith(new Plane3d(this.Center, this.Normal));
                Coord3d local_coord = new Coord3d(this.Center, this._v1, this._v2);
                Point3d p = l.Point.ConvertTo(local_coord);
                Vector3d v = l.Direction.ConvertTo(local_coord);
                double a = this.A;
                double b = this.B;

                if (Abs(v.Y / v.X) > 100)
                {
                    // line is almost vertical, rotate local coord
                    local_coord = new Coord3d(this.Center, this._v2, this._v1);
                    p = l.Point.ConvertTo(local_coord);
                    v = l.Direction.ConvertTo(local_coord);
                    a = this.B;
                    b = this.A;
                }

                // Find intersection of line and ellipse (2D)
                // Solution from: http://www.ambrsoft.com/TrigoCalc/Circles2/Ellipse/EllipseLine.htm

                // Line equation in form: y = mx + c
                double m = v.Y / v.X;
                double c = p.Y - m * p.X;

                double amb = Math.Pow(a, 2) * Math.Pow(m, 2) + Math.Pow(b, 2);
                double det = amb - Math.Pow(c, 2);
                if (det < -GeometRi3D.Tolerance)
                {
                    return null;
                }
                else if (GeometRi3D.AlmostEqual(det, 0))
                {
                    double x = -Math.Pow(a, 2) * m * c / amb;
                    double y = Math.Pow(b, 2) * c / amb;
                    return new Point3d(x, y, 0, local_coord);
                }
                else
                {
                    double x1 = (-Math.Pow(a, 2) * m * c + a * b * Sqrt(det)) / amb;
                    double x2 = (-Math.Pow(a, 2) * m * c - a * b * Sqrt(det)) / amb;
                    double y1 = (Math.Pow(b, 2) * c + a * b * m * Sqrt(det)) / amb;
                    double y2 = (Math.Pow(b, 2) * c - a * b * m * Sqrt(det)) / amb;
                    return new Segment3d(new Point3d(x1, y1, 0, local_coord), new Point3d(x2, y2, 0, local_coord));
                }
            }

        }

        #region "TranslateRotateReflect"
        /// <summary>
        /// Translate ellipse by a vector
        /// </summary>
        public Ellipse Translate(Vector3d v)
        {
            return new Ellipse(this.Center.Translate(v), _v1, _v2);
        }

        /// <summary>
        /// Rotate ellipse by a given rotation matrix
        /// </summary>
        public Ellipse Rotate(Matrix3d m)
        {
            return new Ellipse(this.Center.Rotate(m), _v1.Rotate(m), _v2.Rotate(m));
        }

        /// <summary>
        /// Rotate ellipse by a given rotation matrix around point 'p' as a rotation center
        /// </summary>
        public Ellipse Rotate(Matrix3d m, Point3d p)
        {
            return new Ellipse(this.Center.Rotate(m, p), _v1.Rotate(m), _v2.Rotate(m));
        }

        /// <summary>
        /// Reflect ellipse in given point
        /// </summary>
        public Ellipse ReflectIn(Point3d p)
        {
            return new Ellipse(this.Center.ReflectIn(p), _v1.ReflectIn(p), _v2.ReflectIn(p));
        }

        /// <summary>
        /// Reflect ellipse in given line
        /// </summary>
        public Ellipse ReflectIn(Line3d l)
        {
            return new Ellipse(this.Center.ReflectIn(l), _v1.ReflectIn(l), _v2.ReflectIn(l));
        }

        /// <summary>
        /// Reflect ellipse in given plane
        /// </summary>
        public Ellipse ReflectIn(Plane3d s)
        {
            return new Ellipse(this.Center.ReflectIn(s), _v1.ReflectIn(s), _v2.ReflectIn(s));
        }
        #endregion

        /// <summary>
        /// Determines whether two objects are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || (!object.ReferenceEquals(this.GetType(), obj.GetType())))
            {
                return false;
            }
            Ellipse e = (Ellipse)obj;

            if (GeometRi3D.AlmostEqual(this.A, this.B))
            {
                // Ellipse is circle
                if (GeometRi3D.AlmostEqual(e.A, e.B))
                {
                    // Second ellipse also circle
                    return this.Center == e.Center && GeometRi3D.AlmostEqual(this.A, e.A) && e.Normal.IsParallelTo(this.Normal);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return this.Center == e.Center && GeometRi3D.AlmostEqual(this.A, e.A) && GeometRi3D.AlmostEqual(this.B, e.B) && 
                       e.MajorSemiaxis.IsParallelTo(this.MajorSemiaxis) && e.MinorSemiaxis.IsParallelTo(this.MinorSemiaxis);
            }
        }

        /// <summary>
        /// Returns the hashcode for the object.
        /// </summary>
        public override int GetHashCode()
        {
            return GeometRi3D.HashFunction(_point.GetHashCode(), _v1.GetHashCode(), _v2.GetHashCode());
        }

        /// <summary>
        /// String representation of an object in global coordinate system.
        /// </summary>
        public override String ToString()
        {
            return ToString(Coord3d.GlobalCS);
        }

        /// <summary>
        /// String representation of an object in reference coordinate system.
        /// </summary>
        public String ToString(Coord3d coord)
        {
            string nl = System.Environment.NewLine;

            if (coord == null) { coord = Coord3d.GlobalCS; }
            Point3d P = _point.ConvertTo(coord);
            Vector3d v1 = _v1.ConvertTo(coord);
            Vector3d v2 = _v2.ConvertTo(coord);

            string str = string.Format("Ellipse: ") + nl;
            str += string.Format("  Center -> ({0,10:g5}, {1,10:g5}, {2,10:g5})", P.X, P.Y, P.Z) + nl;
            str += string.Format("  Semiaxis A -> ({0,10:g5}, {1,10:g5}, {2,10:g5})", v1.X, v1.Y, v1.Z) + nl;
            str += string.Format("  Semiaxis B -> ({0,10:g5}, {1,10:g5}, {2,10:g5})", v2.X, v2.Y, v2.Z) + nl;
            return str;
        }

        // Operators overloads
        //-----------------------------------------------------------------

        public static bool operator ==(Ellipse c1, Ellipse c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(Ellipse c1, Ellipse c2)
        {
            return !c1.Equals(c2);
        }

    }
}
