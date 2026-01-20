using UnityEngine;
using TMPro;
using MapLineSegmentToIsometricArc;

namespace Scripts.Utilities
{
    [ExecuteInEditMode]
    public class TextProOnACircleCurve : MonoBehaviour
    {
        [SerializeField]
        private float radius = 50;
        /// <summary>
        /// The text component of interest
        /// </summary>
        private TMP_Text m_TextComponent;

        /// <summary>
        /// Awake
        /// </summary>
        private void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            //every time the object gets enabled, we have to force a re-creation of the text mesh
        }

        /// <summary>
        /// Update
        /// </summary>
        protected void Update()
        {
            //during the loop, vertices represents the 4 vertices of a single character we're analyzing, 
            //while matrix is the roto-translation matrix that will rotate and scale the characters so that they will
            //follow the curve
            Vector3[] vertices;
            Matrix4x4 matrix;

            //Generate the mesh and get information about the text and the characters
            m_TextComponent.ForceMeshUpdate();

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            int characterCount = textInfo.characterCount;

            //if the string is empty, no need to waste time
            if (characterCount == 0)
                return;

            //gets the bounds of the rectangle that contains the text 
            float boundsMinX = m_TextComponent.bounds.min.x;
            float boundsMaxX = m_TextComponent.bounds.max.x;


            var origin = new System.Numerics.Vector2(0, 0 - radius);
            var transformator = new Line2CirArcTransformator(
                new System.Numerics.Vector2(boundsMinX, 0),
                new System.Numerics.Vector2(boundsMaxX, 0),
                origin,
                new System.Numerics.Vector2(0, 0));

            //for each character
            for (int i = 0; i < characterCount; i++)
            {
                //skip if it is invisible
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                //Get the index of the mesh used by this character, then the one of the material... and use all this data to get
                //the 4 vertices of the rect that encloses this character. Store them in vertices
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                vertices = textInfo.meshInfo[materialIndex].vertices;

                //Compute the baseline mid point for each character. This is the central point of the character.
                //we will use this as the point representing this character for the geometry transformations
                Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

                //remove the central point from the vertices point. After this operation, every one of the four vertices 
                //will just have as coordinates the offset from the central position. This will come handy when will deal with the rotations
                vertices[vertexIndex + 0] += -offsetToMidBaseline;
                vertices[vertexIndex + 1] += -offsetToMidBaseline;
                vertices[vertexIndex + 2] += -offsetToMidBaseline;
                vertices[vertexIndex + 3] += -offsetToMidBaseline;

                var result = transformator.MapLinePoint(new System.Numerics.Vector2(offsetToMidBaseline.x, offsetToMidBaseline.y));

                //calculate atan2 as if the origin of circle is (0,0)
                var rayResult = result - origin;
                float angle = Mathf.Atan2(rayResult.Y, rayResult.X); //we need radians for sin and cos

                matrix = Matrix4x4.TRS(new Vector3(result.X, result.Y, 0), Quaternion.AngleAxis(angle * Mathf.Rad2Deg - 90, Vector3.forward), Vector3.one);

                //apply the transformation, and obtain the final position and orientation of the 4 vertices representing this char
                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);
            }

            //Upload the mesh with the revised information
            m_TextComponent.UpdateVertexData();
        }
    }
}


namespace MapLineSegmentToIsometricArc
{
    using System;
    using System.Numerics;
    /* 求线段在某个圆中对应长的弧端点：
     * https://math.stackexchange.com/questions/275201/how-to-find-an-end-point-of-an-arc-given-another-end-point-radius-and-arc-dire
     * 在虚数空间中求线段在弧中的对应位置映射：
     * https://math.stackexchange.com/questions/3912758/a-mapping-that-converts-a-line-segment-to-an-arc
     */

    public class Line2CirArcTransformator
    {
        public Complex A1 { get; private set; }
        public Complex A { get; private set; }
        public Complex LineMidPoint { get; private set; }
        public Complex B { get; private set; }
        public Complex B1 { get; private set; }
        public Complex BBMidPoint { get; private set; }
        public Complex O { get; private set; }
        public double Radius { get; private set; }
        private Complex N { get; set; }
        private double InversionCircleRadius { get; set; }
        /// <summary>
        /// 计算P1时会用到的中间变量
        /// </summary>
        private Complex FactorForP { get; set; }

        /// <summary>
        /// <para>根据给定圆上位置找出一条等长弧并映射直线上的点</para>
        /// <para>根据线段长度和圆半径决定优劣弧</para>
        /// </summary>
        /// <param name="A">直线的端点1</param>
        /// <param name="A1">直线的端点1</param>
        /// <param name="O">圆心</param>
        /// <param name="MapMidPoint">映射圆弧在圆上的中点位置</param>
        public Line2CirArcTransformator(Vector2 A, Vector2 A1, Vector2 O, Vector2 MapMidPoint)
        {
            float Radius = (O - MapMidPoint).Length();
            this.Radius = Radius;
            float segmentLength = (A - A1).Length();
            CalculateEndPoints(segmentLength);

            Initialize(A, A1, O, segmentLength > Mathf.PI * Radius);

            Console.WriteLine("B: " + B);
            Console.WriteLine("B1:" + B1);

            //求线段在圆弧上的端点（中点位置往圆两侧延伸各一半）
            void CalculateEndPoints(float length)
            {
                float lineHalfLength = length / 2;

                B = VectorAsComplex(CalculateEndPoint(true));
                B1 = VectorAsComplex(CalculateEndPoint(false));

                Vector2 CalculateEndPoint(bool clockwise)
                {
                    var angle = Mathf.Atan2(MapMidPoint.Y - O.Y, MapMidPoint.X - O.X);
                    if (clockwise)
                    {
                        angle = angle - lineHalfLength / Radius;
                    }
                    else
                    {
                        angle = angle + lineHalfLength / Radius;
                    }
                    Vector2 result = new Vector2(O.X + Radius * Mathf.Cos(angle), O.Y + Radius * Mathf.Sin(angle));
                    return result;
                }
            }
        }

        /// <summary>
        /// <para>根据给定的弧映射直线上的点</para>
        /// <para>所确定的圆心永远在自B向B1的右边</para>
        /// <para>需要指定优劣弧，默认为劣弧</para>
        /// </summary>
        /// <param name="A">直线的端点1</param>
        /// <param name="A1">直线的端点1</param>
        /// <param name="B">圆弧的端点1</param>
        /// <param name="B1">圆弧的端点2</param>
        /// <param name="Radius">圆的半径</param>
        public Line2CirArcTransformator(Vector2 A, Vector2 A1, Vector2 B, Vector2 B1, float Radius, bool isMajorArc = false)
        {
            this.Radius = Radius;
            this.B = VectorAsComplex(B);
            this.B1 = VectorAsComplex(B1);
            Initialize(A, A1, CalculateO(), isMajorArc);

            Console.WriteLine("B: " + B);
            Console.WriteLine("B1:" + B1);

            //https://math.stackexchange.com/questions/1781438/finding-the-center-of-a-circle-given-two-points-and-a-radius-algebraically
            Vector2 CalculateO()
            {
                var xa = (B1.X - B.X) / 2;
                var ya = (B1.Y - B.Y) / 2;
                var a = Mathf.Sqrt(xa * xa + ya * ya);
                var b = Mathf.Sqrt(Radius * Radius - a * a);
                return new Vector2((B.X + B1.X) / 2 + b * ya / a, (B.Y + B1.Y) / 2 - b * xa / a);
            }
        }

        /// <summary>
        /// 初始化除B以外的变量
        /// </summary>
        /// <param name="A">直线的端点1</param>
        /// <param name="A1">直线的端点1</param>
        /// <param name="O">圆心</param>
        private void Initialize(Vector2 A, Vector2 A1, Vector2 O, bool isMajorArc)
        {
            //测试时发现需要调转个方向
            this.A1 = VectorAsComplex(A);
            this.A = VectorAsComplex(A1);
            this.O = VectorAsComplex(O);
            //this.MapMidPoint = VectorAsComplex(MapMidPoint);

            LineMidPoint = VectorAsComplex((A + A1) / 2);

            //B = VectorAsComplex(new Vector2(MathF.Sqrt(2)/2, MathF.Sqrt(2) / 2));
            //B1 = VectorAsComplex(new Vector2(MathF.Sqrt(2)/2, -MathF.Sqrt(2) / 2));
            BBMidPoint = (B + B1) / 2;
            N = CalculateN();
            InversionCircleRadius = (B - N).Magnitude;
            FactorForP = (B1 - B) / (this.A1 - this.A);

            Complex CalculateN()
            {
                return this.O + (isMajorArc ? 1 : -1) * 2 * Radius / Math.Sqrt(4 * Radius * Radius - Math.Pow((B - B1).Magnitude, 2)) * (BBMidPoint - this.O);
            }
        }

        private Complex VectorAsComplex(Vector2 vector) => new Complex(vector.X, vector.Y);

        public Vector2 MapLinePoint(Vector2 point)
        {
            var p = VectorAsComplex(point);
            // AA' to BB'
            Complex p1 = BBMidPoint + FactorForP * (p - LineMidPoint);
            Complex result = N + InversionCircleRadius * InversionCircleRadius / (Complex.Conjugate(p1) - Complex.Conjugate(N));
            return new Vector2((float)result.Real, (float)result.Imaginary);
        }
    }
}



