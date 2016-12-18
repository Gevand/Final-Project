using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gevand_Balayan____Assignment_8
{
    public partial class Form1 : Form
    {
        int edge = 0;
        float distance = 800;
        double rotationAngle = Math.PI / 2;
        Pen blackPen = new Pen(Color.Black, 1);
        public const double DELTA = .005;

        public Form1()
        {
            InitializeComponent();

        }

        public PointF ScalePoint(float scale, PointF p1, PointF center)
        {
            float x = 0;
            float y = 0;
            //translate
            float dx = p1.X - center.X;
            float dy = p1.Y - center.Y;
            //scale
            x = dx * scale;
            y = dy * scale;
            //translate
            x += center.X;
            y += center.Y;
            return new PointF(x, y);
        }
        private void pbBezlzier_Paint(object sender, PaintEventArgs e)
        {
            List<PointF> bezierPoints = new List<PointF>() 
        { 
       new PointF(0, 200) ,new PointF(800, 200) ,new PointF(83, 156) ,new PointF(310, 139) ,new PointF(601, 168) ,new PointF(93, 232) ,new PointF(318, 228) ,new PointF(593, 215)};

            //draw the x y coordinates
            Graphics g = e.Graphics;
            Pen greenPen = new Pen(Color.Green, 1);
            Pen blackPen = new Pen(Color.Black, 1);
            rotationAngle = 0;

            List<PointF> bezier = new List<PointF>();
            for (int c = 0; c <= 1; c++)
            {
                var workSet = new List<PointF>(); ;
                if (c == 0)
                    workSet = bezierPoints.Where(p => p.Y >= 200).OrderBy(p => p.X).ToList(); //negative
                else
                    workSet = bezierPoints.Where(p => p.Y <= 200).OrderBy(p => p.X).ToList(); //positive
                for (int splineCount = 0; splineCount < workSet.Count - 1; splineCount++)
                {

                    //matrix 
                    //1 2 0 ... 0
                    //0 2 4 2 . 0
                    //....
                    //0 0 ..  1 2
                    var myMatrix = MatrixUtlities.MatrixCreate(workSet.Count, workSet.Count);
                    //3(x1 - x0)... matrix
                    var myCooficientsX = MatrixUtlities.MatrixCreate(1, workSet.Count);
                    //samething but for yws
                    var myCooficientsY = MatrixUtlities.MatrixCreate(1, workSet.Count);
                    //first and last rows
                    myCooficientsX[0][0] = 3 * (workSet[1].X - workSet[0].X);
                    myCooficientsX[0][workSet.Count - 1] = 3 * (workSet[workSet.Count - 1].X - workSet[workSet.Count - 2].X);

                    myCooficientsY[0][0] = 3 * (workSet[1].Y - workSet[0].Y);
                    myCooficientsY[0][workSet.Count - 1] = 3 * (workSet[workSet.Count - 1].Y - workSet[workSet.Count - 2].Y);

                    //middle rows
                    for (int i = 1; i < workSet.Count - 1; i++)
                    {

                        myCooficientsX[0][i] = 3 * (workSet[i + 1].X - workSet[i - 1].X);
                        myCooficientsY[0][i] = 3 * (workSet[i + 1].Y - workSet[i - 1].Y);
                    }
                    myMatrix[0][0] = 2; myMatrix[0][1] = 1; myMatrix[workSet.Count - 1][workSet.Count - 2] = 1; myMatrix[workSet.Count - 1][workSet.Count - 1] = 2;
                    for (int i = 1; i <= workSet.Count - 2; i++)
                        for (int j = 0; j <= workSet.Count - 1; j++)
                        {
                            if (i - 1 == j)
                                myMatrix[i][j] = 1;
                            else if (i == j)
                                myMatrix[i][j] = 4;
                            else if (i + 1 == j)
                                myMatrix[i][j] = 1;
                            else
                                myMatrix[i][j] = 0;
                        }

                    var myMatrixInverse = MatrixUtlities.MatrixInverse(myMatrix);
                    var myXDerivatives = MatrixUtlities.MatrixProduct(myCooficientsX, myMatrixInverse);
                    var myYDerivatives = MatrixUtlities.MatrixProduct(myCooficientsY, myMatrixInverse);



                    for (int i = 0; i < workSet.Count - 1; i++)
                    {
                        double a3x = myXDerivatives[0][i + 1] + myXDerivatives[0][i] - (2 * workSet[i + 1].X) + (2 * workSet[i].X);
                        double a3y = myYDerivatives[0][i + 1] + myYDerivatives[0][i] - (2 * workSet[i + 1].Y) + (2 * workSet[i].Y);
                        double a2x = (3 * workSet[i + 1].X) - (3 * workSet[i].X) - (2 * myXDerivatives[0][i]) - myXDerivatives[0][i + 1];
                        double a2y = (3 * workSet[i + 1].Y) - (3 * workSet[i].Y) - (2 * myYDerivatives[0][i]) - myYDerivatives[0][i + 1];
                        double a1x = myXDerivatives[0][i];
                        double a1y = myYDerivatives[0][i];
                        double a0x = workSet[i].X;
                        double a0y = workSet[i].Y;

                        for (double t = 0; t <= 1; t = t + DELTA)
                        {

                            PointF newPoint = new PointF();
                            newPoint.X = (float)(a3x * Math.Pow((t), 3) + (a2x * Math.Pow((t), 2)) + (a1x * t) + a0x);
                            newPoint.Y = (float)(a3y * Math.Pow((t), 3) + (a2y * Math.Pow((t), 2)) + (a1y * t) + a0y);
                            bezier.Add(ScalePoint(.3f, newPoint, bezierPoints[0]));

                        }
                    }
                }

            }
            var scalePoint = bezierPoints[0];
            if (radioButton2.Checked)
                scalePoint = bezier[bezier.Count - 1];

            if (radioButton3.Checked)
                scalePoint = bezier[400];

            rotationAngle = 0;
            double.TryParse(txtAngle.Text, out rotationAngle);
            rotationAngle = ConvertToRadians(rotationAngle);
            var linePoints1 = new List<PointF>();
            var linePoints2 = new List<PointF>();
            //X' = X * (F/Z)
            //Y' = Y * (F/Z)
            for (int slice = 0; slice <= 29; slice++)
            {
                var scaleFactor = 1 - (slice * .03f);
                int count = 0;
                foreach (var point in bezier)
                {

                    float z1 = (slice * -10f) + -100f;
                    //Angle of attack
                    PointF rotatedPoint = RotatePoint(rotationAngle, point, scalePoint);
                    var newPoint = ScalePoint(scaleFactor, rotatedPoint, scalePoint);
                    newPoint.Y = newPoint.Y + (-10 * slice) + 150;
                    newPoint.X += 150;
                    g.DrawRectangle(greenPen, newPoint.X, newPoint.Y, 1, 1);
                    if (slice == 0 && count == 0)
                        linePoints1.Add(newPoint);
                    else if (slice == 0 && count == bezier.Count - 1)
                        linePoints1.Add(newPoint);
                    else if (slice == 29 && count == 0)
                        linePoints1.Add(newPoint);
                    else if (slice == 29 && count == bezier.Count - 1)
                        linePoints1.Add(newPoint);
                    count++;
                }
                count = 0;
                foreach (var point in bezier)
                {

                    float z2 = (slice * 10f) + -100f;
                    //Angle of attack
                    PointF rotatedPoint = RotatePoint(rotationAngle, point, scalePoint);
                    var newPoint = ScalePoint(scaleFactor, rotatedPoint, scalePoint);

                    newPoint.X += 150;
                    newPoint.Y = newPoint.Y + (10 * slice) + 250;
                    g.DrawRectangle(greenPen, newPoint.X, newPoint.Y, 1, 1);
                    if (slice == 0 && count == 0)
                        linePoints2.Add(newPoint);
                    else if (slice == 0 && count == bezier.Count - 1)
                        linePoints2.Add(newPoint);
                    else if (slice == 29 && count == 0)
                        linePoints2.Add(newPoint);
                    else if (slice == 29 && count == bezier.Count - 1)
                        linePoints2.Add(newPoint);
                    count++;
                }

                //return;

            }
            try
            {
                g.DrawLine(greenPen, linePoints1[0], linePoints1[2]);
                g.DrawLine(greenPen, linePoints1[1], linePoints1[3]);
                g.DrawLine(greenPen, linePoints2[0], linePoints2[2]);
                g.DrawLine(greenPen, linePoints2[1], linePoints2[3]);
            }
            catch { }


        }
        private static PointF RotatePoint(double theta, PointF p1, PointF center)
        {
            float x = 0;
            float y = 0;
            //translate
            float dx = p1.X - center.X;
            float dy = p1.Y - center.Y;
            //rotate
            x = dx * (float)Math.Cos(theta) + dy * (float)Math.Sin(theta);
            y = -dx * (float)Math.Sin(theta) + dy * (float)Math.Cos(theta);
            //translate
            x += center.X;
            y += center.Y;
            return new PointF(x, y);

        }
        private double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            edge = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            edge = 1;

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            edge = 3;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            pbBezlzier.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
