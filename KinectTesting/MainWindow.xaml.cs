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
using System.Threading;
using System.Runtime.InteropServices;


namespace KinectTesting
{

    public partial class MainWindow : Window
    {
        /// <summary>
        /// Active Kinect Sensor.
        /// </summary>
        private KinectSensor sensor; 

        /// <summary>
        /// Access on every stream, including the body stream.
        /// Need to let the sensor know that we need body tracking functionality by adding an additional parameter when initializing the reader.
        /// </summary>
        private MultiSourceFrameReader reader;

        /// <summary>
        /// Array of bodies currently in frame.
        /// </summary>
        private Body[] bodies;

        /// <summary>
        /// Height of kinect color frame
        /// </summary>
        private int colorHeight;

        /// <summary>
        /// Width of kinect color frame
        /// </summary>
        private int colorWidth;

        /// <summary>
        /// Width of kinect depth frame
        /// </summary>
        private int depthWidth;

        /// <summary>
        /// Array used to store color frame.
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Number of bytes used in a pixel. Currently Bgra32 format.
        /// </summary>
        readonly int bytesPerPixel = (PixelFormats.Bgra32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Frame by frame bitmap to display kinect stream
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Basically Graphics object in java
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Coordinate mapper, maps from kinect output (meters) to pixels on screen while adjusting for the offset of the sensor vs the camera, pretty cool
        /// </summary>
        private CoordinateMapper cMapper;

        /// <summary>
        /// Joint Radius for the indictors drawn
        /// </summary>
        private int jointRadius;

        private List<Ball> ball;

        private Ball rightHand;
        private Ball leftHand;
        private bool lHandInit;
        private bool rHandInit; 

        readonly Vect gravity = new Vect(0, .5);

        private int ballRadius;
        private int maxBallBoundX;
        private int maxBallBoundY;
        private int minBallBoundX;
        private int minBallBoundY;

        private int handMass;

        private const int BALLCOUNT = 30;


        public MainWindow()
        {
            InitializeComponent();
            InitializeKinect();
            InitializeBall();
            sensor.Open();
            if (sensor.IsAvailable)
            {
                kinectStatus.Foreground = Brushes.Red;
                kinectStatus.Content = "Error obtaining Kinect sensor data. Please reconnect device and try again.";
            }
            else
            {
                kinectStatus.Foreground = Brushes.Green;
                kinectStatus.Content = "Kinect Connected!";
            }
        }

        private void InitializeKinect()
        {
            sensor = KinectSensor.GetDefault(); //Get the kinect sensor

            cMapper = sensor.CoordinateMapper; 

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body); //Setting frames to color and body
            reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived; //setting the event that is fired when a frame is captured to what we want in the games

            colorHeight = sensor.ColorFrameSource.FrameDescription.Height;
            colorWidth = sensor.ColorFrameSource.FrameDescription.Width;

            depthWidth = sensor.DepthFrameSource.FrameDescription.Width;

            colorPixels = new byte[colorWidth * colorHeight * bytesPerPixel];
            colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96, 96, PixelFormats.Bgra32, null);
            cFrame.Source = colorBitmap;

            bodies = new Body[6];

            drawingGroup = new DrawingGroup();
            drawing.Source = new DrawingImage(drawingGroup); //"translates" DrawingGroup to the Image on the xaml          
        }

        private void InitializeBall()
        {
            lHandInit = false;
            rHandInit = false;
            ballRadius = 60;
            minBallBoundX = ballRadius * 4;
            minBallBoundY = 0;
            maxBallBoundX = colorWidth - ballRadius * 4;
            Console.WriteLine(depthWidth - ballRadius * 2);
            maxBallBoundY = colorHeight - ballRadius * 2;
            
            Random rng = new Random();
            ball = new List<Ball>();
            for (var x = 0; x < BALLCOUNT; x++)
                ball.Add(new Ball(new Vect(rng.Next(minBallBoundX, maxBallBoundX), minBallBoundY), gravity, ballRadius));
            jointRadius = 20;
            handMass = 200;

        }

        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();

            if (frame == null)
                return;

            #region Color Frame
            using (var cFrame = frame.ColorFrameReference.AcquireFrame())
            {
                if (cFrame != null)
                {
                    var frameDescription = cFrame.FrameDescription;

                    if (frameDescription.Width == colorBitmap.PixelWidth && frameDescription.Height == colorBitmap.PixelHeight) //if the frame is equal to the colorframe pixels on x and y
                    {
                        if (cFrame.RawColorImageFormat == ColorImageFormat.Bgra) //and if the formats are the same
                            cFrame.CopyRawFrameDataToArray(colorPixels);
                        else
                            cFrame.CopyConvertedFrameDataToArray(colorPixels, ColorImageFormat.Bgra);

                        colorBitmap.WritePixels(new Int32Rect(0, 0, frameDescription.Width, frameDescription.Height), colorPixels, frameDescription.Width * bytesPerPixel, 0);
                    }
                }
            }
            #endregion

            #region Body Frame
            using (var bFrame = frame.BodyFrameReference.AcquireFrame()) //get the body frame
            {
                if (bFrame != null)
                {
                    bFrame.GetAndRefreshBodyData(bodies);
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        Repaint(dc, colorWidth, colorHeight);
                        foreach (Body body in bodies)
                        {
                            if (body.IsTracked)
                            {
                                bFrame.GetAndRefreshBodyData(bodies);

                                foreach (var joint in body.Joints)
                                {
                                    if (joint.Value.TrackingState == TrackingState.Tracked || joint.Value.TrackingState == TrackingState.Inferred)
                                    {
                                        ColorSpacePoint colorSpacePoint = this.cMapper.MapCameraPointToColorSpace(joint.Value.Position);
                                        
                                        if (((0 + jointRadius) <= (int)colorSpacePoint.X) && (((int)colorSpacePoint.X + jointRadius) <= colorWidth) && ((0 + jointRadius) < (int)colorSpacePoint.Y) &&
                                           (((int)colorSpacePoint.Y + jointRadius) < colorHeight))
                                                DrawJointPoints(dc, colorSpacePoint, (joint.Value.TrackingState == TrackingState.Inferred));
                                        
                                        if ((lHandInit == false) || (rHandInit == false))
                                            HandBallInit(dc, joint.Value, colorSpacePoint);
                                    }
                                }

                                foreach (Ball b in ball)
                                {
                                    UpdateBall(b);
                                    Color col = b.IntToCol((int)(b.pos.x + b.pos.y));
                                    b.drawBall(dc, new SolidColorBrush(col));
                                    UpdateHandCollsions(b);
                                }
                                UpdateHandBall(dc, body.Joints[JointType.HandTipLeft], body.Joints[JointType.HandTipRight]);
                                DrawHandBall(dc, body.Joints[JointType.HandTipLeft], body.Joints[JointType.HandTipRight]);
                                
                            }
                        }
                        
                    }
                }
            }
            #endregion
        }

        private void Repaint(DrawingContext dc, int width, int height)
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, width, height));
        }

        private void DrawJointPoints(DrawingContext dc, ColorSpacePoint colorSpacePoint, bool isInferred)
        {
            if (!isInferred)
                dc.DrawEllipse(Brushes.Blue, null, new Point((int)colorSpacePoint.X, (int)colorSpacePoint.Y), jointRadius, jointRadius);
            else
            {
                if (((0 + jointRadius) <= (int)colorSpacePoint.X) && (((int)colorSpacePoint.X+jointRadius) <= colorWidth) && ((0 + jointRadius) < (int)colorSpacePoint.Y) && 
                   (((int)colorSpacePoint.Y + jointRadius) < colorHeight))
                        dc.DrawEllipse(Brushes.Red, null, new Point((int)colorSpacePoint.X, (int)colorSpacePoint.Y), jointRadius, jointRadius);
            }
        }

        private void UpdateBall(Ball b)
        {
            b.wallColl(.6, minBallBoundX, minBallBoundY, maxBallBoundX, maxBallBoundY);
            b.update();
        }

        private void HandBallInit(DrawingContext dc, Joint joint, ColorSpacePoint colorSpacePoint)
        {

            if (!lHandInit)
                if (joint.JointType.Equals(JointType.HandTipLeft))
                {
                    lHandInit = true;
                    leftHand = new Ball(new Vect((int)colorSpacePoint.X, (int)colorSpacePoint.Y), new Vect(0, 0), 50, handMass);
                }

            if (!rHandInit)
                if (joint.JointType.Equals(JointType.HandTipLeft))
                {
                    rHandInit = true;
                    rightHand = new Ball(new Vect((int)colorSpacePoint.X, (int)colorSpacePoint.Y), new Vect(0, 0), 50, handMass);
                }

        }

        private void UpdateHandBall(DrawingContext dc, Joint left, Joint right)
        {
            ColorSpacePoint colorSpacePoint = this.cMapper.MapCameraPointToColorSpace(left.Position);
            leftHand.pos = new Vect((int)colorSpacePoint.X, (int)colorSpacePoint.Y);
            leftHand.updateVel();

            colorSpacePoint = this.cMapper.MapCameraPointToColorSpace(right.Position);
            rightHand.pos = new Vect((int)colorSpacePoint.X, (int)colorSpacePoint.Y);
            rightHand.updateVel();
        }

        private void UpdateHandCollsions(Ball b)
        {
            leftHand.CheckColl(b);
            rightHand.CheckColl(b);
            b.CheckColl(ball);
        }

        private void DrawHandBall(DrawingContext dc, Joint left, Joint right)
        {
            ColorSpacePoint colorSpacePoint = this.cMapper.MapCameraPointToColorSpace(left.Position);
            if (((0 + jointRadius) <= (int)colorSpacePoint.X) && (((int)colorSpacePoint.X + jointRadius) <= colorWidth) && ((0 + jointRadius) < (int)colorSpacePoint.Y) &&
                (((int)colorSpacePoint.Y + jointRadius) < colorHeight))
                leftHand.drawBall(dc, new Pen(Brushes.Green, 10));
            colorSpacePoint = this.cMapper.MapCameraPointToColorSpace(right.Position);
            if (((0 + jointRadius) <= (int)colorSpacePoint.X) && (((int)colorSpacePoint.X + jointRadius) <= colorWidth) && ((0 + jointRadius) < (int)colorSpacePoint.Y) &&
               (((int)colorSpacePoint.Y + jointRadius) < colorHeight))
                rightHand.drawBall(dc, new Pen(Brushes.Green, 10));
            
        }
    }
 }

