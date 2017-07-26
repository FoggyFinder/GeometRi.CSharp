﻿using System;
using static System.Math;

namespace GeometRi
{
    public class Circle3d : IPlanarObject
    {

        private Point3d _point;
        private double _r;
        private Vector3d _normal;

        public Circle3d(Point3d Center, double Radius, Vector3d Normal)
        {
            _point = Center.Copy();
            _r = Radius;
            _normal = Normal.Copy();
        }


        public Circle3d(Point3d p1, Point3d p2, Point3d p3)
        {
            Vector3d v1 = new Vector3d(p1, p2);
            Vector3d v2 = new Vector3d(p1, p3);
            if (v1.Cross(v2).Norm < GeometRi3D.Tolerance)
            {
                throw new Exception("Collinear points");
            }

            Coord3d CS = new Coord3d(p1, v1, v2);
            Point3d a1 = p1.ConvertTo(CS);
            Point3d a2 = p2.ConvertTo(CS);
            Point3d a3 = p3.ConvertTo(CS);

            double d1 = Math.Pow(a1.X, 2) + Math.Pow(a1.Y, 2);
            double d2 = Math.Pow(a2.X, 2) + Math.Pow(a2.Y, 2);
            double d3 = Math.Pow(a3.X, 2) + Math.Pow(a3.Y, 2);
            double f = 2.0 * (a1.X * (a2.Y - a3.Y) - a1.Y * (a2.X - a3.X) + a2.X * a3.Y - a3.X * a2.Y);

            double X = (d1 * (a2.Y - a3.Y) + d2 * (a3.Y - a1.Y) + d3 * (a1.Y - a2.Y)) / f;
            double Y = (d1 * (a3.X - a2.X) + d2 * (a1.X - a3.X) + d3 * (a2.X - a1.X)) / f;
            //_point = (new Point3d(X, Y, 0, CS)).ConvertTo(p1.Coord);
            _point = new Point3d(X, Y, 0, CS);
            _point = _point.ConvertTo(p1.Coord);
            _r = Sqrt(Math.Pow((X - a1.X), 2) + Math.Pow((Y - a1.Y), 2));
            _normal = v1.Cross(v2);

        }

        /// <summary>
        /// Creates copy of the object
        /// </summary>
        public Circle3d Copy()
        {
            return new Circle3d(_point, _r, _normal);
        }

        /// <summary>
        /// Center of the circle
        /// </summary>
        public Point3d Center
        {
            get { return _point.Copy(); }
            set { _point = value.Copy(); }
        }

        /// <summary>
        /// Radius of the circle
        /// </summary>
        public double R
        {
            get { return _r; }
            set { _r = value; }
        }

        /// <summary>
        /// Normal of the circle
        /// </summary>
        public Vector3d Normal
        {
            get { return _normal.Copy(); }
            set { _normal = value.Copy(); }
        }

        public bool IsOriented
        {
            get { return false; }
        }

        public double Perimeter
        {
            get { return 2 * PI * _r; }
        }

        public double Area
        {
            get { return PI * Math.Pow(_r, 2); }
        }

        public Ellipse ToEllipse
        {
            get
            {
                Vector3d v1 = _r * _normal.OrthogonalVector.Normalized;
                Vector3d v2 = _r * (_normal.Cross(v1)).Normalized;
                return new Ellipse(_point, v1, v2);
            }
        }

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
        /// Returns point on circle for given parameter 't' (0 &lt;= t &lt; 2Pi)
        /// </summary>
        public Point3d ParametricForm(double t)
        {

            // Get two orthogonal coplanar vectors
            Vector3d v1 = _r * _normal.OrthogonalVector.Normalized;
            Vector3d v2 = _r * (_normal.Cross(v1)).Normalized;
            return _point + v1.ToPoint * Cos(t) + v2.ToPoint * Sin(t);

        }

        /// <summary>
        /// Orthogonal projection of the circle to plane
        /// </summary>
        public Ellipse ProjectionTo(Plane3d s)
        {
            return this.ToEllipse.ProjectionTo(s);
        }

        /// <summary>
        /// Intersection of circle with plane.
        /// Returns 'null' (no intersection) or object of type 'Circle3d', 'Point3d' or 'Segment3d'.
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
                Coord3d local_coord = new Coord3d(this.Center, l.Direction, this.Normal.Cross(l.Direction));
                Point3d p = l.Point.ConvertTo(local_coord);

                if (GeometRi3D.Greater(Abs(p.Y), this.R))
                {
                    return null;
                }
                else if (GeometRi3D.AlmostEqual(p.Y, this.R))
                {
                    return new Point3d(0, this.R, 0, local_coord);
                }
                else if (GeometRi3D.AlmostEqual(p.Y, -this.R))
                {
                    return new Point3d(0, -this.R, 0, local_coord);
                }
                else
                {
                    double d = Sqrt(Math.Pow(this.R, 2) - Math.Pow(p.Y, 2));
                    Point3d p1 = new Point3d(-d, p.Y, 0, local_coord);
                    Point3d p2 = new Point3d(d, p.Y, 0, local_coord);
                    return new Segment3d(p1, p2);
                }
            }

        }

        #region "TranslateRotateReflect"
        /// <summary>
        /// Translate circle by a vector
        /// </summary>
        public Circle3d Translate(Vector3d v)
        {
            return new Circle3d(this.Center.Translate(v), this.R, this.Normal);
        }

        /// <summary>
        /// Rotate circle by a given rotation matrix
        /// </summary>
        public Circle3d Rotate(Matrix3d m)
        {
            return new Circle3d(this.Center.Rotate(m), this.R, this.Normal.Rotate(m));
        }

        /// <summary>
        /// Rotate circle by a given rotation matrix around point 'p' as a rotation center
        /// </summary>
        public Circle3d Rotate(Matrix3d m, Point3d p)
        {
            return new Circle3d(this.Center.Rotate(m, p), this.R, this.Normal.Rotate(m));
        }

        /// <summary>
        /// Reflect circle in given point
        /// </summary>
        public Circle3d ReflectIn(Point3d p)
        {
            return new Circle3d(this.Center.ReflectIn(p), this.R, this.Normal.ReflectIn(p));
        }

        /// <summary>
        /// Reflect circle in given line
        /// </summary>
        public Circle3d ReflectIn(Line3d l)
        {
            return new Circle3d(this.Center.ReflectIn(l), this.R, this.Normal.ReflectIn(l));
        }

        /// <summary>
        /// Reflect circle in given plane
        /// </summary>
        public Circle3d ReflectIn(Plane3d s)
        {
            return new Circle3d(this.Center.ReflectIn(s), this.R, this.Normal.ReflectIn(s));
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
            Circle3d c = (Circle3d)obj;

            return c.Center == this.Center && Abs(c.R - this.R) <= GeometRi3D.Tolerance && c.Normal.IsParallelTo(this.Normal);
        }

        /// <summary>
        /// Returns the hashcode for the object.
        /// </summary>
        public override int GetHashCode()
        {
            return GeometRi3D.HashFunction(_point.GetHashCode(), _r.GetHashCode(), _normal.GetHashCode());
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
            Vector3d normal = _normal.ConvertTo(coord);

            string str = string.Format("Circle: ") + nl;
            str += string.Format("  Center -> ({0,10:g5}, {1,10:g5}, {2,10:g5})", P.X, P.Y, P.Z) + nl;
            str += string.Format("  Radius -> {0,10:g5}", _r) + nl;
            str += string.Format("  Normal -> ({0,10:g5}, {1,10:g5}, {2,10:g5})", normal.X, normal.Y, normal.Z);
            return str;
        }

        // Operators overloads
        //-----------------------------------------------------------------

        public static bool operator ==(Circle3d c1, Circle3d c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(Circle3d c1, Circle3d c2)
        {
            return !c1.Equals(c2);
        }

    }
}

