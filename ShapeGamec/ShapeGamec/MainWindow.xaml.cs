using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace ShapeGamec
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        KinectSensor sensor;
        Skeleton[] totalSkeleton = new Skeleton[6];
        WriteableBitmap colorBitmap;
        byte[] colorPixels;
        int currentSkeletonID = 0;
        Skeleton skeleton;
        Thing thing = new Thing(); // a struct for ball
        double gravity = 0.025;
        int count = 0, thre = 0;

        private struct Thing
        {
            public System.Windows.Point Center;
            public double YVelocity;
            public double XVelocity;
            public Rectangle Shape;
            public bool Hit(System.Windows.Point joint)
            {
                double minDxSquared = this.Shape.RenderSize.Width;
                minDxSquared *= minDxSquared;
                double dist = SquaredDistance(Center.X, Center.Y, joint.X, joint.Y);
                if (dist <= minDxSquared)
                {
                    return true;
                }
                else
                    return false;
            }
        }
        private static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            return ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
        }
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.ColorStream.Enable();
            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.image1.Source = this.colorBitmap;
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            this.sensor.SkeletonStream.Enable();
            this.sensor.SkeletonFrameReady += skeletonFrameReady;
            this.sensor.ColorFrameReady += colorFrameReady;
            // start the sensor.
            this.sensor.Start();

            // new code for ball initialization
            thing.Shape = new Rectangle();
            thing.Shape.Width = 30; thing.Shape.Height = 30;
            thing.Shape.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            thing.Center.X = 200; thing.Center.Y = 0;
            thing.Shape.SetValue(Canvas.LeftProperty, thing.Center.X - thing.Shape.Width);
            thing.Shape.SetValue(Canvas.TopProperty, thing.Center.Y - thing.Shape.Width);
            canvas1.Children.Add(thing.Shape);
            canvas1.Children.Add(new TextBlock {Text = " " + count });

        }
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }
        void colorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (null == imageFrame)
                    return;

                imageFrame.CopyPixelDataTo(colorPixels);
                int stride = imageFrame.Width * imageFrame.BytesPerPixel;

                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels, stride, 0);
            }
        }


        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            canvas1.Children.Clear();
            advanceThingPosition();
            canvas1.Children.Add(thing.Shape);
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null) { return; }
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);
                skeleton = (from trackskeleton in totalSkeleton
                            where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                            select trackskeleton).FirstOrDefault();
                if (skeleton == null)
                    return;
                if (skeleton != null && this.currentSkeletonID != skeleton.TrackingId)
                {
                    this.currentSkeletonID = skeleton.TrackingId;
                    int totalTrackedJoints = skeleton.Joints.Where(item => item.TrackingState == JointTrackingState.Tracked).Count();
                    string TrackedTime = DateTime.Now.ToString("hh:mm:ss");
                    string status = "Skeleton Id: " + this.currentSkeletonID + ", total tracked joints: " + totalTrackedJoints + ", TrackTime: " + TrackedTime + "\n";

                }
                DrawSkeleton(skeleton);
            }
            Point handPt = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
            Point shoulderPt = ScalePosition(skeleton.Joints[JointType.ShoulderCenter].Position);
            Point spinePt = ScalePosition(skeleton.Joints[JointType.Spine].Position);
            Point shoulderlPt = ScalePosition(skeleton.Joints[JointType.ShoulderLeft].Position);
            Point elbowlPt = ScalePosition(skeleton.Joints[JointType.ElbowLeft].Position);
            Point wristlPt = ScalePosition(skeleton.Joints[JointType.WristLeft].Position);
            Point handlPt = ScalePosition(skeleton.Joints[JointType.HandLeft].Position);
            Point shoulderrPt = ScalePosition(skeleton.Joints[JointType.ShoulderRight].Position);
            Point elbowPt = ScalePosition(skeleton.Joints[JointType.ElbowRight].Position);
            Point wristPt = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
            Point hipPt = ScalePosition(skeleton.Joints[JointType.HipCenter].Position);
            Point hiplPt = ScalePosition(skeleton.Joints[JointType.HipLeft].Position);
            Point kneelPt = ScalePosition(skeleton.Joints[JointType.KneeLeft].Position);
            Point anklelPt = ScalePosition(skeleton.Joints[JointType.AnkleLeft].Position);
            Point footlPt = ScalePosition(skeleton.Joints[JointType.FootLeft].Position);
            Point hiprPt = ScalePosition(skeleton.Joints[JointType.HipRight].Position);
            Point kneePt = ScalePosition(skeleton.Joints[JointType.KneeRight].Position);
            Point anklePt = ScalePosition(skeleton.Joints[JointType.AnkleRight].Position);
            Point footPt = ScalePosition(skeleton.Joints[JointType.FootRight].Position);
            float x1 = skeleton.Joints[JointType.ElbowRight].Position.X;
            float y1 = skeleton.Joints[JointType.ElbowRight].Position.Y;
            //float z1 = skeleton.Joints[JointType.ElbowLeft].Position.Z;

            float x2 = skeleton.Joints[JointType.HipRight].Position.X;
            float y2 = skeleton.Joints[JointType.HipRight].Position.Y;
            //float z2 = skeleton.Joints[JointType.HipLeft].Position.Z;

            float x3 = skeleton.Joints[JointType.ShoulderRight].Position.X;
            float y3 = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            //float z3 = skeleton.Joints[JointType.ShoulderLeft].Position.Z;

            var diffX = Math.Sqrt(Math.Pow((x3 - x1), 2) + Math.Pow((y3 - y1), 2));
            var diffY = Math.Sqrt(Math.Pow((x3 - x2), 2) + Math.Pow((y3 - y2), 2));
            var diffA = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            var angleDegrees = Math.Acos((diffY * diffY + diffX * diffX - diffA * diffA) / (2 * diffX * diffY)) * 180 / Math.PI;

            if (thing.Hit(handPt) || thing.Hit(shoulderPt) || thing.Hit(spinePt) || thing.Hit(shoulderlPt) || thing.Hit(elbowlPt) || thing.Hit(wristlPt) || thing.Hit(handlPt) || thing.Hit(shoulderrPt) || thing.Hit(hipPt) || thing.Hit(hiplPt) || thing.Hit(elbowPt) || thing.Hit(wristPt) || thing.Hit(kneelPt) || thing.Hit(hiprPt) || thing.Hit(anklelPt) || thing.Hit(footlPt) || thing.Hit(hiprPt) || thing.Hit(kneePt) || thing.Hit(footPt))
            {
                //thing.XVelocity += this.gravity;

                this.thing.YVelocity = -1.0 * this.thing.YVelocity;
                if (angleDegrees > 90)
                {
                    this.thing.XVelocity = 1.0 * this.thing.YVelocity;
                }
                else if (angleDegrees < 90)
                {
                    this.thing.XVelocity = -1.0 * this.thing.YVelocity;
                }
                thre++;

                if (thre == 1)
                {
                    count++;

                    this.textBox.Text = "" + count;

                }
            }

            else
            {
                thre = 0;

            }




        }


        private void DrawSkeleton(Skeleton skeleton)
        {
            drawBone(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);

            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
            drawBone(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            drawBone(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            drawBone(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
            drawBone(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            drawBone(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            drawBone(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

            drawBone(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
            drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
            drawBone(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
            drawBone(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
            drawBone(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

            drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
            drawBone(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
            drawBone(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
            drawBone(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);

        }
        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution320x240Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        void drawBone(Joint trackedJoint1, Joint trackedJoint2)
        {
            Line bone = new Line();
            bone.Stroke = Brushes.Blue;
            bone.StrokeThickness = 3;
            Point joint1 = this.ScalePosition(trackedJoint1.Position);
            bone.X1 = joint1.X;
            bone.Y1 = joint1.Y;

            Point joint2 = this.ScalePosition(trackedJoint2.Position);
            bone.X2 = joint2.X;
            bone.Y2 = joint2.Y;

            canvas1.Children.Add(bone);
        }

        void advanceThingPosition()
        {
            thing.Center.Offset(thing.XVelocity, thing.YVelocity);
            thing.YVelocity += this.gravity;
            thing.XVelocity += 0;


            thing.Shape.SetValue(Canvas.LeftProperty, thing.Center.X - thing.Shape.Width);
            thing.Shape.SetValue(Canvas.TopProperty, thing.Center.Y - thing.Shape.Width);

            // if goes out of bound, reset position, as well as velocity
            if (thing.Center.Y >= canvas1.Height)
            {
                thing.Center.Y = 0;
                thing.XVelocity = 0;
                thing.YVelocity = 0;
                thing.Center.X = 200;
                count = 0;
                this.textBox.Text = "" + thre;

            }


        }
    }
}